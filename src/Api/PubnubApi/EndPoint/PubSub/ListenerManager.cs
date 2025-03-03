using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class ListenerManager : PubnubCoreBase
    {
        private readonly object syncLockSubscribeCallback = new object();
        private readonly PNConfiguration pubnubConfig;
        private readonly IPubnubLog pubnubLog;

        public ListenerManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(config, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
        {
            this.pubnubConfig = config;
            this.pubnubLog = log;
            PubnubInstance = instance;
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
        }

        public bool AddListener(SubscribeCallback listener)
        {
            bool ret = false;
            if (listener != null)
            {
                lock (syncLockSubscribeCallback)
                {
                    try
                    {
                        if (SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
                        {
                            List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                            callbackList.Add(listener);
                            SubscribeCallbackListenerList[PubnubInstance.InstanceId] = callbackList;
                            pubnubConfig.Logger.Debug($"AddListener: Listener added");
                        }
                        else
                        {
                            List<SubscribeCallback> callbackList = new List<SubscribeCallback>();
                            callbackList.Add(listener);
                            SubscribeCallbackListenerList.GetOrAdd(PubnubInstance.InstanceId, callbackList);
                            pubnubConfig.Logger.Debug($"AddListener: Listener added");
                        }
                        ret = true;
                    }
                    catch (Exception ex)
                    {
                        pubnubConfig.Logger.Error($"AddListener exception {ex.Message} stack trace: {ex.StackTrace}");
                    }
                }
            }
            return ret;
        }

        public bool RemoveListener(SubscribeCallback listener)
        {
            bool ret = false;

            if (listener != null)
            {
                lock (syncLockSubscribeCallback)
                {
                    try
                    {
                        if (SubscribeCallbackListenerList.ContainsKey(PubnubInstance.InstanceId))
                        {
                            List<SubscribeCallback> callbackList = SubscribeCallbackListenerList[PubnubInstance.InstanceId];
                            if (callbackList.Remove(listener))
                            {
                                ret = true;
                            }
                            SubscribeCallbackListenerList[PubnubInstance.InstanceId] = callbackList;
                            pubnubConfig.Logger.Debug($"RemoveListener: Listener removed");
                        }
                    }
                    catch (Exception ex)
                    {
                        pubnubConfig.Logger.Error($"RemoveListener exception {ex.Message} stack trace: {ex.StackTrace}");
                    }
                }
            }

            return ret;
        }

        
    }
}
