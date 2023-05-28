using DynaTestExplorerMaps.Models;
using DynaTestExplorerMaps.ViewModels;
using DynaTestExplorerMaps.Interfaces;
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
using System.Windows.Threading;

namespace DynaTestExplorerMaps.Views
{
    public partial class ImageControl : UserControl
    {
        private FrameworkElement _lastElementInCenterView;
        private ImageItem _selectedImage;
        private int _lastImageId;

        public ImageControl(IImageViewModel imageViewModel)
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
                        // Find item from imageControl.items with same Id as _selectedImage
                        var container = imageControl.ItemContainerGenerator.ContainerFromIndex(_selectedImage.Id) as FrameworkElement;
                        if (container != null)
                        {
                            // Detach the event handler before bringing the item into view
                            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;

                            // Bring the item into view if necessary
                            container.BringIntoView();

                            // Get the position of the item relative to the ScrollViewer
                            Rect bounds = container.TransformToAncestor(scrollViewer).TransformBounds(new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight));

                            // Calculate the desired scroll offset to center the item
                            double centerY = bounds.Top + bounds.Height / 2;
                            double containerHeight = scrollViewer.ActualHeight;
                            double offset = centerY - containerHeight / 2;

                            // Scroll the ScrollViewer to center the item
                            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);

                            // Reattach the event handler and ignore pending scroll events
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                            }), DispatcherPriority.ApplicationIdle);
                        }
                    
                    }
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Debug.WriteLine("Scroll changed");
            int firstVisibleIndex = -1;
            int lastVisibleIndex = -1;
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

            for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
            {
                // Get the container for the image
                if (imageControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container && IsElementInCenterView(container, scrollViewer))
                {
                    // Check if this is the same element as last time
                    if (container == _lastElementInCenterView)
                    {
                        return;
                    }

                    // Get the ImageItem instance from the item
                    ImageItem imageItem = (ImageItem)imageControl.Items[i];
                    if (imageItem != null)
                    {
                        // Update the selection in the view model
                        ImageViewModel imageViewModel = DataContext as ImageViewModel;
                        _selectedImage = imageItem;
                        imageViewModel.HandleImageControlScrolled(imageItem.Id);
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
            if (e.PropertyName == nameof(IImageViewModel.SelectionId))
            {
                IImageViewModel imageViewModel = (IImageViewModel)sender;
                SelectedImage = imageControl.Items.Cast<ImageItem>().FirstOrDefault(item => item.Id == imageViewModel.SelectionId);
            }
        }
    }
}
