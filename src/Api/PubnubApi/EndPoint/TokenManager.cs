using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EndPoint
{
    public class TokenManager : IDisposable
    {
        private PNConfiguration pubnubConfig;
        private IPubnubLog pubnubLog;

        private static ConcurrentDictionary<string, ConcurrentDictionary<double, long>> dicEndpointLatency
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<double, long>>();

        public TokenManager(PNConfiguration config, IPubnubLog log)
        {
            this.pubnubConfig = config;
            this.pubnubLog = log;
            if (config != null && config.StoreTokensOnGrant)
            {
                //StartTelemetryTimer();
            }
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                //dicEndpointLatency.Clear();
                pubnubConfig = null;
                pubnubLog = null;
                //if (telemetryTimer != null)
                //{
                //    telemetryTimer.Dispose();
                //    telemetryTimer = null;
                //}

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            DisposeInternal(true);
        }
        #endregion

    }
}
