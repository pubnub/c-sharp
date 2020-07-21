using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNListFilesResult
    {
        public List<PNFileResult> FilesList { get; set; }
        public int Count { get; set; }
        public string Next { get; set; }
    }
}
