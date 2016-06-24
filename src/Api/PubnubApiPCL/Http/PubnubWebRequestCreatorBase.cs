using System;
using System.Net;

namespace PubnubApi
{
    internal abstract class PubnubWebRequestCreatorBase : IWebRequestCreate
    {
        protected IPubnubUnitTest pubnubUnitTest = null;

        public PubnubWebRequestCreatorBase()
        {
        }

        public PubnubWebRequestCreatorBase(IPubnubUnitTest pubnubUnitTest)
        {
            this.pubnubUnitTest = pubnubUnitTest;
        }

        protected abstract HttpWebRequest SetNoCache(HttpWebRequest req, bool nocache);

        protected abstract WebRequest CreateRequest(Uri uri, bool keepAliveRequest, bool nocache);

        public WebRequest Create(Uri uri)
        {
            return CreateRequest(uri, true, true);
        }
        public WebRequest Create(Uri uri, bool keepAliveRequest)
        {
            return CreateRequest(uri, keepAliveRequest, true);
        }
        public WebRequest Create(Uri uri, bool keepAliveRequest, bool nocache)
        {
            return CreateRequest(uri, keepAliveRequest, nocache);
        }
    }
}
