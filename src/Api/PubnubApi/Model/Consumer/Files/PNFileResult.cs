using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNFileResult
    {
        public string Name { get; internal set; }
        public string Id { get; internal set; }
        public int Size { get; internal set; }
        public string Created { get; internal set; }
    }
}
