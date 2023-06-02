using DynaTestExplorerMaps.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
