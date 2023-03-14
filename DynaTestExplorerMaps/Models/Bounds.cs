using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.model
{
    class Bounds
    {
        private double minLat;
        private double minLon;
        private double maxLat;
        private double maxLon;

        public Bounds(double minLat, double minLon, double maxLat, double maxLon)
        {
            this.minLat = minLat;
            this.minLon = minLon;
            this.maxLat = maxLat;
            this.maxLon = maxLon;
        }
    }

}
