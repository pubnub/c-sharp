using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EndPoint
{
    internal class DuplicationManager
    {
        private PNConfiguration pubnubConfig;
        private IPubnubLog pubnubLog;
        private SortedDictionary<string,bool> hashHistory;

        public DuplicationManager(PNConfiguration config, IPubnubLog log)
        {
            this.pubnubConfig = config;
            this.pubnubLog = log;
            this.hashHistory = new SortedDictionary<string, bool>();
        }

        private string GetSubscribeMessageHashKey(SubscribeMessage message)
        {
            return string.Format("{0}{1}", message.Channel, message.Payload.GetHashCode().ToString());
        }

        public bool IsDuplicate(SubscribeMessage message)
        {
            return hashHistory.ContainsKey(this.GetSubscribeMessageHashKey(message));
        }

        public void AddEntry(SubscribeMessage message)
        {
            if (hashHistory.Count >= pubnubConfig.MaximumMessagesCacheSize)
            {
                hashHistory.Remove(hashHistory.First().Key);
            }
            hashHistory.Add(this.GetSubscribeMessageHashKey(message),true);
        }

        public void ClearHistory()
        {
            this.hashHistory.Clear();
        }
    }
}
