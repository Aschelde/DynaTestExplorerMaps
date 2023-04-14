using DynaTestExplorerMaps.Models;
using DynaTestExplorerMaps.ViewModels;
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
        Dictionary<GpsPoint, IriItem> _iriData;
        public DataControl(DataViewModel dataViewModel)
        {
            InitializeComponent();
            
            this.DataContext = dataViewModel;

            _iriData = dataViewModel.GetIriData();

            dataViewModel.PropertyChanged += OnDataViewModelPropertyChanged;
        }

        private void OnDataViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataViewModel.SelectionId))
            {
                DataViewModel dataViewModel = (DataViewModel)sender;
                _selectionId = dataViewModel.SelectionId;

                //set text to value with GpsPoint.Id == SelectionId
                valueText.Text = _iriData[_selectionId];
            }
        }
    }
}
