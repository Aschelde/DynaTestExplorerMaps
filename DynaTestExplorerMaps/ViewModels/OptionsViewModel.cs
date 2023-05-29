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
    public class OptionsViewModel : BaseViewModel, IOptionsViewModel
    {
        private readonly IDataAccessLayer _dataAccessLayer;
        private int _currentMeasurementInterval;
        private string _currentMeasurementType;
        private int _maxMeasurementIntervalDistance;
        private int _minMeasurementIntervalDistance;

        public OptionsViewModel(IDataAccessLayer dataAccessLayer)
        {
            _dataAccessLayer = dataAccessLayer;
            getMaxMinMeasurementInterval();
            _currentMeasurementInterval = 10;
            _currentMeasurementType = "IRI";
        }

        public int MaxMeasurementIntervalDistance
        {
            get { return _maxMeasurementIntervalDistance; }
        }

        public int MinMeasurementIntervalDistance
        {
            get { return _minMeasurementIntervalDistance;}
        }

        public void HandleMeasurementIntervalChanged(int intervalDistance)
        {
            _currentMeasurementInterval = intervalDistance;
            WeakReferenceMessenger.Default.Send(new MeasurementIntervalChangedMessage(intervalDistance));
        }

        public void HandleMeasurementTypeChanged(string type)
        {
            _currentMeasurementType = type;
            WeakReferenceMessenger.Default.Send(new MeasurementTypeChangedMessage(type));
        }

        private void getMaxMinMeasurementInterval ()
        {
            Tuple<int, int> maxMinMeasurementInterval = _dataAccessLayer.GetMaxMinMeasurementInterval();
            _minMeasurementIntervalDistance= maxMinMeasurementInterval.Item1;
            _maxMeasurementIntervalDistance = maxMinMeasurementInterval.Item2;
        }
    }
}
