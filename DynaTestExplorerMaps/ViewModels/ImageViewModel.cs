using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.Interfaces;
using DynaTestExplorerMaps.Messages;
using DynaTestExplorerMaps.EventHandling;
using DynaTestExplorerMaps.model;
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

namespace DynaTestExplorerMaps.ViewModels
{
    public class ImageViewModel: BaseViewModel
    {
        private string _selectionId;
        private List<ImageItem> _images;
        public ICommand ImageControlScrolledCommand { get; set; }

        public ImageViewModel()
        {
            _selectionId = "000000";

            ImageControlScrolledCommand = new RelayCommand<string>(HandleImageControlScrolled);

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

        private void HandleImageControlScrolled(string id)
        {
            Debug.WriteLine("ImageViewModel: SelectionChangedMessage received with new id " + id);
            UpdateSelection(id);
            //send selection changed message
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

        public List<ImageItem> GetImages()
        {
            if (_images == null)
            {
                _images = new List<ImageItem>();
                ImageLoader imageLoader = new ImageLoader();
                imageLoader.setPath("C:\\Users\\Asger\\Bachelor\\3336518-0_Pilagervej - IRI Milestones\\3336518-0_Pilagervej - IRI Milestones\\3Doverlay_images");
                _images = imageLoader.getImages();
            }

            return _images;
        }
    }


}
