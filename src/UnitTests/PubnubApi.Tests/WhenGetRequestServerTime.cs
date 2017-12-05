using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenGetRequestServerTime : TestHarness
    {
        private static ManualResetEvent mreTime = new ManualResetEvent(false);
        private static bool timeReceived = false;
        private static string currentUnitTestCase = "";
        private static long expectedTime = 14725889985315301;
        private static Pubnub pubnub;

        private static Server server;

        [TestFixtureSetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
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
        public static void ThenItShouldReturnTimeStamp()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenItShouldReturnTimeStamp";
            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());
                
            mreTime.WaitOne(310 * 1000);

            pubnub.Destroy();
            pubnub = null;

            Assert.IsTrue(timeReceived, "time() Failed");
        }

        [Test]
        public static void ThenItShouldReturnTimeStampWithSSL()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenItShouldReturnTimeStampWithSSL";

            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config);

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Async(new TimeResult());

            mreTime.WaitOne(310 * 1000);

            pubnub.Destroy();
            pubnub = null;

            Assert.IsTrue(timeReceived, "time() with SSL Failed");
        }

        [Test]
        public static void ThenWithProxyItShouldReturnTimeStamp()
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenWithProxyItShouldReturnTimeStamp";

            Proxy proxy = new Proxy(new Uri("test.pandu.com:808"));
            proxy.Credentials = new System.Net.NetworkCredential("tuvpnfreeproxy", "Rx8zW78k");

            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Proxy = (PubnubCommon.EnableStubTest) ? proxy : null,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);

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
                pubnub.Time().Async(new TimeResult());

                mreTime.WaitOne(310 * 1000);

                pubnub.Destroy();

                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(timeReceived, "time() Failed");
            }
            else
            {
                Assert.Ignore("Proxy setup not configured. After setup Set proxyConfigured to true");
            }
        }

        [Test]
        public static void ThenWithProxyItShouldReturnTimeStampWithSSL()
        {
            server.ClearRequests();

            Proxy proxy = new Proxy(new Uri("test.pandu.com:808"));
            proxy.Credentials = new System.Net.NetworkCredential("tuvpnfreeproxy", "Rx8zW78k");

            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                Proxy = (PubnubCommon.EnableStubTest) ? proxy : null
            };
            server.RunOnHttps(true);

            pubnub = createPubNubInstance(config);

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
                pubnub.Time().Async(new TimeResult());

                mreTime.WaitOne(310 * 1000);

                pubnub.Destroy();

                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(timeReceived, "time() with SSL through proxy Failed");
            }
            else
            {
                Assert.Ignore("Proxy setup for SSL not configured. After setup Set proxyConfigured to true");
            }
        }

        [Test]
        public static void TranslateDateTimeToUnixTime()
        {
            //Test for 26th June 2012 GMT
            DateTime dt = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(dt);
            Assert.True(13406688000000000 == nanoSecondTime);
        }

        [Test]
        public static void TranslateUnixTimeToDateTime()
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
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));

                        if (status.StatusCode == 200 && status.Error == false)
                        {
                            if (PubnubCommon.EnableStubTest)
                            {
                                if (expectedTime == result.Timetoken)
                                {
                                    timeReceived = true;
                                }
                            }
                            else if (result.Timetoken > 0)
                            {
                                timeReceived = true;
                            }
                        }
                    }
                }
                catch { /* ignone */ }
                finally
                {
                    mreTime.Set();
                }

            }
        };

    }
}
