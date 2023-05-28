using DynaTestExplorerMaps.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
