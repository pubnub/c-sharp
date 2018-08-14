using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class TelemetryManager: IDisposable
    {
        private const int TELEMETRY_TIMER_IN_SEC = 60;

        private PNConfiguration pubnubConfig;
        private IPubnubLog pubnubLog;

        private static ConcurrentDictionary<string, ConcurrentDictionary<double, long>> dicEndpointLatency
        {
            get;
            set;
        } = new ConcurrentDictionary<string, ConcurrentDictionary<double, long>>();

        private System.Threading.Timer telemetryTimer { get; set; }

        public TelemetryManager(PNConfiguration config, IPubnubLog log)
        {
            this.pubnubConfig = config;
            this.pubnubLog = log;
            if (config != null && config.EnableTelemetry)
            {
                StartTelemetryTimer();
            }
        }

        private void StartTelemetryTimer()
        {
            StopTelemetryTimer();
            telemetryTimer = new Timer(OnTelemetryIntervalTimeout, null, 0, TELEMETRY_TIMER_IN_SEC * 1000);
        }

        private void OnTelemetryIntervalTimeout(System.Object telemetryState)
        {
            CleanupTelemetryData();
        }

        private void StopTelemetryTimer()
        {
            try
            {
                if (telemetryTimer != null)
                {
                    telemetryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    telemetryTimer.Dispose();
                }
            }
            catch {  /* Ignore exception caused by dispose */  }
            finally { telemetryTimer = null; }
        }

        private static string EndpointNameForOperation(PNOperationType type)
        {
            string endpoint = "";
            switch (type)
            {
                case PNOperationType.PNPublishOperation:
                    endpoint = "l_pub";
                    break;
                case PNOperationType.PNHistoryOperation:
                case PNOperationType.PNFireOperation:
                case PNOperationType.PNDeleteMessageOperation:
                    endpoint = "l_hist";
                    break;
                case PNOperationType.PNUnsubscribeOperation:
                case PNOperationType.PNWhereNowOperation:
                case PNOperationType.PNHereNowOperation:
                case PNOperationType.PNHeartbeatOperation:
                case PNOperationType.PNSetStateOperation:
                case PNOperationType.PNGetStateOperation:
                    endpoint = "l_pres";
                    break;
                case PNOperationType.PNAddChannelsToGroupOperation:
                case PNOperationType.PNRemoveChannelsFromGroupOperation:
                case PNOperationType.PNChannelGroupsOperation:
                case PNOperationType.PNRemoveGroupOperation:
                case PNOperationType.PNChannelsForGroupOperation:
                    endpoint = "l_cg";
                    break;
                case PNOperationType.PushGet:
                case PNOperationType.PushRegister:
                case PNOperationType.PushRemove:
                case PNOperationType.PushUnregister:
                    endpoint = "l_push";
                    break;
                case PNOperationType.PNAccessManagerAudit:
                case PNOperationType.PNAccessManagerGrant:
                    endpoint = "l_pam";
                    break;
                case PNOperationType.PNTimeOperation:
                    endpoint = "l_time";
                    break;
                default:
                    endpoint = "";
                    break;
            }

            return endpoint;
        }

        private static readonly object operationLatencyDataLock = new object();
        public async Task StoreLatency(long latencyMillisec, PNOperationType type)
        {
            await Task.Factory.StartNew(() => 
            {
                try
                {
                    string latencyEndPoint = EndpointNameForOperation(type);
                    if (latencyMillisec > 0 && !string.IsNullOrEmpty(latencyEndPoint))
                    {
                        if (dicEndpointLatency == null)
                        {
                            dicEndpointLatency = new ConcurrentDictionary<string, ConcurrentDictionary<double, long>>();
                        }

                        double epochMillisec = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        if (dicEndpointLatency.ContainsKey(latencyEndPoint) && dicEndpointLatency[latencyEndPoint] != null && dicEndpointLatency[latencyEndPoint].Keys.Count > 0)
                        {
                            if (epochMillisec - dicEndpointLatency[latencyEndPoint].Keys.Max() > 500)
                            {
                                lock (operationLatencyDataLock)
                                {
                                    dicEndpointLatency[latencyEndPoint].AddOrUpdate(epochMillisec, latencyMillisec, (key, oldValue) => latencyMillisec);
                                }
                            }
                        }
                        else
                        {
                            lock (operationLatencyDataLock)
                            {
                                ConcurrentDictionary<double, long> elapsedInfo = new ConcurrentDictionary<double, long>();
                                elapsedInfo.AddOrUpdate(epochMillisec, latencyMillisec, (o, n) => latencyMillisec);
                                dicEndpointLatency.AddOrUpdate(latencyEndPoint, elapsedInfo, (o, n) => elapsedInfo);
                            }
                        }
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - StoreLatency {1} latency = {2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), type, latencyMillisec), pubnubConfig.LogVerbosity);
                    }
                }
                catch (Exception ex)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - StoreLatency error: {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), pubnubConfig.LogVerbosity);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
        }

        public async Task<Dictionary<string, string>> GetOperationsLatency()
        {
            return await Task<Dictionary<string,string>>.Factory.StartNew(() => 
                {
                    Dictionary<string, string> dictionaryOpsLatency = new Dictionary<string, string>();
                    try
                    {
                        lock (operationLatencyDataLock)
                        {
                            if (dicEndpointLatency != null)
                            {
                                foreach (string key in dicEndpointLatency.Keys)
                                {
                                    if (dicEndpointLatency[key] != null && dicEndpointLatency[key].Count > 0)
                                    {

                                        dictionaryOpsLatency.Add(key, Math.Round(((double)dicEndpointLatency[key].Average(kvp => kvp.Value) / 1000.0), 10).ToString(CultureInfo.InvariantCulture)); //Convert millisec to sec
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - GetOperationsLatency error: {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), pubnubConfig.LogVerbosity);
                    }
                    return dictionaryOpsLatency;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ConfigureAwait(false);
        }

        private void CleanupTelemetryData()
        {
            Task.Factory.StartNew(() => 
            {
                lock (operationLatencyDataLock)
                {
                    try
                    {
                        double currentEpochMillisec = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        string[] latencyOpKeys = (dicEndpointLatency != null) ? dicEndpointLatency.Keys.ToArray<string>() : new string[]{ };
                        for (int keyIndex = 0; keyIndex < latencyOpKeys.Length; keyIndex++)
                        {
                            string opKey = latencyOpKeys[keyIndex];
                            ConcurrentDictionary<double, long> outdatedLatencyValue = null;
                            dicEndpointLatency.TryGetValue(opKey, out outdatedLatencyValue);
                            if (outdatedLatencyValue != null)
                            {
                                IEnumerable<KeyValuePair<double, long>> enumerableOutdatedLatencies = outdatedLatencyValue.Where(dt => currentEpochMillisec - dt.Key >= 60000);
                                if (enumerableOutdatedLatencies != null)
                                {
                                    Dictionary<double, long> dicOutdatedLatencies = enumerableOutdatedLatencies.ToDictionary(item => item.Key, item => item.Value);
                                    if (dicOutdatedLatencies != null && dicOutdatedLatencies.Count > 0)
                                    {
                                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - CleanupTelemetryData => {1} dicOutdatedLatencies count = {2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), opKey, dicOutdatedLatencies.Count), pubnubConfig.LogVerbosity);
                                        double[] outLatencyKeys = dicOutdatedLatencies.Keys.ToArray<double>();
                                        for (int outdateIndex = 0; outdateIndex < outLatencyKeys.Length; outdateIndex++)
                                        {
                                            double outKey = outLatencyKeys[outdateIndex];
                                            ConcurrentDictionary<double, long> currentEndPointLatency = null;
                                            dicEndpointLatency.TryGetValue(opKey, out currentEndPointLatency);
                                            if (currentEndPointLatency != null && currentEndPointLatency.ContainsKey(outKey))
                                            {
                                                long removeOutdatedLatency;
                                                if (!currentEndPointLatency.TryRemove(outKey, out removeOutdatedLatency))
                                                {
                                                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - CleanupTelemetryData => removed failed for key = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), outKey), pubnubConfig.LogVerbosity);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - CleanupTelemetryData => Exception = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), pubnubConfig.LogVerbosity);
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);

        }

        public void Destroy()
        {
            StopTelemetryTimer();
            dicEndpointLatency.Clear();
            dicEndpointLatency = null;
            pubnubConfig = null;
            pubnubLog = null;
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                dicEndpointLatency.Clear();
                pubnubConfig = null;
                pubnubLog = null;
                if (telemetryTimer != null)
                {
                    telemetryTimer.Dispose();
                    telemetryTimer = null;
                }

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

