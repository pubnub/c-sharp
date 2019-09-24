namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
		public static readonly bool PAMServerSideGrant = true;
        public static readonly bool PAMServerSideRun = false;
        public static readonly bool SuppressAuthKey = PAMServerSideRun;
        public static readonly bool EnableStubTest = false;

        //USE demo-36 keys for unit tests
        //Bronze Keys
        //public static readonly string PublishKey = "pub-c-03f156ea-a2e3-4c35-a733-9535824be897";//"demo-36";
        //public static readonly string SubscribeKey = "sub-c-d7da9e58-c997-11e9-a139-dab2c75acd6f";// "demo-36";
        //public static readonly string SecretKey = "sec-c-MmUxNTZjMmYtNzFkNS00ODkzLWE2YjctNmQ4YzE5NWNmZDA3";// "demo-36";

        //Pandu Keys
        public static readonly string PublishKey = "pub-c-38994634-9e05-4967-bc66-2ac2cef65ed9";//"demo-36";
        public static readonly string SubscribeKey = "sub-c-c9710928-1b7a-11e3-a0c8-02ee2ddab7fe";// "demo-36";
        public static readonly string SecretKey = "sec-c-ZDkzZTBkOTEtNTQxZS00MmQ3LTljMWUtMTNiNGZjNWUwMTVk";// "demo-36";

        public static readonly string StubOrign = "localhost:9191";
        public static readonly string EncodedSDK = "PubNub%20CSharp";

        static PubnubCommon()
        {
        }
    }
}
