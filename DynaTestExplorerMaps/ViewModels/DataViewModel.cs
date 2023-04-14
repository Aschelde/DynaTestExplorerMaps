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

namespace DynaTestExplorerMaps.ViewModels
{
    public class DataViewModel: BaseViewModel
    {
        private int _selectionId;
        private Dictionary<GpsPoint, IriItem> _iriData;
        private IDataAccessLayer _dataAccessLayer;

        public ICommand DataValueSelectedCommand { get; set; }

        public DataViewModel()
        {
            _selectionId = 0;
            
            _iriData = new Dictionary<GpsPoint, IriItem>();

            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();

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

        public Dictionary<GpsPoint, IriItem> GetIriData()
        {
            var iriItems = _dataAccessLayer.GetIriItems();
            var gpsPoints = _dataAccessLayer.GetGpsPoints();

            for (int i = 0; i < iriItems.Count; i++)
            {
                _iriData.Add(gpsPoints[i], iriItems[i]);
            }
            return _iriData;
        }

    }
}
