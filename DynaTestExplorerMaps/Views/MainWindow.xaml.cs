using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DynaTestExplorerMaps.Views
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            this.mapContentControl.Content = App.AppHost.Services.GetRequiredService<MapControl>();
            this.imageContentControl.Content = App.AppHost.Services.GetRequiredService<ImageControl>();
            this.dataContentControl.Content = App.AppHost.Services.GetRequiredService<DataControl>();
            this.optionsContentControl.Content = App.AppHost.Services.GetRequiredService<OptionsControl>();

            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = WindowState.Maximized;
        }
    }
}
