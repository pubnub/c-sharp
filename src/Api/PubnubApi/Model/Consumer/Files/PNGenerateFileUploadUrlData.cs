using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGenerateFileUploadUrlData
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public string ExpirationDate { get; set; }
        public Dictionary<string, object> FormFields { get; set; }
    }
}
