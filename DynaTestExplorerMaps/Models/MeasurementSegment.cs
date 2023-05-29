using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Models
{
    public class MeasurementSegment
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public Tuple<float, float> DistanceRange { get; set; }
        public float? MeanValue { get; set; }
        public List<int>? Images { get; set; }
        public List<int>? Points { get; set; }
    }
}
