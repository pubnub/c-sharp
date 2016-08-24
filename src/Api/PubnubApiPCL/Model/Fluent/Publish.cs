using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class Publish
    {
        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonPluggableLib = null;
        private IPubnubUnitTest pubnubUnitTest = null;

        private object msg = null;
        private string channelName = "";
        private bool storeInHistory = true;
        private bool httpPost = false;
        private string userMetadata = "";

        public Publish(PNConfiguration pnConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pnUnitTest)
        {
            this.pubnubConfig = pnConfig;
            this.jsonPluggableLib = jsonPluggableLibrary;
            this.pubnubUnitTest = pnUnitTest;
        }

        public class PNCallback
        {
            public Action<PublishAck> result = null;
            public Action<PubnubClientError> error = null;
        }

        public Publish message(object message)
        {
            this.msg = message;
            return this;
        }

        public Publish channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        public Publish shouldStore(bool store)
        {
            this.storeInHistory = store;
            return this;
        }

        public Publish meta(string jsonMetadata)
        {
            this.userMetadata = jsonMetadata;
            return this;
        }

        public Publish usePOST(bool post)
        {
            this.httpPost = post;
            return this;
        }

        public void async(PNCallback callback)
        {
            EndPoint.PublishOperation endPoint = new EndPoint.PublishOperation(pubnubConfig, jsonPluggableLib, pubnubUnitTest);
            endPoint.Publish(this.channelName, this.msg, this.storeInHistory, this.userMetadata, callback.result, callback.error);
        }

    }
}
