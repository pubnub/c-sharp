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
                        }
                        else
                        {
                            List<SubscribeCallback> callbackList = new List<SubscribeCallback>();
                            callbackList.Add(listener);
                            SubscribeCallbackListenerList.GetOrAdd(PubnubInstance.InstanceId, callbackList);
                        }
                        ret = true;
                    }
                    catch (Exception ex)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: AddListener exception {ex}" ,pubnubConfig.LogVerbosity);
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
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: removeListener Exception {ex}", pubnubConfig.LogVerbosity);
                    }
                }
            }

            return ret;
        }

        
    }
}
