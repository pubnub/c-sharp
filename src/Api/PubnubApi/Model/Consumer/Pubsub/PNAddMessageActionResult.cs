using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNAddMessageActionResult
    {
        public long MessageTimetoken { get; internal set; }
        public long ActionTimetoken { get; internal set; }
        public PNMessageAction Action { get; internal set; }
        public string Uuid { get; internal set; }
    }
}
