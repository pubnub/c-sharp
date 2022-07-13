using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;

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

        [SetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
#if NET40
        public static void ThenItShouldReturnTimeStamp()
#else
        public static async Task ThenItShouldReturnTimeStamp()
#endif
        {
            server.ClearRequests();

            currentUnitTestCase = "ThenItShouldReturnTimeStamp";
            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
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
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

#if NET40
            pubnub.Time().Execute(new PNTimeResultExt((result, status)=> 
            {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));

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
            }));
            mreTime.WaitOne(310 * 1000);
#else
            PNResult<PNTimeResult> timeResult = await pubnub.Time().ExecuteAsync();
            if (timeResult.Result != null)
            {
                Debug.WriteLine(string.Format("ASYNC RESULT = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(timeResult.Result)));
                if (timeResult.Status.StatusCode == 200 && timeResult.Status.Error == false)
                {
                    if (PubnubCommon.EnableStubTest)
                    {
                        if (expectedTime == timeResult.Result.Timetoken)
                        {
                            timeReceived = true;
                        }
                    }
                    else if (timeResult.Result.Timetoken > 0)
                    {
                        timeReceived = true;
                    }
                }
            }
#endif




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

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            server.RunOnHttps(true);
            pubnub = createPubNubInstance(config);

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Time().Execute(new TimeResult());

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

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
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
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            if (config.Proxy != null)
            {
                pubnub.Time().Execute(new TimeResult());

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

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
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
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));


            if (config.Proxy != null)
            {
                pubnub.Time().Execute(new TimeResult());

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
                    Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(result));

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
