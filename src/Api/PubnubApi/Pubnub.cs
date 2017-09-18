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
        private static IPubnubUnitTest pubnubUnitTest = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.ListenerManager listenerManager = null;
        private EndPoint.TelemetryManager telemetryManager = null;

        private string instanceId = "";

        private static string sdkVersion = "PubNubCSharp4.0.4.1";

        private object savedSubscribeOperation = null;

        #region "PubNub API Channel Methods"

        public EndPoint.SubscribeOperation<T> Subscribe<T>()
		{
            EndPoint.SubscribeOperation<T> subscribeOperation = new EndPoint.SubscribeOperation<T>(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
            subscribeOperation.CurrentPubnubInstance(this);
            savedSubscribeOperation = subscribeOperation;
            return subscribeOperation;
        }

        public EndPoint.UnsubscribeOperation<T> Unsubscribe<T>()
        {
            EndPoint.UnsubscribeOperation<T>  unsubscribeOperation = new EndPoint.UnsubscribeOperation<T>(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            unsubscribeOperation.CurrentPubnubInstance(this);
            return unsubscribeOperation;
        }

        public EndPoint.UnsubscribeAllOperation<T> UnsubscribeAll<T>()
        {
            EndPoint.UnsubscribeAllOperation<T> unSubscribeAllOperation = new EndPoint.UnsubscribeAllOperation<T>(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager, this);
            //unSubscribeAllOperation.CurrentPubnubInstance(this);
            return unSubscribeAllOperation;
        }

        public EndPoint.PublishOperation Publish()
        {
            EndPoint.PublishOperation publishOperation = new EndPoint.PublishOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            publishOperation.CurrentPubnubInstance(this);
            return publishOperation;
        }

        public EndPoint.FireOperation Fire()
        {
            EndPoint.FireOperation fireOperation = new EndPoint.FireOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            fireOperation.CurrentPubnubInstance(this);
            return fireOperation;
        }

        public EndPoint.HistoryOperation History()
		{
            EndPoint.HistoryOperation historyOperaton = new EndPoint.HistoryOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            historyOperaton.CurrentPubnubInstance(this);
            return historyOperaton;
        }

        public EndPoint.DeleteMessageOperation DeleteMessages()
        {
            EndPoint.DeleteMessageOperation deleteMessageOperaton = new EndPoint.DeleteMessageOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            deleteMessageOperaton.CurrentPubnubInstance(this);
            return deleteMessageOperaton;
        }

        public EndPoint.HereNowOperation HereNow()
		{
            EndPoint.HereNowOperation hereNowOperation = new EndPoint.HereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            hereNowOperation.CurrentPubnubInstance(this);
            return hereNowOperation;
        }

		public EndPoint.WhereNowOperation WhereNow()
		{
            EndPoint.WhereNowOperation whereNowOperation = new EndPoint.WhereNowOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            whereNowOperation.CurrentPubnubInstance(this);
            return whereNowOperation;
        }

		public EndPoint.TimeOperation Time()
		{
            EndPoint.TimeOperation timeOperation = new EndPoint.TimeOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            timeOperation.CurrentPubnubInstance(this);
            return timeOperation;
        }

		public EndPoint.AuditOperation Audit()
		{
            EndPoint.AuditOperation auditOperation = new EndPoint.AuditOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            auditOperation.CurrentPubnubInstance(this);
            return auditOperation;
        }

		public EndPoint.GrantOperation Grant()
		{
            EndPoint.GrantOperation grantOperation = new EndPoint.GrantOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            grantOperation.CurrentPubnubInstance(this);
            return grantOperation;
        }

		public EndPoint.SetStateOperation SetPresenceState()
		{
            EndPoint.SetStateOperation setStateOperation = new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            setStateOperation.CurrentPubnubInstance(this);
            return setStateOperation;
        }

		public EndPoint.GetStateOperation GetPresenceState()
		{
            EndPoint.GetStateOperation getStateOperation = new EndPoint.GetStateOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            getStateOperation.CurrentPubnubInstance(this);
            return getStateOperation;
        }

		public EndPoint.AddPushChannelOperation AddPushNotificationsOnChannels()
		{
            EndPoint.AddPushChannelOperation addPushChannelOperation = new EndPoint.AddPushChannelOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            addPushChannelOperation.CurrentPubnubInstance(this);
            return addPushChannelOperation;
        }

		public EndPoint.RemovePushChannelOperation RemovePushNotificationsFromChannels()
		{
            EndPoint.RemovePushChannelOperation removePushChannelOperation = new EndPoint.RemovePushChannelOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            removePushChannelOperation.CurrentPubnubInstance(this);
            return removePushChannelOperation;
        }

		public EndPoint.AuditPushChannelOperation AuditPushChannelProvisions()
		{
            EndPoint.AuditPushChannelOperation auditPushChannelOperation = new EndPoint.AuditPushChannelOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            auditPushChannelOperation.CurrentPubnubInstance(this);
            return auditPushChannelOperation;
        }

        #endregion

        #region "PubNub API Channel Group Methods"

        public EndPoint.AddChannelsToChannelGroupOperation AddChannelsToChannelGroup()
		{
            EndPoint.AddChannelsToChannelGroupOperation addChannelToChannelGroupOperation = new EndPoint.AddChannelsToChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            addChannelToChannelGroupOperation.CurrentPubnubInstance(this);
            return addChannelToChannelGroupOperation;
        }

		public EndPoint.RemoveChannelsFromChannelGroupOperation RemoveChannelsFromChannelGroup()
		{
            EndPoint.RemoveChannelsFromChannelGroupOperation removeChannelsFromChannelGroupOperation = new EndPoint.RemoveChannelsFromChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            removeChannelsFromChannelGroupOperation.CurrentPubnubInstance(this);
            return removeChannelsFromChannelGroupOperation;
        }

		public EndPoint.DeleteChannelGroupOperation DeleteChannelGroup()
		{
            EndPoint.DeleteChannelGroupOperation deleteChannelGroupOperation = new EndPoint.DeleteChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
            deleteChannelGroupOperation.CurrentPubnubInstance(this);
            return deleteChannelGroupOperation;
        }

		public EndPoint.ListChannelsForChannelGroupOperation ListChannelsForChannelGroup()
		{
            EndPoint.ListChannelsForChannelGroupOperation listChannelsForChannelGroupOperation = new EndPoint.ListChannelsForChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
            listChannelsForChannelGroupOperation.CurrentPubnubInstance(this);
            return listChannelsForChannelGroupOperation;
        }

        public EndPoint.ListAllChannelGroupOperation ListChannelGroups()
		{
            EndPoint.ListAllChannelGroupOperation listAllChannelGroupOperation = new EndPoint.ListAllChannelGroupOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
            listAllChannelGroupOperation.CurrentPubnubInstance(this);
            return listAllChannelGroupOperation;
        }

        public void AddListener(SubscribeCallback listener)
        {
            if (listenerManager == null)
            {
                listenerManager = new EndPoint.ListenerManager(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
                listenerManager.CurrentPubnubInstance(this);
            }
            listenerManager.AddListener(listener);
        }

        public void RemoveListener(SubscribeCallback listener)
        {
            if (listenerManager != null)
            {
                listenerManager.RemoveListener(listener);
            }
            //EndPoint.ListenerManager listenerManager = new EndPoint.ListenerManager(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog);
            //listenerManager.CurrentPubnubInstance(this);
        }
        #endregion

        #region "PubNub API Other Methods"
        public void TerminateCurrentSubscriberRequest()
		{
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
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
            EndPoint.OtherOperation endPoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, telemetryManager);
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
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannels();
        }

        public List<string> GetSubscribedChannelGroups()
        {
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
            endpoint.CurrentPubnubInstance(this);
            return endpoint.GetSubscribedChannelGroups();
        }

        public void Destroy()
        {
            savedSubscribeOperation = null;
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest, pubnubLog, null);
            endpoint.CurrentPubnubInstance(this);
            endpoint.EndPendingRequests();
        }

        public void Reconnect<T>()
        {
            if (savedSubscribeOperation != null && savedSubscribeOperation is EndPoint.SubscribeOperation<T>)
            {
                EndPoint.SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as EndPoint.SubscribeOperation<T>;
                if (subscibeOperationInstance != null)
                {
                    subscibeOperationInstance.Retry(true);
                }
            }
        }

        public void Disconnect<T>()
        {
            if (savedSubscribeOperation != null && savedSubscribeOperation is EndPoint.SubscribeOperation<T>)
            {
                EndPoint.SubscribeOperation<T> subscibeOperationInstance = savedSubscribeOperation as EndPoint.SubscribeOperation<T>;
                if (subscibeOperationInstance != null)
                {
                    subscibeOperationInstance.Retry(false);
                }
            }
        }

        public string Decrypt(string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) throw new ArgumentException("inputString is not valid");
            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey)) throw new Exception("CipherKey missing");

            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey);
            return pc.Decrypt(inputString);
        }

        public string Decrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString)) throw new ArgumentException("inputString is not valid");

            PubnubCrypto pc = new PubnubCrypto(cipherKey);
            return pc.Decrypt(inputString);
        }

        public string Encrypt(string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) throw new ArgumentException("inputString is not valid");
            if (pubnubConfig == null || string.IsNullOrEmpty(pubnubConfig.CipherKey)) throw new Exception("CipherKey missing");

            PubnubCrypto pc = new PubnubCrypto(pubnubConfig.CipherKey);
            return pc.Encrypt(inputString);
        }

        public string Encrypt(string inputString, string cipherKey)
        {
            if (string.IsNullOrEmpty(inputString)) throw new ArgumentException("inputString is not valid");

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

        public static string Version
        {
            get
            {
                if (pubnubUnitTest != null)
                {
                    return pubnubUnitTest.SdkVersion;
                }
                else
                {
                    return sdkVersion;
                }
            }
        }

        public string InstanceId
        {
            get
            {
                return instanceId;
            }
        }

        #endregion

        #region "Constructors"

        public Pubnub(PNConfiguration config)
        {
            instanceId = Guid.NewGuid().ToString();
            pubnubConfig = config;
            if (config != null)
            {
                pubnubLog = config.PubnubLog;
            }
            jsonPluggableLibrary = new NewtonsoftJsonDotNet(config, pubnubLog);
            if (config != null && config.EnableTelemetry)
            {
                telemetryManager = new EndPoint.TelemetryManager(pubnubConfig, pubnubLog);
            }
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

                if (string.IsNullOrEmpty(pubnubConfig.CipherKey))
                {
                    pubnubConfig.CipherKey = "";
                }
            }
        }

		#endregion
	}
}