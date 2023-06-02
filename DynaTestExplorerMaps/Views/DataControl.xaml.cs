using DynaTestExplorerMaps.Interfaces;
using OxyPlot;
using System.Windows.Controls;

namespace DynaTestExplorerMaps.Views
{
    /// <summary>
    /// Interaction logic for DataControl.xaml
    /// </summary>
    public partial class DataControl : UserControl
    {
        public DataControl(IDataViewModel dataViewModel)
        {
            this.DataContext = dataViewModel;

            InitializeComponent();

            // To access the Model property, you need to cast the ScatterPlotModel to PlotModel.
            var plotModel = (PlotModel)dataViewModel.PlotModel;
            plotModel.MouseDown += OnScatterPlotButtonDown;
        }

        private void OnScatterPlotButtonDown(object? sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton == OxyMouseButton.Left)
            {
                var dataViewModel = (IDataViewModel)this.DataContext;
                dataViewModel.HandlePlotClicked(e.Position.X, e.Position.Y);
            }
        }
    }
}
