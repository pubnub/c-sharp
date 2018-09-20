using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EndPoint
{
    internal class DuplicationManager
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly IPubnubLog pubnubLog;
        private readonly PubnubCrypto dedupHasher;
        private static ConcurrentDictionary<string, long> HashHistory { get; } = new ConcurrentDictionary<string, long>();

        public DuplicationManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log)
        {
            this.pubnubConfig = config;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubLog = log;
            dedupHasher = new PubnubCrypto(null, null, null);
        }

        private string GetSubscribeMessageHashKey(SubscribeMessage message)
        {
            
            return dedupHasher.GetHashRaw(jsonLib.SerializeToJsonString(message));
        }

        public bool IsDuplicate(SubscribeMessage message)
        {
            return HashHistory.ContainsKey(this.GetSubscribeMessageHashKey(message));
        }

        public void AddEntry(SubscribeMessage message)
        {
            if (HashHistory.Count >= pubnubConfig.MaximumMessagesCacheSize)
            {
                HashHistory.Remove(HashHistory.Aggregate((l,r)=> l.Value < r.Value ? l : r).Key);
            }
            HashHistory.Add(this.GetSubscribeMessageHashKey(message),Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow));
        }

        public void ClearHistory()
        {
            HashHistory.Clear();
        }
    }
}
