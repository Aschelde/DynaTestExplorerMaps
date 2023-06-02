using System.Windows.Media.Imaging;

namespace DynaTestExplorerMaps.Interfaces
{
    public interface IImageLoader
    {
        BitmapImage GetImage(string path);
    }
}
