using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Web;

namespace CapDocument
{
    public class CapDocument
    {
        public string xmlns;
        /// <summary>
        /// 警報識別碼
        /// </summary>
        public string identifier;
        /// <summary>
        /// 來源者識別碼
        /// </summary>
        public string sender;
        /// <summary>
        /// 發送日期與時間
        /// </summary>
        public string sent;
        /// <summary>
        /// 類別狀態碼
        /// </summary>
        public string status;
        /// <summary>
        /// 指令類別碼
        /// </summary>
        public string msgType;
        /// <summary>
        /// 來源簡述
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string source;
        /// <summary>
        /// 接收者範圍
        /// </summary>
        public string scope;
        /// <summary>
        /// 說明接受者條件
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string restriction;
        /// <summary>
        /// 接收者列表
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string addresses;
        /// <summary>
        /// 特殊處理代碼
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly List<string> code = new List<string>();
        /// <summary>
        /// 描述說明
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string note;
        /// <summary>
        /// 相關的識別碼
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string references;
        /// <summary>
        /// 相關資訊列表
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string incidents;
        /// <summary>
        /// Info
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly List<Info> info = new List<Info>();

        private List<CapValidateResult> capValidateResults;

        public CapDocument()
        {

        }

        public CapDocument(string filePath)
        {
            var fileStream = File.OpenRead(filePath);
            XDocument xDocument = XDocument.Load(fileStream);
            capValidateResults = CapValidator.Validate(xDocument);
            if (IsValid())
            {
                perform(xDocument);
            }
            fileStream.Close();
        }

        /// <summary>
        /// 檢查資料驗證是否通過
        /// </summary>
        /// <returns>驗證結果</returns>
        public bool IsValid()
        {
            return !capValidateResults.Any();
        }

        /// <summary>
        /// 讀取Cap檔案
        /// </summary>
        /// <param name="filePath"></param>
        public void Load(string filePath)
        {
            var fileStream = File.OpenRead(filePath);
            XDocument xDocument = XDocument.Load(fileStream);
            capValidateResults = CapValidator.Validate(xDocument);
            if (IsValid())
            {
                perform(xDocument);
            }
            fileStream.Close();
        }

        /// <summary>
        /// 驗證CapDocument物件
        /// </summary>
        /// <returns>驗證結果</returns>
        public List<CapValidateResult> Validate()
        {
            return CapValidator.Validate(this);
        }

        /// <summary>
        /// 驗證Cap檔案
        /// </summary>
        /// <returns>驗證結果</returns>
        public static List<CapValidateResult> Validate(string filePath)
        {
            var fileStream = File.OpenRead(filePath);
            XDocument xDocument = XDocument.Load(fileStream);
            return CapValidator.Validate(xDocument);
        }

        /// <summary>
        /// 將Cap物件轉為Json
        /// </summary>
        /// <returns>Json字串</returns>
        public string ToJson()
        {
            var json = JsonConvert.SerializeObject(new { alert = this });
            return json;
        }

        /// <summary>
        /// 將Cap物件轉為GeoRss
        /// </summary>
        /// <returns>GeoRss字串</returns>
        public string ToGeoRssItem()
        {
            StringBuilder sbGeoRssItem = new StringBuilder();
            string result = StringUtility.ReplaceContent(CapDocumentSetting.Default.geoRssItemTemplate, "identifier", identifier);
            result = StringUtility.ReplaceContent(result, "sent", sent);
            result = StringUtility.ReplaceContent(result, "sender", sender);
            string @event = string.Empty;
            StringBuilder sbMultiPolygon = new StringBuilder();
            if (info.Any())
            {
                result = StringUtility.ReplaceContent(result, "description", info[0].description);
                @event = info[0].@event;
                foreach (var i in info)
                {
                    if (i.area.Any())
                    {
                        foreach (var a in i.area)
                        {
                            if (a.circle.Any())
                                sbMultiPolygon.Append(string.Format(CapDocumentSetting.Default.geoRssItemPolygonTemplate, string.Join(" ", a.circle)));
                            if (a.polygon.Any())
                                sbMultiPolygon.Append(string.Format(CapDocumentSetting.Default.geoRssItemPolygonTemplate, string.Join(" ", a.polygon)));
                            if (a.geocode.Any())
                            {
                                foreach (var g in a.geocode)
                                {
                                    sbMultiPolygon.Append(string.Format(CapDocumentSetting.Default.geoRssItemGeoCodeTemplate, GeoCodeUtility.GetPolygon(g.value, "gml")));
                                }
                            }
                        }
                    }
                }
                result = StringUtility.ReplaceContent(result, "multipolygon", sbMultiPolygon.ToString());
            }
            result = StringUtility.ReplaceContent(result, "event", @event);
            sbGeoRssItem.Append(result);

            return sbGeoRssItem.ToString();
        }

