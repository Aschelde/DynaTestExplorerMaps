using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.EventHandling;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Xps.Serialization;
using Windows.System;

namespace DynaTestExplorerMaps.ViewModels
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MapViewModel : BaseViewModel, IUserControlViewModel
    {
        private Map _map;
        private GraphicsOverlay _gpsPointsGraphicsOverlay;
        private GraphicsOverlay _linesGraphicsOverlay;
        private List<GpsPoint> points;
        private Dictionary<Graphic, GpsPoint> _pointGraphicToGpsPointMap;
        private string _selectionId;

        public ICommand GeoViewTappedCommand { get; set; }

        public MapViewModel()
        {
            SetupMap();

            _selectionId = "000000";

            CreateGraphics();

            GeoViewTappedCommand = new RelayCommand<Graphic>(HandleGeoViewTapped);

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
                UpdateSelection(m.Value);
            });
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

        }

        private void CreateGraphics()
        {
            // Create a new graphics overlay to contain a variety of graphics.
            _gpsPointsGraphicsOverlay = new GraphicsOverlay()
            {
                Id = "gpsPointsOverlay"
            };

            _linesGraphicsOverlay = new GraphicsOverlay()
            {
                Id = "linesOverlay"
            };

            GraphicsOverlayCollection overlays = new GraphicsOverlayCollection
            {
                _linesGraphicsOverlay,
                _gpsPointsGraphicsOverlay
            };
            this.GraphicsOverlays = overlays;

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
            _linesGraphicsOverlay.Graphics.Add(lineGraphic);
        }

        public void UpdateTracker(string Id)
        {
            // Look for the existing graphic for the selected ID in the _mapView.GraphicsOverlays.
            Graphic selectedGraphic = _gpsPointsGraphicsOverlay.Graphics.FirstOrDefault(g => _pointGraphicToGpsPointMap[g].Name == _selectionId);

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
            Graphic newGraphic = _gpsPointsGraphicsOverlay.Graphics.FirstOrDefault(g => _pointGraphicToGpsPointMap[g] == point);

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
            Debug.WriteLine("UpdateSelection");
            UpdateTracker(newSelectionId);
            _selectionId = newSelectionId;
        }

        private async void HandleGeoViewTapped(Graphic identifiedGraphic)
        {
            Debug.WriteLine("Tapped");
            if (_pointGraphicToGpsPointMap.ContainsKey(identifiedGraphic))
            {
                Debug.WriteLine("Point identified");
                // Get the corresponding GpsPoint from the dictionary
                GpsPoint gpsPoint = _pointGraphicToGpsPointMap[identifiedGraphic];
                UpdateTracker(gpsPoint.Name);
                WeakReferenceMessenger.Default.Send(new SelectionChangedMessage(gpsPoint.Name));
            }
        }
    }
}
