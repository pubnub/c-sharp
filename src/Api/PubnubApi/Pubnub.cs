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
            return new EndPoint.SubscribeOperation<T>(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

        public EndPoint.UnsubscribeOperation<T> Unsubscribe<T>()
        {
            return new EndPoint.UnsubscribeOperation<T>(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

        public EndPoint.UnsubscribeAllOperation<T> UnsubscribeAll<T>()
        {
            return new EndPoint.UnsubscribeAllOperation<T>(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

        public EndPoint.PublishOperation Publish()
        {
            return new EndPoint.PublishOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
        }

        public EndPoint.FireOperation Fire()
        {
            return new EndPoint.FireOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
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

        public void AddListener(SubscribeCallback listener)
        {
            EndPoint.ListenerManager listenerManager = new EndPoint.ListenerManager(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            listenerManager.CurrentPubnubInstance(this);
            listenerManager.AddListener(listener);
        }

        public void RemoveListener(SubscribeCallback listener)
        {
            EndPoint.ListenerManager listenerManager = new EndPoint.ListenerManager(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            listenerManager.CurrentPubnubInstance(this);
            listenerManager.RemoveListener(listener);
        }
        #endregion

        #region "PubNub API Other Methods"
        public void TerminateCurrentSubscriberRequest()
		{
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
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

        public List<string> GetSubscribedChannels()
        {
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            return endpoint.GetSubscribedChannels();
        }

        public List<string> GetSubscribedChannelGroups()
        {
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            return endpoint.GetSubscribedChannelGroups();
        }

        public void Destroy()
        {
            EndPoint.OtherOperation endpoint = new EndPoint.OtherOperation(pubnubConfig, jsonPluggableLibrary, pubnubUnitTest);
            endpoint.EndPendingRequests();
        }
        //public void SetErrorFilterLevel(PubnubErrorFilter.Level errorLevel)
        //{
        //    pubnub.PubnubErrorLevel = errorLevel;
        //}  

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

        #endregion

        #region "Constructors"

        public Pubnub(PNConfiguration config)
        {
            pubnubConfig = config;
            jsonPluggableLibrary = new NewtonsoftJsonDotNet();
            CheckRequiredConfigValues();
        }

        //public Pubnub(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary)
        //{
        //    pubnubConfig = config;
        //    this.jsonPluggableLibrary = jsonPluggableLibrary;
        //    if (jsonPluggableLibrary == null)
        //    {
        //        this.jsonPluggableLibrary = new NewtonsoftJsonDotNet();
        //    }
        //    CheckRequiredConfigValues();
        //}

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