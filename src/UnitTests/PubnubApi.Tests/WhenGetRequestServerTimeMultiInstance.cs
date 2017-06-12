using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenGetRequestServerTimeMultiInstance : TestHarness
    {
        private static ManualResetEvent mreTime = new ManualResetEvent(false);
        private static bool timeReceived1 = false;
        private static string currentUnitTestCase = "";
        private static long expectedTime = 14725889985315301;
        private static Pubnub pubnub1 = null;
        private static Pubnub pubnub2 = null;

        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void ThenItShouldReturnTimeStamp()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenItShouldReturnTimeStamp";
            timeReceived1 = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config1 = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid1",
                Secure = false
            };
            PNConfiguration config2 = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid2",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub1 = this.createPubNubInstance(config1);
            pubnub2 = this.createPubNubInstance(config2);

            string expected1 = "[14725889985315301]";
            string expected2 = "[14725889985315302]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId1")
                    .WithParameter("uuid", config1.Uuid)
                    .WithParameter("instanceid", pubnub1.InstanceId)
                    .WithResponse(expected1)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId2")
                    .WithParameter("uuid", config2.Uuid)
                    .WithParameter("instanceid", pubnub2.InstanceId)
                    .WithResponse(expected2)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub1.Time().Async(new TimeResult());
            mreTime.WaitOne(310 * 1000);

            mreTime = new ManualResetEvent(false);
            pubnub2.Time().Async(new TimeResult());
            mreTime.WaitOne(310 * 1000);

            pubnub1.Destroy();
            pubnub2.Destroy();

            pubnub1 = null;
            pubnub2 = null;

            Assert.IsTrue(timeReceived1, "time() Failed");
        }

        [Test]
        public void ThenItShouldReturnTimeStampWithSSL()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenItShouldReturnTimeStampWithSSL";

            timeReceived1 = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            server.RunOnHttps(true);
            pubnub1 = this.createPubNubInstance(config);

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub1.Time().Async(new TimeResult());

            mreTime.WaitOne(310 * 1000);

            pubnub1.Destroy();
            pubnub1 = null;

            Assert.IsTrue(timeReceived1, "time() with SSL Failed");
        }

        [Test]
        public void ThenWithProxyItShouldReturnTimeStamp()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenWithProxyItShouldReturnTimeStamp";

            Proxy proxy = new Proxy(new Uri("test.pandu.com:808"));
            proxy.Credentials = new System.Net.NetworkCredential("tuvpnfreeproxy", "Rx8zW78k");

            timeReceived1 = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Proxy = (PubnubCommon.EnableStubTest) ? proxy : null,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub1 = this.createPubNubInstance(config);

            expectedTime = 14725889985315301;
            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (config.Proxy != null)
            {
                pubnub1.Time().Async(new TimeResult());

                mreTime.WaitOne(310 * 1000);

                pubnub1.Destroy();

                pubnub1.PubnubUnitTest = null;
                pubnub1 = null;

                Assert.IsTrue(timeReceived1, "time() Failed");
            }
            else
            {
                Assert.Ignore("Proxy setup not configured. After setup Set proxyConfigured to true");
            }
        }

        [Test]
        public void ThenWithProxyItShouldReturnTimeStampWithSSL()
        {
            server.ClearRequests();

            Proxy proxy = new Proxy(new Uri("test.pandu.com:808"));
            proxy.Credentials = new System.Net.NetworkCredential("tuvpnfreeproxy", "Rx8zW78k");

            timeReceived1 = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Proxy = (PubnubCommon.EnableStubTest) ? proxy : null
            };
            server.RunOnHttps(true);

            pubnub1 = this.createPubNubInstance(config);

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));


            if (config.Proxy != null)
            {
                pubnub1.Time().Async(new TimeResult());

                mreTime.WaitOne(310 * 1000);

                pubnub1.Destroy();

                pubnub1.PubnubUnitTest = null;
                pubnub1 = null;

                Assert.IsTrue(timeReceived1, "time() with SSL through proxy Failed");
            }
            else
            {
                Assert.Ignore("Proxy setup for SSL not configured. After setup Set proxyConfigured to true");
            }
        }

        [Test]
        public void TranslateDateTimeToUnixTime()
        {
            //Test for 26th June 2012 GMT
            DateTime dt = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(dt);
            Assert.True(13406688000000000 == nanoSecondTime);
        }

        [Test]
        public void TranslateUnixTimeToDateTime()
        {
            //Test for 26th June 2012 GMT
            DateTime expectedDate = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateTime actualDate = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(13406688000000000);
            Assert.True(expectedDate == actualDate);
        }

        public class TimeResult : PNCallback<PNTimeResult>
        {
            public override void OnResponse(PNTimeResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub1.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub1.JsonPluggableLibrary.SerializeToJsonString(result));

                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            if (PubnubCommon.EnableStubTest)
                            {
                                if (expectedTime == result.Timetoken)
                                {
                                    timeReceived1 = true;
                                }
                            }
                            else if (result.Timetoken > 0)
                            {
                                timeReceived1 = true;
                            }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    mreTime.Set();
                }

            }
        };

    }
}
