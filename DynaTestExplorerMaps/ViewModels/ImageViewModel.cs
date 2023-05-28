using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
using DynaTestExplorerMaps.EventHandling;
using DynaTestExplorerMaps.Models;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace DynaTestExplorerMaps.ViewModels
{
    public class ImageViewModel: BaseViewModel, IImageViewModel
    {
        private readonly IDataAccessLayer _dataAccessLayer;
        private int _selectionId;

        public ImageViewModel()
        {
            _dataAccessLayer = App.AppHost.Services.GetRequiredService<IDataAccessLayer>();

            _selectionId = 0;

            WeakReferenceMessenger.Default.Register<SelectionChangedMessage>(this, (r, m) =>
            {
                Debug.WriteLine("ImageViewModel: SelectionChangedMessage received");
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

        public void HandleImageControlScrolled(int id)
        {
            _selectionId = id;
            //send selection changed message
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

        public List<ImageItem> GetImages()
        {
            return _dataAccessLayer.GetImages();
        }
    }


}
