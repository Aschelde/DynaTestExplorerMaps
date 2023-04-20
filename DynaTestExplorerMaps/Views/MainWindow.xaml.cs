using DynaTestExplorerMaps.ViewModels;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DynaTestExplorerMaps.Views
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            this.mapContentControl.Content = App.AppHost.Services.GetRequiredService<DataControl>();
            this.ImageContentControl.Content = App.AppHost.Services.GetRequiredService<ImageControl>();
            this.DataContentControl.Content = App.AppHost.Services.GetRequiredService<DataControl>();

            this.WindowStyle = WindowStyle.SingleBorderWindow;

            // set the WindowState to Maximized to make the window full screen
            this.WindowState = WindowState.Maximized;

        }
    }
}
