using System;
using System.ComponentModel;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IMapViewModel
    {
        Object Map { get; set; }
        Object GraphicsOverlays { get; set; }
        Object Bounds { get; set; }
        event PropertyChangedEventHandler PropertyChanged;

        void HandleMapTapped(int id);
    }
}
