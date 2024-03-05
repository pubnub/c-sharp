using System;

namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
        private static readonly string EnvPAMServerSideRun = Environment.GetEnvironmentVariable("PN_PAM_SERVER_SIDE");
        
        private static readonly string EnvPublishKey = Environment.GetEnvironmentVariable("SDK_PAM_PUB_KEY");
        private static readonly string EnvSubscribeKey = Environment.GetEnvironmentVariable("SDK_PAM_SUB_KEY");
        private static readonly string EnvSecretKey = Environment.GetEnvironmentVariable("SDK_PAM_SEC_KEY");

        private static readonly string EnvPublishKeyNoPam = Environment.GetEnvironmentVariable("SDK_PUB_KEY");
        private static readonly string EnvSubscribeKeyNoPam = Environment.GetEnvironmentVariable("SDK_SUB_KEY");
        private static readonly string EnvSecretKeyNoPam = Environment.GetEnvironmentVariable("SDK_SEC_KEY");

        public static readonly bool PAMServerSideRun = (!string.IsNullOrEmpty(EnvPAMServerSideRun) && EnvPAMServerSideRun == "1");
        public static readonly bool PAMServerSideGrant = !PAMServerSideRun;
        public static readonly bool SuppressAuthKey = PAMServerSideRun;
        public static readonly bool EnableStubTest = false;

        //USE demo-36 keys for unit tests 
        public static readonly string PublishKey = string.IsNullOrEmpty(EnvPublishKey) ? "demo36" : EnvPublishKey;
        public static readonly string SubscribeKey = string.IsNullOrEmpty(EnvSubscribeKey) ? "demo36" : EnvSubscribeKey;
        public static readonly string SecretKey = string.IsNullOrEmpty(EnvSecretKey) ? "demo36" : EnvSecretKey;

        //TODO: remove it when switch to mocked server!
        public static readonly string PublishKeyNoPam = string.IsNullOrEmpty(EnvPublishKeyNoPam) ? "demo" : EnvPublishKey;
        public static readonly string SubscribeKeyNoPam = string.IsNullOrEmpty(EnvSubscribeKeyNoPam) ? "demo" : EnvSubscribeKey;
        public static readonly string SecretKeyNoPam = string.IsNullOrEmpty(EnvSecretKeyNoPam) ? "demo" : EnvSecretKey;

        public static readonly string StubOrign = "localhost:9191";
        public static readonly string EncodedSDK = "PubNub%20CSharp";

        static PubnubCommon()
        {
        }
    }
}
