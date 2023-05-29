using DynaTestExplorerMaps.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IDataAccessLayer
    {
        event PropertyChangedEventHandler PropertyChanged;
        List<GpsPoint> GetGpsPoints();
        List<GpsPoint> GetInterpolatedImagePoints();
        List<ImageItem> GetImages();
        List<MeasurementItem> GetMeasurementItems();
        List<MeasurementSegment> GetMeasurementSegments();
        Tuple<int, int> GetMaxMinMeasurementInterval();
    }
}
