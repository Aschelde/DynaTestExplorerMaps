
namespace DynaTestExplorerMaps.Interfaces
{
    public interface IOptionsViewModel
    {
        int MaxMeasurementIntervalDistance { get; }
        int MinMeasurementIntervalDistance { get; }
        void HandleMeasurementIntervalChanged(int intervalDistance);
    }
}
