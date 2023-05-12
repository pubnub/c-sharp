using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

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
            dedupHasher = new PubnubCrypto(null, null, null, null);
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
                long keyValue;
                if (!HashHistory.TryRemove(HashHistory.Aggregate((l,r)=> l.Value < r.Value ? l : r).Key, out keyValue))
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} DuplicationManager => AddEntry => TryRemove is false", DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture)), PNLogVerbosity.BODY);
                }
            }
            HashHistory.TryAdd(this.GetSubscribeMessageHashKey(message),Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow));
        }

        public void ClearHistory()
        {
            HashHistory.Clear();
        }
    }
}
