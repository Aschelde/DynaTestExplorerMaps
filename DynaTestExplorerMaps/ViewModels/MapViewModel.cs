using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.EventHandling;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
using DynaTestExplorerMaps.Models;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IDataAccessLayer _dataAccessLayer;
        private Map _map;
        private GraphicsOverlayCollection _graphicsOverlays;
        private GraphicsOverlay _gpsPointsGraphicsOverlay;
        private GraphicsOverlay _linesGraphicsOverlay;
        private Envelope _bounds;
        private List<GpsPoint> points;
        private Dictionary<Graphic, GpsPoint> _pointGraphicToGpsPointMap;
        private int _selectionId;

        public ICommand GeoViewTappedCommand { get; set; }

        public MapViewModel()
        {
            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();

            SetupMap();

            _selectionId = 0;

            CreateGraphics();

            CreateBounds();

            GeoViewTappedCommand = new RelayCommand<Graphic>(HandleGeoViewTapped);

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
                Debug.WriteLine("MapViewModel: SelectionChangedMessage received");
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

        private void CreateBounds()
        {
            // Get the extent of your graphics overlay
            Envelope graphicsExtent = _gpsPointsGraphicsOverlay.Graphics.Select(graphic => graphic.Geometry.Extent).CombineExtents();

            Debug.WriteLine(graphicsExtent.XMin + ", " + graphicsExtent.YMin + ", " + graphicsExtent.XMax + ", " + graphicsExtent.YMax);
            SpatialReference projectedSR = SpatialReferences.WebMercator;
            Bounds = GeometryEngine.Project(graphicsExtent, projectedSR) as Envelope;
        }

        public Envelope Bounds
        {
            get { return _bounds; }
            set
            {
                if (_bounds != value)
                {
                    _bounds = value;
                    OnPropertyChanged(nameof(Bounds));
                }
            }
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

            points = _dataAccessLayer.GetInterpolatedImagePoints();

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

            UpdateTracker(_selectionId);
        }

        public void UpdateTracker(int Id)
        {
            // Look for the existing graphic for the selected ID in the _mapView.GraphicsOverlays.
            Graphic selectedGraphic = _gpsPointsGraphicsOverlay.Graphics.FirstOrDefault(g => _pointGraphicToGpsPointMap[g].Id == _selectionId);

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

            GpsPoint? point = points.Find(GpsPoint => GpsPoint.Id == Id);

            // Find the existing graphic for the new GPS point for the selected ID in the _gpsPointsGraphicsOverlay.
            Graphic newGraphic = _gpsPointsGraphicsOverlay.Graphics.FirstOrDefault(g => _pointGraphicToGpsPointMap[g] == point);

            if (newGraphic != null && _pointGraphicToGpsPointMap[selectedGraphic].Id == _selectionId)
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

        public void UpdateSelection(int newSelectionId) 
        {
            if (_selectionId == newSelectionId)
            {
                return;
            }
            Debug.WriteLine("ImageViewModel: SelectionChangedMessage received with new id " + newSelectionId);
            UpdateTracker(newSelectionId);
            _selectionId = newSelectionId;
        }

        private async void HandleGeoViewTapped(Graphic identifiedGraphic)
        {
            if (_pointGraphicToGpsPointMap.ContainsKey(identifiedGraphic))
            {
                // Get the corresponding GpsPoint from the dictionary
                GpsPoint gpsPoint = _pointGraphicToGpsPointMap[identifiedGraphic];
                UpdateTracker(gpsPoint.Id);
                WeakReferenceMessenger.Default.Send(new SelectionChangedMessage(gpsPoint.Id));
            }
        }
    }
}
