using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Models
{
    public class GpsPoint
    {
        public int Id { get; set; }
        public float Distance { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
