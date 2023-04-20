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



namespace DynaTestExplorerMaps.ViewModels
{
    public class DataViewModel: BaseViewModel
    {
        private int _distancePerIri;
        private int _selectionId;
        private List<IriSegment> _iriSegments;
        private IDataAccessLayer _dataAccessLayer;

        private PlotModel _scatterPlotModel;
        private Legend _legend;

        public ICommand DataValueSelectedCommand { get; set; }

        public DataViewModel()
        {
            _distancePerIri = 15;

            _selectionId = 0;

            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();
            _iriSegments = _dataAccessLayer.GetIriSegments();

            InitializeScatterPlotModel();

            DataValueSelectedCommand = new RelayCommand<int>(HandleDataValueSelected);

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
                UpdateSelection(m.Value);
            });
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
                    OnPropertyChanged(nameof(SelectionId));
                }
            }
        }

        public PlotModel ScatterPlotModel
        {
            get { return _scatterPlotModel; }
            set
            {
                if (_scatterPlotModel != value)
                {
                    _scatterPlotModel = value;
                    OnPropertyChanged(nameof(ScatterPlotModel));
                }
            }
        }

        private void InitializeScatterPlotModel()
        {

            _legend = new Legend();

            // Set the Legend's position
            _legend.LegendPosition = LegendPosition.TopLeft;

            var xAxis = new LinearAxis { Position = AxisPosition.Bottom, Title = "Distance", Key = "CategoryAxis" };
            var yAxis = new LinearAxis { Position = AxisPosition.Left, Title = "Average IRI", Key = "ValueAxis" };

            xAxis.Minimum = _iriSegments.Min(s => s.DistanceRange.Item1) - 20;
            xAxis.Maximum = _iriSegments.Max(s => s.DistanceRange.Item1) + 20;
            yAxis.Minimum = _iriSegments.Min(s => (double)s.AverageIri) - 0.5;
            yAxis.Maximum = _iriSegments.Max(s => (double)s.AverageIri) + 0.5;
            

            _scatterPlotModel = new PlotModel { Title = "IRI vs. Distance" };
            _scatterPlotModel.Axes.Add(xAxis);
            _scatterPlotModel.Axes.Add(yAxis);

            var series = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 7,
                MarkerFill = OxyColors.Blue,
                MarkerStroke = OxyColors.Black,
                MarkerStrokeThickness = 1,
                ItemsSource = _iriSegments,
                DataFieldX = "DistanceRange.Item1",
                DataFieldY = "AverageIri",
                XAxisKey = "CategoryAxis",
                YAxisKey = "ValueAxis",
                Title = "IRI"
            };

            var lineSeries = new LineSeries
            {
                StrokeThickness = 3,
                InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
                Color = OxyColors.Red,
                ItemsSource = _iriSegments,
                DataFieldX = "DistanceRange.Item1",
                DataFieldY = "AverageIri",
                XAxisKey = "CategoryAxis",
                YAxisKey = "ValueAxis"
            };

            //ScatterPoint point = new ScatterPoint(10, 1);
            //point.Size = 7;
            
            // Add point to the ScatterSeries
            //series.Points.Add(point);

            // Add the Legend to the PlotModel
            _scatterPlotModel.Legends.Add(_legend);

            _scatterPlotModel.Series.Add(lineSeries);
            _scatterPlotModel.Series.Add(series);

            ScatterPlotModel = _scatterPlotModel;
        }

        private void HandleDataValueSelected(int id)
        {
            UpdateSelection(id);
            WeakReferenceMessenger.Default.Send(new SelectionChangedMessage(_selectionId));
        }

        private void UpdateSelection(int id)
        {
            if (id == _selectionId)
            {
                return;
            }

            SelectionId = id;
        }

        private void HighlightSelectedBar()
        {
            
        }

        public List<IriSegment> GetIriSegments()
        {
            if (_iriSegments != null)
            {
                return _iriSegments;
            }

            _iriSegments = _dataAccessLayer.GetIriSegments();

            return _iriSegments;
        }   

        /*public Dictionary<List<int>, IriItem> GetAverageIriDataByImage()
        {
            Dictionary<int, IriItem> iriByImage = GetIriDataByImage();
            Dictionary <List<int>, IriItem > averageIri = new Dictionary<List<int>, IriItem>();

            var groupedByDistance = iriByImage.GroupBy(item => (int)Math.Floor(item.Value.Distance / _distancePerIri));

            float currentAverageIri;
            foreach (var group in groupedByDistance)
            {
                currentAverageIri = group.Average(item => item.Value.Iri);
                var imageIds = group.Select(item => item.Key).ToList();
                var iriItem = new IriItem();
                iriItem.Id = group.Key;
                iriItem.Iri = currentAverageIri;
                iriItem.Distance = group.Min(item => item.Value.Distance);
                averageIri[imageIds] = iriItem;
            }

            return averageIri;
        }

        public Dictionary<int, IriItem> GetIriDataByImage()
        {
            var iriItems = _dataAccessLayer.GetIriItems();
            var gpsPoints = _dataAccessLayer.GetInterpolatedImagePoints();

            Dictionary<int, IriItem> result = new Dictionary<int, IriItem>();

            int gpsIndex = 0;
            for (int i = 0; i < iriItems.Count; i++)
            {
                // If the IriItem is before the first GPS point, associate it with the first GPS point
                if (gpsIndex == 0 && iriItems[i].Distance < gpsPoints[0].Distance)
                {
                    result[gpsPoints[0].Id] = iriItems[i];
                    continue;
                }

                // If the IriItem is after the last GPS point, associate it with the last GPS point
                if (gpsIndex == gpsPoints.Count - 1 && iriItems[i].Distance >= gpsPoints[gpsIndex].Distance)
                {
                    result[gpsPoints[gpsIndex].Id] = iriItems[i];
                    continue;
                }

                // Find the closest GPS point for this IriItem using binary search
                int low = gpsIndex;
                int high = gpsPoints.Count - 1;
                while (low < high)
                {
                    int mid = (low + high + 1) / 2;
                    if (gpsPoints[mid].Distance <= iriItems[i].Distance)
                    {
                        low = mid;
                    }
                    else
                    {
                        high = mid - 1;
                    }
                }

                // Associate the IriItem with the GPS point that comes immediately before it
                result[gpsPoints[low].Id] = iriItems[i];

                // Update the GPS index to start the next search at the current point
                gpsIndex = low;
            }

            return result;
        }*/
    }
}
