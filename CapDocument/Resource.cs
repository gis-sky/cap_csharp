using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapDocument
{
    public class Resource
    {
        public string resourceDesc { get; set; }
        public string mimeType { get; set; }
        public string size { get; set; }
        public string uri { get; set; }
        public string derefUri { get; set; }
        public string digest { get; set; }

        public Resource() {}

        public Resource(string resourceDesc)
        {
            this.resourceDesc = resourceDesc;
        }
    }
}
