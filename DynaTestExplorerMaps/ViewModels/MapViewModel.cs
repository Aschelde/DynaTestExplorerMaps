using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.EventHandling;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
using DynaTestExplorerMaps.Models;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Xps.Serialization;
using Windows.System;

namespace DynaTestExplorerMaps.ViewModels
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MapViewModel : BaseViewModel, IMapViewModel
    {
        private readonly IDataAccessLayer _dataAccessLayer;
        private readonly IMapService _mapService;
        private object _map;
        private object _graphicsOverlays;
        private object _bounds;
        private GraphicsOverlay _gpsPointsGraphicsOverlay;
        private GraphicsOverlay _linesGraphicsOverlay;
        private List<GpsPoint> points;
        private Dictionary<Graphic, GpsPoint> _pointGraphicToGpsPointMap;
        private int _selectionId;

        public MapViewModel()
        {
            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();
            _mapService = App.AppHost.Services.GetRequiredService<IMapService>();

            _map = _mapService.CreateMap();

            GraphicsData graphicsData = _mapService.CreateGraphics(_dataAccessLayer, _selectionId);
            _graphicsOverlays = graphicsData.Overlays;
            points = graphicsData.Points;
            _pointGraphicToGpsPointMap = graphicsData.PointGraphicToGpsPointMap;

            _bounds = _mapService.CreateBounds();

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
                Debug.WriteLine("MapViewModel: SelectionChangedMessage received");
                UpdateSelection(m.Value);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Map
        {
            get { return _map; }
            set
            {
                if (_map != value)
                {
                    _map = value;
                    OnPropertyChanged(nameof(Map));
                }
            }
        }

        public object GraphicsOverlays
        {
            get { return _graphicsOverlays; }
            set
            {
                if (_graphicsOverlays != value)
                {
                    _graphicsOverlays = value;
                    OnPropertyChanged(nameof(GraphicsOverlays));
                }
            }
        }

        public object Bounds
        {
            get { return _bounds; }
            set
            {
                if (_bounds != value)
                {
                    _bounds = value;
                    OnPropertyChanged(nameof(Bounds));
                }
            }
        }

        private void UpdateSelection(int newSelectionId)
        {
            if (_selectionId == newSelectionId)
            {
                return;
            }
            Debug.WriteLine("ImageViewModel: SelectionChangedMessage received with new id " + newSelectionId);
            _mapService.UpdateTracker(newSelectionId);
            _selectionId = newSelectionId;
        }

        public void HandleMapTapped(int id)
        {
            GpsPoint gpsPoint;
            try
            {
                gpsPoint = points.Single(point => point.Id == id);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            _mapService.UpdateTracker(gpsPoint.Id);
            WeakReferenceMessenger.Default.Send(new SelectionChangedMessage(gpsPoint.Id));
        }
    }
}
