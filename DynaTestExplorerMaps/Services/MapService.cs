using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Models;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Services
{
    public class MapService : IMapService
    {
        private GraphicsOverlay _gpsPointsGraphicsOverlay;
        private Dictionary<Graphic, GpsPoint> _pointGraphicToGpsPointMap;
        private List<GpsPoint> _points;
        private Graphic _lastSelectedGraphic;

        // Move the map creation logic into the MapService.
        public object CreateMap()
        {
            // Create a new map with a 'topographic vector' basemap.
            return new Map(BasemapStyle.ArcGISStreets);
        }

        public object CreateBounds()
        {
            Envelope graphicsExtent;

            // Check if _gpsPointsGraphicsOverlay is null
            if (_gpsPointsGraphicsOverlay == null || _gpsPointsGraphicsOverlay.Graphics.Count == 0)
            {
                // Create a default envelope of 1km x 1km (assuming spatial reference unit is meter)
                graphicsExtent = new Envelope(0, 0, 1000, 1000, SpatialReferences.WebMercator);
            }
            else
            {
                // Get the extent of your graphics overlay
                graphicsExtent = _gpsPointsGraphicsOverlay.Graphics.Select(graphic => graphic.Geometry.Extent).CombineExtents();

                Debug.WriteLine(graphicsExtent.XMin + ", " + graphicsExtent.YMin + ", " + graphicsExtent.XMax + ", " + graphicsExtent.YMax);
            }

            SpatialReference projectedSR = SpatialReferences.WebMercator;
            return GeometryEngine.Project(graphicsExtent, projectedSR) as Envelope;
        }

        public GraphicsData CreateGraphics(IDataAccessLayer dataAccessLayer, int selectionId)
        {
            // Create a new graphics overlay to contain a variety of graphics.
            _gpsPointsGraphicsOverlay = new GraphicsOverlay()
            {
                Id = "gpsPointsOverlay"
            };

            var linesGraphicsOverlay = new GraphicsOverlay()
            {
                Id = "linesOverlay"
            };

            GraphicsOverlayCollection overlays = new GraphicsOverlayCollection
            {
                linesGraphicsOverlay,
                _gpsPointsGraphicsOverlay
            };

            _points = dataAccessLayer.GetInterpolatedImagePoints();

            // Create a symbol to define how the point is displayed.
            var pointSymbol = new SimpleMarkerSymbol
            {
                Style = SimpleMarkerSymbolStyle.Circle,
                Color = System.Drawing.Color.LightBlue,
                Size = 10.0
            };

            // Add an outline to the symbol.
            pointSymbol.Outline = new SimpleLineSymbol
            {
                Style = SimpleLineSymbolStyle.Solid,
                Color = System.Drawing.Color.Blue,
                Width = 1.0
            };

            _pointGraphicToGpsPointMap = new Dictionary<Graphic, GpsPoint>();

            // Create all Gps points as point graphic.
            foreach (GpsPoint point in _points)
            {
                var pointGraphic = new Graphic(new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84), pointSymbol)
                {
                    Attributes = { ["Id"] = point.Id }
                };
                _gpsPointsGraphicsOverlay.Graphics.Add(pointGraphic);
                _pointGraphicToGpsPointMap.Add(pointGraphic, point);
            }

            // Create a polyline builder from the list of points.
            var polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            foreach (GpsPoint point in _points)
            {
                polylineBuilder.AddPoint(new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84));
            }

            var lineSymbol = new SimpleLineSymbol
            {
                Style = SimpleLineSymbolStyle.Solid,
                Color = System.Drawing.Color.DarkBlue,
                Width = 2.0
            };

            // Create a graphic from the polyline builder.
            var lineGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            linesGraphicsOverlay.Graphics.Add(lineGraphic);

            UpdateTracker(selectionId);

            return new GraphicsData { Overlays = overlays, Points = _points, PointGraphicToGpsPointMap = _pointGraphicToGpsPointMap };
        }

        public void UpdateTracker(int Id)
        {
            // If there's a previously selected graphic, reset its symbol
            if (_lastSelectedGraphic != null)
            {
                var pointSymbol = new SimpleMarkerSymbol
                {
                    Style = SimpleMarkerSymbolStyle.Circle,
                    Color = System.Drawing.Color.LightBlue,
                    Size = 10.0
                };
                pointSymbol.Outline = new SimpleLineSymbol
                {
                    Style = SimpleLineSymbolStyle.Solid,
                    Color = System.Drawing.Color.Blue,
                    Width = 1.0
                };
                _lastSelectedGraphic.Symbol = pointSymbol;
            }

            // Look for the existing graphic for the selected ID in the _gpsPointsGraphicsOverlay.
            Graphic selectedGraphic = _gpsPointsGraphicsOverlay.Graphics.FirstOrDefault(g => _pointGraphicToGpsPointMap[g].Id == Id);

            if (selectedGraphic != null)
            {
                // Change the symbol for the existing graphic to a normal symbol.
                var pointSymbol = new SimpleMarkerSymbol
                {
                    Style = SimpleMarkerSymbolStyle.Circle,
                    Color = System.Drawing.Color.LightBlue,
                    Size = 10.0
                };
                pointSymbol.Outline = new SimpleLineSymbol
                {
                    Style = SimpleLineSymbolStyle.Solid,
                    Color = System.Drawing.Color.Blue,
                    Width = 1.0
                };
                selectedGraphic.Symbol = pointSymbol;
            }

            GpsPoint? point = _points.Find(GpsPoint => GpsPoint.Id == Id);
            GpsPoint? nextPoint = _points.Find(GpsPoint => GpsPoint.Id == Id + 1);

            double angle = 0;
            // Calculate the angle between the two points
            if (nextPoint != null)
            {
                angle = Math.Atan2(nextPoint.Latitude - point.Latitude, nextPoint.Longitude - point.Longitude) * 180 / Math.PI;
            }
            else
            {
                // Use angle from current to last point
                GpsPoint? lastPoint = _points.Find(GpsPoint => GpsPoint.Id == Id - 1);
                if (lastPoint != null)
                {
                    angle = Math.Atan2(lastPoint.Latitude - point.Latitude, lastPoint.Longitude - point.Longitude) * 180 / Math.PI;
                }
            }

            // Find the existing graphic for the new GPS point for the selected ID in the _gpsPointsGraphicsOverlay.
            Graphic newGraphic = _gpsPointsGraphicsOverlay.Graphics.FirstOrDefault(g => _pointGraphicToGpsPointMap[g] == point);

            if (newGraphic != null && _pointGraphicToGpsPointMap[selectedGraphic].Id == Id)
            {
                var pictureMarkerSymbol = new PictureMarkerSymbol(new Uri("pack://application:,,,/Graphics/arrow.png"));

                pictureMarkerSymbol.Width = 50;
                pictureMarkerSymbol.Height = 50;
                pictureMarkerSymbol.Angle = -angle;

                // Update the existing graphic for the new GPS point with a different symbol.
                var pointSymbol = new SimpleMarkerSymbol
                {
                    Style = SimpleMarkerSymbolStyle.Circle,
                    Color = System.Drawing.Color.Red,
                    Size = 15.0
                };
                pointSymbol.Outline = new SimpleLineSymbol
                {
                    Style = SimpleLineSymbolStyle.Solid,
                    Color = System.Drawing.Color.DarkRed,
                    Width = 3.0
                };
                newGraphic.Symbol = pictureMarkerSymbol;
            }

            _lastSelectedGraphic = newGraphic;
        }
    }
}
