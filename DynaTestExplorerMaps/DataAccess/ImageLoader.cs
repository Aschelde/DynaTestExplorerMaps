using System;
using System.Windows.Media.Imaging;
using DynaTestExplorerMaps.Interfaces;

namespace DynaTestExplorerMaps.DataAccess
{
    public class ImageLoader : IImageLoader
    {
        public BitmapImage GetImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid file path.");
            }

            try
            {
                // Load the bitmap image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;

            }
            catch (Exception ex)
            {
                throw new Exception("Error reading image file.", ex);
            }
        }
    }
}
