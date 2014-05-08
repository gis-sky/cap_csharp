using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDocument
{
    public class CapValidateResult
    {
        private readonly string _subject;
        private readonly string _message;

        public CapValidateResult(string subject, string message)
        {
            _subject = subject;
            _message = message;
        }

        /// <summary>
        /// 取得主旨
        /// </summary>
        /// <returns>主旨</returns>
        public string GetSubject()
        {
            return _subject;
        }

        /// <summary>
        /// 取得訊息
        /// </summary>
        /// <returns>訊息</returns>
        public string GetMessage()
        {
            return _message;
        }
    }
}
