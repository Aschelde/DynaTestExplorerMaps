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
using DynaTestExplorerMaps.model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DynaTestExplorerMaps.ViewModels
{
    public class DataViewModel: BaseViewModel
    {
        private string _selectionId;
        private Dictionary<GpsPoint, IriItem> _iriData;

        public ICommand DataValueSelectedCommand { get; set; }

        public DataViewModel()
        {
            _selectionId = "000000";
            
            _iriData = new List<IriItem>();

            DataValueSelectedCommand = new RelayCommand<string>(HandleDataValueSelected);

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

        public string SelectionId
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

        private void HandleDataValueSelected(string id)
        {
            UpdateSelection(id);
            WeakReferenceMessenger.Default.Send(new SelectionChangedMessage(_selectionId));
        }

        private void UpdateSelection(string id)
        {
            if (id == _selectionId)
            {
                return;
            }

            SelectionId = id;
        }

        public List<IriItem> GetIriData()
        {
            if (_iriData == null)
            {
                _iriData = new List<IriItem>();
                IriDataLoader iriDataLoader = new IriDataLoader();
                iriDataLoader.setPath("C:\\Users\\Asger\\Bachelor\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej.RSP");
                _iriData = iriDataLoader.GetIriItems();
            }

            return _iriData;
        }

    }
}
