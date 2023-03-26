using DynaTestExplorerMaps.ViewModels;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime.UI;
using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.Messages;
using Esri.ArcGISRuntime.Data;
using System.Diagnostics;

namespace DynaTestExplorerMaps.Views
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        public MapControl()
        {
            InitializeComponent();

            this.DataContext = App.AppHost.Services.GetRequiredService<MapViewModel>();
  
            MainMapView.Loaded += MainMapView_Loaded;
            MainMapView.GeoViewTapped += OnGeoViewTapped;
        }
        public MapControl(MapViewModel mapViewModel)
        {
            InitializeComponent();

            this.DataContext = mapViewModel;

            MainMapView.Loaded += MainMapView_Loaded;
            MainMapView.GeoViewTapped += OnGeoViewTapped;
        }

        private void MainMapView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the view point after the MainMapView control has loaded
            MapPoint mapCenterPoint = new MapPoint(11.32630045, 55.41475820, SpatialReferences.Wgs84);
            MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 10000));
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            MapViewModel mapViewModel = DataContext as MapViewModel;
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MainMapView.IdentifyGraphicsOverlaysAsync(e.Position, tolerance: 2, false);

            foreach (IdentifyGraphicsOverlayResult result in results)
            {
                if (result.GraphicsOverlay.Id == "gpsPointsOverlay")
                {
                    if (result.Graphics.Count > 0)
                    {
                        Graphic identifiedGraphic = result.Graphics.First();
                        mapViewModel.GeoViewTappedCommand.Execute(identifiedGraphic);
                    }
                }
                else if (result.GraphicsOverlay.Id == "linesOverlay")
                {
                    if (result.Graphics.Count > 0)
                    {
                        // Do nothing so far
                    }
                }
            }
        }
    }
}
