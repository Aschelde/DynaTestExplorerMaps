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

        public double MinLat { get => minLat; set => minLat = value; }
        public double MinLon { get => minLon; set => minLon = value; }
        public double MaxLat { get => maxLat; set => maxLat = value; }
        public double MaxLon { get => maxLon; set => maxLon = value; }
    }

}
