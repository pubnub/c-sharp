using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using PubnubApi;

namespace PubnubApi
{
	public class Pubnub
	{
        PNConfiguration pubnubConfig = null;
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

        public EndPoint.PublishOperation publish()
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

		public void DetailedHistory(string channel, long start, long end, int count, bool reverse, bool includeToken, Action<DetailedHistoryAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.HistoryOperation endPoint = new EndPoint.HistoryOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.History(channel, start, end, count, reverse, includeToken, userCallback, errorCallback);
		}

		public void DetailedHistory(string channel, long start, bool includeToken, Action<DetailedHistoryAck> userCallback, Action<PubnubClientError> errorCallback, bool reverse)
		{
            EndPoint.HistoryOperation endPoint = new EndPoint.HistoryOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.History(channel, start, -1, -1, reverse, includeToken, userCallback, errorCallback);
		}

		public void DetailedHistory(string channel, int count, bool includeToken, Action<DetailedHistoryAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.HistoryOperation endPoint = new EndPoint.HistoryOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.History(channel, -1, -1, count, false, includeToken, userCallback, errorCallback);
		}

		public void HereNow(string[] channels, Action<HereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.HereNowOperation endPoint = new EndPoint.HereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.HereNow(channels, null, true, false, userCallback, errorCallback);
		}

        public void HereNow(string[] channels, string[] channelGroups, Action<HereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.HereNowOperation endPoint = new EndPoint.HereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.HereNow(channels, channelGroups, true, false, userCallback, errorCallback);
		}

		public void HereNow(string[] channels, bool showUUIDList, bool includeUserState, Action<HereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.HereNowOperation endPoint = new EndPoint.HereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.HereNow(channels, null, showUUIDList, includeUserState, userCallback, errorCallback);
		}

        public void HereNow(string[] channels, string[] channelGroups, bool showUUIDList, bool includeUserState, Action<HereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.HereNowOperation endPoint = new EndPoint.HereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.HereNow(channels, channelGroups, showUUIDList, includeUserState, userCallback, errorCallback);
		}

		public void GlobalHereNow(bool showUUIDList, bool includeUserState, Action<GlobalHereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GlobalHereNowOperation endPoint = new EndPoint.GlobalHereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GlobalHereNow(showUUIDList, includeUserState, userCallback, errorCallback);
		}

		public void WhereNow(string uuid, Action<WhereNowAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.WhereNowOperation endPoint = new EndPoint.WhereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.WhereNow(uuid, userCallback, errorCallback);
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

		public void Time(Action<long> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.TimeOperation endPoint = new EndPoint.TimeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
			endPoint.Time(userCallback, errorCallback);
		}

		public void AuditAccess(string channel, string channelGroup, string[] authKeys, Action<AuditAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.AuditOperation endPoint = new EndPoint.AuditOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.AuditAccess<AuditAck>(channel, channelGroup, authKeys, userCallback, errorCallback);
		}

		public void AuditAccess(string channel, string channelGroup, Action<AuditAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.AuditOperation endPoint = new EndPoint.AuditOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.AuditAccess<AuditAck>(channel, channelGroup, null, userCallback, errorCallback);
		}

		public void AuditAccess(Action<AuditAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.AuditOperation endPoint = new EndPoint.AuditOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.AuditAccess<AuditAck>("", "", null, userCallback, errorCallback);
		}

		public void GrantAccess(string[] channels, string[] channelGroups, bool read, bool write, bool manage, int ttl, Action<GrantAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GrantOperation endPoint = new EndPoint.GrantOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GrantAccess<GrantAck>(channels, channelGroups, null, read, write, manage, ttl, userCallback, errorCallback);
		}

		public void GrantAccess(string[] channels, string[] channelGroups, bool read, bool write, bool manage, Action<GrantAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GrantOperation endPoint = new EndPoint.GrantOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GrantAccess<GrantAck>(channels, channelGroups, null, read, write, manage, -1, userCallback, errorCallback);
		}

		public void GrantAccess(string[] channels, string[] channelGroups, string[] authenticationKeys, bool read, bool write, bool manage, int ttl, Action<GrantAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GrantOperation endPoint = new EndPoint.GrantOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GrantAccess<GrantAck>(channels, channelGroups, authenticationKeys, read, write, manage, ttl, userCallback, errorCallback);
		}

		public void GrantAccess(string[] channels, string[] channelGroups, string[] authenticationKeys, bool read, bool write, bool manage, Action<GrantAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GrantOperation endPoint = new EndPoint.GrantOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GrantAccess<GrantAck>(channels, channelGroups, authenticationKeys, read, write, manage, -1, userCallback, errorCallback);
		}

		public void SetUserState(string[] channels, string[] channelGroups, string uuid, string jsonUserState, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SetStateOperation endPoint = new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.SetUserState(channels, channelGroups, uuid, jsonUserState, userCallback, errorCallback);
		}

		public void SetUserState(string[] channels, string[] channelGroups, string jsonUserState, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SetStateOperation endPoint = new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary), pubnubUnitTest;
            endPoint.SetUserState(channels, channelGroups, "", jsonUserState, userCallback, errorCallback);
		}

		public void SetUserState(string[] channels, string jsonUserState, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SetStateOperation endPoint = new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.SetUserState(channels, null, "", jsonUserState, userCallback, errorCallback);
		}

