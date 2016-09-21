using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class ListenerManager : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private static IPubnubUnitTest unitTest = null;

        private object _syncLockSubscribeCallback = new object();

        public ListenerManager(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public ListenerManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            pubnub = instance;
        }

        public void AddListener(SubscribeCallback listener)
        {
            if (listener != null)
            {
                lock (_syncLockSubscribeCallback)
                {
                    SubscribeCallbackListenerList.Add(listener);
                }
            }
        }

        public void RemoveListener(SubscribeCallback listener)
        {
            if (listener != null)
            {
                lock (_syncLockSubscribeCallback)
                {
                    SubscribeCallbackListenerList.Remove(listener);
                }
            }
        }

        
    }
}
