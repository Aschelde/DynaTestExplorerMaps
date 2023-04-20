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
        public DataControl(DataViewModel dataViewModel)
        {
            this.DataContext = dataViewModel;

            InitializeComponent();

            dataViewModel.PropertyChanged += OnDataViewModelPropertyChanged;
            ScatterPlotView.MouseDown += OnScatterPlotButtonDown;
        }

        private void LineSeries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedSegment = e.AddedItems[0] as IriSegment;
                // Do something with the selected segment, such as setting a highlight color
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

        // Create OnScatterPlotButtonDown here
        private void OnScatterPlotButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }


        private void OnDataViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataViewModel.SelectionId))
            {
                DataViewModel dataViewModel = (DataViewModel)sender;
                SelectionId = dataViewModel.SelectionId;
            }
        }
    }
}
