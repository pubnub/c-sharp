using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMessageActionItem
    {
        public long MessageTimetoken { get; set; }
        public PNMessageAction Action { get; set; }
        public string Uuid { get; set; }
        public long ActionTimetoken { get; set; }
    }
}
