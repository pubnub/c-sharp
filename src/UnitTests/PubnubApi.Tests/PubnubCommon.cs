using System;

namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
        private static readonly string EnvPAMServerSideRun = Environment.GetEnvironmentVariable("PN_PAM_SERVER_SIDE");
        private static readonly string EnvPublishKey = Environment.GetEnvironmentVariable("PN_PAM_PUB_KEY");
        private static readonly string EnvSubscribeKey = Environment.GetEnvironmentVariable("PN_PAM_SUB_KEY");
        private static readonly string EnvSecretKey = Environment.GetEnvironmentVariable("PN_PAM_SEC_KEY");

        public static readonly bool PAMServerSideRun = (!string.IsNullOrEmpty(EnvPAMServerSideRun) && EnvPAMServerSideRun == "1");
        public static readonly bool PAMServerSideGrant = !PAMServerSideRun;
        public static readonly bool SuppressAuthKey = PAMServerSideRun;
        public static readonly bool EnableStubTest = false;

        //USE demo-36 keys for unit tests 
        public static readonly string PublishKey = "pub-c-923646d2-d693-49b8-a49a-74ff272fc4ff";//string.IsNullOrEmpty(EnvPublishKey) ? "demo-36" : EnvPublishKey;
        public static readonly string SubscribeKey = "sub-c-39f90a72-489d-4836-9d05-9b96aa3bede4";//string.IsNullOrEmpty(EnvSubscribeKey) ? "demo-36" : EnvSubscribeKey;
        public static readonly string SecretKey = "sec-c-Y2Y2NWY1NzktODBlMi00Zjc3LWFkOWEtMmQxNzE4ODU5YTRk";//string.IsNullOrEmpty(EnvSecretKey) ? "demo-36" : EnvSecretKey;

        public static readonly string StubOrign = "localhost:9191";
        public static readonly string EncodedSDK = "PubNubCSharp";
        
        public static string GrantToken = "";

        public static readonly string NonPAMPublishKey = "pub-c-92e62c76-408a-4ac4-aefc-a1d20a83b2a6";//Environment.GetEnvironmentVariable("PN_PUB_KEY");
        public static readonly string NONPAMSubscribeKey = "sub-c-d0b8e542-12a0-41c4-999f-a2d569dc4255";//Environment.GetEnvironmentVariable("PN_SUB_KEY");
        
        static PubnubCommon()
        {
        }
    }
}