using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenStatelessSubscribeToAChannel : TestHarness
    {
        private static Pubnub pubnub;
        private static Server server;

        public class TestLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }

        [SetUp]
        public static void Init()
        {
            server = Server.Instance();
            server.Start();
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [TearDown]
        public void Cleanup()
        {

        }

        [Test]
        public static async Task ThenStatelessSubscribeShouldReturnReceivedMessage()
        {
            server.ClearRequests();
            PNConfiguration config = new PNConfiguration("mytestuuid")
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };

            pubnub = createPubNubInstance(config);

            string channel = "hello_my_channel";

            ManualResetEvent mre = new ManualResetEvent(false);
            await pubnub.StatelessSubscribe<string>().Channels(new List<string> { channel }).Execute();
            mre.WaitOne(600*1000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.Ignore("WhenStatelessSubscribeToAChannel --> ThenStatelessSubscribeShouldReturnReceivedMessage");
        }

    }
}
