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
        public static readonly string PublishKey = "demo-36";
        public static readonly string SubscribeKey = "demo-36";
        public static readonly string SecretKey = "demo-36";

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
