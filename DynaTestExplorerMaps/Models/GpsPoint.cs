using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.model
{
    class GpsPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
        public DateTime Time { get; set; }
        public string? Name { get; set; }
    }
}
