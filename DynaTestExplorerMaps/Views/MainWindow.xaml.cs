using System.Windows;

namespace DynaTestExplorerMaps.Views
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WindowStyle = WindowStyle.SingleBorderWindow;

            // set the WindowState to Maximized to make the window full screen
            this.WindowState = WindowState.Maximized;
        }
    }
}
