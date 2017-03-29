using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PubnubApi
{
    public class PNAccessManagerKeysData
    {
        public Dictionary<string, PNAccessManagerKeyData> AuthKeys { get; set; }
    }
}
