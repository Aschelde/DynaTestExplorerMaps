using DynaTestExplorerMaps.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IImageViewModel
    {
        event PropertyChangedEventHandler PropertyChanged;

        int SelectionId { get; set; }

        void HandleImageControlScrolled(int id);

        List<ImageItem> GetImages();
    }
}
