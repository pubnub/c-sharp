using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PubNubMessaging.Core
{
	public class Pubnub
	{
        PubnubWin pubnub;
        
        #region "PubNub API Channel Methods"
		public void Subscribe<T>(string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.Subscribe<T>(channel, userCallback, connectCallback, errorCallback);
		}

        public void Subscribe<T>(string channel, string channelGroup, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Subscribe<T>(channel, channelGroup, userCallback, connectCallback, errorCallback);
        }

		public void Subscribe(string channel, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.Subscribe(channel, userCallback, connectCallback, errorCallback);
		}

        public void Subscribe(string channel, string channelGroup, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Subscribe(channel, channelGroup, userCallback, connectCallback, errorCallback);
        }

		public bool Publish(string channel, object message, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.Publish(channel, message, true, userCallback, errorCallback);
		}

		public bool Publish<T>(string channel, object message, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.Publish<T>(channel, message, true, userCallback, errorCallback);
		}

        public bool Publish(string channel, object message, bool storeInHistory, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Publish(channel, message, storeInHistory, userCallback, errorCallback);
        }

        public bool Publish<T>(string channel, object message, bool storeInHistory, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.Publish<T>(channel, message, storeInHistory, userCallback, errorCallback);
        }

		public void Presence<T>(string channel, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.Presence<T>(channel, userCallback, connectCallback, errorCallback);
		}

        public void Presence<T>(string channel, string channelGroup, Action<T> userCallback, Action<T> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Presence<T>(channel, channelGroup, userCallback, connectCallback, errorCallback);
        }

		public void Presence(string channel, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.Presence(channel, userCallback, connectCallback, errorCallback);
		}

        public void Presence(string channel, string channelGroup, Action<object> userCallback, Action<object> connectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Presence(channel, channelGroup, userCallback, connectCallback, errorCallback);
        }

		public bool DetailedHistory(string channel, long start, long end, int count, bool reverse, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.DetailedHistory(channel, start, end, count, reverse, userCallback, errorCallback);
		}

		public bool DetailedHistory<T>(string channel, long start, long end, int count, bool reverse, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.DetailedHistory<T>(channel, start, end, count, reverse, userCallback, errorCallback);
		}

		public bool DetailedHistory(string channel, long start, Action<object> userCallback, Action<PubnubClientError> errorCallback, bool reverse)
		{
			return DetailedHistory<object>(channel, start, -1, -1, reverse, userCallback, errorCallback);
		}

		public bool DetailedHistory<T>(string channel, long start, Action<T> userCallback, Action<PubnubClientError> errorCallback, bool reverse)
		{
			return DetailedHistory<T>(channel, start, -1, -1, reverse, userCallback, errorCallback);
		}

		public bool DetailedHistory(string channel, int count, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return DetailedHistory<object>(channel, -1, -1, count, false, userCallback, errorCallback);
		}

		public bool DetailedHistory<T>(string channel, int count, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return DetailedHistory<T>(channel, -1, -1, count, false, userCallback, errorCallback);
		}

		public bool HereNow(string channel, Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.HereNow(channel, userCallback, errorCallback);
		}

        public bool HereNow(string channel, bool showUUIDList, bool includeUserState, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.HereNow(channel, showUUIDList, includeUserState, userCallback, errorCallback);
        }

		public bool HereNow<T>(string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.HereNow<T>(channel, userCallback, errorCallback);
		}

        public bool HereNow<T>(string channel, bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.HereNow<T>(channel, showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public void GlobalHereNow(bool showUUIDList, bool includeUserState, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GlobalHereNow(showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public bool GlobalHereNow<T>(bool showUUIDList, bool includeUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GlobalHereNow<T>(showUUIDList, includeUserState, userCallback, errorCallback);
        }

        public void WhereNow(string uuid, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.WhereNow(uuid, userCallback, errorCallback);
        }

        public void WhereNow<T>(string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.WhereNow<T>(uuid, userCallback, errorCallback);
        }

        public void Unsubscribe<T>(string channel, string channelGroup, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Unsubscribe<T>(channel, channelGroup, userCallback, connectCallback, disconnectCallback, errorCallback);
        }

        public void Unsubscribe(string channel, string channelGroup, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.Unsubscribe(channel, channelGroup, userCallback, connectCallback, disconnectCallback, errorCallback);
        }


		public void Unsubscribe<T>(string channel, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.Unsubscribe<T>(channel, null, userCallback, connectCallback, disconnectCallback, errorCallback);
		}

		public void Unsubscribe(string channel, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.Unsubscribe(channel, null, userCallback, connectCallback, disconnectCallback, errorCallback);
		}

        public void PresenceUnsubscribe(string channel, string channelGroup, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.PresenceUnsubscribe(channel, channelGroup, userCallback, connectCallback, disconnectCallback, errorCallback);
        }

        public void PresenceUnsubscribe<T>(string channel, string channelGroup, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.PresenceUnsubscribe<T>(channel, channelGroup, userCallback, connectCallback, disconnectCallback, errorCallback);
        }

		public void PresenceUnsubscribe(string channel, Action<object> userCallback, Action<object> connectCallback, Action<object> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.PresenceUnsubscribe(channel, userCallback, connectCallback, disconnectCallback, errorCallback);
		}

		public void PresenceUnsubscribe<T>(string channel, Action<T> userCallback, Action<T> connectCallback, Action<T> disconnectCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.PresenceUnsubscribe<T>(channel, userCallback, connectCallback, disconnectCallback, errorCallback);
		}

		public bool Time(Action<object> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.Time(userCallback, errorCallback);
		}

		public bool Time<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.Time<T> (userCallback, errorCallback);
		}

        public void AuditAccess<T>(string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditAccess(channel, authenticationKey, userCallback, errorCallback);
        }

        public void AuditAccess<T>(string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            pubnub.AuditAccess(channel, userCallback, errorCallback);
		}

        public void AuditAccess<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			pubnub.AuditAccess<T>(userCallback, errorCallback);
		}

        public void AuditPresenceAccess<T>(string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditPresenceAccess<T>(channel, userCallback, errorCallback);
        }

        public void AuditPresenceAccess<T>(string channel, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AuditPresenceAccess<T>(channel, authenticationKey, userCallback, errorCallback);
        }

        public bool GrantAccess<T>(string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.GrantAccess<T>(channel, read, write, ttl, userCallback, errorCallback);
		}

		public bool GrantAccess<T>(string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
			return pubnub.GrantAccess<T>(channel, read, write, userCallback, errorCallback);
		}

        public bool GrantAccess<T>(string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantAccess<T>(channel, authenticationKey, read, write, ttl, userCallback, errorCallback);
        }

        public bool GrantAccess<T>(string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantAccess<T>(channel, authenticationKey, read, write, userCallback, errorCallback);
        }


		public bool GrantPresenceAccess<T>(string channel, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            return pubnub.GrantPresenceAccess<T>(channel, read, write, userCallback, errorCallback);
		}

		public bool GrantPresenceAccess<T>(string channel, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
		{
            return pubnub.GrantPresenceAccess(channel, read, write, ttl, userCallback, errorCallback);
		}

        public bool GrantPresenceAccess<T>(string channel, string authenticationKey, bool read, bool write, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantPresenceAccess<T>(channel, authenticationKey, read, write, userCallback, errorCallback);
        }

        public bool GrantPresenceAccess<T>(string channel, string authenticationKey, bool read, bool write, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.GrantPresenceAccess(channel, authenticationKey, read, write, ttl, userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(string channelGroup, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.ChannelGroupAuditAccess(channelGroup, authenticationKey, userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(string channelGroup, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.ChannelGroupAuditAccess(channelGroup, userCallback, errorCallback);
        }

        public void ChannelGroupAuditAccess<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.ChannelGroupAuditAccess<T>(userCallback, errorCallback);
        }

        public void ChannelGroupAuditPresenceAccess<T>(string channelGroup, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.ChannelGroupAuditPresenceAccess<T>(channelGroup, userCallback, errorCallback);
        }

        public void ChannelGroupAuditPresenceAccess<T>(string channelGroup, string authenticationKey, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.ChannelGroupAuditPresenceAccess<T>(channelGroup, authenticationKey, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, bool read, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantAccess<T>(channelGroup, read, false, manage, ttl, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, bool read, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantAccess<T>(channelGroup, read, false, manage, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, string authenticationKey, bool read, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantAccess<T>(channelGroup, authenticationKey, read, false, manage, ttl, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantAccess<T>(string channelGroup, string authenticationKey, bool read, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantAccess<T>(channelGroup, authenticationKey, read, false, manage, userCallback, errorCallback);
        }


        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, bool read, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantPresenceAccess<T>(channelGroup, read, false, manage, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, bool read, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantPresenceAccess(channelGroup, read, false, manage, ttl, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, string authenticationKey, bool read, bool manage, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantPresenceAccess<T>(channelGroup, authenticationKey, read, false, manage, userCallback, errorCallback);
        }

        public bool ChannelGroupGrantPresenceAccess<T>(string channelGroup, string authenticationKey, bool read, bool manage, int ttl, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            return pubnub.ChannelGroupGrantPresenceAccess(channelGroup, authenticationKey, read, false, manage, ttl, userCallback, errorCallback);
        }


        public void SetUserState<T>(string channel, string channelGroup, string uuid, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T>(channel, channelGroup, uuid, jsonUserState, userCallback, errorCallback);
        }
        
        public void SetUserState<T>(string channel, string channelGroup, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T>(channel, channelGroup, "", jsonUserState, userCallback, errorCallback);
        }

        public void SetUserState<T>(string channel, string jsonUserState, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T>(channel,"", jsonUserState, userCallback, errorCallback);
        }

        public void SetUserState<T>(string channel, string channelGroup, string uuid, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T>(channel, channelGroup, uuid, keyValuePair, userCallback, errorCallback);
        }

        public void SetUserState<T>(string channel, string channelGroup, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T>(channel, channelGroup, "", keyValuePair, userCallback, errorCallback);
        }

        public void SetUserState<T>(string channel, System.Collections.Generic.KeyValuePair<string, object> keyValuePair, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.SetUserState<T>(channel, "", keyValuePair, userCallback, errorCallback);
        }

        public void GetUserState<T>(string channel, string channelGroup, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetUserState<T>(channel, channelGroup, "", userCallback, errorCallback);
        }

        //public void GetUserState<T>(string channel, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        //{
        //    pubnub.GetUserState<T>(channel, "", userCallback, errorCallback);
        //}

        public void GetUserState<T>(string channel, string channelGroup, string uuid, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetUserState<T>(channel, channelGroup, uuid, userCallback, errorCallback);
        }

        public void RegisterDeviceForPush<T>(string channel, PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RegisterDeviceForPush<T>(channel, pushType, pushToken, userCallback, errorCallback);
        }
        public void RegisterDeviceForPush(string channel, PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RegisterDeviceForPush<object>(channel, pushType, pushToken, userCallback, errorCallback);
        }

        public void UnregisterDeviceForPush<T>(PushTypeService pushType, string pushToken, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.UnregisterDeviceForPush<T>(pushType, pushToken, userCallback, errorCallback);
        }
        public void UnregisterDeviceForPush(PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.UnregisterDeviceForPush<object>(pushType, pushToken, userCallback, errorCallback);
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
        public void GetChannelsForDevicePush(PushTypeService pushType, string pushToken, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetChannelsForDevicePush<object>(pushType, pushToken, userCallback, errorCallback);
        }
        #endregion

        #region "PubNub API Channel Group Methods"
        public void AddChannelsToChannelGroup(string[] channels, string groupName, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AddChannelsToChannelGroup<object>(channels, groupName, userCallback, errorCallback);
        }

        public void AddChannelsToChannelGroup<T>(string[] channels, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AddChannelsToChannelGroup<T>(channels, groupName, userCallback, errorCallback);
        }

        public void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AddChannelsToChannelGroup<object>(channels, nameSpace, groupName, userCallback, errorCallback);
        }

        public void AddChannelsToChannelGroup<T>(string[] channels, string nameSpace, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.AddChannelsToChannelGroup<T>(channels, nameSpace, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelsFromChannelGroup(string[] channels, string nameSpace, string groupName, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelsFromChannelGroup(channels, nameSpace, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelsFromChannelGroup<T>(string[] channels, string nameSpace, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelsFromChannelGroup<T>(channels, nameSpace, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelsFromChannelGroup<T>(string[] channels, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelsFromChannelGroup<T>(channels, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelsFromChannelGroup(string[] channels, string groupName, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelsFromChannelGroup(channels, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelGroup(string nameSpace, string groupName, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelGroup(nameSpace, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelGroup<T>(string nameSpace, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelGroup<T>(nameSpace, groupName, userCallback, errorCallback);
        }

        public void RemoveChannelGroupNameSpace(string nameSpace, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelGroupNameSpace(nameSpace, userCallback, errorCallback);
        }

        public void RemoveChannelGroupNameSpace<T>(string nameSpace, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.RemoveChannelGroupNameSpace<T>(nameSpace, userCallback, errorCallback);
        }

        public void GetChannelsForChannelGroup(string nameSpace, string groupName, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetChannelsForChannelGroup(nameSpace, groupName, userCallback, errorCallback);
        }

        public void GetChannelsForChannelGroup<T>(string nameSpace, string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetChannelsForChannelGroup<T>(nameSpace, groupName, userCallback, errorCallback);
        }

        public void GetChannelsForChannelGroup(string groupName, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetChannelsForChannelGroup(groupName, userCallback, errorCallback);
        }
        
        public void GetChannelsForChannelGroup<T>(string groupName, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetChannelsForChannelGroup<T>(groupName, userCallback, errorCallback);
        }
        
        public void GetAllChannelGroups(string nameSpace, Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetAllChannelGroups(nameSpace, userCallback, errorCallback);
        }
        
        public void GetAllChannelGroups<T>(string nameSpace, Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetAllChannelGroups<T>(nameSpace, userCallback, errorCallback);
        }

        public void GetAllChannelGroups(Action<object> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetAllChannelGroups(userCallback, errorCallback);
        }

        public void GetAllChannelGroups<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetAllChannelGroups<T>(userCallback, errorCallback);
        }
        public void GetAllChannelGroupNamespaces<T>(Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            pubnub.GetAllChannelGroupNamespaces<T>(userCallback, errorCallback);
        }

        public void GetAllChannelGroupNamespaces(Action<object> userCallback, Action<PubnubClientError> errorCallback)
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
			pubnub.EndPendingRequests();
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


		#endregion

		#region "Properties"
		public string AuthenticationKey {
			get {return pubnub.AuthenticationKey;}
			set {pubnub.AuthenticationKey = value;}
		}

		public int LocalClientHeartbeatInterval {
			get {return pubnub.LocalClientHeartbeatInterval;}
			set {pubnub.LocalClientHeartbeatInterval = value;}
		}

		public int NetworkCheckRetryInterval {
			get {return pubnub.NetworkCheckRetryInterval;}
			set {pubnub.NetworkCheckRetryInterval = value;}
		}

		public int NetworkCheckMaxRetries {
			get {return pubnub.NetworkCheckMaxRetries;}
			set {pubnub.NetworkCheckMaxRetries = value;}
		}

		public int NonSubscribeTimeout {
			get {return pubnub.NonSubscribeTimeout;}
			set {pubnub.NonSubscribeTimeout = value;}
		}

		public int SubscribeTimeout {
			get {return pubnub.SubscribeTimeout;}
			set {pubnub.SubscribeTimeout = value;}
		}

		public bool EnableResumeOnReconnect {
			get {return pubnub.EnableResumeOnReconnect;}
			set {pubnub.EnableResumeOnReconnect = value;}
		}

		public string SessionUUID {
			get {return pubnub.SessionUUID;}
			set {pubnub.SessionUUID = value;}
		}

		public string Origin {
			get {return pubnub.Origin;}
			set {pubnub.Origin = value;}
		}

        public int PresenceHeartbeat
        {
            get
            {
                return pubnub.PresenceHeartbeat;
            }
            set
            {
                pubnub.PresenceHeartbeat = value;
            }
        }

        public int PresenceHeartbeatInterval
        {
            get
            {
                return pubnub.PresenceHeartbeatInterval;
            }
            set
            {
                pubnub.PresenceHeartbeatInterval = value;
            }
        }
        
        public IPubnubUnitTest PubnubUnitTest
		{
			get
			{
				return pubnub.PubnubUnitTest;
			}
			set
			{
				pubnub.PubnubUnitTest = value;
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

        public IJsonPluggableLibrary JsonPluggableLibrary
        {
            get
            {
                return pubnub.JsonPluggableLibrary;
            }
            set
            {
                pubnub.JsonPluggableLibrary = value;
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
		#endregion

		#region "Constructors"

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