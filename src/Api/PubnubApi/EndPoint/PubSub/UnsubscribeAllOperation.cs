using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class UnsubscribeAllOperation<T> : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TokenManager pubnubTokenMgr;
        private Dictionary<string, object> queryParam;

        public UnsubscribeAllOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTokenMgr = tokenManager;
            CurrentPubnubInstance(instance);

            UnsubscribeAll();
        }

        public UnsubscribeAllOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        private void UnsubscribeAll()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTokenMgr, PubnubInstance);
                manager.CurrentPubnubInstance(PubnubInstance);
                manager.MultiChannelUnSubscribeAll<T>(PNOperationType.PNUnsubscribeOperation, this.queryParam);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog, pubnubTokenMgr, PubnubInstance);
                manager.CurrentPubnubInstance(PubnubInstance);
                manager.MultiChannelUnSubscribeAll<T>(PNOperationType.PNUnsubscribeOperation, this.queryParam);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, CancellationTokenSource>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
        }
    }

}
