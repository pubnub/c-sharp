using System;
using System.Linq;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif
using PubnubApi.Security.Crypto.Common;

namespace PubnubApi.EndPoint
{
    internal class DuplicationManager
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib;
        private static ConcurrentDictionary<string, long> HashHistory { get; } = new ConcurrentDictionary<string, long>();

        public DuplicationManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
        }

        private string GetSubscribeMessageHashKey(SubscribeMessage message)
        {
            return Util.GetHashRaw(jsonLib.SerializeToJsonString(message));
        }

        public bool IsDuplicate(SubscribeMessage message)
        {
            return HashHistory.ContainsKey(GetSubscribeMessageHashKey(message));
        }

        public void AddEntry(SubscribeMessage message)
        {
            if (HashHistory.Count >= pubnubConfig.MaximumMessagesCacheSize)
            {
                long keyValue;
                if (!HashHistory.TryRemove(HashHistory.Aggregate((l,r)=> l.Value < r.Value ? l : r).Key, out keyValue))
                {
                    pubnubConfig?.Logger?.Debug("DuplicationManager AddEntry TryRemove False");
                }
            }
            HashHistory.TryAdd(GetSubscribeMessageHashKey(message),Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow));
        }

        public void ClearHistory()
        {
            HashHistory.Clear();
        }
    }
}