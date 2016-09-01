namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
		public const bool PAMEnabled = false;
		public const bool EnableStubTest = true;

        public static readonly string PublishKey = "demo-36";
        public static readonly string SubscribeKey = "demo-36";
        public static readonly string SecretKey = "demo-36";

        public static readonly string StubOrign = "localhost:9191";
        public static readonly string EncodedSDK = "PubNub%20CSharp%204.0";

        static PubnubCommon()
        {
        }
    }
}
