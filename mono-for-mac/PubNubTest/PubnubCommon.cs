using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
        public const bool PAMEnabled = true;
        public const bool EnableStubTest = false;
        public static readonly string PublishKey = "demo";
        public static readonly string SubscribeKey = "demo";
        public static readonly string SecretKey = "demo";

        static PubnubCommon()
        {
            if (PAMEnabled && !EnableStubTest)
                {
                //PublishKey = "pub-c-a2650a22-deb1-44f5-aa87-1517049411d5";
                //  SubscribeKey = "sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe";
                //  SecretKey = "sec-c-YjFmNzYzMGMtYmI3NC00NzJkLTlkYzYtY2MwMzI4YTJhNDVh";
                PublishKey = "pub-c-199e0a5c-8aa6-418b-bbca-3e90c20569a8";
                SubscribeKey = "sub-c-a3d5a1c8-ae97-11e3-a952-02ee2ddab7fe";
                SecretKey = "sec-c-NGVlNmRkYjAtY2Q1OS00OWM2LWE4NzktNzM5YzIxNGQxZjg3";
                }
        }
    }
}
