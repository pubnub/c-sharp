using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Collections.Concurrent;


namespace PubnubApi.EndPoint
{
	public class SubscribeOperation<T> : PubnubCoreBase, ISubscribeOperation<T>
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly TokenManager pubnubTokenManager;

        private List<string> subscribeChannelNames = new List<string>();
        private List<string> subscribeChannelGroupNames = new List<string>();
        private long subscribeTimetoken = -1;
        private bool presenceSubscribeEnabled;
        private SubscribeManager manager;
        private Dictionary<string, object> queryParam;

        public SubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubTokenManager = tokenManager;

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
                ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, CancellationTokenSource>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelLocalUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupLocalUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelUserState.ContainsKey(instance.InstanceId))
            {
                ChannelUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelReconnectTimer.ContainsKey(instance.InstanceId))
            {
                ChannelReconnectTimer.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Timer>());
            }
            if (!ChannelGroupReconnectTimer.ContainsKey(instance.InstanceId))
            {
                ChannelGroupReconnectTimer.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Timer>());
            }
            if (!SubscribeDisconnected.ContainsKey(instance.InstanceId))
            {
                SubscribeDisconnected.GetOrAdd(instance.InstanceId, false);
            }
            if (!LastSubscribeTimetoken.ContainsKey(instance.InstanceId))
            {
                LastSubscribeTimetoken.GetOrAdd(instance.InstanceId, 0);
            }
            if (!LastSubscribeRegion.ContainsKey(instance.InstanceId))
            {
                LastSubscribeRegion.GetOrAdd(instance.InstanceId, 0);
            }
            if (!SubscribeRequestTracker.ContainsKey(instance.InstanceId))
            {
                SubscribeRequestTracker.GetOrAdd(instance.InstanceId, DateTime.Now);
            }
            if (!SubscriptionChannels.ContainsKey(instance.InstanceId))
            {
                SubscriptionChannels.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!SubscriptionChannelGroups.ContainsKey(instance.InstanceId))
            {
                SubscriptionChannelGroups.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }

        }

        public List<SubscribeCallback> SubscribeListenerList
        {
            get;
            set;
        } = new List<SubscribeCallback>();

        public ISubscribeOperation<T> Channels(string[] channels)
        {
            if (channels != null && channels.Length > 0 && !string.IsNullOrEmpty(channels[0]))
            {
                subscribeChannelNames.AddRange(channels);
            }
            return this;
        }

        public ISubscribeOperation<T> ChannelGroups(string[] channelGroups)
        {
            if (channelGroups != null && channelGroups.Length > 0 && !string.IsNullOrEmpty(channelGroups[0]))
            {
                subscribeChannelGroupNames.AddRange(channelGroups);
            }
            return this;
        }

        public ISubscribeOperation<T> WithTimetoken(long timetoken)
        {
            subscribeTimetoken = timetoken;
            return this;
        }

        public ISubscribeOperation<T> WithPresence()
        {
            presenceSubscribeEnabled = true;
            return this;
        }

        public ISubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public void Execute()
        {
            logger?.Debug($"PUBNUB: Subscribe request Execute()received with tt={subscribeTimetoken}, channels = {string.Join(", ", subscribeChannelNames)}");
            if (config == null || string.IsNullOrEmpty(config.SubscribeKey))
            {
                throw new MissingMemberException("Invalid Subscribe key");
            }
            if (subscribeChannelNames == null)
            {
                subscribeChannelNames = new List<string>();
            }

            if (subscribeChannelGroupNames == null)
            {
                subscribeChannelGroupNames = new List<string>();
            }

            if (presenceSubscribeEnabled)
            {
                List<string> presenceChannelNames = (subscribeChannelNames != null && subscribeChannelNames.Count > 0 && !string.IsNullOrEmpty(subscribeChannelNames[0])) 
                                                ? subscribeChannelNames.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", c)).ToList() : new List<string>();
                List<string> presenceChannelGroupNames = (subscribeChannelGroupNames != null && subscribeChannelGroupNames.Count > 0 && !string.IsNullOrEmpty(subscribeChannelGroupNames[0])) 
                                                ? subscribeChannelGroupNames.Select(c => string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", c)).ToList() : new List<string>();

                if (subscribeChannelNames != null && presenceChannelNames.Count > 0)
                {
                    subscribeChannelNames.AddRange(presenceChannelNames);
                }

                if (subscribeChannelGroupNames != null && presenceChannelGroupNames.Count > 0)
                {
                    subscribeChannelGroupNames.AddRange(presenceChannelGroupNames);
                }
            }

            string[] channelNames = subscribeChannelNames != null ? subscribeChannelNames.ToArray() : null;
            string[] channelGroupNames = subscribeChannelGroupNames != null ? subscribeChannelGroupNames.ToArray() : null;

            Subscribe(channelNames, channelGroupNames, queryParam);
        }

        private void Subscribe(string[] channels, string[] channelGroups, Dictionary<string, object> externalQueryParam)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length  == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            string channel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            config.Logger?.Debug($"requested subscribe for channel(s)={channel} and channel group(s)={channelGroup}");

            Dictionary<string, string> initialSubscribeUrlParams = new Dictionary<string, string>();
            if (subscribeTimetoken >= 0)
            {
                logger?.Debug($"SubscribeOperation subscribe timetoken= {subscribeTimetoken}");
                initialSubscribeUrlParams.Add("tt", subscribeTimetoken.ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrEmpty(config.FilterExpression) && config.FilterExpression.Trim().Length > 0)
            {
                initialSubscribeUrlParams.Add("filter-expr", UriUtil.EncodeUriComponent(config.FilterExpression, PNOperationType.PNSubscribeOperation, false, false, false));
            }
            
            manager = new SubscribeManager(config, jsonLibrary, unit, pubnubTokenManager, PubnubInstance);
            manager.CurrentPubnubInstance(PubnubInstance);
            manager.MultiChannelSubscribeInit<T>(PNOperationType.PNSubscribeOperation, channels??[], channelGroups??[], initialSubscribeUrlParams, externalQueryParam);
        }

        internal bool Retry(bool reconnect)
        {
            if (manager == null)
            {
                return false;
            }

            if (reconnect)
            {
                return manager.Reconnect<T>(false);
            }
            else
            {
                return manager.Disconnect();
            }
        }

        internal bool Retry(bool reconnect, bool resetSubscribeTimetoken)
        {
            if (manager == null)
            {
                return false;
            }

            if (reconnect)
            {
                return manager.Reconnect<T>(resetSubscribeTimetoken);
            }
            else
            {
                return manager.Disconnect();
            }
        }

    }
}
