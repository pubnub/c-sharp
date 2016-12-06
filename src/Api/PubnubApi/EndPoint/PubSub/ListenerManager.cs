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
        private IPubnubUnitTest unit = null;

        private object syncLockSubscribeCallback = new object();

        public ListenerManager(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public ListenerManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public ListenerManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
        }

        public void AddListener(SubscribeCallback listener)
        {
            if (listener != null)
            {
                lock (syncLockSubscribeCallback)
                {
                    SubscribeCallbackListenerList.Add(listener);
                }
            }
        }

        public void RemoveListener(SubscribeCallback listener)
        {
            if (listener != null)
            {
                lock (syncLockSubscribeCallback)
                {
                    SubscribeCallbackListenerList.Remove(listener);
                }
            }
        }

        
    }
}
