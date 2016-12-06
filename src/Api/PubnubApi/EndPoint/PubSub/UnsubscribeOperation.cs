using System;

namespace PubnubApi.EndPoint
{
    public class UnsubscribeOperation<T> : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        private string[] subscribeChannelNames = null;
        private string[] subscribeChannelGroupNames = null;
        private string[] presenceChannelNames = new string[] { };
        private string[] presenceChannelGroupNames = new string[] { };

        public UnsubscribeOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public UnsubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public UnsubscribeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
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

        public void Execute()
        {
            Unsubscribe(subscribeChannelNames, subscribeChannelGroupNames);
        }

        private void Unsubscribe(string[] channels, string[] channelGroups)
        {
            if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            string channel = (channels != null) ? string.Join(",", channels) : "";
            string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

            LoggingMethod.WriteToLog(string.Format("DateTime {0}, requested unsubscribe for channel(s)={1}", DateTime.Now.ToString(), channel), PNLogVerbosity.BODY);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit);
                manager.MultiChannelUnSubscribeInit<T>(PNOperationType.PNUnsubscribeOperation, channel, channelGroup);
            });
        }
    }
}
