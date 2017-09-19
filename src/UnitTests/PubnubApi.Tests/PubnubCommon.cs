namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
		public const bool PAMEnabled = true;
		public const bool EnableStubTest = false;

        //USE demo-36 keys for unit tests
        public static readonly string PublishKey = "demo-36";
        public static readonly string SubscribeKey = "demo-36";
        public static readonly string SecretKey = "demo-36";

        public static readonly string StubOrign = "localhost:9191";
        public static readonly string EncodedSDK = "PubNub%20CSharp";

        static PubnubCommon()
        {
        }
    }
}
