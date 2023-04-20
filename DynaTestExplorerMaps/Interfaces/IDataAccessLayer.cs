using DynaTestExplorerMaps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IDataAccessLayer
    {
        List<GpsPoint> GetGpsPoints();
        List<GpsPoint> GetGpsPointsReduced(int distancePerPoint);
        List<GpsPoint> GetInterpolatedImagePoints();
        List<ImageItem> GetImages();
        List<IriItem> GetIriItems();
        List<IriSegment> GetIriSegments();
    }
}
