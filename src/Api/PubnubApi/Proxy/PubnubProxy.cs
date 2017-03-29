using System;
using System.Net;

namespace PubnubApi
{
    public class Proxy
#if !NETSTANDARD10
        : IWebProxy
#endif
    {
#if NETSTANDARD10
        public Proxy()
        {
            throw new NotSupportedException("NETStandard 1.0 not supported");
        }
#else 
        private readonly Uri pubnubProxyUri;

        public Proxy(Uri proxyUri)
        {
            pubnubProxyUri = proxyUri;
        }

        public ICredentials Credentials
        {
            get;
            set;
        }

        public Uri GetProxy(Uri destination)
        {
            return pubnubProxyUri;
        }

        public bool IsBypassed(Uri host)
        {
            return false;
        }
#endif
    }
}
