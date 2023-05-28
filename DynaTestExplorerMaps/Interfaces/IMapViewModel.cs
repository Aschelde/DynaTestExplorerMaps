using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IMapViewModel
    {
        Map Map { get; set; }
        GraphicsOverlayCollection GraphicsOverlays { get; set; }
        Envelope Bounds { get; set; }
        event PropertyChangedEventHandler PropertyChanged;

        void HandleMapTapped(int id);
    }
}
