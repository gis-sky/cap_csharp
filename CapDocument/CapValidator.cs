using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CapDocument
{
    public class CapValidator
    {
        public const string ALERT = "alert";
        public const string INFO = "info";

        public static readonly List<string> ALERT_TAGS = new List<string>()
            {
                "identifier",
                "sender",
                "sent",
                "status",
                "msgType",
                "scope",
                "info"
                //"code"
            };

        public static readonly List<string> INFO_TAGS = new List<string>()
            {
                "language",
                "category",
                "event",
                "responseType",
                "urgency",
                "severity",
                "certainty",
                "audience",
                "eventCode",
                "effective",
                "onset",
                "expires",
                "senderName",
                "headline",
                "description",
                "instruction",
                "web",
                "contact",
                "parameter",
                "resource",
                "area"
            };

        public static readonly List<string> INFO_OPTION_TAGS = new List<string>()
            {
                "language",
                "responseType",
                "audience",
                "eventCode",
                "onset",
                "senderName",
                "headline",
                "description",
                "instruction",
                "web",
                "contact",
                "parameter",
                "resource"
            };

        public static readonly Dictionary<string, Regex> INFO_REGEX_DIC = new Dictionary<string, Regex>()
            {
                {"effective", new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d")},
                {"onset", new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d")},
                {"expires", new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d")},
                {"headline", new Regex(@"^.{1,80}$")}
            };

        public static readonly Dictionary<string, List<string>> INFO_VALID_DIC = new Dictionary<string, List<string>>()
            {
                {"category", new List<string>()
                    {
                        "Geo",
                        "Met",
                        "Safety",
                        "Security",
                        "Rescue",
                        "Fire",
                        "Health",
                        "Env",
                        "Transport",
                        "Infra",
                        "CBRNE",
                        "Other"
                    }},
                {"responseType", new List<string>()
                    {
                        "Shelter",
                        "Evacuate",
                        "Prepare",
                        "Execute",
                        "Avoid",
                        "Monitor",
                        "Assess",
                        "AllClear",
                        "None"
                    }},
                {"urgency", new List<string>()
                    {
                        "Immediate",
                        "Expected",
                        "Future",
                        "Past",
                        "Unknown"
                    }},
                {"severity", new List<string>()
                    {
                        "Extreme",
                        "Severe",
                        "Moderate",
                        "Minor",
                        "Unknown"
                    }},
                {"certainty", new List<string>()
                    {
                        "Observed",
                        "Likely",
                        "Possible",
                        "Unlikely",
                        "Unknown"
                    }}
            };

        public static readonly List<string> STATUS = new List<string>()
            {
                "Actual",
                "Exercise",
                "System",
                "Test",
                "Draft"
            };

        public static readonly List<string> MSG_TYPE = new List<string>()
            {
                "Alert",
                "Update",
                "Cancel",
                "Ack",
                "Error"
            };

        public static readonly List<string> SCOPE = new List<string>()
            {
                "Public",
                "Restricted",
                "Private"
            };

        /// <summary>
        /// 驗證Cap物件
        /// </summary>
        /// <param name="capDocument">Cap物件</param>
        /// <returns></returns>
        public static List<CapValidateResult> Validate(CapDocument capDocument)
        {
            var capValidateResults = new List<CapValidateResult>();
            foreach (var name in ALERT_TAGS)
            {
                DateTime tester = new DateTime();
                Regex regex = new Regex(@"^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d$");
                switch (name)
                {
                    case "sent":
                        if (!string.IsNullOrEmpty(capDocument.sent))
                        {
                            if (!regex.IsMatch(capDocument.sent) || !DateTime.TryParse(capDocument.sent.ToString().Replace("T", " ").Replace("+08:00", ""), out tester))
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format("{0}不是正確的時間格式：{1}", name, capDocument.sent)));
                            }
                        }
                        else
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("發送的檔案中缺少{0}將造成讀取錯誤，請忽略後續錯誤並填入後重新檢核。", name)));
                        }

                        break;

                    case "status":
                        if (!string.IsNullOrEmpty(capDocument.status))
                        {
                            if (!STATUS.Contains(capDocument.status))
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format("發送的檔案中缺少{0}", name)));
                            }
                        }
                        else
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("發送的檔案中缺少{0}", name)));
                        }
                        break;

                    case "msgType":
                        if (!MSG_TYPE.Contains(capDocument.msgType))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("發送的檔案中缺少{0}", name)));
                        }
                        else
                        {
                            if (!capDocument.msgType.Equals("Alert"))
                            {
                                if (string.IsNullOrEmpty(capDocument.references))
                                {
                                    capValidateResults.Add(new CapValidateResult("references", string.Format("msgType為{0}的示警檔案必須填寫references", capDocument.msgType)));
                                    break;
                                }
                                StringBuilder ErrorsOfRefer = new StringBuilder();
                                List<string> References = new List<string>();
                                References = capDocument.references.Split(new Char[] { ' ' }).ToList();
                                foreach (var triplet in References)
                                {
                                    int commaCheck = 0;
                                    MatchCollection mc;
                                    Regex r = new Regex(",");

                                    commaCheck = r.Matches(triplet).Count;

                                    if (commaCheck != 2)
                                    {
                                        capValidateResults.Add(new CapValidateResult("references", string.Format("references格式有誤(應為sender,identifier,sent三項一組並以空格分組)。此組填寫的內容為：{0}", "[" + triplet + "]"))); 
                                        continue;
                                    }
                                    int firstComma = triplet.IndexOf(",");
                                    int secondComma = triplet.Substring(firstComma + 1, triplet.Length - firstComma - 1).IndexOf(",") + firstComma;

                                    if (firstComma == 0 && secondComma == 0)
                                    {
                                        ErrorsOfRefer.AppendLine("references格式有誤(應為sender,identifier,sent三項一組並以空格分組)：" + capDocument.references);
                                    }
                                    if (firstComma == 0 && secondComma != 0)
                                    {
                                        ErrorsOfRefer.AppendLine("references格式有誤(應為sender,identifier,sent三項一組並以空格分組)，其中缺少sender部分：" + capDocument.references);
                                    }
                                    if (firstComma != 0 && secondComma == 0)
                                    {
                                        ErrorsOfRefer.AppendLine("references格式有誤(應為sender,identifier,sent三項一組並以空格分組)，其中缺少sent部分：" + capDocument.references);
                                    }
                                    if (secondComma == firstComma)
                                    {
                                        ErrorsOfRefer.AppendLine("references格式有誤(應為sender,identifier,sent三項一組並以空格分組)，其中缺少identifier部分：" + capDocument.references);
                                    }
                                    string senderOfRefer = triplet.Substring(0, firstComma);
                                    try
                                    {
                                        string idOfRefer = triplet.Substring(firstComma + 1, secondComma - firstComma);
                                    }
                                    catch (Exception)
                                    {
                                        ErrorsOfRefer.AppendLine("references格式有誤(應為sender,identifier,sent三項一組並以空格分組)：" + capDocument.references);
                                    }
                                    
                                    string sentOfRefer = triplet.Substring(secondComma + 2, triplet.Length - secondComma - 2);

                                    //檢查sent是否是標準時間格式
                                    if (!regex.IsMatch(sentOfRefer) || !DateTime.TryParse(sentOfRefer.Replace("T", " ").Replace("+08:00", ""), out tester))
                                    {
                                        ErrorsOfRefer.AppendLine("references格式有誤(應為sender,identifier,sent三項一組並以空格分組)，其中sent非正確的時間格式：" + capDocument.references);
                                    }
                                    //ErrorsOfRefer.AppendLine("第一點" + firstComma + "第二點"+secondComma);
                                }
                                if (string.IsNullOrEmpty(ErrorsOfRefer.ToString()))
                                {
                                    break;
                                }

                                capValidateResults.Add(new CapValidateResult("references", ErrorsOfRefer.ToString()));
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(capDocument.references))
                                {
                                    capValidateResults.Add(new CapValidateResult("references", string.Format("msgType為{0}的示警不得填寫references", capDocument.msgType)));
                                    break;
                                }
                            }
                        }
                        break;

                    case "scope":
                        if (!SCOPE.Contains(capDocument.scope))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}資料錯誤{1}", name, capDocument.scope)));
                        }
                        else if (capDocument.scope.Equals("restricted"))
                        {
                            if (!string.IsNullOrEmpty(capDocument.restriction))
                                capValidateResults.Add(new CapValidateResult("restriction", string.Format("當scope為restricted時必須填寫{0}", "restriction")));
                        }
                        else if (capDocument.scope.Equals("private"))
                        {
                            if (!string.IsNullOrEmpty(capDocument.addresses))
                                capValidateResults.Add(new CapValidateResult("addresses", string.Format("當scope為private時必須填寫{0}", "addresses")));
                        }
                        break;
                    //case "code":
                    //    if (!capDocument.code.Any())
                    //    {
                    //        capValidateResults.Add(new CapValidateResult(name, string.Format("{0}不可為空.", name)));
                    //    }
                    //    break;
                }
            }

            var infos = capDocument.info;
            if (infos.Any())
            {
                int InfoIndex = 1;
                if (infos.Count == 1)
                {
                    InfoIndex = -1;
                }
                foreach (var info in infos)
                {
                    string responseType = info.responseType.FirstOrDefault();
                    string category = info.category.FirstOrDefault();
                    string infoText;
                    if (InfoIndex == -1)
                    {
                        infoText = "";
                    }
                    else
                    {
                        infoText = string.Format("第{0}個", InfoIndex.ToString());
                    }

                    List<EventCode> eventCode = new List<EventCode>(info.eventCode);
                    List<Resource> resource = new List<Resource>(info.resource);
                    List<Parameter> parameter = new List<Parameter>(info.parameter);
                    List<Area> area = new List<Area>(info.area);

                    if (string.IsNullOrEmpty(info.expires))
                    {
                        capValidateResults.Add(new CapValidateResult("expires", infoText + "info中expires未填入任何值，請務必填入失效的日期與時間"));
                    }

                    if (area.Count > 0)
                    {
                        foreach (var itemArea in area)
                        {
                            capValidateResults.AddRange(areaValidator(itemArea, InfoIndex));
                        }
                    }
                    else
                    {
                        capValidateResults.Add(new CapValidateResult("area", infoText + "info中不含任何area，area至少要有一個以上"));
                    }

                    capValidateResults.AddRange(checkInfoValueByName(info, InfoIndex, "responseType", responseType));
                    capValidateResults.AddRange(checkInfoValueByName(info, InfoIndex, "category", category));
                    capValidateResults.AddRange(infoValidator(info, InfoIndex));
                    InfoIndex++;
                }
            }
            else
            {
                capValidateResults.Add(new CapValidateResult("info", string.Format("{0}至少要有一個以上，此檔案不含任何info", "info")));
            }
            return capValidateResults;
        }

        private static IEnumerable<CapValidateResult> areaValidator(Area itemArea, int index)
        {
            string infoIndex = index < 0 ? "" : "第" + index + "個info中";
            var capValidateResults = new List<CapValidateResult>();

            var areaDesc = itemArea.areaDesc;
            if (string.IsNullOrEmpty(areaDesc))
            {
                capValidateResults.Add(new CapValidateResult("areaDesc", string.Format(infoIndex + "必須有{0}節點", "areaDesc")));
            }
            List<Geocode> geocode = new List<Geocode>(itemArea.geocode);
            foreach (var itemGC in geocode)
            {
                if (string.IsNullOrEmpty(itemGC.valueName))
                {
                    capValidateResults.Add(new CapValidateResult("geocode", string.Format(infoIndex + "{2}Goecode沒有對應的值，Code:{0}，值：{1}", "空", itemGC.value, itemArea.areaDesc)));
                }
                if (string.IsNullOrEmpty(itemGC.value))
                {
                    capValidateResults.Add(new CapValidateResult("geocode", string.Format(infoIndex + "{2}Goecode沒有對應的值，Code:{0}，值：{1}", itemGC.valueName, "空", itemArea.areaDesc)));
                }
            }
            return capValidateResults;
        }

        private static IEnumerable<CapValidateResult> infoValidator(Info info, int index)
        {
            var capValidateResults = new List<CapValidateResult>();

            foreach (var name in INFO_TAGS)
            {
                var obj = typeof(Info).GetProperty(name);
                if (obj == null || obj.GetValue(info, null) == null)
                {
                    //進入此處表示值為空，不做處理。
                }
                else
                {
                    var value = obj.GetValue(info, null).ToString();
                    if (string.IsNullOrEmpty(value))
                        continue;

                    capValidateResults.AddRange(checkInfoValueByName(info, index, name, value));
                }
            }
            return capValidateResults;
        }

        private static IEnumerable<CapValidateResult> checkInfoValueByName(Info info, int index, string name, string value)
        {
            string infoIndex = index < 0 ? "" : "第" + index + "個info中";
            var capValidateResults = new List<CapValidateResult>();
            DateTime tester = new DateTime();
            Regex regex = new Regex(@"^\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d$");

            if (INFO_REGEX_DIC.ContainsKey(name))
            {
                if (!INFO_REGEX_DIC[name].IsMatch(value))
                {
                    capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}資料錯誤：{1}", name, value)));
                }
                else
                {
                    switch (name)
                    {
                        case "effective":
                            if (!string.IsNullOrEmpty(value))
                            {
                                if (!regex.IsMatch(value) || !DateTime.TryParse(value.Replace("T", " ").Replace("+08:00", ""), out tester))
                                {
                                    capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}不是正確的時間格式：{1}", name, value, index)));
                                }
                            }
                            else
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "缺少{0}將造成讀取錯誤，請忽略後續錯誤並填入後重新檢核。", name, index)));
                            }
                            break;
                        case "onset":
                            if (!regex.IsMatch(value) || !string.IsNullOrEmpty(value))
                            {
                                if (!DateTime.TryParse(value.Replace("T", " ").Replace("+08:00", ""), out tester))
                                {
                                    capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}不是正確的時間格式：{1}", name, value, index)));
                                }
                            }
                            else
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "缺少{0}將造成讀取錯誤，請忽略後續錯誤並填入後重新檢核。", name, index)));
                            }
                            break;
                        case "expires":
                            if (!string.IsNullOrEmpty(value))
                            {
                                if (!regex.IsMatch(value) || !DateTime.TryParse(value.Replace("T", " ").Replace("+08:00", ""), out tester))
                                {
                                    capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}不是正確的時間格式：{1}", name, value, index)));
                                    break;
                                }
                                else
                                {
                                    if (tester < DateTime.Now)
                                    {
                                        capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}早於現在的時間，此示警現已失效：{1}", name, value, index)));
                                    }
                                }
                            }
                            else
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "缺少{0}將造成讀取錯誤，請忽略後續錯誤並填入後重新檢核。", name, index)));
                            }
                            break;
                    }
                    if (name.Equals("expires"))
                    {
                        var dtExpires = new DateTime();
                        bool TimeFomat = DateTime.TryParse(value, out dtExpires);
                        if (TimeFomat)
                        {
                            var onsetStr = GetObjectValue(typeof(Info), "onset", info);
                            if (onsetStr != null)
                            {
                                var dt = new DateTime();
                                DateTime.TryParse(onsetStr, out dt);
                                if (dtExpires < dt)
                                {
                                    capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}必須晚於onset：結束時間{0}-{2}早於開始時間onset-{1}", name, info.onset, value)));
                                    //continue;
                                }
                            }
                            var effectiveStr = GetObjectValue(typeof(Info), "effective", info);
                            if (effectiveStr != null)
                            {
                                var dt = new DateTime();
                                DateTime.TryParse(effectiveStr, out dt);
                                if (dtExpires < dt)
                                {
                                    capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}必須晚於effective：結束時間{0}-{2}早於生效時間effective-{1}", name, info.effective, value)));
                                    //continue;
                                }
                            }
                            else
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "必須要有{0}", name)));
                                //continue;
                            }
                        }
                    }
                }
            }
            else if (INFO_VALID_DIC.ContainsKey(name))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!INFO_VALID_DIC[name].Contains(value))
                    {
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format(infoIndex + "{0}中填寫的值不正確，填寫的值為：{1}", name, value)));
                        }
                        //continue;
                    }
                }
            }

            return capValidateResults;
        }

        /// <summary>
        /// 驗證Cap XML物件
        /// </summary>
        /// <param name="xDocument">Cap XML物件</param>
        /// <returns></returns>
        public static List<CapValidateResult> Validate(XDocument xDocument)
        {
            var capValidateResults = new List<CapValidateResult>();
            if (xDocument.Root == null || !xDocument.Root.Name.LocalName.Equals(ALERT, StringComparison.OrdinalIgnoreCase))
            {
                capValidateResults.Add(new CapValidateResult(ALERT, string.Format("根節點必須是{0}.", ALERT)));
                return capValidateResults;
            }
            foreach (var name in ALERT_TAGS)
            {
                var xelements = xDocument.Root.Elements().Where(x => x.Name.LocalName.Equals(name)).ToList();
                if (capValidateResults.Any(c => c.GetSubject().Equals(name)))
                    continue;
                if (!xelements.Any())
                {
                    capValidateResults.Add(new CapValidateResult(name, string.Format("必須有{0}節點.", name)));
                    continue;
                }
                var tag = xelements.FirstOrDefault();
                if (tag == null)
                    throw new ArgumentNullException("tag");
                switch (name)
                {
                    case "sent":
                        Regex regex = new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d");
                        if (!regex.IsMatch(tag.Value))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料格式錯誤.", name)));
                        }
                        else
                        {
                            DateTime dt;
                            if (!DateTime.TryParse(tag.Value, out dt))
                                capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料格式錯誤.", name)));
                        }
                        break;

                    case "status":
                        if (!STATUS.Contains(tag.Value))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料格式錯誤.", name)));
                        }
                        break;

                    case "msgType":
                        if (!MSG_TYPE.Contains(tag.Value))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料格式錯誤.", name)));
                        }
                        else
                        {
                            if (!tag.Value.Equals("Alert"))
                            {
                                var referencesElements = xDocument.Root.Elements().Where(x => x.Name.LocalName.Equals("references")).ToList();
                                if (!referencesElements.Any())
                                {
                                    capValidateResults.Add(new CapValidateResult("references", string.Format("必須有{0}節點.", "references")));
                                }
                            }
                        }
                        break;

                    case "scope":
                        if (!SCOPE.Contains(tag.Value))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料格式錯誤.", name)));
                        }
                        else if (tag.Value.Equals("restricted"))
                        {
                            var restrictionElements = xDocument.Root.Elements().Where(x => x.Name.LocalName.Equals("restriction")).ToList();
                            if (!restrictionElements.Any())
                            {
                                capValidateResults.Add(new CapValidateResult("restriction", string.Format("必須要有{0}節點.", "restriction")));
                            }
                        }
                        else if (tag.Value.Equals("private"))
                        {
                            var addressesElements = xDocument.Root.Elements().Where(x => x.Name.LocalName.Equals("addresses")).ToList();
                            if (!addressesElements.Any())
                            {
                                capValidateResults.Add(new CapValidateResult("addresses", string.Format("必須要有{0}節點.", "addresses")));
                            }
                        }
                        break;
                }
            }
            var infos = xDocument.Root.Elements().Where(x => x.Name.LocalName.Equals(INFO, StringComparison.OrdinalIgnoreCase)).ToList();
            if (infos.Any())
            {
                foreach (var info in infos)
                {
                    foreach (var name in INFO_TAGS)
                    {
                        var xelements = info.Elements().Where(x => x.Name.LocalName.Equals(name)).ToList();
                        if (capValidateResults.Any(c => c.GetSubject().Equals(name)))
                            continue;
                        if (!xelements.Any())
                        {
                            if (!INFO_OPTION_TAGS.Contains(name))
                                capValidateResults.Add(new CapValidateResult(name, string.Format("必須有{0}節點.", name)));
                            continue;
                        }
                        var tag = xelements.FirstOrDefault();
                        if (tag == null)
                            throw new ArgumentNullException("tag");
                        if (INFO_REGEX_DIC.ContainsKey(name))
                        {
                            if (!INFO_REGEX_DIC[name].IsMatch(tag.Value))
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料格式錯誤.", name)));
                                continue;
                            }
                            else if (name.Equals("expires"))
                            {
                                DateTime dt;
                                if (!DateTime.TryParse(tag.Value, out dt))
                                    capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料格式錯誤.", name)));

                                var dtExpires = new DateTime();
                                DateTime.TryParse(tag.Value, out dtExpires);
                                var firstOrDefault =
                                    info.Elements()
                                        .Where(x => x.Name.LocalName.Equals("onset"))
                                        .ToList()
                                        .FirstOrDefault();
                                if (firstOrDefault != null)
                                {
                                    var onset = firstOrDefault.Value;
                                    if (INFO_REGEX_DIC[name].IsMatch(onset))
                                    {
                                        dt = new DateTime();
                                        DateTime.TryParse(onset, out dt);
                                        if (dtExpires < dt)
                                        {
                                            capValidateResults.Add(new CapValidateResult(name,
                                                                                         string.Format("{0}必須晚於onset.",
                                                                                                       name)));
                                            continue;
                                        }
                                    }
                                }
                                firstOrDefault =
                                    info.Elements()
                                        .Where(x => x.Name.LocalName.Equals("effective"))
                                        .ToList()
                                        .FirstOrDefault();
                                if (firstOrDefault != null)
                                {
                                    var effective = firstOrDefault.Value;
                                    if (INFO_REGEX_DIC[name].IsMatch(effective))
                                    {
                                        dt = new DateTime();
                                        DateTime.TryParse(effective, out dt);
                                        if (dtExpires < dt)
                                        {
                                            capValidateResults.Add(new CapValidateResult(name,
                                                                                         string.Format(
                                                                                             "{0}必須晚於effective.", name)));
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        if (INFO_VALID_DIC.ContainsKey(name))
                        {
                            if (!INFO_VALID_DIC[name].Contains(tag.Value))
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format("{0}節點資料錯誤.", name)));
                                continue;
                            }
                        }
                        switch (name)
                        {
                            case "eventCode":
                            case "parameter":
                                foreach (var elementName in new ArrayList() { "valueName", "value" })
                                {
                                    if (!tag.Elements().Any(t => t.Name.LocalName.Equals(elementName)))
                                        capValidateResults.Add(new CapValidateResult(name,
                                                                                     string.Format("{0}節點缺少{1}.", name,
                                                                                                   elementName)));
                                }
                                break;

                            case "resource":
                                foreach (var elementName in new ArrayList() { "resourceDesc", "mimeType" })
                                {
                                    if (!tag.Elements().Any(t => t.Name.LocalName.Equals(elementName)))
                                        capValidateResults.Add(new CapValidateResult(name,
                                                                                     string.Format("{0}節點缺少{1}.", name,
                                                                                                   elementName)));
                                }
                                break;

                            case "area":
                                foreach (var elementName in new ArrayList() { "areaDesc" })
                                {
                                    if (!tag.Elements().Any(t => t.Name.LocalName.Equals(elementName)))
                                        capValidateResults.Add(new CapValidateResult(name,
                                                                                     string.Format("{0}節點缺少{1}.", name,
                                                                                                   elementName)));
                                }
                                var geocodes = tag.Elements().Where(t => t.Name.LocalName.Equals("geocode"));
                                foreach (var geocode in geocodes)
                                {
                                    foreach (var elementName in new ArrayList() { "valueName", "value" })
                                    {
                                        if (!geocode.Elements().Any(t => t.Name.LocalName.Equals(elementName)))
                                            capValidateResults.Add(new CapValidateResult(name,
                                                                                         string.Format("{0}節點缺少{1}.",
                                                                                                       "geocode",
                                                                                                       elementName)));
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            else
            {
                capValidateResults.Add(new CapValidateResult("info", string.Format("{0}至少有一個以上.", "info")));
            }
            return capValidateResults;
        }

        private static string GetObjectValue(Type type, string name, Object info)
        {
            var obj = type.GetProperty(name);
            if (obj == null || obj.GetValue(info, null) == null)
                return null;
            var value = obj.GetValue(info, null).ToString();
            if (string.IsNullOrEmpty(value))
                return null;
            return value;
        }
    }
}