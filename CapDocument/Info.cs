using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CapDocument
{
    public class Info
    {
        /// <summary>
        /// 語言代碼
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string language { get; set; }
        /// <summary>
        /// 訊息種類
        /// </summary>
        public List<string> category = new List<string>();
        /// <summary>
        /// 事件主題類型描述
        /// </summary>
        public string @event { get; set; }
        /// <summary>
        /// 應變代碼
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> responseType = new List<string>();
        /// <summary>
        /// 緊急代碼
        /// </summary>
        public string urgency { get; set; }
        /// <summary>
        /// 嚴重代碼
        /// </summary>
        public string severity { get; set; }
        /// <summary>
        /// 確定代碼
        /// </summary>
        public string certainty { get; set; }
        /// <summary>
        /// 描述可能對象
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string audience { get; set; }
        /// <summary>
        /// 事件代碼
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<EventCode> eventCode = new List<EventCode>();
        /// <summary>
        /// 生效日期與時間
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string effective { get; set; }
        /// <summary>
        /// 預期影響日期與時間
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string onset { get; set; }
        /// <summary>
        /// 到期日期與時間
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string expires { get; set; }
        /// <summary>
        /// 發送者名稱
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string senderName { get; set; }
        /// <summary>
        /// 標題
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string headline { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string description { get; set; }
        /// <summary>
        /// 建議採取應變方案
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string instruction { get; set; }
        /// <summary>
        /// 網頁資訊
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string web { get; set; }
        /// <summary>
        /// 聯絡資訊
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string contact { get; set; }
        /// <summary>
        /// 參數傳遞
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Parameter> parameter = new List<Parameter>();
        /// <summary>
        /// Resource
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Resource> resource = new List<Resource>();
        /// <summary>
        /// Area
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Area> area = new List<Area>();

        public Info()
        {
            language = string.Empty;
        }

        /// <summary>
        /// 資料設定
        /// </summary>
        /// <param name="prop">Info屬性</param>
        /// <param name="value">值</param>
        public void SetValue(InfoProp prop, string value)
        {
            switch (prop)
            {
                case InfoProp.LANGUAGE:
                    language = value;
                    break;
                case InfoProp.EVENT:
                    @event = value;
                    break;
                case InfoProp.URGENCY:
                    urgency = value;
                    break;
                case InfoProp.SEVERITY:
                    severity = value;
                    break;
                case InfoProp.CERTAINTY:
                    certainty = value;
                    break;
                case InfoProp.AUDIENCE:
                    audience = value;
                    break;
                case InfoProp.EFFECTIVE:
                    effective = value;
                    break;
                case InfoProp.ONSET:
                    onset = value;
                    break;
                case InfoProp.EXPIRES:
                    expires = value;
                    break;
                case InfoProp.SENDERNAME:
                    senderName = value;
                    break;
                case InfoProp.HEADLINE:
                    headline = value;
                    break;
                case InfoProp.DESCRIPTION:
                    description = value;
                    break;
                case InfoProp.INSTRUCTION:
                    instruction = value;
                    break;
                case InfoProp.WEB:
                    web = value;
                    break;
                case InfoProp.CONTACT:
                    contact = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Info屬性
    /// </summary>
    public enum InfoProp
    {
        LANGUAGE,
        CATEGORY,
        EVENT,
        RESPONSETYPE,
        URGENCY,
        SEVERITY,
        CERTAINTY,
        AUDIENCE,
        EVENTCODE,
        EFFECTIVE,
        ONSET,
        EXPIRES,
        SENDERNAME,
        HEADLINE,
        DESCRIPTION,
        INSTRUCTION,
        WEB,
        CONTACT,
        PARAMETER,
        RESOURCE,
        AREA
    };
}
