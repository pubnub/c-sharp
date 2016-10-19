using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class SubscribeOperation<T> : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        private List<string> subscribeChannelNames = new List<string>();
        private List<string> subscribeChannelGroupNames = new List<string>();
        private List<string> presenceChannelNames = new List<string>();
        private List<string> presenceChannelGroupNames = new List<string>();
        private long subscribeTimetoken = -1;
        private bool presenceSubscribeEnabled = false;

        public SubscribeOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public SubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public SubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
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

            string channel = (channels != null) ? string.Join(",", channels) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

            LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested subscribe for channel(s)={1} and channel group(s)={2}", DateTime.Now.ToString(), channel, channelGroup), LoggingMethod.LevelInfo);

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


            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit);
                manager.MultiChannelSubscribeInit<T>(PNOperationType.PNSubscribeOperation, channels, channelGroups, initialSubscribeUrlParams);
            });
        }

        //private void Presence(string[] channels, string[] channelGroups, Action<PNPresenceEventResult> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
        //{
        //    //if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
        //    //{
        //    //    throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
        //    //}

        //    if (presenceCallback == null)
        //    {
        //        throw new ArgumentException("Missing presenceCallback");
        //    }

        //    if (errorCallback == null)
        //    {
        //        throw new ArgumentException("Missing errorCallback");
        //    }

        //    string channel = (channels != null) ? string.Join(",", channels) : "";
        //    string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

        //    LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested presence for channel={1} and channel group={2}", DateTime.Now.ToString(), channel, channelGroup), LoggingMethod.LevelInfo);
        //    //string[] arrayChannel = new string[] { };
        //    //string[] arrayChannelGroup = new string[] { };

        //    //if (!string.IsNullOrEmpty(channel) && channel.Trim().Length > 0)
        //    //{
        //    //    arrayChannel = channel.Trim().Split(',');
        //    //}

        //    //if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
        //    //{
        //    //    arrayChannelGroup = channelGroup.Trim().Split(',');
        //    //}

        //    System.Threading.Tasks.Task.Factory.StartNew(() =>
        //    {
        //        SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unitTest);
        //        manager.MultiChannelSubscribeInit<T>(ResponseType.Presence, channels, channelGroups, null, presenceCallback, connectCallback, disconnectCallback, null, errorCallback);
        //    });
        //}
    }
}
