using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using PubnubApi;

namespace PubnubApi
{
	public class Pubnub
	{
        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonPluggableLibrary = null;
        private IPubnubUnitTest pubnubUnitTest = null;

		#region "PubNub API Channel Methods"

		public void Subscribe<T>(string channel, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}

            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Subscribe<T>(channel, "", subscribeCallback, connectCallback, disconnectCallback, null, errorCallback);
        }

        public void Subscribe<T>(string channel, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}

            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Subscribe<T>(channel, "", subscribeCallback, connectCallback, disconnectCallback, wildcardPresenceCallback, errorCallback);
        }

        public void Subscribe<T>(string channel, string channelGroup, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Subscribe<T>(channel, channelGroup, subscribeCallback, connectCallback, disconnectCallback, null, errorCallback);
        }

        public void Subscribe<T>(string channel, string channelGroup, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Subscribe<T>(channel, channelGroup, subscribeCallback, connectCallback, disconnectCallback, wildcardPresenceCallback, errorCallback);
        }

        public EndPoint.PublishOperation Publish()
        {
            return new EndPoint.PublishOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public void Presence(string channel, Action<PresenceAck> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}
            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Presence(channel, "", presenceCallback, connectCallback, disconnectCallback, errorCallback);
		}

		public void Presence(string channel, string channelGroup, Action<PresenceAck> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Presence(channel, channelGroup, presenceCallback, connectCallback, disconnectCallback, errorCallback);
		}

		public EndPoint.HistoryOperation History()
		{
            return new EndPoint.HistoryOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.HereNowOperation HereNow()
		{
            return new EndPoint.HereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.WhereNowOperation WhereNow()
		{
            return new EndPoint.WhereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public void Unsubscribe<T>(string channel, string channelGroup, Action<PubnubClientError> errorCallback)
		{
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
                endPoint.Unsubscribe<T>(channel, channelGroup, errorCallback);
            });
        }

		public void Unsubscribe<T>(string channel, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
                endPoint.Unsubscribe<T>(channel, null, errorCallback);
            });
        }

		public void PresenceUnsubscribe(string channel, string channelGroup, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.PresenceUnsubscribe(channel, channelGroup, disconnectCallback, errorCallback);
		}

		public void PresenceUnsubscribe(string channel, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.PresenceUnsubscribe(channel, "", disconnectCallback, errorCallback);
		}

		public EndPoint.TimeOperation Time()
		{
            return new EndPoint.TimeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.AuditOperation Audit()
		{
            return new EndPoint.AuditOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.GrantOperation Grant()
		{
            return new EndPoint.GrantOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.SetStateOperation SetPresenceState()
		{
            return new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.GetStateOperation GetPresenceState()
		{
            return new EndPoint.GetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.AddPushChannelOperation AddPushNotificationsOnChannels()
		{
            return new EndPoint.AddPushChannelOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.RemovePushChannelOperation RemovePushNotificationsFromChannels()
		{
            return new EndPoint.RemovePushChannelOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.AuditPushChannelOperation AuditPushChannelProvisions()
		{
            return new EndPoint.AuditPushChannelOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

        #endregion

        #region "PubNub API Channel Group Methods"

        public EndPoint.AddChannelsToChannelGroupOperation AddChannelsToChannelGroup()
		{
            return new EndPoint.AddChannelsToChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.RemoveChannelsFromChannelGroupOperation RemoveChannelsFromChannelGroup()
		{
            return new EndPoint.RemoveChannelsFromChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.DeleteChannelGroupOperation DeleteChannelGroup()
		{
            return new EndPoint.DeleteChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

		public EndPoint.ListChannelsForChannelGroupOperation ListChannelsForChannelGroup()
		{
            return new EndPoint.ListChannelsForChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

        public EndPoint.ListAllChannelGroupOperation ListChannelGroups()
		{
            return new EndPoint.ListAllChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }
        
        #endregion

        #region "PubNub API Other Methods"
        public void TerminateCurrentSubscriberRequest()
		{
            EndPoint.OtherOperation.TerminateCurrentSubscriberRequest();
		}

		public void EnableSimulateNetworkFailForTestingOnly()
		{
            EndPoint.OtherOperation.EnableSimulateNetworkFailForTestingOnly();
        }

		public void DisableSimulateNetworkFailForTestingOnly()
		{
            EndPoint.OtherOperation.DisableSimulateNetworkFailForTestingOnly();
		}

		public void EnableMachineSleepModeForTestingOnly()
		{
            EndPoint.OtherOperation.EnableMachineSleepModeForTestingOnly();
		}

		public void DisableMachineSleepModeForTestingOnly()
		{
            EndPoint.OtherOperation.DisableMachineSleepModeForTestingOnly();
		}

		public void EndPendingRequests()
		{
            EndPoint.OtherOperation endPoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.EndPendingRequests();
        }

        public Guid GenerateGuid()
		{
			return Guid.NewGuid();
		}

		public void ChangeUUID(string newUUID)
		{
            EndPoint.OtherOperation endPoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.ChangeUUID(newUUID);
		}

		public static long TranslateDateTimeToPubnubUnixNanoSeconds(DateTime dotNetUTCDateTime)
		{
			return EndPoint.OtherOperation.TranslateDateTimeToPubnubUnixNanoSeconds(dotNetUTCDateTime);
		}

		public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(long unixNanoSecondTime)
		{
			return EndPoint.OtherOperation.TranslatePubnubUnixNanoSecondsToDateTime(unixNanoSecondTime);
		}

		public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(string unixNanoSecondTime)
		{
			return EndPoint.OtherOperation.TranslatePubnubUnixNanoSecondsToDateTime(unixNanoSecondTime);
		}

        //public void SetErrorFilterLevel(PubnubErrorFilter.Level errorLevel)
        //{
        //    pubnub.PubnubErrorLevel = errorLevel;
        //}  

		#endregion

		#region "Properties"
		public string AuthenticationKey {
			get {return pubnubConfig.AuthKey;}
			set { pubnubConfig.AuthKey = value;}
		}

        public string SessionUUID
        {
            get { return pubnubConfig.Uuid; }
            set { pubnubConfig.Uuid = value; }
        }

        public IPubnubUnitTest PubnubUnitTest
        {
            get
            {
                return pubnubUnitTest;
            }
            set
            {
                pubnubUnitTest = value;
            }
        }

        //TO BE REMOVED
        public bool EnableJsonEncodingForPublish
        {
            get
            {
                throw new Exception("No support");
            }
            set
            {
                throw new Exception("No support");
            }
        }

        //TO BE REMOVED
        public bool EnableDebugForPushPublish
        {
            get
            {
                throw new Exception("No support");
            }
            set
            {
                throw new Exception("No support");
            }
        }

        public PNConfiguration PNConfig
        {
            get
            {
                return pubnubConfig;
            }
        }

        public IJsonPluggableLibrary JsonPluggableLibrary
		{
			get
			{
				return jsonPluggableLibrary;
			}
		}

        #endregion

        #region "Constructors"

        public Pubnub(PNConfiguration config)
        {
            pubnubConfig = config;
            jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            CheckRequiredConfigValues();
        }

        public Pubnub(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            pubnubConfig = config;
            this.jsonPluggableLibrary = jsonPluggableLibrary;
            if (jsonPluggableLibrary == null)
            {
                this.jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            }
            CheckRequiredConfigValues();
        }

        public Pubnub(PNConfiguration config, IPubnubUnitTest pubnubUnitTest)
        {
            this.pubnubConfig = config;
            this.jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            this.pubnubUnitTest = pubnubUnitTest;
            CheckRequiredConfigValues();
        }

        private void CheckRequiredConfigValues()
        {
            if (pubnubConfig != null)
            {
                if (string.IsNullOrEmpty(pubnubConfig.SubscribeKey))
                {
                    pubnubConfig.SubscribeKey = "";
                }

                if (string.IsNullOrEmpty(pubnubConfig.PublishKey))
                {
                    pubnubConfig.PublishKey = "";
                }

                if (string.IsNullOrEmpty(pubnubConfig.SecretKey))
                {
                    pubnubConfig.SecretKey = "";
                }

                if (string.IsNullOrEmpty(pubnubConfig.CiperKey))
                {
                    pubnubConfig.CiperKey = "";
                }
            }
        }

        //TO BE REMOVED
        public Pubnub(string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
		{
            throw new Exception("No support");
        }

        //TO BE REMOVED
        public Pubnub(string publishKey, string subscribeKey, string secretKey)
		{
            throw new Exception("No support");
        }

        //TO BE REMOVED
        public Pubnub(string publishKey, string subscribeKey)
		{
            throw new Exception("No support");
        }
		#endregion
	}
}