using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PubnubApi.Interface;
using System.Globalization;

namespace PubnubApi.EndPoint
{
    public class ListenerManager : PubnubCoreBase
    {
        private object syncLockSubscribeCallback = new object();
        private readonly PNConfiguration pubnubConfig;
        private readonly IPubnubLog pubnubLog;

        public ListenerManager(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(config, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
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
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, ListenerManager AddListener => Exception = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), pubnubConfig.LogVerbosity);
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
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, ListenerManager RemoveListener => Exception = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), pubnubConfig.LogVerbosity);
                    }
                }
            }

            return ret;
        }

        
    }
}
