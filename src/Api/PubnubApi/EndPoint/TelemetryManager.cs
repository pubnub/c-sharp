using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PubnubApi.Interface;
using System.Net;
using System.Threading.Tasks;
using System.Collections;

namespace PubnubApi.EndPoint
{
    public class TelemetryManager
    {
        private const int TELEMETRY_TIMER_IN_SEC = 60;

        private PNConfiguration pubnubConfig = null;
        private IPubnubLog pubnubLog = null;

        private static Dictionary<string, ConcurrentDictionary<double, long>> dicEndpointLatency
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<double, long>>();

        private System.Threading.Timer telemetryTimer { get; set; } = null;

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
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - OnTelemetryIntervalTimeout => CleanupTelemetryData", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
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
            catch { }
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
                //case PNOperationType.PNPushNotificationEnabledChannelsOperation:
                //case PNOperationType.PNAddPushNotificationsOnChannelsOperation:
                //case PNOperationType.PNRemovePushNotificationsFromChannelsOperation:
                //case PNOperationType.PNRemoveAllPushNotificationsOperation:
                //    endpoint = "push";
                //    break;
                case PNOperationType.PNAccessManagerAudit:
                case PNOperationType.PNAccessManagerGrant:
                    endpoint = "l_pam";
                    break;
                case PNOperationType.PNTimeOperation:
                    endpoint = "l_time";
                    break;
                default:
                    break;
            }

            return endpoint;
        }

        public void StoreLatency(long latencyMillisec, PNOperationType type)
        {
            try
            {
                string latencyEndPoint = EndpointNameForOperation(type);
                if (latencyMillisec > 0 && !string.IsNullOrEmpty(latencyEndPoint))
                {
                    double epochMillisec = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    if (dicEndpointLatency.ContainsKey(latencyEndPoint))
                    {
                        dicEndpointLatency[latencyEndPoint].AddOrUpdate(epochMillisec, latencyMillisec, (key, oldValue) => latencyMillisec);
                    }
                    else
                    {
                        ConcurrentDictionary<double, long> elapsedInfo = new ConcurrentDictionary<double, long>();
                        elapsedInfo.Add(epochMillisec, latencyMillisec);
                        dicEndpointLatency.Add(latencyEndPoint, elapsedInfo);
                    }
                    //private string epoch2string(int epoch) { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch).ToShortDateString();
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} latency = {1}", type, latencyMillisec));
                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - StoreLatency error: {1}", DateTime.Now.ToString(), ex.ToString()), pubnubConfig.LogVerbosity);
            }
        }

        public Dictionary<string, string> GetOperationsLatency()
        {
            Dictionary<string, string> dictionaryOpsLatency = new Dictionary<string, string>();
            foreach (string key in dicEndpointLatency.Keys)
            {
                if (dicEndpointLatency[key] != null && dicEndpointLatency[key].Count > 0)
                {
                    
                    dictionaryOpsLatency.Add(key, Math.Round(((double)dicEndpointLatency[key].Average(kvp => kvp.Value) / (double)1000.0), 10).ToString()); //Convert millisec to sec
                }
            }
            return dictionaryOpsLatency;
        }

        private void CleanupTelemetryData()
        {
            double currentEpochMillisec = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            foreach (string key in dicEndpointLatency.Keys)
            {
                List<KeyValuePair<double, long>> outdatedLatencies = dicEndpointLatency[key].Where(dt => currentEpochMillisec - dt.Key >= 60000).ToList();
                if (outdatedLatencies != null && outdatedLatencies.Count > 0)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - CleanupTelemetryData => {1} outdatedLatencies count = {2}", DateTime.Now.ToString(), key, outdatedLatencies.Count), pubnubConfig.LogVerbosity);
                    try
                    {
                        foreach (KeyValuePair<double, long> kvp in outdatedLatencies)
                        {
                            if (dicEndpointLatency[key].ContainsKey(kvp.Key))
                            {
                                long removeOutdatedLatency;
                                if (!dicEndpointLatency[key].TryRemove(kvp.Key, out removeOutdatedLatency))
                                {
                                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - CleanupTelemetryData => removed failed for key = {1}", DateTime.Now.ToString(), kvp.Key), pubnubConfig.LogVerbosity);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - CleanupTelemetryData => Exception = {1}", DateTime.Now.ToString(), ex.ToString()), pubnubConfig.LogVerbosity);
                    }
                }
            }
        }

        public void Destroy()
        {
            StopTelemetryTimer();
            dicEndpointLatency.Clear();
            dicEndpointLatency = null;
        }
    }
}

