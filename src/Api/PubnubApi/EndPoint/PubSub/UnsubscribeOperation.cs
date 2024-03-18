﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Subscribe;
using PubnubApi.EventEngine.Subscribe.Events;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class UnsubscribeOperation<T> : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private string[] subscribeChannelNames;
        private string[] subscribeChannelGroupNames;
        private Dictionary<string, object> queryParam;
        private SubscribeEventEngineFactory subscribeEventEngineFactory;

        public UnsubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance, SubscribeEventEngineFactory subscribeEventEngineFactory) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;
            this.subscribeEventEngineFactory = subscribeEventEngineFactory;
        }

        public UnsubscribeOperation<T> Channels(string[] channels)
        {
            this.subscribeChannelNames = channels;
            return this;
        }

        public UnsubscribeOperation<T> ChannelGroups(string[] channelGroups)
        {
            this.subscribeChannelGroupNames = channelGroups;
            return this;
        }

        public UnsubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute()
        {
            if (config.EnableEventEngine && subscribeEventEngineFactory != null) {
                if (subscribeEventEngineFactory.hasEventEngine(PubnubInstance.InstanceId)) {
                    var subscribeEventEngine = subscribeEventEngineFactory.getEventEngine(PubnubInstance.InstanceId);
                    subscribeEventEngine.Channels = subscribeEventEngine.Channels.Except(this.subscribeChannelNames).ToArray();
                    subscribeEventEngine.Channelgroups = subscribeEventEngine.Channelgroups.Except(this.subscribeChannelGroupNames).ToArray();
                    subscribeEventEngine.eventQueue.Enqueue(new SubscriptionChangedEvent() {
                        Channels = subscribeEventEngine.Channels, ChannelGroups = subscribeEventEngine.Channelgroups
                    });
                    if (ChannelLocalUserState.TryGetValue(PubnubInstance.InstanceId, out var userState)) {
                        foreach (var channel in this.subscribeChannelNames ?? new string[0]) {
                            userState.TryRemove(channel, out _);
                        }
                    }
                    if (ChannelGroupLocalUserState.TryGetValue(PubnubInstance.InstanceId, out var channelGroupUserState)) {
                        foreach (var channelGroup in this.subscribeChannelGroupNames ?? new string[0]) {
                            channelGroupUserState.TryRemove(channelGroup, out _);
                        }
                    }
                }
            }
            else {
                Unsubscribe(subscribeChannelNames, subscribeChannelGroupNames);
            }
            
        }

        private void Unsubscribe(string[] channels, string[] channelGroups)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            string channel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, requested unsubscribe for channel(s)={1}, cg(s)={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel, channelGroup), config.LogVerbosity);

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
                manager.CurrentPubnubInstance(PubnubInstance);
                manager.MultiChannelUnSubscribeInit<T>(PNOperationType.PNUnsubscribeOperation, channel, channelGroup, this.queryParam);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr, PubnubInstance);
                manager.CurrentPubnubInstance(PubnubInstance);
                manager.MultiChannelUnSubscribeInit<T>(PNOperationType.PNUnsubscribeOperation, channel, channelGroup, this.queryParam);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

            if (!MultiChannelSubscribe.ContainsKey(instance.InstanceId))
            {
                MultiChannelSubscribe.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, long>());
            }
            if (!MultiChannelGroupSubscribe.ContainsKey(instance.InstanceId))
            {
                MultiChannelGroupSubscribe.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, long>());
            }
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
