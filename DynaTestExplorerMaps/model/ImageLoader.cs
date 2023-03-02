using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Xps;

namespace DynaTestExplorerMaps.model
{
    class ImageLoader
    {
        private string path = "";

        public void setPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("The specified file does not exist.", nameof(path));
            }
            this.path = path;
            //TODO: check if path contains images
        }

        //return list of ImageItems with image and id that is at the end of image path
        public List<ImageItem> getImages()
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid document path.");
            }

            try
            {
                List<ImageItem> images = new List<ImageItem>();
                foreach (string file in Directory.GetFiles(path, "*.jpg"))
                {
                    // Extract the number from the file name
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string id = fileName.Substring(fileName.LastIndexOf("_") + 1);

                    // Load the bitmap image
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(file, UriKind.Relative);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Create an ImageItem with the ID and bitmap image
                    ImageItem item = new ImageItem { Id = id, Image = bitmap };
                    images.Add(item);
                }
                return images;

            }
            catch (Exception ex)
            {
                throw new Exception("Error reading image file.", ex);
            }
        }


    }
}
