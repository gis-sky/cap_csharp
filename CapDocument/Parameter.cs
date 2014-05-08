using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDocument
{
    public class Parameter : Value
    {
        public Parameter(string valueName, string value)
            : base(valueName, value)
        {
        }
    }
}
