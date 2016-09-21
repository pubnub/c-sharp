using System;

namespace PubnubApi.EndPoint
{
    public class UnsubscribeAllOperation<T> : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;

        public UnsubscribeAllOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public UnsubscribeAllOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }


        public void UnsubscribeAll()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary);
                manager.MultiChannelUnSubscribeAll<T>(PNOperationType.PNUnsubscribeOperation);
            });
        }

    }

}
