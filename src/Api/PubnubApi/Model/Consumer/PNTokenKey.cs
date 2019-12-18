using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PubnubApi
{
    public class PNTokenKey
    {
        [JsonProperty("ResourceType ")]
        public string ResourceType { get; set; }

        [JsonProperty("ResourceId ")]
        public string ResourceId { get; set; }

        [JsonProperty("PatternFlag ")]
        public int PatternFlag { get; set; }

        public override bool Equals(object obj)
        {
            PNTokenKey currentKey = obj as PNTokenKey;
            if (currentKey == null)
            {
                return false;
            }
            return currentKey.ResourceType == this.ResourceType && currentKey.ResourceId == this.ResourceId && currentKey.PatternFlag == this.PatternFlag;
        }

        public override int GetHashCode() => ResourceType.GetHashCode() ^ PatternFlag.GetHashCode() ^ ResourceId.GetHashCode();
    }
}
