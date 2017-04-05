using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class UnsubscribeAllOperation<T> : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;

        public UnsubscribeAllOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;

            UnsubscribeAll();
        }

        private void UnsubscribeAll()
        {
            Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit);
                manager.MultiChannelUnSubscribeAll<T>(PNOperationType.PNUnsubscribeOperation);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

    }

}