        /// <summary>
        /// 將Cap物件轉為Cap
        /// </summary>
        /// <returns></returns>
        public string ToCap()
        {
            // alert
            StringBuilder sbAlert = new StringBuilder();
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "identifier", identifier));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "sender", sender));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "sent", sent));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "status", status));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "msgType", msgType));
            if (!string.IsNullOrEmpty(source))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "source", source));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "scope", scope));
            if (!string.IsNullOrEmpty(restriction))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "restriction", restriction));
            if (!string.IsNullOrEmpty(addresses))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "addresses", addresses));
            if (code.Any())
            {
                foreach (var c in code)
                {
                    sbAlert.Append(string.Format("<{0}>{1}</{0}>", "code", c));
                }
            }
            if (!string.IsNullOrEmpty(note))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "note", note));
            if (!string.IsNullOrEmpty(references))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "references", references));
            if (!string.IsNullOrEmpty(incidents))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "incidents", incidents));
            string result = StringUtility.ReplaceContent(CapDocumentSetting.Default.CapTemplate, "alert", sbAlert.ToString());

            // info
            StringBuilder sbInfo = new StringBuilder();
            if (info.Any())
            {
                foreach (var i in info)
                {
                    StringBuilder sbInfoItem = new StringBuilder();
                    if (!string.IsNullOrEmpty(i.language))
                        sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "language", i.language));
                    if (i.category.Any())
                    {
                        foreach (var c in i.category)
                        {
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "category", c));
                        }
                    }
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "event", i.@event));
                    if (i.responseType.Any())
                    {
                        foreach (var r in i.responseType)
                        {
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "responseType", r));
                        }
                    }
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "urgency", i.urgency));
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "severity", i.severity));
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "certainty", i.certainty));
                    StringUtility.SetCotent(sbInfoItem, "audience", i.audience);
                    if (i.eventCode.Any())
                    {
                        foreach (var e in i.eventCode)
                        {
                            string contentString = string.Format("<{0}>{1}</{0}>", "valueName", e.valueName) + string.Format("<{0}>{1}</{0}>", "value", e.value);
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "eventCode", contentString));
                        }
                    }
                    StringUtility.SetCotent(sbInfoItem, "effective", i.effective);
                    StringUtility.SetCotent(sbInfoItem, "onset", i.onset);
                    StringUtility.SetCotent(sbInfoItem, "expires", i.expires);
                    StringUtility.SetCotent(sbInfoItem, "senderName", i.senderName);
                    StringUtility.SetCotent(sbInfoItem, "headline", i.headline);
                    StringUtility.SetCotent(sbInfoItem, "description", i.description);
                    StringUtility.SetCotent(sbInfoItem, "instruction", i.instruction);
                    StringUtility.SetCotent(sbInfoItem, "web",  HttpUtility.HtmlEncode(i.web));
                    StringUtility.SetCotent(sbInfoItem, "contact", i.contact);
                    //StringUtility.SetCotent(sbInfoItem, "headline", i.headline);
                    if (i.parameter.Any())
                    {
                        foreach (var p in i.parameter)
                        {
                            string contentString = string.Format("<{0}>{1}</{0}>", "valueName", p.valueName) + string.Format("<{0}>{1}</{0}>", "value", p.value);
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "parameter", contentString));
                        }
                    }

                    // resource
                    if (i.resource.Any())
                    {
                        StringBuilder sbResourceItem = new StringBuilder();
                        foreach (var r in i.resource)
                        {
                            StringUtility.SetCotent(sbResourceItem, "resourceDesc", r.resourceDesc);
                            StringUtility.SetCotent(sbResourceItem, "mimeType", r.mimeType);
                            StringUtility.SetCotent(sbResourceItem, "size", r.size);
                            StringUtility.SetCotent(sbResourceItem, "uri", r.uri);
                            StringUtility.SetCotent(sbResourceItem, "derefUri", r.derefUri);
                            StringUtility.SetCotent(sbResourceItem, "digest", r.digest);
                            //sbResourceItem.Append(string.Format(CapDocumentSetting.Default.kmlResourceTemplate, sbResourceItem.ToString()));
                        }
                        sbInfoItem.Append(string.Format(CapDocumentSetting.Default.kmlResourceScopeTemplate, sbResourceItem.ToString()));
                    }

                    // area
                    if (i.area.Any())
                    {
                        StringBuilder sbAreaItem = new StringBuilder();
                        foreach (var a in i.area)
                        {
                            StringBuilder sbAreaPolygon = new StringBuilder();
                            StringUtility.SetCotent(sbAreaPolygon, "areaDesc", a.areaDesc);
                            if (a.polygon.Any())
                            {
                                foreach (var p in a.polygon)
                                {
                                    StringUtility.SetCotent(sbAreaPolygon, "polygon", p);
                                }
                            }
                            if (a.circle.Any())
                            {
                                foreach (var c in a.circle)
                                {
                                    StringUtility.SetCotent(sbAreaPolygon, "circle", c);
                                }
                            }
                            if (a.geocode.Any())
                            {
                                foreach (var g in a.geocode)
                                {
                                    sbAreaPolygon.Append(string.Format("<{0}>", "geocode"));
                                    sbAreaPolygon.Append(string.Format("<{0}>{1}</{0}><{2}>{3}</{2}>", "valueName", g.valueName, "value", g.value));
                                    sbAreaPolygon.Append(string.Format("</{0}>", "geocode"));
                                }
                            }
                            StringUtility.SetCotent(sbAreaPolygon, "altitude", a.altitude.ToString());
                            StringUtility.SetCotent(sbAreaPolygon, "ceiling", a.ceiling.ToString());
                            StringUtility.SetCotent(sbAreaItem, "area", sbAreaPolygon.ToString());
                        }
                        sbInfoItem.Append(sbAreaItem.ToString());
                    }


                    sbInfo.Append(string.Format("<{0}>{1}</{0}>", "info", sbInfoItem.ToString()));
                }
            }
            result = StringUtility.ReplaceContent(result, "info", sbInfo.ToString());

            return result;
        }

        /// <summary>
        /// 將Cap物件轉為Kml
        /// </summary>
        /// <returns></returns>
        public string ToKml()
        {
            // alert
            StringBuilder sbAlert = new StringBuilder();
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "identifier", identifier));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "sender", sender));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "sent", sent));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "status", status));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "msgType", msgType));
            if(!string.IsNullOrEmpty(source))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "source", source));
            sbAlert.Append(string.Format("<{0}>{1}</{0}>", "scope", scope));
            if(!string.IsNullOrEmpty(restriction))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "restriction", restriction));
            if(!string.IsNullOrEmpty(addresses))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "addresses", addresses));
            if (code.Any())
            {
                foreach (var c in code)
                {
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "code", c));
                }
            }
            if(!string.IsNullOrEmpty(note))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "note", note));
            if(!string.IsNullOrEmpty(references))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "references", references));
            if(!string.IsNullOrEmpty(incidents))
                sbAlert.Append(string.Format("<{0}>{1}</{0}>", "incidents", incidents));
            string result = StringUtility.ReplaceContent(CapDocumentSetting.Default.kmlTemplate, "alert", sbAlert.ToString());

            // info
            StringBuilder sbInfo = new StringBuilder();
            if (info.Any())
            {
                foreach (var i in info)
                {
                    StringBuilder sbInfoItem = new StringBuilder();
                    if (!string.IsNullOrEmpty(i.language))
                        sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "language", i.language));
                    if (i.category.Any())
                    {
                        foreach (var c in i.category)
                        {
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "category", c));
                        }
                    }
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "event", i.@event));
                    if (i.responseType.Any())
                    {
                        foreach (var r in i.responseType)
                        {
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "responseType", r));
                        }
                    }
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "urgency", i.urgency));
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "severity", i.severity));
                    sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "certainty", i.certainty));
                    StringUtility.SetCotent(sbInfoItem, "audience", i.audience);
                    if (i.eventCode.Any())
                    {
                        foreach (var e in i.eventCode)
                        {
                            string contentString = string.Format("<{0}>{1}</{0}>", "valueName", e.valueName) + string.Format("<{0}>{1}</{0}>", "value", e.value);
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "eventCode", contentString));
                        }
                    }
                    StringUtility.SetCotent(sbInfoItem, "effective", i.effective);
                    StringUtility.SetCotent(sbInfoItem, "onset", i.onset);
                    StringUtility.SetCotent(sbInfoItem, "expires", i.expires);
                    StringUtility.SetCotent(sbInfoItem, "senderName", i.senderName);
                    StringUtility.SetCotent(sbInfoItem, "headline", i.headline);
                    StringUtility.SetCotent(sbInfoItem, "description", i.description);
                    StringUtility.SetCotent(sbInfoItem, "instruction", i.instruction);
                    StringUtility.SetCotent(sbInfoItem, "web", HttpUtility.HtmlEncode(i.web));
                    StringUtility.SetCotent(sbInfoItem, "contact", i.contact);
                    StringUtility.SetCotent(sbInfoItem, "headline", i.headline);
                    if (i.parameter.Any())
                    {
                        foreach (var p in i.parameter)
                        {
                            string contentString = string.Format("<{0}>{1}</{0}>", "valueName", p.valueName) + string.Format("<{0}>{1}</{0}>", "value", p.value);
                            sbInfoItem.Append(string.Format("<{0}>{1}</{0}>", "parameter", contentString));
                        }
                    }

                    // area
                    StringBuilder sbArea = new StringBuilder();
                    if (i.area.Any())
                    {
                        StringBuilder sbAreaItem = new StringBuilder();
                        foreach (var a in i.area)
                        {
                            StringBuilder sbAreaPolygon = new StringBuilder();
                            if (a.polygon.Any())
                            {
                                sbAreaPolygon.Append(string.Format(CapDocumentSetting.Default.kmlAreaPolygonTemplate, "polygon", string.Join(" ", a.polygon)));
                                //foreach (var p in a.polygon)
                                //{
                                //    sbAreaPolygon.Append(string.Format("<{0}>{1}</{0}>", "polygon", p));
                                //}
                            }
                            if (a.circle.Any())
                            {
                                sbAreaPolygon.Append(string.Format(CapDocumentSetting.Default.kmlAreaPolygonTemplate, "circle", string.Join(" ", a.circle)));
                                //foreach (var c in a.circle)
                                //{
                                //    sbAreaPolygon.Append(string.Format("<{0}>{1}</{0}>", "circle", c));
                                //}
                            }
                            if (a.geocode.Any())
                            {
                                foreach (var g in a.geocode)
                                {
                                    sbAreaPolygon.Append(GeoCodeUtility.GetPolygon(g.value, "kml"));
                                    //string contentString = string.Format("<{0}>{1}</{0}>", "valueName", g.valueName) + string.Format("<{0}>{1}</{0}>", "value", g.value);
                                    //sbAreaPolygon.Append(string.Format("<{0}>{1}</{0}>", "geocode", contentString));
                                }
                            }
                            StringBuilder sbAreaOther = new StringBuilder();
                            StringUtility.SetCotent(sbAreaOther, "altitude", a.altitude.ToString());
                            StringUtility.SetCotent(sbAreaOther, "ceiling", a.ceiling.ToString());
                            sbAreaItem.Append(string.Format(CapDocumentSetting.Default.kmlAreaTemplate, a.areaDesc,
                                                        sbAreaPolygon.ToString(), sbAreaOther.ToString()));
                        }
                        sbArea.Append(string.Format(CapDocumentSetting.Default.kmlAreaScopeTemplate, sbAreaItem.ToString()));
                    }

                    // resource
                    StringBuilder sbResource = new StringBuilder();
                    if (i.resource.Any())
                    {
                        StringBuilder sbResourceItem = new StringBuilder();
                        foreach (var r in i.resource)
                        {
                            StringUtility.SetCotent(sbResourceItem, "resourceDesc", r.resourceDesc);
                            StringUtility.SetCotent(sbResourceItem, "mimeType", r.mimeType);
                            StringUtility.SetCotent(sbResourceItem, "size", r.size.ToString());
                            StringUtility.SetCotent(sbResourceItem, "uri", r.uri);
                            StringUtility.SetCotent(sbResourceItem, "derefUri", r.derefUri);
                            StringUtility.SetCotent(sbResourceItem, "digest", r.digest);
                            sbResourceItem.Append(string.Format(CapDocumentSetting.Default.kmlResourceTemplate, sbResourceItem.ToString()));
                        }
                        sbResource.Append(string.Format(CapDocumentSetting.Default.kmlResourceScopeTemplate, sbResourceItem.ToString()));
                    }

                    sbInfo.Append(string.Format(CapDocumentSetting.Default.kmlInfoTemplate, sbInfoItem.ToString(), sbArea.ToString(), sbResource.ToString()));
                }
            }
            result = StringUtility.ReplaceContent(result, "info", sbInfo.ToString());

            return result;
        }

        private void perform(XDocument xDocument)
        {
            if (xDocument.Root == null)
                throw new NullReferenceException();
            xmlns = xDocument.Root.Name.Namespace.ToString();
            var rootElements = xDocument.Root.Elements();
            var xElements = rootElements as XElement[] ?? rootElements.ToArray();
            identifier = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("identifier")).Value;
            sender = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("sender")).Value;
            sent = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("sent")).Value;
            status = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("status")).Value;
            msgType = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("msgType")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("source")))
                source = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("source")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("scope")))
                scope = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("scope")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("restriction")))
                restriction = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("restriction")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("addresses")))
                addresses = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("addresses")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("code")))
                if (xElements.Any(e => e.Name.LocalName.Equals("code")))
                {
                    foreach (var code in xElements.Where(e => e.Name.LocalName.Equals("code")).ToList())
                    {
                        code.Add(code.Value);
                    }
                }
            if (xElements.Any(e => e.Name.LocalName.Equals("note")))
                note = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("note")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("references")))
                references = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("references")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("incidents")))
                incidents = xElements.FirstOrDefault(e => e.Name.LocalName.Equals("incidents")).Value;
            if (xElements.Any(e => e.Name.LocalName.Equals("info")))
            {
                foreach (var info in xElements.Where(e => e.Name.LocalName.Equals("info")).ToList())
                {
                    var infoElements = info.Elements();
                    Info i = new Info();
                    if (infoElements.Any(e => e.Name.LocalName.Equals("language")))
                        i.SetValue(InfoProp.LANGUAGE, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("language")).Value);
                    foreach (var category in infoElements.Where(e => e.Name.LocalName.Equals("category")).ToList())
                    {
                        i.category.Add(category.Value);
                    }
                    i.SetValue(InfoProp.EVENT, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("event")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("responseType")))
                    {
                        foreach (var responseType in infoElements.Where(e => e.Name.LocalName.Equals("responseType")).ToList())
                        {
                            i.responseType.Add(responseType.Value);
                        }
                    }
                    i.SetValue(InfoProp.URGENCY, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("urgency")).Value);
                    i.SetValue(InfoProp.SEVERITY, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("severity")).Value);
                    i.SetValue(InfoProp.CERTAINTY, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("certainty")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("audience")))
                        i.SetValue(InfoProp.AUDIENCE, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("audience")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("eventCode")))
                    {
                        foreach (var eventCode in infoElements.Where(e => e.Name.LocalName.Equals("eventCode")).ToList())
                        {
                            var eventCodeElements = eventCode.Elements();
                            EventCode eventCodeObj = new EventCode(
                                        eventCodeElements.FirstOrDefault(g => g.Name.LocalName.Equals("valueName")).Value,
                                        eventCodeElements.FirstOrDefault(g => g.Name.LocalName.Equals("value")).Value);
                            i.eventCode.Add(eventCodeObj);
                        }
                    }
                    if (infoElements.Any(e => e.Name.LocalName.Equals("effective")))
                        i.SetValue(InfoProp.EFFECTIVE, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("effective")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("onset")))
                        i.SetValue(InfoProp.ONSET, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("onset")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("expires")))
                        i.SetValue(InfoProp.EXPIRES, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("expires")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("senderName")))
                        i.SetValue(InfoProp.SENDERNAME, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("senderName")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("headline")))
                        i.SetValue(InfoProp.HEADLINE, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("headline")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("description")))
                        i.SetValue(InfoProp.DESCRIPTION, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("description")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("instruction")))
                        i.SetValue(InfoProp.INSTRUCTION, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("instruction")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("web")))
                        i.SetValue(InfoProp.WEB, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("web")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("contact")))
                        i.SetValue(InfoProp.CONTACT, infoElements.FirstOrDefault(e => e.Name.LocalName.Equals("contact")).Value);
                    if (infoElements.Any(e => e.Name.LocalName.Equals("parameter")))
                    {
                        foreach (var parameter in infoElements.Where(e => e.Name.LocalName.Equals("parameter")).ToList())
                        {
                            var parameterElements = parameter.Elements();
                            Parameter parameterObj = new Parameter(
                                parameterElements.FirstOrDefault(g => g.Name.LocalName.Equals("valueName")).Value,
                                parameterElements.FirstOrDefault(g => g.Name.LocalName.Equals("value")).Value);
                            i.parameter.Add(parameterObj);
                        }
                    }
                    if (infoElements.Any(e => e.Name.LocalName.Equals("resource")))
                    {
                        foreach (var resource in infoElements.Where(e => e.Name.LocalName.Equals("resource")).ToList())
                        {
                            var resourceElements = resource.Elements();
                            Resource resourceObj = new Resource(resourceElements.FirstOrDefault(a => a.Name.LocalName.Equals("resourceDesc")).Value);
                            foreach (string resourceElementName in new ArrayList() { "mimeType", "size", "uri", "derefUri", "digest" })
                            {
                                if (resourceElements.Any(e => e.Name.LocalName.Equals(resourceElementName)))
                                    typeof(Resource).GetProperty(resourceElementName).SetValue(resourceObj, resourceElements.FirstOrDefault(a => a.Name.LocalName.Equals(resourceElementName)).Value);
                            }
                            i.resource.Add(resourceObj);
                        }
                    }
                    if (infoElements.Any(e => e.Name.LocalName.Equals("area")))
                    {
                        foreach (var area in infoElements.Where(e => e.Name.LocalName.Equals("area")).ToList())
                        {
                            var areaElements = area.Elements();
                            Area areaObj = new Area(areaElements.FirstOrDefault(a => a.Name.LocalName.Equals("areaDesc")).Value);
                            if (areaElements.Any(e => e.Name.LocalName.Equals("geocode")))
                            {
                                foreach (var geocode in areaElements.Where(e => e.Name.LocalName.Equals("geocode")).ToList())
                                {
                                    var geocodeElements = geocode.Elements();
                                    Geocode geocodeObj = new Geocode(
                                        geocodeElements.FirstOrDefault(g => g.Name.LocalName.Equals("valueName")).Value,
                                        geocodeElements.FirstOrDefault(g => g.Name.LocalName.Equals("value")).Value);
                                    areaObj.geocode.Add(geocodeObj);
                                }
                            }
                            if (areaElements.Any(e => e.Name.LocalName.Equals("polygon")))
                            {
                                foreach (var polygon in areaElements.Where(e => e.Name.LocalName.Equals("polygon")).ToList())
                                {
                                    areaObj.polygon.Add(polygon.Value);
                                }
                                foreach (string areaElementName in new ArrayList() { "altitude", "ceiling" })
                                {
                                    if (areaElements.Any(e => e.Name.LocalName.Equals(areaElementName)))
                                        typeof(Area).GetProperty(areaElementName).SetValue(areaObj, areaElements.FirstOrDefault(a => a.Name.LocalName.Equals(areaElementName)).Value);
                                }
                            }
                            if (areaElements.Any(e => e.Name.LocalName.Equals("circle")))
                            {
                                foreach (var circle in areaElements.Where(e => e.Name.LocalName.Equals("circle")).ToList())
                                {
                                    areaObj.circle.Add(circle.Value);
                                }
                            }
                            
                            i.area.Add(areaObj);
                        }
                    }
                    this.info.Add(i);
                }
            }
        }
    }
}
