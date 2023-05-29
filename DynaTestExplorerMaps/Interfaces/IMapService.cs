using DynaTestExplorerMaps.Models;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
