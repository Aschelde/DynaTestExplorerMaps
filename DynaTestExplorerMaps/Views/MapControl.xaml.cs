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
using System.ComponentModel;
using static System.Formats.Asn1.AsnWriter;
using System.Xml.Linq;
using DynaTestExplorerMaps.Interfaces;

namespace DynaTestExplorerMaps.Views
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        private TaskCompletionSource<bool> _mapViewLoadedTaskCompletionSource = new TaskCompletionSource<bool>();

        public MapControl(IMapViewModel mapViewModel)
        {
            InitializeComponent();

            this.DataContext = mapViewModel;
            Debug.WriteLine("MapControl constructor start");

            MainMapView.Loaded += MainMapView_Loaded;
            mapViewModel.PropertyChanged += OnMapViewModelPropertyChanged;
            MainMapView.GeoViewTapped += OnGeoViewTapped;

            Debug.WriteLine("MapControl constructor done");

            if (mapViewModel.Bounds != null)
            {
                Debug.WriteLine("Bounds was not null");
                UpdateViewPointAsync(mapViewModel.Bounds as Envelope);
            }
        }

        private void OnMapViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMapViewModel.Bounds))
            {
                IMapViewModel mapViewModel = (IMapViewModel)sender;
                UpdateViewPointAsync(mapViewModel.Bounds as Envelope);
            }
        }

        private async void UpdateViewPointAsync(Envelope bounds)
        {
            double padding = 10;
            await _mapViewLoadedTaskCompletionSource.Task;
            Debug.WriteLine("Sizes: " + MainMapView.ActualWidth + ", " + bounds.Width);
            double conversionFactor = 500;
            double scale = (Math.Max(bounds.Width / MainMapView.ActualWidth, bounds.Height / MainMapView.ActualHeight) + padding) * conversionFactor;
            MainMapView.SetViewpoint(new Viewpoint(bounds.GetCenter(), scale));
        }

        private void MainMapView_Loaded(object sender, RoutedEventArgs e)
        {
            _mapViewLoadedTaskCompletionSource.SetResult(true);
        }

        private async void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            var mapViewModel = DataContext as IMapViewModel;
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MainMapView.IdentifyGraphicsOverlaysAsync(e.Position, tolerance: 2, false);

            foreach (IdentifyGraphicsOverlayResult result in results)
            {
                if (result.GraphicsOverlay.Id == "gpsPointsOverlay")
                {
                    if (result.Graphics.Count > 0)
                    {
                        Graphic identifiedGraphic = result.Graphics.First();
                        if (identifiedGraphic.Attributes.TryGetValue("Id", out object idObject) && idObject is int id)
                        {
                            mapViewModel.HandleMapTapped(id);
                        }
                    }
                }
            }
        }
    }
}
