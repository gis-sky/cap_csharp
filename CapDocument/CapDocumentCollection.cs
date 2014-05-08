using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDocument
{
    public class CapDocumentCollection
    {
        /// <summary>
        /// GeoRss ID
        /// </summary>
        public string id;
        /// <summary>
        /// GeoRss標題
        /// </summary>
        public string title;
        /// <summary>
        /// GeoRss更新時間
        /// </summary>
        public string updated;
        /// <summary>
        /// GeoRss名稱
        /// </summary>
        public string name;
        /// <summary>
        /// GeoRss連結
        /// </summary>
        public string link;
        /// <summary>
        /// Cap物件
        /// </summary>
        public List<CapDocument> CapDocuments = new List<CapDocument>();

        public CapDocumentCollection(string id, string title, string updated, string name, string link)
        {
            this.id = id;
            this.title = title;
            this.updated = updated;
            this.name = name;
            this.link = link;
        }

        /// <summary>
        /// 讀取Cap檔案
        /// </summary>
        /// <param name="path">Cap檔案路徑</param>
        public void LoadCapFile(string path)
        {
            CapDocument capDocument = new CapDocument();
            capDocument.Load(path);
            CapDocuments.Add(capDocument);
        }

        /// <summary>
        /// 轉出GeoRss字串
        /// </summary>
        /// <returns>GeoRss字串</returns>
        public string ToGeoRss()
        {
            StringBuilder sbGeoRss = new StringBuilder();
            StringBuilder sbCap = new StringBuilder();
            if (CapDocuments.Any())
            {
                foreach (var capDocument in CapDocuments)
                {
                    sbCap.Append(capDocument.ToGeoRssItem());
                }
            }
            sbGeoRss.Append(string.Format(CapDocumentSetting.Default.geoRssTemplate, id, title, updated, name, link, sbCap.ToString()));
            return sbGeoRss.ToString();
        }
    }
}
