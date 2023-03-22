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
            MainMapView.GeoViewTapped += OnGeoViewTapped;
        }
        public MapControl(MapViewModel mapViewModel)
        {
            InitializeComponent();

            this.DataContext = mapViewModel;
            MainMapView.GeoViewTapped += OnGeoViewTapped;
        }

        private void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            MapViewModel mapViewModel = DataContext as MapViewModel;
            mapViewModel.GeoViewTappedCommand.Execute(e);
        }

        /*private void MainMapView_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: use bounds to set the center point
            // Set the view point after the MainMapView control has loaded
            MapPoint mapCenterPoint = new MapPoint(11.32630045, 55.41475820, SpatialReferences.Wgs84);
            MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 10000));
        }*/
    }
}
