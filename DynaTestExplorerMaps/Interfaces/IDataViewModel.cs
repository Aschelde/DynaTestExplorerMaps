using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IDataViewModel : INotifyPropertyChanged
    {
        int SelectionId { get; set; }
        object PlotModel { get; set; }
        void HandlePlotClicked(double posX, double posY);
    }
}
