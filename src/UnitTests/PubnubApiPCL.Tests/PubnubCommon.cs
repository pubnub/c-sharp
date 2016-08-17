namespace PubNubMessaging.Tests
{
    public static class PubnubCommon
    {
		public const bool PAMEnabled = false;
		public const bool EnableStubTest = false;

        public static readonly string PublishKey = "demo-36";
        public static readonly string SubscribeKey = "demo-36";
        public static readonly string SecretKey = "demo-36";

        public static readonly string StubOrign = "localhost:9191";

        static PubnubCommon()
        {
            if (PAMEnabled && !EnableStubTest)
            {
                PublishKey = "pam";
                SubscribeKey = "pam";
                SecretKey = "pam";
            }
        }
    }
}
