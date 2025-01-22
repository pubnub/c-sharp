using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly IPubnubLog pubnubLog;
        private static ConcurrentDictionary<string, long> HashHistory { get; } = new ConcurrentDictionary<string, long>();

        public DuplicationManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log)
        {
            this.pubnubConfig = config;
            this.jsonLib = jsonPluggableLibrary;
            this.pubnubLog = log;
        }

        private string GetSubscribeMessageHashKey(SubscribeMessage message)
        {
            return Util.GetHashRaw(jsonLib.SerializeToJsonString(message));
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
                    LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] DuplicationManager => AddEntry => TryRemove is false",  PNLogVerbosity.BODY);
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