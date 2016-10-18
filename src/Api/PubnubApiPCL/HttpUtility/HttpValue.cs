using System;

namespace PubnubApi
{
    public sealed class HttpValue
    {
        public HttpValue()
        {
        }

        public HttpValue(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
