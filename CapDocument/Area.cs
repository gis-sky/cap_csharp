using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CapDocument
{
    public class Area
    {
        /// <summary>
        /// 區域描述
        /// </summary>
        public string areaDesc;
        /// <summary>
        /// 多邊形各點的坐標
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> polygon = new List<string>();
        /// <summary>
        /// 中心點坐標及半徑
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> circle = new List<string>();
        /// <summary>
        /// 區域代碼
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Geocode> geocode = new List<Geocode>();
        /// <summary>
        /// 高度
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int altitude;
        /// <summary>
        /// 區域的最高高度值
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ceiling;

        public Area(string areaDesc)
        {
            this.areaDesc = areaDesc;
        }
    }
}
