using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNFileUploadResult
    {
        public long Timetoken { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
    }
}
