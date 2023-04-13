using System;
using System.Collections.Generic;
using DynaTestExplorerMaps.Interfaces;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace DynaTestExplorerMaps.Models
{
    class DataAccessLayer : IDataAccessLayer
    {
        private int currentSurveyId;
        private string connectionString = "Server=localhost;Port=5433;Database=dynatestexplorer;User Id=postgres;Password=password;";

        public DataAccessLayer(int surveyId)
        {
            currentSurveyId = surveyId;
        }

        public List<GpsPoint> GetGpsPoints()
        {
            var gpsPoints = new List<GpsPoint>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT point_id, distance, SELECT ST_X(geom) AS latitude, ST_Y(geom) AS longitude " +
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
    }
}
