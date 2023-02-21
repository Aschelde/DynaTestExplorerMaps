using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Devices.Geolocation;

namespace DynaTestExplorerMaps.model
{
    class GPSPointLoader
    {
        private string docPath = "";
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
                XmlNodeList? nodes = xmlDoc.SelectNodes("//rtept");
                if (nodes == null)
                {
                    throw new Exception("No nodes found in XML document.");
                }


                List<GpsPoint> points = new List<GpsPoint>();
                foreach (XmlNode node in nodes)
                {
                    GpsPoint point = new GpsPoint
                    {
                        Latitude = double.Parse(node.Attributes?["lat"]?.Value ?? throw new InvalidOperationException("Latitude attribute not found.")),
                        Longitude = double.Parse(node.Attributes["lon"]?.Value ?? throw new InvalidOperationException("Longitude attribute not found.")),
                        Elevation = double.Parse(node.SelectSingleNode("ele")?.InnerText ?? throw new InvalidOperationException("Elevation node not found.")),
                        Time = DateTime.Parse(node.SelectSingleNode("time")?.InnerText ?? throw new InvalidOperationException("Time node not found.")),
                        Name = node.SelectSingleNode("name")?.InnerText ?? "",
                    };
                    points.Add(point);
                }

                return points;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading GPS points.", ex);
            }
        }
    }
}
