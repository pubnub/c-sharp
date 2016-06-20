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


        static PubnubCommon()
        {
            if (PAMEnabled && !EnableStubTest)
            {
                PublishKey = "pam";
                SubscribeKey = "pam";
                SecretKey = "pam";

                PublishKey = "pub-c-1f02ff4e-e509-486e-854f-56e59dfa6f88";
                SubscribeKey = "sub-c-4ef11dea-22b2-11e6-9327-02ee2ddab7fe";
                SecretKey = "sec-c-MmRkOWY2NTktMTU0ZS00YzA2LThhOWYtZWQxMDAwNGM2MjQ1";


            }
        }
    }
}
