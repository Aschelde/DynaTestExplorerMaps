using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Models
{
    public class GraphicsData
    {
        public GraphicsOverlayCollection Overlays { get; set; }
        public List<GpsPoint> Points { get; set; }
        public Dictionary<Graphic, GpsPoint> PointGraphicToGpsPointMap { get; set; }
    }
}
