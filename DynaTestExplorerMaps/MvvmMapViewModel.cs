using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynaTestExplorerMaps.model;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Xps.Serialization;
using Windows.System;

namespace DynaTestExplorerMaps
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MvvmMapViewModel
    {
        private Map _map;
        private MapView _mapView;
        private GraphicsOverlay _gpsPointsGraphicsOverlay;
        private List<GpsPoint> points;
        private Dictionary<Graphic, GpsPoint> _pointGraphicToGpsPointMap;
        private string _selectionId;

        public MvvmMapViewModel()
        {
            SetupMap();

            _selectionId = "000000";

            CreateGraphics();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Map Map
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        public MapView MapView
        {
            get { return _mapView; }
            set
            {
                _mapView = value;
                OnPropertyChanged();
            }
        }

        private GraphicsOverlayCollection _graphicsOverlays;
        public GraphicsOverlayCollection GraphicsOverlays
        {
            get { return _graphicsOverlays; }
            set
            {
                _graphicsOverlays = value;
                OnPropertyChanged();
            }
        }

        private void SetupMap()
        {
            // Create a new map with a 'topographic vector' basemap.
            Map = new Map(BasemapStyle.ArcGISStreets);

            _mapView = new MapView();
            _mapView.Map = Map;

        }

        private void CreateGraphics()
        {
            // Create a new graphics overlay to contain a variety of graphics.
            _gpsPointsGraphicsOverlay = new GraphicsOverlay();

            GPSPointLoader loader = new GPSPointLoader();
            loader.setPath("C:\\Users\\Asger\\Bachelor\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej.GPX");
            points = loader.getGpsPoints();

            // Create a symbol to define how the point is displayed.
            var pointSymbol = new SimpleMarkerSymbol
            {
                Style = SimpleMarkerSymbolStyle.Circle,
                Color = System.Drawing.Color.Orange,
                Size = 5.0
            };

            // Add an outline to the symbol.
            pointSymbol.Outline = new SimpleLineSymbol
            {
                Style = SimpleLineSymbolStyle.Solid,
                Color = System.Drawing.Color.Blue,
                Width = 1.0
            };

            _pointGraphicToGpsPointMap = new Dictionary<Graphic, GpsPoint>();

            //create all Gps points as point graphic.
            foreach (GpsPoint point in points)
            {
                var pointGraphic = new Graphic(new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84), pointSymbol);
                _gpsPointsGraphicsOverlay.Graphics.Add(pointGraphic);

                _pointGraphicToGpsPointMap.Add(pointGraphic, point);
            }

            // Create a polyline builder from the list of points.
            var polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            foreach (GpsPoint point in points)
            {
                polylineBuilder.AddPoint(new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84));
            }

            var lineSymbol = new SimpleLineSymbol
            {
                Style = SimpleLineSymbolStyle.Solid,
                Color = System.Drawing.Color.Red,
                Width = 2.0
            };

            // Create a graphic from the polyline builder.
            var lineGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            _gpsPointsGraphicsOverlay.Graphics.Add(lineGraphic);

            //Add the graphics overlay to the MapView.
            _mapView.GraphicsOverlays.Add(_gpsPointsGraphicsOverlay);
        }

        public void UpdateTracker(string Id)
        {
            // Look for the existing graphic for the selected ID in the _mapView.GraphicsOverlays.
            Graphic selectedGraphic = _mapView.GraphicsOverlays.SelectMany(g => g.Graphics).FirstOrDefault(g => _pointGraphicToGpsPointMap[g].Name == _selectionId);

            if (selectedGraphic != null)
            {
                // Change the symbol for the existing graphic to a normal symbol.
                var pointSymbol = new SimpleMarkerSymbol
                {
                    Style = SimpleMarkerSymbolStyle.Circle,
                    Color = System.Drawing.Color.Orange,
                    Size = 5.0
                };
                pointSymbol.Outline = new SimpleLineSymbol
                {
                    Style = SimpleLineSymbolStyle.Solid,
                    Color = System.Drawing.Color.Blue,
                    Width = 1.0
                };
                selectedGraphic.Symbol = pointSymbol;
            }

            GpsPoint? point = points.Find(GpsPoint => GpsPoint.Name == Id);

            // Find the existing graphic for the new GPS point for the selected ID in the _gpsPointsGraphicsOverlay.
            Graphic newGraphic = _mapView.GraphicsOverlays.SelectMany(g => g.Graphics).FirstOrDefault(g => _pointGraphicToGpsPointMap[g] == point);

            if (newGraphic != null && _pointGraphicToGpsPointMap[selectedGraphic].Name == _selectionId)
            {
                // Update the existing graphic for the new GPS point with a different symbol.
                var pointSymbol = new SimpleMarkerSymbol
                {
                    Style = SimpleMarkerSymbolStyle.Circle,
                    Color = System.Drawing.Color.Green,
                    Size = 12.0
                };
                pointSymbol.Outline = new SimpleLineSymbol
                {
                    Style = SimpleLineSymbolStyle.Solid,
                    Color = System.Drawing.Color.DarkGreen,
                    Width = 3.0
                };
                newGraphic.Symbol = pointSymbol;
            }
        }

        public void UpdateSelection(string newSelectionId)
        {
            UpdateTracker(newSelectionId);
            _selectionId = newSelectionId;
        }
    }
}
