using System.ComponentModel;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IDataViewModel : INotifyPropertyChanged
    {
        object PlotModel { get; set; }
        void HandlePlotClicked(double posX, double posY);
        event PropertyChangedEventHandler PropertyChanged;
    }
}
