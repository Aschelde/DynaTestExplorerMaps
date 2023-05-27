using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.EventHandling;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
using Microsoft.Extensions.DependencyInjection;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace DynaTestExplorerMaps.ViewModels
{
    public class OptionsViewModel
    {
        private readonly IDataAccessLayer _dataAccessLayer;
        private int _currentMeasurementInterval;
        private int _maxMeasurementIntervalDistance;
        private int _minMeasurementIntervalDistance;

        public ICommand MeasurementIntervalChangedCommand { get; set; }
        public OptionsViewModel()
        {
            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();
            getMaxMinMeasurementInterval();
            _currentMeasurementInterval = 10;
            MeasurementIntervalChangedCommand = new RelayCommand<int>(HandleMeasurementIntervalChanged);
        }

        public int MaxMeasurementIntervalDistance
        {
            get { return _maxMeasurementIntervalDistance; }
        }

        public int MinMeasurementIntervalDistance
        {
            get { return _minMeasurementIntervalDistance;}
        }

        private void HandleMeasurementIntervalChanged(int intervalDistance)
        {
            _currentMeasurementInterval = intervalDistance;
            WeakReferenceMessenger.Default.Send(new MeasurementIntervalChangedMessage(intervalDistance));
        }

        private void getMaxMinMeasurementInterval ()
        {
            Tuple<int, int> maxMinMeasurementInterval = _dataAccessLayer.GetMaxMinMeasurementInterval();
            _minMeasurementIntervalDistance= maxMinMeasurementInterval.Item1;
            _maxMeasurementIntervalDistance = maxMinMeasurementInterval.Item2;
        }
    }
}
