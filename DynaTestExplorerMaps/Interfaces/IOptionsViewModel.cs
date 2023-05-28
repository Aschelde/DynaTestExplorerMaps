using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IOptionsViewModel
    {
        int MaxMeasurementIntervalDistance { get; }
        int MinMeasurementIntervalDistance { get; }
        void HandleMeasurementIntervalChanged(int intervalDistance);
    }
}
