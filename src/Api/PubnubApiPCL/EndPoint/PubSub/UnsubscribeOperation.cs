using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class UnsubscribeOperation<T> : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private static IPubnubUnitTest unitTest = null;
        private string[] subscribeChannelNames = null;
        private string[] subscribeChannelGroupNames = null;
        private string[] presenceChannelNames = new string[] { };
        private string[] presenceChannelGroupNames = new string[] { };

        private bool presenceUnsubEnabled = false;
        private bool ignoreUnsubsButPresenceUnsub = false;

        public UnsubscribeOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public UnsubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public UnsubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnitTest) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unitTest = pubnubUnitTest;
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

        public UnsubscribeOperation<T> WithPresence()
        {
            this.presenceUnsubEnabled = true;
            this.ignoreUnsubsButPresenceUnsub = false;
            return this;
        }

        public UnsubscribeOperation<T> WithPresence(bool ignoreUnsubscribe)
        {
            this.presenceUnsubEnabled = true;
            this.ignoreUnsubsButPresenceUnsub = ignoreUnsubscribe;
            return this;
        }

        public void Execute(UnsubscribeCallback callback)
        {
            if (this.subscribeChannelNames == null)
            {
                this.subscribeChannelNames = new string[] { };
            }

            if (this.subscribeChannelGroupNames == null)
            {
                this.subscribeChannelGroupNames = new string[] { };
            }

            if (this.presenceUnsubEnabled)
            {
                this.presenceChannelNames = (this.subscribeChannelNames != null && this.subscribeChannelNames.Length > 0 && !string.IsNullOrEmpty(this.subscribeChannelNames[0]))
                                                ? this.subscribeChannelNames.Select(c => string.Format("{0}-pnpres", c)).ToArray() : new string[] { };
                this.presenceChannelGroupNames = (this.subscribeChannelGroupNames != null && this.subscribeChannelGroupNames.Length > 0 && !string.IsNullOrEmpty(this.subscribeChannelGroupNames[0]))
                                                ? this.subscribeChannelGroupNames.Select(c => string.Format("{0}-pnpres", c)).ToArray() : new string[] { };

            }

            if (this.ignoreUnsubsButPresenceUnsub)
            {
                this.subscribeChannelNames = new string[] { };
                this.subscribeChannelGroupNames = new string[] { };
            }

            string[] channelNames = this.subscribeChannelNames.Concat(this.presenceChannelNames).ToArray();
            string[] channelGroupNames = this.subscribeChannelGroupNames.Concat(this.presenceChannelGroupNames).ToArray();

            Unsubscribe<T>(channelNames, channelGroupNames, callback.Error);
            //Unsubscribe<T>(this.subscribeChannelNames, this.subscribeChannelGroupNames, callback.Error);
            //PresenceUnsubscribe(this.presenceChannelNames, this.presenceChannelGroupNames, callback.Error);
        }

        private void Unsubscribe<T>(string[] channels, string[] channelGroups, Action<PubnubClientError> errorCallback)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            if (errorCallback == null)
            {
                throw new ArgumentException("Missing errorCallback");
            }

            string channel = (channels != null) ? string.Join(",", channels) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

            LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested unsubscribe for channel(s)={1}", DateTime.Now.ToString(), channel), LoggingMethod.LevelInfo);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unitTest);
                manager.MultiChannelUnSubscribeInit<T>(ResponseType.Unsubscribe, channel, channelGroup, errorCallback);
            });
        }

        private void PresenceUnsubscribe(string[] channels, string[] channelGroups, Action<PubnubClientError> errorCallback)
        {
            //if ((string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0) && (string.IsNullOrEmpty(channelGroup) || channelGroup.Trim().Length <= 0))
            //{
            //    throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            //}
            //if (disconnectCallback == null)
            //{
            //    throw new ArgumentException("Missing disconnectCallback");
            //}
            //if (errorCallback == null)
            //{
            //    throw new ArgumentException("Missing errorCallback");
            //}
            string channel = (channels != null) ? string.Join(",", channels) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";


            LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested presence-unsubscribe for channel(s)={1}", DateTime.Now.ToString(), channel), LoggingMethod.LevelInfo);

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unitTest);
                manager.MultiChannelUnSubscribeInit<object>(ResponseType.PresenceUnsubscribe, channel, channelGroup, errorCallback);
            });
        }


    }
}
