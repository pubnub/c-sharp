using System;
using Newtonsoft.Json;

namespace PubnubApi
{
    public class PNAccessManagerKeyData
    {
        public bool ReadEnabled { get; set; }

        public bool WriteEnabled { get; set; }

        public bool ManageEnabled { get; set; }
    }
}
