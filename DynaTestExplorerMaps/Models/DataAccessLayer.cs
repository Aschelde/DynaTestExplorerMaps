using System;
using System.Collections.Generic;
using DynaTestExplorerMaps.Interfaces;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Threading;
using System.Diagnostics;
using System.Windows.Media;

namespace DynaTestExplorerMaps.Models
{
    class DataAccessLayer : IDataAccessLayer
    {
        private int currentSurveyId;
        private int _distancePerSegment;
        private string connectionString = "Server=localhost;Port=5433;Database=dynatestexplorer;User Id=postgres;Password=password;";

        public DataAccessLayer(int surveyId)
        {
            currentSurveyId = surveyId;
            _distancePerSegment = 30;
        }

        public List<GpsPoint> GetGpsPoints()
        {
            var gpsPoints = new List<GpsPoint>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT point_id, distance, ST_X(geom) AS latitude, ST_Y(geom) AS longitude " +
                        $"FROM points WHERE survey_id = {currentSurveyId}", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var gpsPoint = new GpsPoint
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("point_id")),
                                Distance = reader.GetFloat(reader.GetOrdinal("distance")),
                                Latitude = (float)reader.GetDouble(reader.GetOrdinal("latitude")),
                                Longitude = (float)reader.GetDouble(reader.GetOrdinal("longitude")),
                            };

                            gpsPoints.Add(gpsPoint);
                        }
                    }
                }
            }

            return gpsPoints;
        }

        public List<GpsPoint> GetGpsPointsReduced(int distancePerPoint)
        {
            var gpsPoints = new List<GpsPoint>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT DISTINCT ON (distance / {distancePerPoint}) point_id, distance, time_s, latitude, longitude, elevation " +
                        $"FROM points WHERE survey_id = {currentSurveyId}", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var gpsPoint = new GpsPoint
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("point_id")),
                                Distance = reader.GetFloat(reader.GetOrdinal("distance")),
                                Latitude = reader.GetFloat(reader.GetOrdinal("latitude")),
                                Longitude = reader.GetFloat(reader.GetOrdinal("longitude")),
                           };

                            gpsPoints.Add(gpsPoint);
                        }
                    }
                }
            }

            return gpsPoints;
        }

        public List<GpsPoint> GetInterpolatedImagePoints()
        {
            var gpsPoints = new List<GpsPoint>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT section_id, distance_begin, ST_Y(ip) AS latitude, ST_X(ip) AS longitude FROM ( " +
                $"SELECT section_id, distance_begin, interpolate_point({currentSurveyId}, section_id) AS ip " +
                $"FROM image_sections WHERE image_sections.survey_id = {currentSurveyId}) AS subquery", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var gpsPoint = new GpsPoint
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("section_id")),
                                Distance = (float)reader.GetDouble(reader.GetOrdinal("distance_begin")),
                                Latitude = (float)reader.GetDouble(reader.GetOrdinal("latitude")),
                                Longitude = (float)reader.GetDouble(reader.GetOrdinal("longitude"))
                            };

                            gpsPoints.Add(gpsPoint);
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(gpsPoints.Count);
            return gpsPoints;
        }

        public List<ImageItem> GetImages()
        {
            var images = new List<ImageItem>();
            ImageLoader imageLoader = new ImageLoader();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT section_id, image_path FROM image_sections " +
                        $"WHERE survey_id = {currentSurveyId}", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var imageItem = new ImageItem
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("section_id")),
                                Image = imageLoader.GetImage(reader.GetString(reader.GetOrdinal("image_path")))
                            };

                            images.Add(imageItem);
                        }
                    }
                }
            }

            return images;
        }

        public List<IriItem> GetIriItems()
        {
            var iriItems = new List<IriItem>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT value_id, distance_begin, distance_end, iri_value " +
                        $"FROM iri_values WHERE survey_id = {currentSurveyId}", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var iriItem = new IriItem
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("value_id")),
                                DistanceRange = new Tuple<float, float>(reader.GetFloat(reader.GetOrdinal("distance_begin")), reader.GetFloat(reader.GetOrdinal("distance_end"))),
                                Iri = reader.GetFloat(reader.GetOrdinal("iri_value"))
                            };

                            iriItems.Add(iriItem);
                        }
                    }
                }
            }

            return iriItems;
        }

        public List<IriSegment> GetIriSegments()
        {
            List<IriSegment> segments = new List<IriSegment>();
            List<IriItem> iriItems = GetIriItems();
            List<GpsPoint> pathPoints = GetGpsPoints();
            List<GpsPoint> interpolatedImagePoints = GetInterpolatedImagePoints();

            List<IriItem> itemsInRange;
            List<int> imagePointIds;

            float startDistance = pathPoints[0].Distance;
            float endDistance = startDistance + _distancePerSegment;
            int currentId = 0;
            int i = 0;
            while (i < pathPoints.Count)
            {
                if (pathPoints[i].Distance > endDistance)
                {
                    // Calculate new startDistance and endDistance
                    startDistance = endDistance;
                    endDistance = pathPoints[i].Distance + _distancePerSegment;

                    currentId++;
                }

                // Find all the path points within the current segment
                List<int> pointIds = pathPoints
                    .Where(x => x.Distance >= startDistance && x.Distance <= endDistance)
                    .Select(x => x.Id)
                    .ToList();

                
                if (currentId == 0)
                {
                    // Find all the IriItems within the current segment
                    itemsInRange = iriItems
                        .Where(x => x.DistanceRange.Item2 <= endDistance)
                        .ToList();

                    // Find all the image points within the current segment
                    imagePointIds = interpolatedImagePoints
                        .Where(x => x.Distance <= endDistance)
                        .Select(x => x.Id)
                        .ToList();
                } else if (endDistance >= pathPoints[^1].Distance)
                {
                    // Find all the IriItems within the current segment
                    itemsInRange = iriItems
                        .Where(x => x.DistanceRange.Item2 > startDistance)
                        .ToList();

                    // Find all the image points within the current segment
                    imagePointIds = interpolatedImagePoints
                        .Where(x => x.Distance > startDistance)
                        .Select(x => x.Id)
                        .ToList();
                } else
                {
                    // Find all the IriItems within the current segment
                    itemsInRange = iriItems
                        .Where(x => x.DistanceRange.Item1 > startDistance && x.DistanceRange.Item2 <= endDistance)
                        .ToList();

                    // Find all the image points within the current segment
                    imagePointIds = interpolatedImagePoints
                        .Where(x => x.Distance > startDistance && x.Distance <= endDistance)
                        .Select(x => x.Id)
                        .ToList();
                }
                

                // Calculate the average IRI within the current segment
                float averageIri = itemsInRange.Any() ? itemsInRange.Average(x => x.Iri) : 0;

                // Create the segment and add it to the list
                IriSegment segment = new IriSegment
                {
                    Id = currentId,
                    DistanceRange = new Tuple<float, float>(startDistance, endDistance),
                    AverageIri = averageIri,
                    Images = imagePointIds,
                    Points = pointIds
                };

                // Set i to next pathPoint after current segment
                i = pointIds[^1]+1;

                segments.Add(segment);
            }

            return segments;
        }
    }
}
