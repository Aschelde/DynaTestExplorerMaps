using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using Windows.Devices.Geolocation;
using System.IO;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Globalization;

namespace DynaTestExplorerMaps.model
{
    class GPSPointLoader
    {
        private string docPath = "";

        public void setPath(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException("The specified file does not exist.", nameof(path));
            }
            if (Path.GetExtension(path) != ".GPX")
            {
                throw new ArgumentException("The specified file is not a GPX file.", nameof(path));
            }
            this.docPath = path;
        }
        public List<GpsPoint> getGpsPoints()
        {
            if (string.IsNullOrEmpty(docPath))
            {
                throw new ArgumentException("Invalid document path.");
            }

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(docPath);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("g", "http://www.topografix.com/GPX/1/0");

                XmlNodeList? nodes = xmlDoc.SelectNodes("//g:rtept", nsmgr);
                if (nodes == null || nodes.Count <= 0)
                {
                    throw new Exception("No nodes found in XML document.");
                }

                List<GpsPoint> points = new List<GpsPoint>();
                foreach (XmlNode node in nodes)
                {
                    GpsPoint point = new GpsPoint
                    {
                        Latitude = double.Parse(node.Attributes?["lat"]?.Value ??   
                        throw new InvalidOperationException("Latitude attribute not found."), CultureInfo.InvariantCulture),
                        Longitude = double.Parse(node.Attributes["lon"]?.Value ?? 
                        throw new InvalidOperationException("Longitude attribute not found."), CultureInfo.InvariantCulture),
                        Elevation = double.Parse(node.SelectSingleNode("g:ele", nsmgr)?.InnerText ?? 
                        throw new InvalidOperationException("Elevation node not found."), CultureInfo.InvariantCulture),
                        Time = DateTime.Parse(node.SelectSingleNode("g:time", nsmgr)?.InnerText ?? 
                        throw new InvalidOperationException("Time node not found.")),
                        Name = node.SelectSingleNode("g:name", nsmgr)?.InnerText ?? "",
                    };
                    points.Add(point);
                }

                //if points is null then throw exception
                if (points.Count > 0)
                {
                    return points;
                } else
                {
                    throw new Exception("No GPS points found.");
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error loading GPS points.", ex);
            }
        }
    }
}
