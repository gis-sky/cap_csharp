using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDocument
{
    public class Value
    {
        /// <summary>
        /// Value Name
        /// </summary>
        public readonly string valueName;
        /// <summary>
        /// Value
        /// </summary>
        public readonly string value;

        public Value(string valueName, string value)
        {
            this.valueName = valueName;
            this.value = value;
        }
    }
}
