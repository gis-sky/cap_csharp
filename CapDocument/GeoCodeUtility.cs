using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDocument
{
    public static class GeoCodeUtility
    {
        /// <summary>
        /// 取得Polygon
        /// </summary>
        /// <param name="geocode">geocode</param>
        /// <param name="type">type</param>
        /// <returns>Polygon字串</returns>
        public static string GetPolygon(string geocode, string type)
        {
            string path = ConfigurationManager.AppSettings["geocodePath"];
            DirectoryInfo di = new DirectoryInfo(path);
            switch (type)
            {
                case "gml":
                    var gmlDi = di.GetDirectories("gml")[0];
                    var gmlFis = gmlDi.GetFiles();
                    foreach (var fileInfo in gmlFis)
                    {
                        if (fileInfo.Name.StartsWith(geocode + "-"))
                        {
                            return new StreamReader(fileInfo.OpenRead()).ReadLine();
                        }
                    }
                    return string.Empty;
                case "kml":
                    var kmlDi = di.GetDirectories("kml")[0];
                    var kmlFis = kmlDi.GetFiles();
                    foreach (var fileInfo in kmlFis)
                    {
                        if (fileInfo.Name.StartsWith(geocode + "-"))
                        {
                            return new StreamReader(fileInfo.OpenRead()).ReadLine();
                        }
                    }
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}
