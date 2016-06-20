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

#if NETFX_CORE
        protected HttpWebRequest SetUserAgent(HttpWebRequest req, bool keepAliveRequest)
        {
            req.Headers["UserAgent"] = string.Format("ua_string=({0} {1}) PubNub-csharp/3.6", "WindowsStore", "Win8.1"); 
            return req;
        }
#else
        protected HttpWebRequest SetUserAgent(HttpWebRequest req, bool keepAliveRequest, OperatingSystem userOS)
        {
#if (SILVERLIGHT || WINDOWS_PHONE)
            req.Headers["UserAgent"] = string.Format("ua_string=({0} {1}) PubNub-csharp/3.6", userOS.Platform.ToString(), userOS.Version.ToString());
#else
            req.KeepAlive = keepAliveRequest;
            req.UserAgent = string.Format("ua_string=({0}) PubNub-csharp/3.6", userOS.VersionString);
#endif
            return req;
        }
#endif

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
#if NETFX_CORE
            req = SetUserAgent(req, keepAliveRequest);
#else
            OperatingSystem userOS = System.Environment.OSVersion;
            req = SetUserAgent(req, keepAliveRequest, userOS);
#endif

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
