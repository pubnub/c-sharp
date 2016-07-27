using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace PubnubApi
{
	public class Pubnub
	{
        PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonPluggableLibrary = null;
        private IPubnubUnitTest pubnubUnitTest = null;

        PubnubWin pubnub;

		#region "PubNub API Channel Methods"

		public void Subscribe<T>(string channel, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}
            System.Threading.Tasks.Task.Factory.StartNew(() => 
                {
                    EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
                    endPoint.Subscribe<T>(channel, "", subscribeCallback, connectCallback, disconnectCallback, null, errorCallback);
                });
		}

		public void Subscribe<T>(string channel, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
                endPoint.Subscribe<T>(channel, "", subscribeCallback, connectCallback, disconnectCallback, wildcardPresenceCallback, errorCallback);
            });
		}

		public void Subscribe<T>(string channel, string channelGroup, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
                endPoint.Subscribe<T>(channel, channelGroup, subscribeCallback, connectCallback, disconnectCallback, null, errorCallback);
            });
		}

		public void Subscribe<T>(string channel, string channelGroup, Action<Message<T>> subscribeCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PresenceAck> wildcardPresenceCallback, Action<PubnubClientError> errorCallback)
		{
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                EndPoint.SubscribeOperation endPoint = new EndPoint.SubscribeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
                endPoint.Subscribe<T>(channel, channelGroup, subscribeCallback, connectCallback, disconnectCallback, wildcardPresenceCallback, errorCallback);
            });
		}

		public void Publish(string channel, object message, Action<PublishAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.PublishOperation endPoint = new EndPoint.PublishOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Publish(channel, message, true, "", userCallback, errorCallback);
		}

        public void Publish(string channel, object message, bool storeInHistory, string jsonUserMetaData, Action<PublishAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.PublishOperation endPoint = new EndPoint.PublishOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.Publish(channel, message, storeInHistory, jsonUserMetaData, userCallback, errorCallback);
		}

		public void Presence(string channel, Action<PresenceAck> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}

			pubnub.Presence(channel, "", presenceCallback, connectCallback, disconnectCallback, errorCallback);
		}

		public void Presence(string channel, string channelGroup, Action<PresenceAck> presenceCallback, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.Presence(channel, channelGroup, presenceCallback, connectCallback, disconnectCallback, errorCallback);
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
			pubnub.Unsubscribe<T>(channel, channelGroup, errorCallback);
		}

		public void Unsubscribe<T>(string channel, Action<PubnubClientError> errorCallback)
		{
			if (string.IsNullOrEmpty(channel) || channel.Trim().Length <= 0)
			{
				throw new ArgumentException("Channel should be provided.");
			}
			pubnub.Unsubscribe<T>(channel, null, errorCallback);
		}

		public void PresenceUnsubscribe(string channel, string channelGroup, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.PresenceUnsubscribe(channel, channelGroup, disconnectCallback, errorCallback);
		}

		public void PresenceUnsubscribe(string channel, Action<ConnectOrDisconnectAck> connectCallback, Action<ConnectOrDisconnectAck> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.PresenceUnsubscribe(channel, "", disconnectCallback, errorCallback);
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
			pubnub.RegisterDeviceForPush<T>(channel, pushType, pushToken, userCallback, errorCallback);
		}

		public void UnregisterDeviceForPush<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.UnregisterDeviceForPush<T>(pushType, pushToken, userCallback, errorCallback);
		}

		public void RemoveChannelForDevicePush<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.RemoveChannelForDevicePush<T>(channel, pushType, pushToken, userCallback, errorCallback);
		}
		public void RemoveChannelForDevicePush(string channel, PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.RemoveChannelForDevicePush<object>(channel, pushType, pushToken, userCallback, errorCallback);
		}

		public void GetChannelsForDevicePush<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.GetChannelsForDevicePush<T>(pushType, pushToken, userCallback, errorCallback);
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
			pubnub.RemoveChannelGroup(nameSpace, groupName, userCallback, errorCallback);
		}

		public void RemoveChannelGroupNameSpace(string nameSpace, Action<RemoveNamespaceAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.RemoveChannelGroupNameSpace(nameSpace, userCallback, errorCallback);
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
			pubnub.GetAllChannelGroups(nameSpace, userCallback, errorCallback);
		}

		public void GetAllChannelGroups(Action<GetAllChannelGroupsAck> userCallback, Action<PubnubClientError> errorCallback)
		{
            EndPoint.GetAllChannelGroupOperation endPoint = new EndPoint.GetAllChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endPoint.GetAllChannelGroup(userCallback, errorCallback);
		}

		public void GetAllChannelGroupNamespaces(Action<GetAllNamespacesAck> userCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.GetAllChannelGroupNamespaces(userCallback, errorCallback);
		}

		#endregion

		#region "PubNub API Other Methods"
		public void TerminateCurrentSubscriberRequest()
		{
			pubnub.TerminateCurrentSubscriberRequest();
		}

		public void EnableSimulateNetworkFailForTestingOnly()
		{
			pubnub.EnableSimulateNetworkFailForTestingOnly();
		}

		public void DisableSimulateNetworkFailForTestingOnly()
		{
			pubnub.DisableSimulateNetworkFailForTestingOnly();
		}

		public void EnableMachineSleepModeForTestingOnly()
		{
			pubnub.EnableMachineSleepModeForTestingOnly();
		}

		public void DisableMachineSleepModeForTestingOnly()
		{
			pubnub.DisableMachineSleepModeForTestingOnly();
		}

		public void EndPendingRequests()
		{
			//pubnub.EndPendingRequests();
		}

		public Guid GenerateGuid()
		{
			return pubnub.GenerateGuid();
		}

		public void ChangeUUID(string newUUID)
		{
			pubnub.ChangeUUID(newUUID);
		}

		public static long TranslateDateTimeToPubnubUnixNanoSeconds(DateTime dotNetUTCDateTime)
		{
			return PubnubWin.TranslateDateTimeToPubnubUnixNanoSeconds(dotNetUTCDateTime);
		}

		public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(long unixNanoSecondTime)
		{
			return PubnubWin.TranslatePubnubUnixNanoSecondsToDateTime(unixNanoSecondTime);
		}

		public static DateTime TranslatePubnubUnixNanoSecondsToDateTime(string unixNanoSecondTime)
		{
			return PubnubWin.TranslatePubnubUnixNanoSecondsToDateTime(unixNanoSecondTime);
		}

        public void SetPubnubLog(IPubnubLog pubnubLog) 
         { 
             pubnub.PubnubLog = pubnubLog; 
         } 

        public void SetInternalLogLevel(LoggingMethod.Level logLevel)
        {
            pubnub.PubnubLogLevel = logLevel;
        }

        public void SetErrorFilterLevel(PubnubErrorFilter.Level errorLevel)
        {
            pubnub.PubnubErrorLevel = errorLevel;
        }  

		#endregion

		#region "Properties"
		public string AuthenticationKey {
			get {return pubnubConfig.AuthKey;}
			set { pubnubConfig.AuthKey = value;}
		}

        //public int LocalClientHeartbeatInterval {
        //	get {return pubnub.LocalClientHeartbeatInterval;}
        //	set {pubnub.LocalClientHeartbeatInterval = value;}
        //}

        //public int NetworkCheckRetryInterval {
        //	get {return pubnub.NetworkCheckRetryInterval;}
        //	set {pubnub.NetworkCheckRetryInterval = value;}
        //}

        //public int NetworkCheckMaxRetries {
        //	get {return pubnub.NetworkCheckMaxRetries;}
        //	set {pubnub.NetworkCheckMaxRetries = value;}
        //}

        //public bool EnableResumeOnReconnect {
        //	get {return pubnub.EnableResumeOnReconnect;}
        //	set {pubnub.EnableResumeOnReconnect = value;}
        //}

        public string SessionUUID
        {
            get { return pubnubConfig.Uuid; }
            set { pubnubConfig.Uuid = value; }
        }

        //public string Origin {
        //	get {return pubnub.Origin;}
        //	set {pubnub.Origin = value;}
        //}

        //public int PresenceHeartbeat
        //{
        //	get
        //	{
        //		return pubnub.PresenceHeartbeat;
        //	}
        //	set
        //	{
        //		pubnub.PresenceHeartbeat = value;
        //	}
        //}

        //public int PresenceHeartbeatInterval
        //{
        //	get
        //	{
        //		return pubnub.PresenceHeartbeatInterval;
        //	}
        //	set
        //	{
        //		pubnub.PresenceHeartbeatInterval = value;
        //	}
        //}

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

        public bool EnableJsonEncodingForPublish
		{
			get
			{
				return pubnub.EnableJsonEncodingForPublish;
			}
			set
			{
				pubnub.EnableJsonEncodingForPublish = value;
			}
		}

        public PubnubProxy Proxy
        {
            get
            {
                return pubnub.Proxy;
            }
            set
            {
                pubnub.Proxy = value;
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

		public IPubnubSubscribeMessageType SubscribeMessageType
		{
			get
			{
				return pubnub.SubscribeMessageType;
			}
			set
			{
				pubnub.SubscribeMessageType = value;
			}
		}

		public bool EnableDebugForPushPublish
		{
			get
			{
				return pubnub.EnableDebugForPushPublish;
			}
			set
			{
				pubnub.EnableDebugForPushPublish = value;
			}
		}

		public Collection<Uri> PushRemoteImageDomainUri
		{
			get
			{
				return pubnub.PushRemoteImageDomainUri;
			}

			set
			{
				pubnub.PushRemoteImageDomainUri = value;
			}
		}

		public string PushServiceName
		{
			get
			{
				return pubnub.PushServiceName;
			}

			set
			{
				pubnub.PushServiceName = value;
			}
		}

        public bool AddPayloadToPublishResponse
        {
            get
            {
                return pubnub.AddPayloadToPublishResponse;
            }
            set
            {
                pubnub.AddPayloadToPublishResponse = value;
            }
        }
        #endregion

        #region "Constructors"

        public Pubnub(PNConfiguration pnConfig)
        {
            pubnubConfig = pnConfig;
            jsonPluggableLibrary = new NewtonsoftJsonDotNet();
        }

        public Pubnub(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            pubnubConfig = pnConfig;
            this.jsonPluggableLibrary = jsonPluggableLibrary;
            if (jsonPluggableLibrary == null)
            {
                this.jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            }
        }

        public Pubnub(PNConfiguration pnConfig, IPubnubUnitTest pubnubUnitTest)
        {
            this.pubnubConfig = pnConfig;
            this.jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            this.pubnubUnitTest = pubnubUnitTest;
        }

        public Pubnub(string publishKey, string subscribeKey, string secretKey, string cipherKey, bool sslOn)
		{
			pubnub = new PubnubWin (publishKey, subscribeKey, secretKey, cipherKey, sslOn);
		}

		public Pubnub(string publishKey, string subscribeKey, string secretKey)
		{
			pubnub = new PubnubWin (publishKey, subscribeKey, secretKey);
		}

		public Pubnub(string publishKey, string subscribeKey)
		{
			pubnub = new PubnubWin (publishKey, subscribeKey);
		}
		#endregion
	}
}