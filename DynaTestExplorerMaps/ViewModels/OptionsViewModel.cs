using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
using System;

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
            GetMaxMinMeasurementInterval();
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

        private void GetMaxMinMeasurementInterval()
        {
            Tuple<int, int> maxMinMeasurementInterval = _dataAccessLayer.GetMaxMinMeasurementInterval();
            _minMeasurementIntervalDistance= maxMinMeasurementInterval.Item1;
            _maxMeasurementIntervalDistance = maxMinMeasurementInterval.Item2;
        }
    }
}
