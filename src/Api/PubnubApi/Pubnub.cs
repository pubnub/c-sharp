using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace PubnubApi
{
    public class Pubnub
	{
        private PNConfiguration pubnubConfig;
        private IPubnubUnitTest pubnubUnitTest;
        private IPubnubLog pubnubLog;
        private EndPoint.ListenerManager listenerManager;
        private readonly EndPoint.TelemetryManager telemetryManager;
        private readonly EndPoint.TokenManager tokenManager;
        private object savedSubscribeOperation;
        private readonly string savedSdkVerion;

        static Pubnub() 
        {
#if NET35 || NET40
            var assemblyVersion = typeof(Pubnub).Assembly.GetName().Version;
#else
            var assembly = typeof(Pubnub).GetTypeInfo().Assembly;
            var assemblyName = new AssemblyName(assembly.FullName);
            string assemblyVersion = assemblyName.Version.ToString();
#endif
            Version = string.Format("{0}CSharp{1}", PNPlatform.Get(), assemblyVersion);
        }

        #region "PubNub API Channel Methods"

        public EndPoint.SubscribeOperation<T> Subscribe<T>()
		{
            EndPoint.SubscribeOperation<T> subscribeOperation = new EndPoint.SubscribeOperation<T>(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            subscribeOperation.CurrentPubnubInstance(this);
            savedSubscribeOperation = subscribeOperation;
            return subscribeOperation;
        }

        public EndPoint.StatelessSubscribeOperation<T> StatelessSubscribe<T>()
        {
            EndPoint.StatelessSubscribeOperation<T> subscribeOperation = new EndPoint.StatelessSubscribeOperation<T>(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            //subscribeOperation.CurrentPubnubInstance(this);
            savedSubscribeOperation = subscribeOperation;
            return subscribeOperation;
        }

        public EndPoint.UnsubscribeOperation<T> Unsubscribe<T>()
        {
            EndPoint.UnsubscribeOperation<T>  unsubscribeOperation = new EndPoint.UnsubscribeOperation<T>(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            unsubscribeOperation.CurrentPubnubInstance(this);
            return unsubscribeOperation;
        }

        public EndPoint.UnsubscribeAllOperation<T> UnsubscribeAll<T>()
        {
            EndPoint.UnsubscribeAllOperation<T> unSubscribeAllOperation = new EndPoint.UnsubscribeAllOperation<T>(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return unSubscribeAllOperation;
        }

        public EndPoint.PublishOperation Publish()
        {
            EndPoint.PublishOperation publishOperation = new EndPoint.PublishOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            publishOperation.CurrentPubnubInstance(this);
            return publishOperation;
        }

        public EndPoint.FireOperation Fire()
        {
            EndPoint.FireOperation fireOperation = new EndPoint.FireOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            fireOperation.CurrentPubnubInstance(this);
            return fireOperation;
        }

        public EndPoint.SignalOperation Signal()
        {
            EndPoint.SignalOperation signalOperation = new EndPoint.SignalOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return signalOperation;
        }

        public EndPoint.HistoryOperation History()
		{
            EndPoint.HistoryOperation historyOperaton = new EndPoint.HistoryOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return historyOperaton;
        }

        public EndPoint.FetchHistoryOperation FetchHistory()
        {
            EndPoint.FetchHistoryOperation historyOperaton = new EndPoint.FetchHistoryOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return historyOperaton;
        }

        public EndPoint.DeleteMessageOperation DeleteMessages()
        {
            EndPoint.DeleteMessageOperation deleteMessageOperaton = new EndPoint.DeleteMessageOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return deleteMessageOperaton;
        }

        public EndPoint.MessageCountsOperation MessageCounts()
        {
            EndPoint.MessageCountsOperation messageCount = new EndPoint.MessageCountsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            messageCount.CurrentPubnubInstance(this);
            return messageCount;
        }

        public EndPoint.HereNowOperation HereNow()
		{
            EndPoint.HereNowOperation hereNowOperation = new EndPoint.HereNowOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            hereNowOperation.CurrentPubnubInstance(this);
            return hereNowOperation;
        }

		public EndPoint.WhereNowOperation WhereNow()
		{
            EndPoint.WhereNowOperation whereNowOperation = new EndPoint.WhereNowOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            whereNowOperation.CurrentPubnubInstance(this);
            return whereNowOperation;
        }

		public EndPoint.TimeOperation Time()
		{
            EndPoint.TimeOperation timeOperation = new EndPoint.TimeOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, this);
            timeOperation.CurrentPubnubInstance(this);
            return timeOperation;
        }

		public EndPoint.AuditOperation Audit()
		{
            EndPoint.AuditOperation auditOperation = new EndPoint.AuditOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, this);
            auditOperation.CurrentPubnubInstance(this);
            return auditOperation;
        }

        public EndPoint.GrantTokenOperation GrantToken()
        {
            EndPoint.GrantTokenOperation grantOperation = new EndPoint.GrantTokenOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return grantOperation;
        }

        public EndPoint.RevokeTokenOperation RevokeToken()
        {
            EndPoint.RevokeTokenOperation revokeTokenOperation = new EndPoint.RevokeTokenOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return revokeTokenOperation;
        }

        public EndPoint.GrantOperation Grant()
        {
            EndPoint.GrantOperation grantOperation = new EndPoint.GrantOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, this);
            grantOperation.CurrentPubnubInstance(this);
            return grantOperation;
        }

        public EndPoint.SetStateOperation SetPresenceState()
		{
            EndPoint.SetStateOperation setStateOperation = new EndPoint.SetStateOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return setStateOperation;
        }

		public EndPoint.GetStateOperation GetPresenceState()
		{
            EndPoint.GetStateOperation getStateOperation = new EndPoint.GetStateOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            getStateOperation.CurrentPubnubInstance(this);
            return getStateOperation;
        }

		public EndPoint.AddPushChannelOperation AddPushNotificationsOnChannels()
		{
            EndPoint.AddPushChannelOperation addPushChannelOperation = new EndPoint.AddPushChannelOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return addPushChannelOperation;
        }

		public EndPoint.RemovePushChannelOperation RemovePushNotificationsFromChannels()
		{
            EndPoint.RemovePushChannelOperation removePushChannelOperation = new EndPoint.RemovePushChannelOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return removePushChannelOperation;
        }

        public EndPoint.RemoveAllPushChannelsOperation RemoveAllPushNotificationsFromDeviceWithPushToken()
        {
            EndPoint.RemoveAllPushChannelsOperation removeAllPushChannelsOperation = new EndPoint.RemoveAllPushChannelsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            removeAllPushChannelsOperation.CurrentPubnubInstance(this);
            return removeAllPushChannelsOperation;
        }

        public EndPoint.AuditPushChannelOperation AuditPushChannelProvisions()
		{
            EndPoint.AuditPushChannelOperation auditPushChannelOperation = new EndPoint.AuditPushChannelOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            auditPushChannelOperation.CurrentPubnubInstance(this);
            return auditPushChannelOperation;
        }

        public EndPoint.SetUuidMetadataOperation SetUuidMetadata()
        {
            EndPoint.SetUuidMetadataOperation setUuidMetadataOperation = new EndPoint.SetUuidMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return setUuidMetadataOperation;
        }

        public EndPoint.RemoveUuidMetadataOperation RemoveUuidMetadata()
        {
            EndPoint.RemoveUuidMetadataOperation removeUuidMetadataOperation = new EndPoint.RemoveUuidMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return removeUuidMetadataOperation;
        }

        public EndPoint.GetAllUuidMetadataOperation GetAllUuidMetadata()
        {
            EndPoint.GetAllUuidMetadataOperation getAllUuidMetadataOperation = new EndPoint.GetAllUuidMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getAllUuidMetadataOperation;
        }

        public EndPoint.GetUuidMetadataOperation GetUuidMetadata()
        {
            EndPoint.GetUuidMetadataOperation getUuidMetadataOperation = new EndPoint.GetUuidMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getUuidMetadataOperation;
        }

        public EndPoint.SetChannelMetadataOperation SetChannelMetadata()
        {
            EndPoint.SetChannelMetadataOperation setChannelMetadataOperation = new EndPoint.SetChannelMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return setChannelMetadataOperation;
        }

        public EndPoint.RemoveChannelMetadataOperation RemoveChannelMetadata()
        {
            EndPoint.RemoveChannelMetadataOperation removeChannelMetadataOperation = new EndPoint.RemoveChannelMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return removeChannelMetadataOperation;
        }

        public EndPoint.GetAllChannelMetadataOperation GetAllChannelMetadata()
        {
            EndPoint.GetAllChannelMetadataOperation getAllChannelMetadataOperation = new EndPoint.GetAllChannelMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getAllChannelMetadataOperation;
        }

        public EndPoint.GetChannelMetadataOperation GetChannelMetadata()
        {
            EndPoint.GetChannelMetadataOperation getSingleSpaceOperation = new EndPoint.GetChannelMetadataOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getSingleSpaceOperation;
        }

        public EndPoint.GetMembershipsOperation GetMemberships()
        {
            EndPoint.GetMembershipsOperation getMembershipOperation = new EndPoint.GetMembershipsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getMembershipOperation;
        }

        public EndPoint.SetMembershipsOperation SetMemberships()
        {
            EndPoint.SetMembershipsOperation setMembershipsOperation = new EndPoint.SetMembershipsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return setMembershipsOperation;
        }
        public EndPoint.RemoveMembershipsOperation RemoveMemberships()
        {
            EndPoint.RemoveMembershipsOperation removeMembershipsOperation = new EndPoint.RemoveMembershipsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return removeMembershipsOperation;
        }
        public EndPoint.ManageMembershipsOperation ManageMemberships()
        {
            EndPoint.ManageMembershipsOperation manageMembershipsOperation = new EndPoint.ManageMembershipsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return manageMembershipsOperation;
        }

        public EndPoint.GetChannelMembersOperation GetChannelMembers()
        {
            EndPoint.GetChannelMembersOperation getChannelMembersOperation = new EndPoint.GetChannelMembersOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getChannelMembersOperation;
        }

        public EndPoint.SetChannelMembersOperation SetChannelMembers()
        {
            EndPoint.SetChannelMembersOperation setChannelMembersOperation = new EndPoint.SetChannelMembersOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return setChannelMembersOperation;
        }

        public EndPoint.RemoveChannelMembersOperation RemoveChannelMembers()
        {
            EndPoint.RemoveChannelMembersOperation removeChannelMembersOperation = new EndPoint.RemoveChannelMembersOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return removeChannelMembersOperation;
        }

        public EndPoint.ManageChannelMembersOperation ManageChannelMembers()
        {
            EndPoint.ManageChannelMembersOperation channelMembersOperation = new EndPoint.ManageChannelMembersOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return channelMembersOperation;
        }

        public EndPoint.AddMessageActionOperation AddMessageAction()
        {
            EndPoint.AddMessageActionOperation addMessageActionOperation = new EndPoint.AddMessageActionOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return addMessageActionOperation;
        }

        public EndPoint.RemoveMessageActionOperation RemoveMessageAction()
        {
            EndPoint.RemoveMessageActionOperation removeMessageActionOperation = new EndPoint.RemoveMessageActionOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return removeMessageActionOperation;
        }

        public EndPoint.GetMessageActionsOperation GetMessageActions()
        {
            EndPoint.GetMessageActionsOperation getMessageActionsOperation = new EndPoint.GetMessageActionsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getMessageActionsOperation;
        }

#endregion

#region "PubNub API Channel Group Methods"

        public EndPoint.AddChannelsToChannelGroupOperation AddChannelsToChannelGroup()
		{
            EndPoint.AddChannelsToChannelGroupOperation addChannelToChannelGroupOperation = new EndPoint.AddChannelsToChannelGroupOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            addChannelToChannelGroupOperation.CurrentPubnubInstance(this);
            return addChannelToChannelGroupOperation;
        }

		public EndPoint.RemoveChannelsFromChannelGroupOperation RemoveChannelsFromChannelGroup()
		{
            EndPoint.RemoveChannelsFromChannelGroupOperation removeChannelsFromChannelGroupOperation = new EndPoint.RemoveChannelsFromChannelGroupOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            removeChannelsFromChannelGroupOperation.CurrentPubnubInstance(this);
            return removeChannelsFromChannelGroupOperation;
        }

		public EndPoint.DeleteChannelGroupOperation DeleteChannelGroup()
		{
            EndPoint.DeleteChannelGroupOperation deleteChannelGroupOperation = new EndPoint.DeleteChannelGroupOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            deleteChannelGroupOperation.CurrentPubnubInstance(this);
            return deleteChannelGroupOperation;
        }

		public EndPoint.ListChannelsForChannelGroupOperation ListChannelsForChannelGroup()
		{
            EndPoint.ListChannelsForChannelGroupOperation listChannelsForChannelGroupOperation = new EndPoint.ListChannelsForChannelGroupOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            listChannelsForChannelGroupOperation.CurrentPubnubInstance(this);
            return listChannelsForChannelGroupOperation;
        }

        public EndPoint.ListAllChannelGroupOperation ListChannelGroups()
		{
            EndPoint.ListAllChannelGroupOperation listAllChannelGroupOperation = new EndPoint.ListAllChannelGroupOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            listAllChannelGroupOperation.CurrentPubnubInstance(this);
            return listAllChannelGroupOperation;
        }

        public bool AddListener(SubscribeCallback listener)
        {
            if (listenerManager == null)
            {
                listenerManager = new EndPoint.ListenerManager(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            }
            return listenerManager.AddListener(listener);
        }

        public bool RemoveListener(SubscribeCallback listener)
        {
            bool ret = false;
            if (listenerManager != null)
            {
                ret = listenerManager.RemoveListener(listener);
            }
            return ret;
        }
        #endregion

        public EndPoint.SendFileOperation SendFile()
        {
            EndPoint.SendFileOperation uploadFileOperation = new EndPoint.SendFileOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return uploadFileOperation;
        }

        public EndPoint.GetFileUrlOperation GetFileUrl()
        {
            EndPoint.GetFileUrlOperation getFileUrlOperation = new EndPoint.GetFileUrlOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getFileUrlOperation;
        }

        public EndPoint.DownloadFileOperation DownloadFile()
        {
            EndPoint.DownloadFileOperation downloadFileOperation = new EndPoint.DownloadFileOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return downloadFileOperation;
        }

        public EndPoint.ListFilesOperation ListFiles()
        {
            EndPoint.ListFilesOperation listFilesOperation = new EndPoint.ListFilesOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return listFilesOperation;
        }

        public EndPoint.DeleteFileOperation DeleteFile()
        {
            EndPoint.DeleteFileOperation deleteFileOperation = new EndPoint.DeleteFileOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return deleteFileOperation;
        }

        public EndPoint.PublishFileMessageOperation PublishFileMessage()
        {
            EndPoint.PublishFileMessageOperation publshFileMessageOperation = new EndPoint.PublishFileMessageOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return publshFileMessageOperation;
        }

        #region "PubNub API Other Methods"
        public void TerminateCurrentSubscriberRequest()
		{
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            endpoint.TerminateCurrentSubscriberRequest();
		}

		public void EnableMachineSleepModeForTestingOnly()
		{
            EndPoint.OtherOperation.EnableMachineSleepModeForTestingOnly();
		}

		public void DisableMachineSleepModeForTestingOnly()
		{
            EndPoint.OtherOperation.DisableMachineSleepModeForTestingOnly();
		}

        public Guid GenerateGuid()
		{
			return Guid.NewGuid();
		}

        public void ChangeUUID(string newUUID)
        {
            if (newUUID != null && string.IsNullOrEmpty(newUUID.Trim()))
            {
                if (pubnubLog != null && pubnubConfig != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, UUID cannot be null/empty.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
                }
                throw new MissingMemberException("UUID cannot be null/empty");
            }
            EndPoint.OtherOperation endPoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            endPoint.CurrentPubnubInstance(this);
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

        public List<string> GetSubscribedChannels()
        {
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannels();
        }

        public List<string> GetSubscribedChannelGroups()
        {
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannelGroups();
        }

        public void Destroy()
        {
            savedSubscribeOperation = null;
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, tokenManager, this);
            endpoint.CurrentPubnubInstance(this);
            endpoint.EndPendingRequests();
        }

        /// <summary>
        /// Parses the token and provides token details. This is client only method (works without secret key)
        /// </summary>
        /// <param name="token">string</param>
        /// <returns>PNTokenContent</returns>
        public PNTokenContent ParseToken(string token)
        {
            PNTokenContent result = null;
            if (tokenManager != null)
            {
                result = tokenManager.ParseToken(token);
            }
            return result;
        }

        /// <summary>
        /// Sets the auth token.  This is client only method (works without secret key)
        /// </summary>
        /// <param name="token"></param>
        public void SetAuthToken(string token)
        {
            if (tokenManager != null)
            {
                tokenManager.SetAuthToken(token);
            }
        }

        public bool Reconnect<T>()
        {
            bool ret = false;
            if (savedSubscribeOperation is EndPoint.SubscribeOperation<T>)
            {
                EndPoint.SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as EndPoint.SubscribeOperation<T>;
                if (subscibeOperationInstance != null)
                {
                    ret = subscibeOperationInstance.Retry(true, false);
                }
            }
            return ret;
        }

        public bool Reconnect<T>(bool resetSubscribeTimetoken)
        {
            bool ret = false;
            if (savedSubscribeOperation is EndPoint.SubscribeOperation<T>)
            {
                EndPoint.SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as EndPoint.SubscribeOperation<T>;
                if (subscibeOperationInstance != null)
                {
                    ret = subscibeOperationInstance.Retry(true, resetSubscribeTimetoken);
                }
            }
            return ret;
        }

        public bool Disconnect<T>()
        {
            bool ret = false;
            if (savedSubscribeOperation is EndPoint.SubscribeOperation<T>)
            {
                EndPoint.SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as EndPoint.SubscribeOperation<T>;
                if (subscibeOperationInstance != null)
                {
                    ret = subscibeOperationInstance.Retry(false);
                }
            }
            return ret;
        }

        public string Decrypt(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                throw new ArgumentException("CipherKey missing");
            }

            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Decrypt(inputString);
        }

        public string Decrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            PubnubCrypto pc = new PubnubCrypto(cipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Decrypt(inputString);
        }

        public string Encrypt(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                throw new MissingMemberException("CipherKey missing");
            }

            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Encrypt(inputString);
        }

        public string Encrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            PubnubCrypto pc = new PubnubCrypto(cipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Encrypt(inputString);
        }

        public byte[] EncryptFile(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }
            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                throw new MissingMemberException("CipherKey missing");
            }
            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Encrypt(inputBytes, true);
        }
        public byte[] EncryptFile(byte[] inputBytes, string cipherKey)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }
            if (pubnubConfig != null && !string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                //Ignore this. Added this condition to pass Codacy recommendation of making this as static method
            }

            PubnubCrypto pc = new PubnubCrypto(cipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Encrypt(inputBytes, true);
        }
        public void EncryptFile(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                throw new MissingMemberException("CipherKey missing");
            }
