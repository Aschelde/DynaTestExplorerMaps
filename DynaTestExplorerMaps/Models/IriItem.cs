﻿using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Models
{
    public class IriItem
    {
        public int Id { get; set; }
        public Tuple<float, float> DistanceRange { get; set; }
        public float Iri { get; set; }
    }
}
