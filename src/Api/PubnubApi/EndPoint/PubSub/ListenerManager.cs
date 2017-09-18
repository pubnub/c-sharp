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
        private object syncLockSubscribeCallback = new object();

        public ListenerManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager)
        {
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
                    if (SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
                    {
                        List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                        callbackList.Add(listener);
                        SubscribeCallbackListenerList[PubnubInstance.InstanceId] = callbackList;
                    }
                    else
                    {
                        List<SubscribeCallback> callbackList = new List<SubscribeCallback>();
                        callbackList.Add(listener);
                        SubscribeCallbackListenerList.Add(PubnubInstance.InstanceId, callbackList);
                    }
                    
                }
            }
        }

        public void RemoveListener(SubscribeCallback listener)
        {
            if (listener != null)
            {
                lock (syncLockSubscribeCallback)
                {
                    if (SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
                    {
                        List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                        callbackList.Remove(listener);
                        SubscribeCallbackListenerList[PubnubInstance.InstanceId] = callbackList;
                    }
                }
            }
        }

        
    }
}