#if !NETSTANDARD10 && !NETSTANDARD11
            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }
            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = EncryptFile(inputBytes);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
#else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif
        }
        public void EncryptFile(string sourceFile, string destinationFile, string cipherKey)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }

#if !NETSTANDARD10 && !NETSTANDARD11
            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }
            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = EncryptFile(inputBytes, cipherKey);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
#else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif
        }

        public byte[] DecryptFile(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }

            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                throw new ArgumentException("CipherKey missing");
            }

            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Decrypt(inputBytes, true);
        }
        public void DecryptFile(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("sourceFile is not valid");
            }

            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                throw new ArgumentException("CipherKey missing");
            }

#if !NETSTANDARD10 && !NETSTANDARD11
            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }
            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = DecryptFile(inputBytes, pubnubConfig.CipherKey);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
#else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif
        }
        public byte[] DecryptFile(byte[] inputBytes, string cipherKey)
        {
            if (inputBytes == null)
            {
                throw new ArgumentException("inputBytes is not valid");
            }
            if (pubnubConfig != null && !string.IsNullOrEmpty(pubnubConfig.CipherKey))
            {
                //Ignore this. Added this condition to pass Codacy recommendation of making this as static method
            }
            PubnubCrypto pc = new PubnubCrypto(cipherKey, pubnubConfig, pubnubLog, pubnubUnitTest);
            return pc.Decrypt(inputBytes, true);
        }
        public void DecryptFile(string sourceFile, string destinationFile, string cipherKey)
        {
            if (string.IsNullOrEmpty(sourceFile) || sourceFile.Length < 1)
            {
                throw new ArgumentException("inputFile is not valid");
            }
#if !NETSTANDARD10 && !NETSTANDARD11
            bool validSource = System.IO.File.Exists(sourceFile);
            if (!validSource)
            {
                throw new ArgumentException("sourceFile is not valid");
            }
            string destDirectory = System.IO.Path.GetDirectoryName(destinationFile);
            bool validDest = System.IO.Directory.Exists(destDirectory);
            if (!string.IsNullOrEmpty(destDirectory) && !validDest)
            {
                throw new ArgumentException("destination path is not valid");
            }
            byte[] inputBytes = System.IO.File.ReadAllBytes(sourceFile);
            byte[] outputBytes = DecryptFile(inputBytes, cipherKey);
            System.IO.File.WriteAllBytes(destinationFile, outputBytes);
#else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif
        }
        #endregion

        #region "Properties"
        public IPubnubUnitTest PubnubUnitTest
        {
            get
            {
                return pubnubUnitTest;
            }
            set
            {
                pubnubUnitTest = value;
                if (pubnubUnitTest != null)
                {
                    Version = pubnubUnitTest.SdkVersion;
                }
                else
                {
                    Version = savedSdkVerion;
                }
            }
        }

        public PNConfiguration PNConfig
        {
            get
            {
                return pubnubConfig;
            }
        }

        public IJsonPluggableLibrary JsonPluggableLibrary { get; internal set; }

        public void SetJsonPluggableLibrary(IJsonPluggableLibrary customJson)
        {
            JsonPluggableLibrary = customJson;
        }

        public static string Version { get; private set; }



        public string InstanceId { get; private set; }

        #endregion

        #region "Constructors"

        public Pubnub(PNConfiguration config)
        {
            savedSdkVerion = Version;
            InstanceId = Guid.NewGuid().ToString();
            pubnubConfig = config;
            if (config != null)
            {
                pubnubLog = config.PubnubLog;
            }
            JsonPluggableLibrary = new NewtonsoftJsonDotNet(config, pubnubLog);
            if (config != null && config.EnableTelemetry)
            {
                telemetryManager = new EndPoint.TelemetryManager(pubnubConfig, pubnubLog);
            }
            CheckRequiredConfigValues();
            if (config != null)
            {
                tokenManager = new EndPoint.TokenManager(pubnubConfig, JsonPluggableLibrary, pubnubLog, this.InstanceId);
            }
            if (config != null && pubnubLog != null)
            {
                PNPlatform.Print(config, pubnubLog);
            }
            if (config != null && config.PresenceTimeout < 20)
            {
                config.PresenceTimeout = 20;
                if (pubnubLog != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, WARNING: The PresenceTimeout cannot be less than 20, defaulting the value to 20. Please update the settings in your code.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                }
            }
            if (config != null && (string.IsNullOrEmpty(config.Uuid) || string.IsNullOrEmpty(config.Uuid.Trim())))
            {
                if (pubnubLog != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, PNConfiguration.Uuid is required to use the SDK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                }
                throw new MissingMemberException("PNConfiguration.Uuid is required to use the SDK");
            }
            if (config != null && config.EnableSubscribeBeta)
            {
                throw new NotImplementedException();
            }
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

                if (string.IsNullOrEmpty(pubnubConfig.CipherKey))
                {
                    pubnubConfig.CipherKey = "";
                }
            }
        }

#endregion
	}
}