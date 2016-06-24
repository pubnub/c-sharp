using System;
using System.Net;

namespace PubnubApi
{
    internal class PubnubWebRequestCreator : PubnubWebRequestCreatorBase
    {

        public PubnubWebRequestCreator()
            : base()
        {
        }

        public PubnubWebRequestCreator(IPubnubUnitTest pubnubUnitTest)
            : base(pubnubUnitTest)
        {
        }

        protected HttpWebRequest SetUserAgent(HttpWebRequest req, bool keepAliveRequest)
        {
            req.Headers["UserAgent"] = string.Format("ua_string=({0}) PubNub-csharp/3.7", "PCL");
            return req;
        }

        protected override HttpWebRequest SetNoCache(HttpWebRequest req, bool nocache)
        {
            if (nocache)
            {
                req.Headers["Cache-Control"] = "no-cache";
                req.Headers["Pragma"] = "no-cache";
#if (WINDOWS_PHONE)
                req.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString();
#endif
            }
            return req;
        }

        protected override WebRequest CreateRequest(Uri uri, bool keepAliveRequest, bool nocache)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req = SetUserAgent(req, keepAliveRequest);

            req = SetNoCache(req, nocache);
            if (this.pubnubUnitTest is IPubnubUnitTest)
            {
                return new PubnubWebRequest(req, pubnubUnitTest);
            }
            else
            {
                return new PubnubWebRequest(req);
            }
        }

    }
}
