
using System.Net;

namespace PubnubApi
{
    public class PubnubWebRequest : PubnubWebRequestBase
    {

#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
        public override long ContentLength
        {
            get
            {
                return request.ContentLength;
            }
        }
#endif
#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
        private int _timeout;
        public override int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
                if (request != null)
                {
                    request.Timeout = _timeout;
                }
            }
        }

        public override IWebProxy Proxy
        {
            get
            {
                return request.Proxy;
            }
            set
            {
                request.Proxy = value;
            }
        }

        public override bool PreAuthenticate
        {
            get
            {
                return request.PreAuthenticate;
            }
            set
            {
                request.PreAuthenticate = value;
            }
        }
        public override System.Net.Cache.RequestCachePolicy CachePolicy
        {
            get
            {
                return request.CachePolicy;
            }
        }

        public override string ConnectionGroupName
        {
            get
            {
                return request.ConnectionGroupName;
            }
        }
#endif

#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
        public ServicePoint ServicePoint;
#endif

#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
        public override WebResponse GetResponse()
        {
            return request.GetResponse();
        }
#endif

        public PubnubWebRequest(HttpWebRequest request)
            : base(request)
        {
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
            this.ServicePoint = this.request.ServicePoint;
#endif
        }
        public PubnubWebRequest(HttpWebRequest request, IPubnubUnitTest pubnubUnitTest)
            : base(request, pubnubUnitTest)
        {
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
            this.ServicePoint = this.request.ServicePoint;
#endif
        }
    }
}
