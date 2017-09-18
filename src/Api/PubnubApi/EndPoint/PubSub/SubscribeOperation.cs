using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Net;

namespace PubnubApi.EndPoint
{
    public class SubscribeOperation<T> : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.TelemetryManager pubnubTelemetryMgr;

        private List<string> subscribeChannelNames = new List<string>();
        private List<string> subscribeChannelGroupNames = new List<string>();
        private List<string> presenceChannelNames = new List<string>();
        private List<string> presenceChannelGroupNames = new List<string>();
        private long subscribeTimetoken = -1;
        private bool presenceSubscribeEnabled = false;
        private SubscribeManager manager = null;

        public SubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public SubscribeOperation<T> Channels(string[] channels)
        {
            if (channels != null && channels.Length > 0 && !string.IsNullOrEmpty(channels[0]))
            {
                this.subscribeChannelNames.AddRange(channels);
            }
            return this;
        }

        public SubscribeOperation<T> ChannelGroups(string[] channelGroups)
        {
            if (channelGroups != null && channelGroups.Length > 0 && !string.IsNullOrEmpty(channelGroups[0]))
            {
                this.subscribeChannelGroupNames.AddRange(channelGroups);
            }
            return this;
        }

        public SubscribeOperation<T> WithTimetoken(long timetoken)
        {
            this.subscribeTimetoken = timetoken;
            return this;
        }

        public SubscribeOperation<T> WithPresence()
        {
            this.presenceSubscribeEnabled = true;
            return this;
        }

        public void Execute()
        {
            if (this.subscribeChannelNames == null)
            {
                this.subscribeChannelNames = new List<string>();
            }

            if (this.subscribeChannelGroupNames == null)
            {
                this.subscribeChannelGroupNames = new List<string>();
            }

            if (this.presenceSubscribeEnabled)
            {
                this.presenceChannelNames = (this.subscribeChannelNames != null && this.subscribeChannelNames.Count > 0 && !string.IsNullOrEmpty(this.subscribeChannelNames[0])) 
                                                ? this.subscribeChannelNames.Select(c => string.Format("{0}-pnpres",c)).ToList() : new List<string>();
                this.presenceChannelGroupNames = (this.subscribeChannelGroupNames != null && this.subscribeChannelGroupNames.Count > 0 && !string.IsNullOrEmpty(this.subscribeChannelGroupNames[0])) 
                                                ? this.subscribeChannelGroupNames.Select(c => string.Format("{0}-pnpres", c)).ToList() : new List<string>();

                if (this.presenceChannelNames.Count > 0)
                {
                    this.subscribeChannelNames.AddRange(this.presenceChannelNames);
                }

                if (this.presenceChannelGroupNames.Count > 0)
                {
                    this.subscribeChannelGroupNames.AddRange(this.presenceChannelGroupNames);
                }
            }

            string[] channelNames = this.subscribeChannelNames.ToArray();
            string[] channelGroupNames = this.subscribeChannelGroupNames.ToArray();

            Subscribe(channelNames, channelGroupNames);
        }

        private void Subscribe(string[] channels, string[] channelGroups)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length  == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            string channel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            PNPlatform.Print(config, pubnubLog);

            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, requested subscribe for channel(s)={1} and channel group(s)={2}", DateTime.Now.ToString(), channel, channelGroup), config.LogVerbosity);

            string[] arrayChannel = new string[] { };
            string[] arrayChannelGroup = new string[] { };

            if (!string.IsNullOrEmpty(channel) && channel.Trim().Length > 0)
            {
                arrayChannel = channel.Trim().Split(',');
            }

            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                arrayChannelGroup = channelGroup.Trim().Split(',');
            }

            Dictionary<string, string> initialSubscribeUrlParams = new Dictionary<string, string>();
            if (this.subscribeTimetoken >= 0)
            {
                initialSubscribeUrlParams.Add("tt", this.subscribeTimetoken.ToString());
            }
            if (!string.IsNullOrEmpty(config.FilterExpression) && config.FilterExpression.Trim().Length > 0)
            {
                initialSubscribeUrlParams.Add("filter-expr", new UriUtil().EncodeUriComponent(config.FilterExpression, PNOperationType.PNSubscribeOperation, false, false));
            }


            Task.Factory.StartNew(() =>
            {
                manager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
                manager.CurrentPubnubInstance(PubnubInstance);
                manager.MultiChannelSubscribeInit<T>(PNOperationType.PNSubscribeOperation, channels, channelGroups, initialSubscribeUrlParams);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry(bool reconnect)
        {
            if (reconnect)
            {
                manager.Reconnect<T>();
            }
            else
            {
                manager.Disconnect<T>();
            }
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
            if (!MultiChannelSubscribe.ContainsKey(instance.InstanceId))
            {
                MultiChannelSubscribe.Add(instance.InstanceId, new ConcurrentDictionary<string, long>());
            }
            if (!MultiChannelGroupSubscribe.ContainsKey(instance.InstanceId))
            {
                MultiChannelGroupSubscribe.Add(instance.InstanceId, new ConcurrentDictionary<string, long>());
            }
            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.Add(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelLocalUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupLocalUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelUserState.ContainsKey(instance.InstanceId))
            {
                ChannelUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelReconnectTimer.ContainsKey(instance.InstanceId))
            {
                ChannelReconnectTimer.Add(instance.InstanceId, new ConcurrentDictionary<string, Timer>());
            }
            if (!ChannelGroupReconnectTimer.ContainsKey(instance.InstanceId))
            {
                ChannelGroupReconnectTimer.Add(instance.InstanceId, new ConcurrentDictionary<string, Timer>());
            }
            if (!SubscribeDisconnected.ContainsKey(instance.InstanceId))
            {
                SubscribeDisconnected.Add(instance.InstanceId, false);
            }
            if (!LastSubscribeTimetoken.ContainsKey(instance.InstanceId))
            {
                LastSubscribeTimetoken.Add(instance.InstanceId, 0);
            }
            if (!SubscribeRequestTracker.ContainsKey(instance.InstanceId))
            {
                SubscribeRequestTracker.Add(instance.InstanceId, DateTime.Now);
            }
        }
    }
}
