using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDocument
{
    public static class StringUtility
    {
        /// <summary>
        /// 設定內容
        /// </summary>
        /// <param name="sb">字串容器</param>
        /// <param name="tagName">Tag名稱</param>
        /// <param name="content">內容</param>
        /// <returns>處裡完成字串</returns>
        public static StringBuilder SetCotent(StringBuilder sb, string tagName, string content)
        {
            if (!string.IsNullOrEmpty(content))
                sb.Append(string.Format("<{0}>{1}</{0}>", tagName, content));
            return sb;
        }

        /// <summary>
        /// 置換內容
        /// </summary>
        /// <param name="temp">置換字串</param>
        /// <param name="target">目標變數</param>
        /// <param name="content">內容</param>
        /// <returns>處裡完成字串</returns>
        public static string ReplaceContent(string temp, string target, string content)
        {
            return temp.Replace(string.Format("{{{{{0}}}}}", target), content);
        }

    }
}
