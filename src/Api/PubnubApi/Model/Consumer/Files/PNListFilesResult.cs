using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNListFilesResult
    {
        public List<PNFileResult> FilesList { get; internal set; }
        public int Count { get; internal set; }
        public string Next { get; internal set; }
    }
}
