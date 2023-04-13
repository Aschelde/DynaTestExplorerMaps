using DynaTestExplorerMaps.Models;
using DynaTestExplorerMaps.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace DynaTestExplorerMaps.Views
{
    public partial class ImageControl : UserControl
    {
        private FrameworkElement _lastElementInCenterView;
        private ImageItem _selectedImage;
        private int _lastImageId;

        public ImageControl(ImageViewModel imageViewModel)
        {
            InitializeComponent();
            var images = imageViewModel.GetImages();
            imageControl.ItemsSource = images;
            this.DataContext = imageViewModel;

            _lastImageId = images[^1].Id;

            imageViewModel.PropertyChanged += OnImageViewModelPropertyChanged;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        public ImageItem SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                if (_selectedImage != value)
                {
                    _selectedImage = value;
                    if (_selectedImage != null)
                    {
                        Debug.WriteLine("Got this far in SelectedImage");
                        //find item from imageControl.items with same Id as _selectedImage

                        var container = imageControl.ItemContainerGenerator.ContainerFromIndex(_selectedImage.Id) as FrameworkElement;
                        if (container != null)
                        {
                            Debug.WriteLine("container was not null");
                            container.BringIntoView();
                        }
                    }
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            int firstVisibleIndex = -1;
            int lastVisibleIndex = -1;
            Debug.WriteLine("Scroller changed");
            // Find the index of the first and last visible items
            for (int i = 0; i < imageControl.Items.Count; i++)
            {
                if (imageControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container && IsElementVisible(container))
                {
                    if (firstVisibleIndex == -1)
                    {
                        firstVisibleIndex = i;
                    }
                    lastVisibleIndex = i;
                }
            }

            Debug.WriteLine("Found indexes: " + firstVisibleIndex + " " + lastVisibleIndex);
            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                // Get the container for the image
                if (imageControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container && IsElementInCenterView(container, scrollViewer))
                {
                    Debug.WriteLine("Found element in center");
                    // Check if this is the same element as last time
                    if (container == _lastElementInCenterView)
                    {
                        return;
                    }

                    // Get the ImageItem instance from the item
                    ImageItem imageItem = (ImageItem)imageControl.Items[i];
                    if (imageItem != null)
                    {
                        Debug.WriteLine("Found imageItem: " + imageItem.Id);
                        // Update the selection in the view model
                        ImageViewModel imageViewModel = DataContext as ImageViewModel;
                        _selectedImage = imageItem;
                        imageViewModel.ImageControlScrolledCommand.Execute(imageItem.Id);
                    }
                    break;
                }
            }
        }

        private bool IsElementInCenterView(FrameworkElement element, FrameworkElement container)
        {
            // Create Rect to represent the element's bounds in the container's coordinate space
            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));

            // Calculate the distance between the top of the element and the vertical center of the container
            double elementTop = bounds.Top;
            double elementBottom = bounds.Bottom;
            double containerYCenter = container.ActualHeight / 2;

            if ((elementTop < containerYCenter) && (elementBottom > containerYCenter))
            {
                return true;
            }
            return false;
        }

        private bool IsElementVisible(FrameworkElement element)
        {
            // Get the position of the element relative to the ScrollViewer
            var elementRect = element.TransformToAncestor(scrollViewer).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));

            // Check if the element is within the bounds of the ScrollViewer's viewport
            return elementRect.Bottom > 0 && elementRect.Top < scrollViewer.ViewportHeight;
        }

        private void OnImageViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("ImageControl: Recieved PropertyChangedEvent from view model");
            if (e.PropertyName == nameof(ImageViewModel.SelectionId))
            {
                Debug.WriteLine("PropertyChangedEvent args: " + e.PropertyName);
                ImageViewModel imageViewModel = (ImageViewModel)sender;
                SelectedImage = imageViewModel.GetImages().FirstOrDefault(image => image.Id == imageViewModel.SelectionId);
                Debug.WriteLine("Found matching Image: " + SelectedImage.Id);
            }
        }
    }
}
