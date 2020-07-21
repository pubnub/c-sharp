using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGenerateFileUploadUrlResult
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public PNGenerateFileUploadUrlData FileUploadRequest { get; set; }
    }
}
