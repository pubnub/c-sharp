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

        private Dictionary<string, ConcurrentDictionary<double, long>> dicEndpointLatency
        {
            get;
            set;
        } = new Dictionary<string, ConcurrentDictionary<double, long>>();

        private System.Threading.Timer telemetryTimer { get; set; } = null;

        public TelemetryManager(PNConfiguration config, IPubnubLog log)
        {
            this.pubnubConfig = config;
            this.pubnubLog = log;
        }

        private void StartTelemetryTimer()
        {
            StopTelemetryTimer();
            telemetryTimer = new Timer(OnTelemetryIntervalTimeout, null, 0, TELEMETRY_TIMER_IN_SEC * 1000);
        }

        private void OnTelemetryIntervalTimeout(System.Object telemetryState)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - OnTelemetryIntervalTimeout", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);

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
                    endpoint = "pub";
                    break;
                case PNOperationType.PNHistoryOperation:
                case PNOperationType.PNFireOperation:
                case PNOperationType.PNDeleteMessageOperation:
                    endpoint = "hist";
                    break;
                case PNOperationType.PNUnsubscribeOperation:
                case PNOperationType.PNWhereNowOperation:
                case PNOperationType.PNHereNowOperation:
                case PNOperationType.PNHeartbeatOperation:
                case PNOperationType.PNSetStateOperation:
                case PNOperationType.PNGetStateOperation:
                    endpoint = "pres";
                    break;
                case PNOperationType.PNAddChannelsToGroupOperation:
                case PNOperationType.PNRemoveChannelsFromGroupOperation:
                case PNOperationType.PNChannelGroupsOperation:
                case PNOperationType.PNRemoveGroupOperation:
                case PNOperationType.PNChannelsForGroupOperation:
                    endpoint = "cg";
                    break;
                //case PNOperationType.PNPushNotificationEnabledChannelsOperation:
                //case PNOperationType.PNAddPushNotificationsOnChannelsOperation:
                //case PNOperationType.PNRemovePushNotificationsFromChannelsOperation:
                //case PNOperationType.PNRemoveAllPushNotificationsOperation:
                //    endpoint = "push";
                //    break;
                case PNOperationType.PNAccessManagerAudit:
                case PNOperationType.PNAccessManagerGrant:
                    endpoint = "pam";
                    break;
                case PNOperationType.PNTimeOperation:
                    endpoint = "time";
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
                if (latencyMillisec > 0 && !string.IsNullOrEmpty(latencyEndPoint) && dicEndpointLatency.ContainsKey(latencyEndPoint))
                {
                    double epochMillisec = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                    dicEndpointLatency[latencyEndPoint].AddOrUpdate(epochMillisec, latencyMillisec, (key, oldValue) => latencyMillisec);
                    //private string epoch2string(int epoch) { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch).ToShortDateString();
                }
                System.Diagnostics.Debug.WriteLine(string.Format("{0} latency = {1}", type, latencyMillisec));

            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, TelemetryManager - StoreLatency error: {1}", DateTime.Now.ToString(), ex.ToString()), pubnubConfig.LogVerbosity);
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

