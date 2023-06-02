using DynaTestExplorerMaps.Models;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IMapService
    {
        object CreateMap();
        object CreateBounds();
        GraphicsData CreateGraphics(IDataAccessLayer dataAccessLayer, int selectionId);
        void UpdateTracker(int Id);
    }
}
