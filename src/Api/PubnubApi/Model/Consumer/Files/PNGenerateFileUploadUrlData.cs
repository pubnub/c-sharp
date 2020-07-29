using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGenerateFileUploadUrlData
    {
        public string Url { get; internal set; }
        public string Method { get; internal set; }
        public string ExpirationDate { get; internal set; }
        public Dictionary<string, object> FormFields { get; internal set; }
    }
}
