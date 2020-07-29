using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNHeartbeatResult
    {
        public int Status { get; internal set; }
        public string Message { get; internal set; } = "";
    }
}
