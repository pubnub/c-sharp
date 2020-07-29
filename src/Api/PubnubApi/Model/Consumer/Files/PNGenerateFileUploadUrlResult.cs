using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGenerateFileUploadUrlResult
    {
        public string FileId { get; internal set; }
        public string FileName { get; internal set; }
        public PNGenerateFileUploadUrlData FileUploadRequest { get; internal set; }
    }
}
