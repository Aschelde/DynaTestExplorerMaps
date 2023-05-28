using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Models;
using DynaTestExplorerMaps.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
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
    /// <summary>
    /// Interaction logic for DataControl.xaml
    /// </summary>
    public partial class DataControl : UserControl
    {
        private int _selectionId;
        public DataControl(IDataViewModel dataViewModel)
        {
            this.DataContext = dataViewModel;

            InitializeComponent();

            dataViewModel.PropertyChanged += OnDataViewModelPropertyChanged;

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

        public int SelectionId
        {
            get { return _selectionId; }
            set
            {
                if (_selectionId != value)
                {
                    _selectionId = value;
                }
            }
        }

        private void OnDataViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IDataViewModel.SelectionId))
            {
                IDataViewModel dataViewModel = (IDataViewModel)sender;
                SelectionId = dataViewModel.SelectionId;
            }
        }
    }
}
