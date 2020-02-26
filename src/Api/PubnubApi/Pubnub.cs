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

        public EndPoint.CreateUserOperation CreateUser()
        {
            EndPoint.CreateUserOperation createUserOperation = new EndPoint.CreateUserOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return createUserOperation;
        }

        public EndPoint.UpdateUserOperation UpdateUser()
        {
            EndPoint.UpdateUserOperation updateUserOperation = new EndPoint.UpdateUserOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return updateUserOperation;
        }

        public EndPoint.DeleteUserOperation DeleteUser()
        {
            EndPoint.DeleteUserOperation deleteUserOperation = new EndPoint.DeleteUserOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return deleteUserOperation;
        }

        public EndPoint.GetUsersOperation GetUsers()
        {
            EndPoint.GetUsersOperation getUsersOperation = new EndPoint.GetUsersOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getUsersOperation;
        }

        public EndPoint.GetUserOperation GetUser()
        {
            EndPoint.GetUserOperation getUserOperation = new EndPoint.GetUserOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getUserOperation;
        }

        public EndPoint.CreateSpaceOperation CreateSpace()
        {
            EndPoint.CreateSpaceOperation createSpaceOperation = new EndPoint.CreateSpaceOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return createSpaceOperation;
        }

        public EndPoint.UpdateSpaceOperation UpdateSpace()
        {
            EndPoint.UpdateSpaceOperation updateSpaceOperation = new EndPoint.UpdateSpaceOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return updateSpaceOperation;
        }

        public EndPoint.DeleteSpaceOperation DeleteSpace()
        {
            EndPoint.DeleteSpaceOperation deleteSpaceOperation = new EndPoint.DeleteSpaceOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return deleteSpaceOperation;
        }

        public EndPoint.GetSpacesOperation GetSpaces()
        {
            EndPoint.GetSpacesOperation getAllSpacesOperation = new EndPoint.GetSpacesOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getAllSpacesOperation;
        }

        public EndPoint.GetSpaceOperation GetSpace()
        {
            EndPoint.GetSpaceOperation getSingleSpaceOperation = new EndPoint.GetSpaceOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getSingleSpaceOperation;
        }

        public EndPoint.ManageMembershipsOperation ManageMemberships()
        {
            EndPoint.ManageMembershipsOperation membershipOperation = new EndPoint.ManageMembershipsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return membershipOperation;
        }

        public EndPoint.ManageMembersOperation ManageMembers()
        {
            EndPoint.ManageMembersOperation membersOperation = new EndPoint.ManageMembersOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return membersOperation;
        }

        public EndPoint.GetMembershipsOperation GetMemberships()
        {
            EndPoint.GetMembershipsOperation getMembershipOperation = new EndPoint.GetMembershipsOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getMembershipOperation;
        }

        public EndPoint.GetMembersOperation GetMembers()
        {
            EndPoint.GetMembersOperation getMembersOperation = new EndPoint.GetMembersOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, tokenManager, this);
            return getMembersOperation;
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
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, null, this);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannels();
        }

        public List<string> GetSubscribedChannelGroups()
        {
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, null, this);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannelGroups();
        }

        public void Destroy()
        {
            savedSubscribeOperation = null;
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, JsonPluggableLibrary, pubnubUnitTest, pubnubLog, null, null, this);
            endpoint.CurrentPubnubInstance(this);
            endpoint.EndPendingRequests();
        }

        public PNGrantToken ParseToken(string token)
        {
            PNGrantToken result = null;
            if (tokenManager != null)
            {
                result = tokenManager.ParseToken(token);
            }
            return result;
        }

        public void SetToken(string token)
        {
            if (tokenManager != null)
            {
                tokenManager.SetToken(token);
            }
        }

        public void SetTokens(string[] tokens)
        {
            if (tokenManager != null)
            {
                for (int index=0; index < tokens.Length; index++)
                {
                    tokenManager.SetToken(tokens[index]);
                }
            }
        }

        public Dictionary<PNTokenKey, string> GetTokens()
        {
            Dictionary<PNTokenKey, string> result = null;
            if (tokenManager != null)
            {
                result = tokenManager.GetAllTokens();
            }
            return result;
        }

        public string GetToken(string resourceType, string resourceId)
        {
            string result = "";
            if (tokenManager != null)
            {
                result = tokenManager.GetToken(resourceType, resourceId);
            }
            return result;
        }

        public Dictionary<PNTokenKey, string> GetTokensByResource(string resourceType)
        {
            Dictionary<PNTokenKey, string> result = null;
            if (tokenManager != null)
            {
                result = tokenManager.GetTokensByResource(resourceType);
            }
            return result;
        }

        public void ClearTokens()
        {
            if (tokenManager != null)
            {
                tokenManager.ClearTokens();
            }
        }

        public bool Reconnect<T>()
        {
            bool ret = false;
            if (savedSubscribeOperation != null && savedSubscribeOperation is EndPoint.SubscribeOperation<T>)
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

            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey);
            return pc.Decrypt(inputString);
        }

        public string Decrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            PubnubCrypto pc = new PubnubCrypto(cipherKey);
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

            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey);
            return pc.Encrypt(inputString);
        }

        public string Encrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("inputString is not valid");
            }

            PubnubCrypto pc = new PubnubCrypto(cipherKey);
            return pc.Encrypt(inputString);
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
            if (config != null && string.IsNullOrEmpty(config.SecretKey) && config.EnableTokenManager)
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