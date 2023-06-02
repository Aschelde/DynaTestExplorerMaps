using System;
using System.Collections.Generic;
using DynaTestExplorerMaps.Interfaces;
using System.Linq;
using Npgsql;
using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.Messages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DynaTestExplorerMaps.Models;

namespace DynaTestExplorerMaps.DataAccess
{
    class DataAccessLayer : IDataAccessLayer
    {
        private int _currentSurveyId;
        private int _distancePerSegment;
        private string _measurementType;
        private int _imageLength;
        private string _connectionString = "Server=localhost;Port=5433;Database=dynatestexplorer;User Id=postgres;Password=password;";
        private List<GpsPoint> _gpsPoints;
        private List<GpsPoint> _interpolatedImagePoints;
        private List<MeasurementItem> _measurementItems;
        private List<MeasurementSegment> _segments;

        IImageLoader _imageLoader;

        public DataAccessLayer(IImageLoader imageLoader)
        {
            _currentSurveyId = 0;
            _distancePerSegment = 10;
            _measurementType = "IRI";

            GetGpsPoints();
            GetInterpolatedImagePoints();
            GetMeasurementItems();

            WeakReferenceMessenger.Default.Register<MeasurementIntervalChangedMessage>(this, (r, m) =>
            {
                _distancePerSegment = m.Value;
                _segments = null;

                OnPropertyChanged();
            });

            WeakReferenceMessenger.Default.Register<MeasurementTypeChangedMessage>(this, (r, m) =>
            {
                _measurementType = m.Value;
                _segments = null;

                OnPropertyChanged();
            });

            _imageLoader = imageLoader;
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

                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"SELECT point_id, distance, ST_X(geom) AS latitude, ST_Y(geom) AS longitude " +
                            $"FROM points WHERE survey_id = {_currentSurveyId}", conn))
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


        public List<GpsPoint> GetInterpolatedImagePoints()
        {
            if (_interpolatedImagePoints == null)
            {
                _interpolatedImagePoints = new List<GpsPoint>();

                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"SELECT section_id, distance_begin, ST_Y(ip) AS latitude, ST_X(ip) AS longitude FROM ( " +
                    $"SELECT section_id, distance_begin, interpolate_point({_currentSurveyId}, section_id) AS ip " +
                    $"FROM image_sections WHERE image_sections.survey_id = {_currentSurveyId}) AS subquery", conn))
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

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"SELECT section_id, image_path FROM image_sections " +
                        $"WHERE survey_id = {_currentSurveyId}", conn))
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

        public List<MeasurementItem> GetMeasurementItems()
        {
            if (_measurementItems == null)
            {
                _measurementItems = new List<MeasurementItem>();

                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"SELECT value_id, distance_begin, distance_end, measurement_value, measurement_type " +
                            $"FROM measurements WHERE survey_id = {_currentSurveyId} AND measurement_type = 'IRI'", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var iriItem = new MeasurementItem
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("value_id")),
                                    Type = reader.GetString(reader.GetOrdinal("measurement_type")),
                                    DistanceRange = new Tuple<float, float>(reader.GetFloat(reader.GetOrdinal("distance_begin")), reader.GetFloat(reader.GetOrdinal("distance_end"))),
                                    Value = reader.GetFloat(reader.GetOrdinal("measurement_value"))
                                };

                                _measurementItems.Add(iriItem);
                            }
                        }
                    }
                }
            }

            return _measurementItems;
        }

        public List<MeasurementSegment> GetMeasurementSegments()
        {
            if (_segments == null)
            {
                _segments = new List<MeasurementSegment>();

                List<MeasurementItem> itemsInRange;
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
                        // Find all the MeasurementItems within the current segment
                        itemsInRange = _measurementItems
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
                        itemsInRange = _measurementItems
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
                        itemsInRange = _measurementItems
                            .Where(x => x.DistanceRange.Item1 > startDistance && x.DistanceRange.Item2 <= endDistance)
                            .ToList();

                        // Find all the image points within the current segment
                        imagePointIds = _interpolatedImagePoints
                            .Where(x => x.Distance > startDistance && x.Distance <= endDistance)
                            .Select(x => x.Id)
                            .ToList();
                    }


                    // Calculate the average IRI within the current segment
                    float meanValue = itemsInRange.Any() ? itemsInRange.Average(x => x.Value) : 0;

                    // Create the segment and add it to the list
                    MeasurementSegment segment = new MeasurementSegment
                    {
                        Id = currentId,
                        Type = _measurementType,
                        DistanceRange = new Tuple<float, float>(startDistance, endDistance),
                        MeanValue = meanValue,
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
