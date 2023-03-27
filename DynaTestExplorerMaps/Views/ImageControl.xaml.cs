using DynaTestExplorerMaps.model;
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

        public ImageControl()
        {
            InitializeComponent();
            ImageViewModel imageViewModel = App.AppHost.Services.GetRequiredService<ImageViewModel>();
            imageControl.ItemsSource = imageViewModel.GetImages();

            this.DataContext = imageViewModel;

            imageViewModel.PropertyChanged += OnImageViewModelPropertyChanged;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        public ImageControl(ImageViewModel imageViewModel)
        {
            InitializeComponent();
            imageControl.ItemsSource = imageViewModel.GetImages();
            this.DataContext = imageViewModel;

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
                        Debug.WriteLine("Go this far in SelectedImage");
                        //find item from imageControl.items with same Id as _selectedImage
                        var container = imageControl.ItemContainerGenerator.ContainerFromItem(_selectedImage) as FrameworkElement;
                        if (container != null)
                        {
                            container.BringIntoView();
                        }
                    }
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Debug.WriteLine("Discovered scrollevent");
            foreach (var item in imageControl.Items)
            {
                // Get the container for the image
                if (imageControl.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container && IsElementInCenterView(container, scrollViewer))
                {
                    // Check if this is the same element as last time
                    if (container == _lastElementInCenterView)
                    {
                        return;
                    }

                    // Get the ImageItem instance from the item
                    ImageItem imageItem = (ImageItem)item;
                    if (imageItem != null)
                    {
                        // Update the selection in the view model
                        ImageViewModel imageViewModel = DataContext as ImageViewModel;
                        imageViewModel.ImageControlScrolledCommand.Execute(imageItem.Id);
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
