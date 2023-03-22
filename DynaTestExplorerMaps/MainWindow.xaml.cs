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

        private FrameworkElement lastElementInCenterView;
        public MainWindow()
        {
            InitializeComponent();
            ImageLoader imageLoader = new ImageLoader();
            MvvmMapViewModel mvvmMapViewModel = new MvvmMapViewModel();
            DataContext = mvvmMapViewModel;


            this.WindowStyle = WindowStyle.SingleBorderWindow;

            // set the WindowState to Maximized to make the window full screen
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
            MvvmMapViewModel mvvmMapViewModel = (MvvmMapViewModel)DataContext;
            if (mvvmMapViewModel == null)
            { 
                return;
            }

            foreach (var item in imageControl.Items)
            {
                // Get the container for the image
                var container = imageControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null && IsElementInCenterView(container, scrollViewer))
                {
                    // Check if this is the same element as last time
                    if (container == lastElementInCenterView)
                    {
                        return;
                    }

                    // Get the ImageItem instance from the item
                    ImageItem imageItem = (ImageItem)item;
                    if (imageItem != null)
                    {
                        // Update the MapPoint in the MapViewModel
                        mvvmMapViewModel.UpdateTracker(imageItem.Id);
                    }
                    break;
                }
            }
        }

        private bool IsElementInCenterView(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            // Create Rect to represent the element's bounds in the container's coordinate space
            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));

            // calculate the center of the element's bounds
            double elementYCenter = bounds.Top + (bounds.Height / 2);
            double containerYCenter = container.ActualHeight / 2;

            //check if the element is within the container's center
            double verticalOffset = containerYCenter - elementYCenter;
            return Math.Abs(verticalOffset) <= (container.ActualHeight / 2);
        }
    }
}
