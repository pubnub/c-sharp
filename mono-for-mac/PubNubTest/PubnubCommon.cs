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

        static PubnubCommon ()
        {
            if (PAMEnabled && !EnableStubTest) {
                PublishKey = "pam";
                SubscribeKey = "pam";
                SecretKey = "pam";
            }
        }
    }
}
