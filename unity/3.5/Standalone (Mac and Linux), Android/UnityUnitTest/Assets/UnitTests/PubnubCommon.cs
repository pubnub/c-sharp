using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
        public const bool PAMEnabled = true;
        public const bool EnableStubTest = true;

        public static readonly string PublishKey = "demo";
        public static readonly string SubscribeKey = "demo";
        public static readonly string SecretKey = "demo";


        static PubnubCommon()
        {
            if (PAMEnabled && !EnableStubTest)
            {
                PublishKey = "pub-c-a2650a22-deb1-44f5-aa87-1517049411d5";
                SubscribeKey = "sub-c-a478dd2a-c33d-11e2-883f-02ee2ddab7fe";
                SecretKey = "sec-c-YjFmNzYzMGMtYmI3NC00NzJkLTlkYzYtY2MwMzI4YTJhNDVh";
            }
        }
    }
}
