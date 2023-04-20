using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Models
{
    public class IriSegment
    {
        public int Id { get; set; }
        public Tuple<float, float> DistanceRange { get; set; }
        public float? AverageIri { get; set; }
        public List<int>? Images { get; set; }
        public List<int>? Points { get; set; }
    }
}
