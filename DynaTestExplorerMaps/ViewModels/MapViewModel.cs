using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
using DynaTestExplorerMaps.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
        private List<GpsPoint> _points;
        private int _selectionId;

        public MapViewModel()
        {
            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();
            _mapService = App.AppHost.Services.GetRequiredService<IMapService>();

            _map = _mapService.CreateMap();

            GraphicsData graphicsData = _mapService.CreateGraphics(_dataAccessLayer, _selectionId);
            _graphicsOverlays = graphicsData.Overlays;
            _points = graphicsData.Points;

            _bounds = _mapService.CreateBounds();

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
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
            _mapService.UpdateTracker(newSelectionId);
            _selectionId = newSelectionId;
        }

        public void HandleMapTapped(int id)
        {
            GpsPoint gpsPoint;
            try
            {
                gpsPoint = _points.Single(point => point.Id == id);
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
