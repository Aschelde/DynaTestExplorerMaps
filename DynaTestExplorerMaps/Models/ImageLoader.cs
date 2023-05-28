using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Diagnostics;
using DynaTestExplorerMaps.Interfaces;

namespace DynaTestExplorerMaps.Models
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
