using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CapDocument
{
    class CapValidator
    {
        private const string ALERT = "alert";
        private const string INFO = "info";

        private static readonly List<string> ALERT_TAGS = new List<string>()
            {
                "identifier",
                "sender",
                "sent",
                "status",
                "msgType",
                "scope",
                "code"
            };

        private static readonly List<string> INFO_TAGS = new List<string>()
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

        private static readonly List<string> INFO_OPTION_TAGS = new List<string>()
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

        private static readonly Dictionary<string, Regex> INFO_REGEX_DIC = new Dictionary<string, Regex>()
            {
                {"effective", new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d")},
                {"onset", new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d")},
                {"expires", new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d")},
                {"headline", new Regex(@"^.{1,20}$")}
            };

        private static readonly Dictionary<string, List<string>> INFO_VALID_DIC = new Dictionary<string, List<string>>()
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

        private static readonly List<string> STATUS = new List<string>()
            {
                "Actual",
                "Exercise",
                "System",
                "Test",
                "Draft"
            };

        private static readonly List<string> MSG_TYPE = new List<string>()
            {
                "Alert",
                "Update",
                "Cancel",
                "Ack",
                "Error"
            };

        private static readonly List<string> SCOPE = new List<string>()
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
                switch (name)
                {
                    case "sent":
                        Regex regex = new Regex(@"\d\d\d\d-\d\d-\d\dT\d\d:\d\d:\d\d[-,+]\d\d:\d\d");
                        if (!regex.IsMatch(capDocument.sent))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}資料格式錯誤.", name)));
                        }
                        break;
                    case "status":
                        if (!STATUS.Contains(capDocument.status))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}資料格式錯誤.", name)));
                        }
                        break;
                    case "msgType":
                        if (!MSG_TYPE.Contains(capDocument.msgType))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}資料格式錯誤.", name)));
                        }
                        else
                        {
                            if (!capDocument.msgType.Equals("Alert") && string.IsNullOrEmpty(capDocument.references))
                            {
                                capValidateResults.Add(new CapValidateResult("references", string.Format("{0}必須存在.", "references")));
                            }
                        }
                        break;
                    case "scope":
                        if (!SCOPE.Contains(capDocument.scope))
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}資料格式錯誤.", name)));
                        }
                        else if (capDocument.scope.Equals("restricted"))
                        {
                            if (!string.IsNullOrEmpty(capDocument.restriction))
                                capValidateResults.Add(new CapValidateResult("restriction", string.Format("必須要有{0}節點.", "restriction")));
                        }
                        else if (capDocument.scope.Equals("private"))
                        {
                            if (!string.IsNullOrEmpty(capDocument.addresses))
                                capValidateResults.Add(new CapValidateResult("addresses", string.Format("必須要有{0}節點.", "addresses")));
                        }
                        break;
                    case "code":
                        if (!capDocument.code.Any())
                        {
                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}不可為空.", name)));
                        }
                        break;
                }
            }
            var infos = capDocument.info;
            if (infos.Any())
            {
                foreach (var info in infos)
                {
                    foreach (var name in INFO_TAGS)
                    {
                        var obj = typeof (Info).GetProperty(name);
                        if (obj == null || obj.GetValue(info) == null)
                            continue;
                        var value = obj.GetValue(info).ToString();
                        if (string.IsNullOrEmpty(value))
                            continue;
                        if (INFO_REGEX_DIC.ContainsKey(name))
                        {
                            if (!INFO_REGEX_DIC[name].IsMatch(value))
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format("{0}資料格式錯誤.", name)));
                                continue;
                            }
                            else if (name.Equals("expires"))
                            {
                                var dtExpires = new DateTime();
                                DateTime.TryParse(value, out dtExpires);
                                var onsetStr = GetObjectValue(typeof (Info), "onset", info);
                                if (onsetStr != null)
                                {
                                    if (INFO_REGEX_DIC[name].IsMatch(onsetStr))
                                    {
                                        var dt = new DateTime();
                                        DateTime.TryParse(onsetStr, out dt);
                                        if (dtExpires < dt)
                                        {
                                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}必須晚於onset.", name)));
                                            continue;
                                        }
                                    }
                                }
                                var effectiveStr = GetObjectValue(typeof(Info), "effective", info);
                                if (effectiveStr != null)
                                {
                                    if (INFO_REGEX_DIC[name].IsMatch(effectiveStr))
                                    {
                                        var dt = new DateTime();
                                        DateTime.TryParse(effectiveStr, out dt);
                                        if (dtExpires < dt)
                                        {
                                            capValidateResults.Add(new CapValidateResult(name, string.Format("{0}必須晚於effective.", name)));
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        else if (INFO_VALID_DIC.ContainsKey(name))
                        {
                            if (!INFO_VALID_DIC[name].Contains(value))
                            {
                                capValidateResults.Add(new CapValidateResult(name, string.Format("{0}資料錯誤.", name)));
                                continue;
                            }
                        }
                        else if (name.Equals("area"))
                        {
                            var areaDesc = GetObjectValue(typeof(Info), "areaDesc", info);
                            if (string.IsNullOrEmpty(areaDesc))
                            {
                                capValidateResults.Add(new CapValidateResult("areaDesc", string.Format("必須有{0}節點.", "areaDesc")));
                                continue;
                            }
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
                                foreach (var elementName in new ArrayList() {"valueName", "value"})
                                {
                                    if (!tag.Elements().Any(t => t.Name.LocalName.Equals(elementName)))
                                        capValidateResults.Add(new CapValidateResult(name,
                                                                                     string.Format("{0}節點缺少{1}.", name,
                                                                                                   elementName)));
                                }
                                break;
                            case "resource":
                                foreach (var elementName in new ArrayList() {"resourceDesc", "mimeType"})
                                {
                                    if (!tag.Elements().Any(t => t.Name.LocalName.Equals(elementName)))
                                        capValidateResults.Add(new CapValidateResult(name,
                                                                                     string.Format("{0}節點缺少{1}.", name,
                                                                                                   elementName)));
                                }
                                break;
                            case "area":
                                foreach (var elementName in new ArrayList() {"areaDesc"})
                                {
                                    if (!tag.Elements().Any(t => t.Name.LocalName.Equals(elementName)))
                                        capValidateResults.Add(new CapValidateResult(name,
                                                                                     string.Format("{0}節點缺少{1}.", name,
                                                                                                   elementName)));
                                }
                                var geocodes = tag.Elements().Where(t => t.Name.LocalName.Equals("geocode"));
                                foreach (var geocode in geocodes)
                                {
                                    foreach (var elementName in new ArrayList() {"valueName", "value"})
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
            if (obj == null || obj.GetValue(info) == null)
                return null;
            var value = obj.GetValue(info).ToString();
            if (string.IsNullOrEmpty(value))
                return null;
            return value;
        }
    }
}
