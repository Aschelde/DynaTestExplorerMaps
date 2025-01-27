﻿using System;
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
using DynaTestExplorerMaps.Interfaces;

namespace DynaTestExplorerMaps.Views
{
    /// <summary>
    /// Interaction logic for OptionsControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        public OptionsControl(IOptionsViewModel optionsViewModel)
        {
            InitializeComponent();
            this.DataContext = optionsViewModel;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.DataContext is IOptionsViewModel optionsViewModel)
            {
                optionsViewModel.HandleMeasurementIntervalChanged((int)Math.Round(e.NewValue));
                SliderValueText.Text = "" + Math.Round(e.NewValue);
            }
        }
    }
}
