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
        private static IPubnubUnitTest unitTest = null;
        private string[] subscribeChannelNames = null;
        private string[] subscribeChannelGroupNames = null;
        private string[] presenceChannelNames = new string[] { };
        private string[] presenceChannelGroupNames = new string[] { };
        private long subscribeTimetoken;
        private bool presenceSubscribeEnabled = false;
        private bool ignoreSubscribeButPresence = false;

        protected List<SubscribeCallback<T>> subscribeCallbackListeners = new List<SubscribeCallback<T>>();

        public SubscribeOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public SubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public SubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
        }

        public SubscribeOperation<T> Channels(string[] channels)
        {
            this.subscribeChannelNames = channels;
            return this;
        }

        public SubscribeOperation<T> ChannelGroups(string[] channelGroups)
        {
            this.subscribeChannelGroupNames = channelGroups;
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

        public SubscribeOperation<T> WithPresence(bool ignoreSubscribe)
        {
            this.presenceSubscribeEnabled = true;
            this.ignoreSubscribeButPresence = ignoreSubscribe;
            return this;
        }

        public void Execute(SubscribeCallback<T> callback)
        {
            if (this.subscribeChannelNames == null)
            {
                this.subscribeChannelNames = new string[] { };
            }

            if (this.subscribeChannelGroupNames == null)
            {
                this.subscribeChannelGroupNames = new string[] { };
            }

            if (this.presenceSubscribeEnabled)
            {
                this.presenceChannelNames = (this.subscribeChannelNames != null && this.subscribeChannelNames.Length > 0 && !string.IsNullOrEmpty(this.subscribeChannelNames[0])) 
                                                ? this.subscribeChannelNames.Select(c => string.Format("{0}-pnpres",c)).ToArray() : new string[] { };
                this.presenceChannelGroupNames = (this.subscribeChannelGroupNames != null && this.subscribeChannelGroupNames.Length > 0 && !string.IsNullOrEmpty(this.subscribeChannelGroupNames[0])) 
                                                ? this.subscribeChannelGroupNames.Select(c => string.Format("{0}-pnpres", c)).ToArray() : new string[] { };
            }

            if (this.ignoreSubscribeButPresence)
            {
                this.subscribeChannelNames = new string[] { };
                this.subscribeChannelGroupNames = new string[] { };
            }

            string[] channelNames = this.subscribeChannelNames.Concat(this.presenceChannelNames).ToArray();
            string[] channelGroupNames = this.subscribeChannelGroupNames.Concat(this.presenceChannelGroupNames).ToArray();

            //Presence(this.presenceChannelNames, this.presenceChannelGroupNames, callback.Presence, callback.Connect, callback.Disconnect, callback.Error);
            
            Subscribe(channelNames, channelGroupNames, callback.Message, callback.Presence, callback.Connect, callback.Disconnect, callback.WildPresence, callback.Error);
        }

        private void Subscribe(string[] channels, string[] channelGroups, Action<PNMessageResult<T>> subscribeCallback, Action<PNPresenceEventResult> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PNPresenceEventResult> wildPresenceCallback, Action<PubnubClientError> errorCallback)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length  == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            string channel = (channels != null) ? string.Join(",", channels) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

            if (subscribeCallback == null)
            {
                new PNCallbackService(config, jsonLibrary).CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                            channel, channelGroup, errorCallback, "Missing subscribeCallback", PubnubErrorCode.InvalidChannel,
                            null, null);
            }

            if (connectCallback == null)
            {
                throw new ArgumentException("Missing connectCallback");
            }

            LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested subscribe for channel={1} and channel group={2}", DateTime.Now.ToString(), channel, channelGroup), LoggingMethod.LevelInfo);

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

            //Action<object> anyPresenceCallback = null;
            //PubnubChannelCallbackKey anyPresenceKey = new PubnubChannelCallbackKey() { Channel = string.Format("{0}-pnpres",channel), ResponseType = ResponseType.Presence };
            //if (channelCallbacks != null && channelCallbacks.ContainsKey(anyPresenceKey))
            //{
            //    var currentType = Activator.CreateInstance(channelCallbacks[anyPresenceKey].GetType());
            //    anyPresenceCallback = channelCallbacks[anyPresenceKey] as Action<object>;
            //}
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unitTest);
                manager.MultiChannelSubscribeInit<T>(ResponseType.Subscribe, channels, channelGroups, subscribeCallback, presenceCallback, connectCallback, disconnectCallback, wildPresenceCallback, errorCallback);
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
