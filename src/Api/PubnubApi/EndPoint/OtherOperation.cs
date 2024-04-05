﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    internal class OtherOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        public OtherOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public void ChangeUserId(UserId newUserId)
        {
            if (newUserId == null || string.IsNullOrEmpty(newUserId.ToString().Trim()) || config.UserId.ToString() == newUserId.ToString())
            {
                return;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    config.UserId = newUserId;
                    CurrentUserId.AddOrUpdate(PubnubInstance.InstanceId, newUserId, (k, o) => newUserId);
                    UserIdChanged.AddOrUpdate(PubnubInstance.InstanceId, true, (k, o) => true);


                    string[] channels = GetCurrentSubscriberChannels();
                    string[] channelGroups = GetCurrentSubscriberChannelGroups();

                    channels = (channels != null) ? channels : new string[] { };
                    channelGroups = (channelGroups != null) ? channelGroups : new string[] { };

                    if (channels.Length > 0 || channelGroups.Length > 0)
                    {
                        string channelsJsonState = BuildJsonUserState(channels.ToArray(), channelGroups.ToArray(), false);
                        IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

                        Uri request = urlBuilder.BuildMultiChannelLeaveRequest("GET", "", channels, channelGroups, channelsJsonState, null);

                        RequestState<string> requestState = new RequestState<string>();
                        requestState.Channels = channels;
                        requestState.ChannelGroups = channelGroups;
                        requestState.ResponseType = PNOperationType.Leave;
                        requestState.Reconnect = false;

                        UrlProcessRequest(request, requestState, false).ContinueWith(r => { }, TaskContinuationOptions.ExecuteSynchronously).Wait(); // connectCallback = null
                    }

                    TerminateCurrentSubscriberRequest();
                }
                catch {  /* ignore */ }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
        }

        public UserId GetCurrentUserId()
        {
            UserId ret;
            if (CurrentUserId.TryGetValue(PubnubInstance.InstanceId, out ret))
            {
                return ret;
            }
            return ret;
        }
        public static long TranslateDateTimeToSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
            return timeStamp;
        }

        /// <summary>
        /// Convert the UTC/GMT DateTime to Unix Nano Seconds format
        /// </summary>
        /// <param name="dotNetUTCDateTime"></param>
        /// <returns></returns>
        public static long TranslateDateTimeToPubnubUnixNanoSeconds(DateTime dotNetUTCDateTime)
        {
            TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds * 10000000);
            return timeStamp;
        }

        /// <summary>
        /// Convert the Unix Nano Seconds format time to UTC/GMT DateTime
        /// </summary>
        /// <param name="unixNanoSecondTime"></param>
        /// <returns></returns>
        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(long unixNanoSecondTime)
        {
            try
            {
                double timeStamp = (double)unixNanoSecondTime / 10000000;
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp);
                return dateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(string unixNanoSecondTime)
        {
            long numericTime;
            bool tried = Int64.TryParse(unixNanoSecondTime, out numericTime);
            if (tried && numericTime != 0)
            {
                try
                {
                    double timeStamp = (double)numericTime / 10000000;
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp);
                    return dateTime;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public List<string> GetSubscribedChannels()
        {
            List<string> ret = null;
            string[] currentSubscribedChannels = GetCurrentSubscriberChannels();
            if (currentSubscribedChannels != null)
            {
                ret = currentSubscribedChannels.ToList();
            }

            return ret;
        }

        public List<string> GetSubscribedChannelGroups()
        {
            List<string> ret = null;
            string[] currentSubscribedChannelGroups = GetCurrentSubscriberChannelGroups();
            if (currentSubscribedChannelGroups != null)
            {
                ret = currentSubscribedChannelGroups.ToList();
            }

            return ret;
        }

        public void Destory()
        {
            EndPendingRequests();
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
        }
    }
}
