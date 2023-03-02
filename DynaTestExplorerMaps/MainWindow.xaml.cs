using DynaTestExplorerMaps.model;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace DynaTestExplorerMaps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ImageLoader imageLoader = new ImageLoader();
            MapViewModel mapViewModel = new MapViewModel();
            DataContext = mapViewModel;

            this.WindowStyle = WindowStyle.SingleBorderWindow;

            // set the WindowState to Maximized to make the window full screen while keeping the title bar and window borders visible
            this.WindowState = WindowState.Maximized;

            MainMapView.Loaded += MainMapView_Loaded;

            imageLoader.setPath("C:\\Users\\Asger\\Bachelor\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej - IRI Milestones\\3Doverlay_images");
            imageControl.ItemsSource = imageLoader.getImages();

            scrollViewer.ScrollChanged += scrollViewer_ScrollChanged;
        }

        private void MainMapView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the view point after the MainMapView control has loaded
            MapPoint mapCenterPoint = new MapPoint(11.32630045, 55.41475820, SpatialReferences.Wgs84);
            MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 10000));
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            MapViewModel mapViewModel = (MapViewModel)DataContext;
            if (mapViewModel == null)
            { 
                return;
            }

            foreach (var item in imageControl.Items)
            {
                var container = imageControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null && IsUserVisible(container, scrollViewer))
                {
                    // Get the ImageItem instance from the item
                    ImageItem imageItem = (ImageItem)item;
                    if (imageItem != null)
                    {
                        // Update the MapPoint in the MapViewModel
                        mapViewModel.UpdateTracker(imageItem.Id);
                    }
                    break;
                }
            }
        }

        //https://stackoverflow.com/questions/1517743/in-wpf-how-can-i-determine-whether-a-control-is-visible-to-the-user
        private bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            double elementYCenter = bounds.Top + (bounds.Height / 2);
            double containerYCenter = container.ActualHeight / 2;
            double verticalOffset = containerYCenter - elementYCenter;
            return Math.Abs(verticalOffset) <= (container.ActualHeight / 2);
        }
    }
}
