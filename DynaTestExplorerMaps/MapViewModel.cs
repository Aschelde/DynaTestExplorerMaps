using DynaTestExplorerMaps.model;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps.Serialization;

namespace DynaTestExplorerMaps
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    class MapViewModel : INotifyPropertyChanged
    {

        public MapViewModel()
        {
            SetupMap();

            CreateGraphics();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Map _map;
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
            this.Map = new Map(BasemapStyle.ArcGISStreets);
        }

        private void CreateGraphics()
        {
            // Create a new graphics overlay to contain a variety of graphics.
            var malibuGraphicsOverlay = new GraphicsOverlay();

            // Add the overlay to a graphics overlay collection.
            GraphicsOverlayCollection overlays = new GraphicsOverlayCollection
            {
                malibuGraphicsOverlay
            };

            // Set the view model's "GraphicsOverlays" property (will be consumed by the map view).
            this.GraphicsOverlays = overlays;

            // Create a point geometry.

            var pilAgerVej0 = new MapPoint(11.32630045, 55.41475820, SpatialReferences.Wgs84);

            GPSPointLoader loader = new GPSPointLoader();
            loader.setPath("C:\\Users\\Asger\\Bachelor\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej.GPX");
            List<GpsPoint> points = loader.getGpsPoints();

            // Create a symbol to define how the point is displayed.
            var pointSymbol = new SimpleMarkerSymbol
            {
                Style = SimpleMarkerSymbolStyle.Circle,
                Color = System.Drawing.Color.Orange,
                Size = 10.0
            };

            // Add an outline to the symbol.
            pointSymbol.Outline = new SimpleLineSymbol
            {
                Style = SimpleLineSymbolStyle.Solid,
                Color = System.Drawing.Color.Blue,
                Width = 2.0
            };

            //create all Gps points as point graphic.
            foreach (GpsPoint point in points)
            {
                var pointGraphic = new Graphic(new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84), pointSymbol);
                malibuGraphicsOverlay.Graphics.Add(pointGraphic);
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
            malibuGraphicsOverlay.Graphics.Add(lineGraphic);
        }
    }
}
