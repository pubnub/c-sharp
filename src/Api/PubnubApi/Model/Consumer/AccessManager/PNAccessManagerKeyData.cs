using System;

namespace PubnubApi
{
    public class PNAccessManagerKeyData
    {
        public bool ReadEnabled { get; internal set; }

        public bool WriteEnabled { get; internal set; }

        public bool ManageEnabled { get; internal set; }

        public bool DeleteEnabled { get; internal set; }

        public bool CreateEnabled { get; internal set; }
    }
}
