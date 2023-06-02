using System;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using DynaTestExplorerMaps.Models;
using DynaTestExplorerMaps.Messages;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DynaTestExplorerMaps.Interfaces;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace DynaTestExplorerMaps.ViewModels
{
    public class DataViewModel: BaseViewModel, IDataViewModel
    {
        private int _selectionId;
        private bool _isInitialized;
        private List<MeasurementSegment> _measurementSegments;
        private IDataAccessLayer _dataAccessLayer;

        private object _scatterPlotModel;
        private Legend _legend;

        private ScatterSeries _pointerSeries;
        private ScatterSeries _dataSeries;
        private LineSeries _lineSeries;


        public DataViewModel(IDataAccessLayer dataAccessLayer)
        {
            _selectionId = 0;
            _isInitialized = false;

            _dataAccessLayer = dataAccessLayer;
            _measurementSegments = _dataAccessLayer.GetMeasurementSegments();

            UpdateScatterPlotModel();

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
                SelectionId = m.Value;
            });

            _dataAccessLayer.PropertyChanged += DataAccessLayer_PropertyChanged;
        }

        private void DataAccessLayer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("DataAccesLayer PropertyChanged");
            _measurementSegments = _dataAccessLayer.GetMeasurementSegments();
            UpdateScatterPlotModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int SelectionId
        {
            get { return _selectionId; }
            set
            {
                if (_selectionId != value)
                {
                    _selectionId = value;
                    UpdateTracker();
                }
            }
        }

        public object PlotModel
        {
            get { return _scatterPlotModel; }
            set
            {
                if (_scatterPlotModel != value)
                {
                    _scatterPlotModel = value;
                    OnPropertyChanged(nameof(PlotModel));
                }
            }
        }

        private void UpdateScatterPlotModel()
        {
            if (!_isInitialized)
            {
                _legend = new Legend();

                // Set the Legend's position
                _legend.LegendPosition = LegendPosition.TopLeft;

                var xAxis = new LinearAxis { Position = AxisPosition.Bottom, Title = "Distance", Key = "CategoryAxis" };
                var yAxis = new LinearAxis { Position = AxisPosition.Left, Title = "Mean " + _measurementSegments[0].Type, Key = "ValueAxis" };

                xAxis.Minimum = _measurementSegments.Min(s => s.DistanceRange.Item1) - 20;
                xAxis.Maximum = _measurementSegments.Max(s => s.DistanceRange.Item1) + 20;
                yAxis.Minimum = _measurementSegments.Min(s => (double)s.MeanValue) - 0.5;
                yAxis.Maximum = _measurementSegments.Max(s => (double)s.MeanValue) + 1.0;

                PlotModel model;
                model = new PlotModel { Title = _measurementSegments[0].Type + " vs. Distance" };
                model.Axes.Add(xAxis);
                model.Axes.Add(yAxis);

                _dataSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 6,
                    MarkerFill = OxyColors.LightBlue,
                    MarkerStroke = OxyColors.Blue,
                    MarkerStrokeThickness = 1,
                    ItemsSource = _measurementSegments,
                    DataFieldX = "DistanceRange.Item1",
                    DataFieldY = "MeanValue",
                    XAxisKey = "CategoryAxis",
                    YAxisKey = "ValueAxis",
                    Title = _measurementSegments[0].Type
                };

                _lineSeries = new LineSeries
                {
                    StrokeThickness = 3,
                    InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                    Color = OxyColors.DarkBlue,
                    ItemsSource = _measurementSegments,
                    DataFieldX = "DistanceRange.Item1",
                    DataFieldY = "MeanValue",
                    XAxisKey = "CategoryAxis",
                    YAxisKey = "ValueAxis"
                };

                var markerSegment = new List<MeasurementSegment>();
                markerSegment.Add(_measurementSegments[0]);


                _pointerSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 7,
                    MarkerFill = OxyColors.Red,
                    MarkerStroke = OxyColors.DarkRed,
                    MarkerStrokeThickness = 1,
                    ItemsSource = markerSegment,
                    DataFieldX = "DistanceRange.Item1",
                    DataFieldY = "MeanValue",
                    XAxisKey = "CategoryAxis",
                    YAxisKey = "ValueAxis",
                    Title = "Marker"
                };

                model.Legends.Add(_legend);

                model.Series.Add(_lineSeries);
                model.Series.Add(_dataSeries);
                model.Series.Add(_pointerSeries);

                PlotModel = model;

                _isInitialized = true;

            } else
            {
                _dataSeries.ItemsSource = _measurementSegments;
                _lineSeries.ItemsSource = _measurementSegments;

                UpdateTracker();
            }
        }

        public void HandlePlotClicked(double posX, double posY)
        {
            var scatterPlotModel = PlotModel as PlotModel;
            // Transform the mouse position to data coordinates
            var x = scatterPlotModel.Axes[0].InverseTransform(posX);
            var y = scatterPlotModel.Axes[1].InverseTransform(posY);

            var maxX = scatterPlotModel.Axes[0].Maximum;
            var maxY = scatterPlotModel.Axes[1].Maximum;

            // Calculate the distance between the clicked point and each point in the list
            var distances = _measurementSegments.Select(segment => new { Segment = segment, Distance = Math.Sqrt(Math.Pow(segment.DistanceRange.Item1 - x, 2) + Math.Pow(segment.MeanValue.Value - y, 2)) });

            // Find the segment with the smallest distance
            var closestSegment = distances.OrderBy(d => d.Distance).FirstOrDefault()?.Segment;

            if (closestSegment != null)
            {
                if ((closestSegment.DistanceRange.Item1 < x + (1.0 / 100.0 * maxX)) 
                    && (closestSegment.DistanceRange.Item1 > x - (1.0 / 100.0 * maxX)) 
                    && (closestSegment.MeanValue < y + (1.0 / 50.0 * maxY))
                    && (closestSegment.MeanValue > y - (1.0 / 50.0 * maxY))) 
                {
                    HandleDataValueSelected(closestSegment.Id);
                }
            }
        }


        private void HandleDataValueSelected(int segmentId)
        {
            if (!(_measurementSegments[segmentId].Images.Contains(_selectionId)))
            {
                SelectionId = _measurementSegments[segmentId].Images[0];
                WeakReferenceMessenger.Default.Send(new SelectionChangedMessage(_measurementSegments[segmentId].Images[0]));
            }
        }

        private void UpdateTracker()
        {
            var markerSegment = new List<MeasurementSegment>();
            markerSegment.Add(_measurementSegments.Where(s => s.Images.Contains(_selectionId)).FirstOrDefault());
            
            // Modify the ItemsSource property of the existing pointerSeries
            _pointerSeries.ItemsSource = markerSegment;

            var scatterPlotModel = PlotModel as PlotModel;
            scatterPlotModel.InvalidatePlot(true);
        }
    }
}
