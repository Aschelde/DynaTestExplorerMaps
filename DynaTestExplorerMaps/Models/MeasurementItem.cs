using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Models
{
    public class MeasurementItem
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public Tuple<float, float> DistanceRange { get; set; }
        public float Value { get; set; }
    }
}
