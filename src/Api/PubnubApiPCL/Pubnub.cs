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

        public EndPoint.SubscribeOperation<T> Subscribe<T>()
		{
            return new EndPoint.SubscribeOperation<T>(pubnubConfig, jsonPluggableLibrary);
        }

        public EndPoint.UnsubscribeOperation<T> Unsubscribe<T>()
        {
            return new EndPoint.UnsubscribeOperation<T>(pubnubConfig, jsonPluggableLibrary);
        }

        public EndPoint.UnsubscribeAllOperation<T> UnsubscribeAll<T>()
        {
            return new EndPoint.UnsubscribeAllOperation<T>(pubnubConfig, jsonPluggableLibrary);
        }

        public EndPoint.PublishOperation Publish()
        {
            return new EndPoint.PublishOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.HistoryOperation History()
		{
            return new EndPoint.HistoryOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.HereNowOperation HereNow()
		{
            return new EndPoint.HereNowOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.WhereNowOperation WhereNow()
		{
            return new EndPoint.WhereNowOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.TimeOperation Time()
		{
            return new EndPoint.TimeOperation(pubnubConfig, jsonPluggableLibrary);
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
            return new EndPoint.SetStateOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.GetStateOperation GetPresenceState()
		{
            return new EndPoint.GetStateOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.AddPushChannelOperation AddPushNotificationsOnChannels()
		{
            return new EndPoint.AddPushChannelOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.RemovePushChannelOperation RemovePushNotificationsFromChannels()
		{
            return new EndPoint.RemovePushChannelOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.AuditPushChannelOperation AuditPushChannelProvisions()
		{
            return new EndPoint.AuditPushChannelOperation(pubnubConfig, jsonPluggableLibrary);
        }

        #endregion

        #region "PubNub API Channel Group Methods"

        public EndPoint.AddChannelsToChannelGroupOperation AddChannelsToChannelGroup()
		{
            return new EndPoint.AddChannelsToChannelGroupOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.RemoveChannelsFromChannelGroupOperation RemoveChannelsFromChannelGroup()
		{
            return new EndPoint.RemoveChannelsFromChannelGroupOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.DeleteChannelGroupOperation DeleteChannelGroup()
		{
            return new EndPoint.DeleteChannelGroupOperation(pubnubConfig, jsonPluggableLibrary);
        }

		public EndPoint.ListChannelsForChannelGroupOperation ListChannelsForChannelGroup()
		{
            return new EndPoint.ListChannelsForChannelGroupOperation(pubnubConfig, jsonPluggableLibrary);
        }

        public EndPoint.ListAllChannelGroupOperation ListChannelGroups()
		{
            return new EndPoint.ListAllChannelGroupOperation(pubnubConfig, jsonPluggableLibrary);
        }

        public void AddListener(SubscribeCallback listener)
        {
            EndPoint.ListenerManager listenerManager = new EndPoint.ListenerManager(pubnubConfig, jsonPluggableLibrary);
            listenerManager.CurrentPubnubInstance(this);
            listenerManager.AddListener(listener);
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
            EndPoint.OtherOperation endPoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary);
            endPoint.EndPendingRequests();
        }

        public Guid GenerateGuid()
		{
			return Guid.NewGuid();
		}

		public void ChangeUUID(string newUUID)
		{
            EndPoint.OtherOperation endPoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary);
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

		#endregion
	}
}