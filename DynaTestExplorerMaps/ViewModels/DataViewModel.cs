using System;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using DynaTestExplorerMaps.Models;
using DynaTestExplorerMaps.Messages;
using DynaTestExplorerMaps.EventHandling;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DynaTestExplorerMaps.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using OxyPlot.Legends;
using OxyPlot.Annotations;
using OxyPlot.Utilities;
using Windows.Security.Authentication.Web.Core;
using Esri.ArcGISRuntime.Geometry;

namespace DynaTestExplorerMaps.ViewModels
{
    public class DataViewModel: BaseViewModel, IDataViewModel
    {
        private int _selectionId;
        private bool _isInitialized;
        private List<IriSegment> _iriSegments;
        private IDataAccessLayer _dataAccessLayer;

        private object _scatterPlotModel;
        private Legend _legend;

        private ScatterSeries pointerSeries;
        private ScatterSeries dataSeries;
        private LineSeries lineSeries;


        public DataViewModel()
        {
            _selectionId = 0;
            _isInitialized = false;

            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();
            _iriSegments = _dataAccessLayer.GetIriSegments();

            UpdateScatterPlotModel();

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
                SelectionId = m.Value;
            });

            _dataAccessLayer.PropertyChanged += _dataAccessLayer_PropertyChanged;
        }

        private void _dataAccessLayer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("DataAccesLayer PropertyChanged");
            _iriSegments = _dataAccessLayer.GetIriSegments();
            UpdateScatterPlotModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int SelectionId
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
                var yAxis = new LinearAxis { Position = AxisPosition.Left, Title = "Average IRI", Key = "ValueAxis" };

                xAxis.Minimum = _iriSegments.Min(s => s.DistanceRange.Item1) - 20;
                xAxis.Maximum = _iriSegments.Max(s => s.DistanceRange.Item1) + 20;
                yAxis.Minimum = _iriSegments.Min(s => (double)s.AverageIri) - 0.5;
                yAxis.Maximum = _iriSegments.Max(s => (double)s.AverageIri) + 1.0;

                PlotModel model;
                model = new PlotModel { Title = "IRI vs. Distance" };
                model.Axes.Add(xAxis);
                model.Axes.Add(yAxis);

                dataSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 6,
                    MarkerFill = OxyColors.LightBlue,
                    MarkerStroke = OxyColors.Blue,
                    MarkerStrokeThickness = 1,
                    ItemsSource = _iriSegments,
                    DataFieldX = "DistanceRange.Item1",
                    DataFieldY = "AverageIri",
                    XAxisKey = "CategoryAxis",
                    YAxisKey = "ValueAxis",
                    Title = "IRI"
                };

                lineSeries = new LineSeries
                {
                    StrokeThickness = 3,
                    InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                    Color = OxyColors.DarkBlue,
                    ItemsSource = _iriSegments,
                    DataFieldX = "DistanceRange.Item1",
                    DataFieldY = "AverageIri",
                    XAxisKey = "CategoryAxis",
                    YAxisKey = "ValueAxis"
                };

                var markerSegment = new List<IriSegment>();
                markerSegment.Add(_iriSegments[0]);


                pointerSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 7,
                    MarkerFill = OxyColors.Red,
                    MarkerStroke = OxyColors.DarkRed,
                    MarkerStrokeThickness = 1,
                    ItemsSource = markerSegment,
                    DataFieldX = "DistanceRange.Item1",
                    DataFieldY = "AverageIri",
                    XAxisKey = "CategoryAxis",
                    YAxisKey = "ValueAxis",
                    Title = "Marker"
                };

                model.Legends.Add(_legend);

                model.Series.Add(lineSeries);
                model.Series.Add(dataSeries);
                model.Series.Add(pointerSeries);

                PlotModel = model;

                _isInitialized = true;

            } else
            {
                dataSeries.ItemsSource = _iriSegments;
                lineSeries.ItemsSource = _iriSegments;

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
            var distances = _iriSegments.Select(segment => new { Segment = segment, Distance = Math.Sqrt(Math.Pow(segment.DistanceRange.Item1 - x, 2) + Math.Pow(segment.AverageIri.Value - y, 2)) });

            // Find the segment with the smallest distance
            var closestSegment = distances.OrderBy(d => d.Distance).FirstOrDefault()?.Segment;

            if (closestSegment != null)
            {
                if ((closestSegment.DistanceRange.Item1 < x + (1.0 / 100.0 * maxX)) 
                    && (closestSegment.DistanceRange.Item1 > x - (1.0 / 100.0 * maxX)) 
                    && (closestSegment.AverageIri < y + (1.0 / 50.0 * maxY))
                    && (closestSegment.AverageIri > y - (1.0 / 50.0 * maxY))) 
                {
                    HandleDataValueSelected(closestSegment.Id);
                }
            }
        }


        private void HandleDataValueSelected(int segmentId)
        {
            if (!(_iriSegments[segmentId].Images.Contains(_selectionId)))
            {
                SelectionId = _iriSegments[segmentId].Images[0];
                Debug.WriteLine("Selected " + _selectionId);
                WeakReferenceMessenger.Default.Send(new SelectionChangedMessage(_iriSegments[segmentId].Images[0]));
            }
        }

        private void UpdateTracker()
        {
            var markerSegment = new List<IriSegment>();
            markerSegment.Add(_iriSegments.Where(s => s.Images.Contains(_selectionId)).FirstOrDefault());
            
            // Modify the ItemsSource property of the existing pointerSeries
            pointerSeries.ItemsSource = markerSegment;

            var scatterPlotModel = PlotModel as PlotModel;
            scatterPlotModel.InvalidatePlot(true);
        }

        private List<IriSegment> GetIriSegments()
        {
            if (_iriSegments != null)
            {
                return _iriSegments;
            }

            _iriSegments = _dataAccessLayer.GetIriSegments();

            return _iriSegments;
        }   
    }
}