		public void SetUserState(string[] channels, string[] channelGroups, string uuid, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SetStateOperation endPoint = new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.SetUserState(channels, channelGroups, uuid, keyValuePair, userCallback, errorCallback);
		}

		public void SetUserState(string[] channels, string[] channelGroups, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SetStateOperation endPoint = new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.SetUserState(channels, channelGroups, "", keyValuePair, userCallback, errorCallback);
		}

		public void SetUserState(string[] channels, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<SetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.SetStateOperation endPoint = new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.SetUserState(channels, null, "", keyValuePair, userCallback, errorCallback);
		}

		public void GetUserState(string[] channels, string[] channelGroups, Action<GetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GetStateOperation endPoint = new EndPoint.GetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GetUserState(channels, channelGroups, "", userCallback, errorCallback);
		}

		public void GetUserState(string[] channels, string[] channelGroups, string uuid, Action<GetUserStateAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GetStateOperation endPoint = new EndPoint.GetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GetUserState(channels, channelGroups, uuid, userCallback, errorCallback);
		}

		public void RegisterDeviceForPush<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.PushOperation endPoint = new EndPoint.PushOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.RegisterDevice<T>(channel, pushType, pushToken, userCallback, errorCallback);
		}

		public void UnregisterDeviceForPush<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.PushOperation endPoint = new EndPoint.PushOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.UnregisterDevice<T>(pushType, pushToken, userCallback, errorCallback);
		}

		public void RemoveChannelForDevicePush<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.PushOperation endPoint = new EndPoint.PushOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.RemoveChannelForDevice<T>(channel, pushType, pushToken, userCallback, errorCallback);
		}

		public void GetChannelsForDevicePush<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.PushOperation endPoint = new EndPoint.PushOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GetChannelsForDevice<T>(pushType, pushToken, userCallback, errorCallback);
		}
        #endregion

        #region "PubNub API Channel Group Methods"
        public void AddChannelsToChannelGroup(string[] channels, string groupName, Action<AddChannelToChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.AddChannelsToChannelGroupOperation endPoint = new EndPoint.AddChannelsToChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.AddChannelsToChannelGroup(channels, "", groupName, userCallback, errorCallback);
		}

		public void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Action<AddChannelToChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.AddChannelsToChannelGroupOperation endPoint = new EndPoint.AddChannelsToChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.AddChannelsToChannelGroup(channels, nameSpace, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelsFromChannelGroup(string[] channels, string nameSpace, string groupName, Action<RemoveChannelFromChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.RemoveChannelsFromChannelGroupOperation endPoint = new EndPoint.RemoveChannelsFromChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.RemoveChannelsFromChannelGroup(channels, nameSpace, groupName, userCallback, errorCallback);
		}

		public void RemoveChannelsFromChannelGroup(string[] channels, string groupName, Action<RemoveChannelFromChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.RemoveChannelsFromChannelGroupOperation endPoint = new EndPoint.RemoveChannelsFromChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.RemoveChannelsFromChannelGroup(channels, "", groupName, userCallback, errorCallback);
		}

		public void RemoveChannelGroup(string nameSpace, string groupName, Action<RemoveChannelGroupAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            //TO BE CONVERTED
			//pubnub.RemoveChannelGroup(nameSpace, groupName, userCallback, errorCallback);
		}

		public void RemoveChannelGroupNameSpace(string nameSpace, Action<RemoveNamespaceAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            //TO BE CONVERTED
            //pubnub.RemoveChannelGroupNameSpace(nameSpace, userCallback, errorCallback);
		}

		public void GetChannelsForChannelGroup(string nameSpace, string groupName, Action<GetChannelGroupChannelsAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GetChannelsForChannelGroupOperation endPoint = new EndPoint.GetChannelsForChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GetChannelsForChannelGroup(nameSpace, groupName, userCallback, errorCallback);
		}

		public void GetChannelsForChannelGroup(string groupName, Action<GetChannelGroupChannelsAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GetChannelsForChannelGroupOperation endPoint = new EndPoint.GetChannelsForChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GetChannelsForChannelGroup(groupName, userCallback, errorCallback);
		}

		public void GetAllChannelGroups(string nameSpace, Action<GetAllChannelGroupsAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            //TO BE CONVERTED
            //pubnub.GetAllChannelGroups(nameSpace, userCallback, errorCallback);
        }

        public void GetAllChannelGroups(Action<GetAllChannelGroupsAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GetAllChannelGroupOperation endPoint = new EndPoint.GetAllChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GetAllChannelGroup(userCallback, errorCallback);
		}

		public void GetAllChannelGroupNamespaces(Action<GetAllNamespacesAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            //TO BE CONVERTED
            //pubnub.GetAllChannelGroupNamespaces(userCallback, errorCallback);
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

        public Pubnub(PNConfiguration pnConfig)
        {
            pubnubConfig = pnConfig;
            jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            CheckRequiredConfigValues();
        }

        public Pubnub(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            pubnubConfig = pnConfig;
            this.jsonPluggableLibrary = jsonPluggableLibrary;
            if (jsonPluggableLibrary == null)
            {
                this.jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            }
            CheckRequiredConfigValues();
        }

        public Pubnub(PNConfiguration pnConfig, IPubnubUnitTest pubnubUnitTest)
        {
            this.pubnubConfig = pnConfig;
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