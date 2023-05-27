using System;
using System.Collections.Generic;
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
using DynaTestExplorerMaps.ViewModels;

namespace DynaTestExplorerMaps.Views
{
    /// <summary>
    /// Interaction logic for OptionsControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        private OptionsViewModel _optionsViewModel;

        public OptionsControl(OptionsViewModel optionsViewModel)
        {
            InitializeComponent();
            _optionsViewModel = optionsViewModel;
            this.DataContext = _optionsViewModel;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_optionsViewModel != null)
            {
                _optionsViewModel.MeasurementIntervalChangedCommand?.Execute((int)Math.Round(e.NewValue));
                SliderValueText.Text = "" + Math.Round(e.NewValue);
            }
        }
    }
}
