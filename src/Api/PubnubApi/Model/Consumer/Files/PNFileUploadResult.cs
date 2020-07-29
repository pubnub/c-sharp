using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNFileUploadResult
    {
        public long Timetoken { get; internal set; }
        public string FileId { get; internal set; }
        public string FileName { get; internal set; }
    }
}
