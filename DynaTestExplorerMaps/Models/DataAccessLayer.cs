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
using System.Security.Cryptography.X509Certificates;
using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.Messages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace DynaTestExplorerMaps.Models
{
    class DataAccessLayer : IDataAccessLayer
    {
        private int currentSurveyId;
        private int _distancePerSegment;
        private int _imageLength;
        private string connectionString = "Server=localhost;Port=5433;Database=dynatestexplorer;User Id=postgres;Password=password;";
        private List<GpsPoint> _gpsPoints;
        private List<GpsPoint> _interpolatedImagePoints;
        private List<IriItem> _iriItems;
        private List<IriSegment> _segments;

        IImageLoader _imageLoader;

        public DataAccessLayer(IImageLoader imageLoader, int surveyId)
        {
            currentSurveyId = surveyId;
            _distancePerSegment = 10;

            GetGpsPoints();
            GetInterpolatedImagePoints();
            GetIriItems();

            WeakReferenceMessenger.Default.Register<MeasurementIntervalChangedMessage>(this, (r, m) =>
            {
                _distancePerSegment = m.Value;
                _segments = null;

                OnPropertyChanged();
            });

            this._imageLoader = imageLoader;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<GpsPoint> GetGpsPoints()
        {
            if (_gpsPoints == null)
            {
                _gpsPoints = new List<GpsPoint>();

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

                                _gpsPoints.Add(gpsPoint);
                            }
                        }
                    }
                }
            }
            return _gpsPoints;
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
            if (_interpolatedImagePoints == null)
            {
                _interpolatedImagePoints = new List<GpsPoint>();

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

                                _interpolatedImagePoints.Add(gpsPoint);
                            }
                        }
                    }
                }
            }

            // Set the distance per segment to be the length of an image
            _imageLength = (int)Math.Round(_interpolatedImagePoints[1].Distance);
            _distancePerSegment = _imageLength;

            return _interpolatedImagePoints;
        }

        public List<ImageItem> GetImages()
        {
            var images = new List<ImageItem>();

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
                                Image = _imageLoader.GetImage(reader.GetString(reader.GetOrdinal("image_path")))
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
            if (_iriItems == null)
            {
                _iriItems = new List<IriItem>();

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

                                _iriItems.Add(iriItem);
                            }
                        }
                    }
                }
            }

            return _iriItems;
        }

        public List<IriSegment> GetIriSegments()
        {
            if (_segments == null)
            {
                _segments = new List<IriSegment>();

                List<IriItem> itemsInRange;
                List<int> imagePointIds;

                float startDistance = _gpsPoints[0].Distance;
                float endDistance = startDistance + _distancePerSegment;
                int currentId = 0;
                int i = 0;
                while (i < _gpsPoints.Count)
                {
                    if (_gpsPoints[i].Distance > endDistance)
                    {
                        // Calculate new startDistance and endDistance
                        startDistance = endDistance;
                        endDistance = _gpsPoints[i].Distance + _distancePerSegment;

                        currentId++;
                    }

                    // Find all the path points within the current segment
                    List<int> pointIds = _gpsPoints
                        .Where(x => x.Distance >= startDistance && x.Distance <= endDistance)
                        .Select(x => x.Id)
                        .ToList();


                    if (currentId == 0)
                    {
                        // Find all the IriItems within the current segment
                        itemsInRange = _iriItems
                            .Where(x => x.DistanceRange.Item2 <= endDistance)
                            .ToList();

                        // Find all the image points within the current segment
                        imagePointIds = _interpolatedImagePoints
                            .Where(x => x.Distance <= endDistance)
                            .Select(x => x.Id)
                            .ToList();
                    }
                    else if (endDistance >= _gpsPoints[^1].Distance)
                    {
                        // Find all the IriItems within the current segment
                        itemsInRange = _iriItems
                            .Where(x => x.DistanceRange.Item2 > startDistance)
                            .ToList();

                        // Find all the image points within the current segment
                        imagePointIds = _interpolatedImagePoints
                            .Where(x => x.Distance > startDistance)
                            .Select(x => x.Id)
                            .ToList();
                    }
                    else
                    {
                        // Find all the IriItems within the current segment
                        itemsInRange = _iriItems
                            .Where(x => x.DistanceRange.Item1 > startDistance && x.DistanceRange.Item2 <= endDistance)
                            .ToList();

                        // Find all the image points within the current segment
                        imagePointIds = _interpolatedImagePoints
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
                    i = pointIds[^1] + 1;

                    _segments.Add(segment);
                }
            }

            return _segments;
        }

        public Tuple<int, int> GetMaxMinMeasurementInterval()
        {
            return new Tuple<int, int>(_imageLength, (int)Math.Round(_gpsPoints[^1].Distance));
        }
    }
}
