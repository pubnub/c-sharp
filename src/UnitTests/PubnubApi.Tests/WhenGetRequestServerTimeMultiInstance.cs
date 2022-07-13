using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenGetRequestServerTimeMultiInstance : TestHarness
    {
        private static long expectedTime = 14725889985315301;
        private static Pubnub pubnub1;
        private static Pubnub pubnub2;

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
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenItShouldReturnTimeStamp()
        {
            server.ClearRequests();
            bool timeReceived1 = false;

            PNConfiguration config1 = new PNConfiguration(new UserId("mytestuuid1"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            PNConfiguration config2 = new PNConfiguration(new UserId("mytestuuid2"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub1 = createPubNubInstance(config1);
            pubnub2 = createPubNubInstance(config2);

            string expected1 = "[14725889985315301]";
            string expected2 = "[14725889985315302]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId1")
                    .WithParameter("uuid", config1.UserId.ToString())
                    .WithParameter("instanceid", pubnub1.InstanceId)
                    .WithResponse(expected1)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId2")
                    .WithParameter("uuid", config2.UserId.ToString())
                    .WithParameter("instanceid", pubnub2.InstanceId)
                    .WithResponse(expected2)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent mreTime = new ManualResetEvent(false);
            pubnub1.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub1.JsonPluggableLibrary.SerializeToJsonString(s));
                    if (r != null)
                    {
                        Debug.WriteLine(pubnub1.JsonPluggableLibrary.SerializeToJsonString(r));
                        if (s.StatusCode == 200 && s.Error == false && ((PubnubCommon.EnableStubTest && expectedTime == r.Timetoken) || r.Timetoken > 0))
                        {
                            timeReceived1 = true;
                        }
                    }
                }
                catch { /* ignore */ }
                finally { mreTime.Set(); }
            }));
            mreTime.WaitOne(310 * 1000);

            mreTime = new ManualResetEvent(false);
            pubnub2.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub1.JsonPluggableLibrary.SerializeToJsonString(s));
                    if (r != null)
                    {
                        Debug.WriteLine(pubnub1.JsonPluggableLibrary.SerializeToJsonString(r));
                        if (s.StatusCode == 200 && s.Error == false && ((PubnubCommon.EnableStubTest && expectedTime == r.Timetoken) || r.Timetoken > 0))
                        {
                            timeReceived1 = true;
                        }
                    }
                }
                catch { /* ignore */ }
                finally { mreTime.Set(); }
            }));
            mreTime.WaitOne(310 * 1000);

            pubnub1.Destroy();
            pubnub2.Destroy();

            pubnub1 = null;
            pubnub2 = null;

            Assert.IsTrue(timeReceived1, "time() Failed");
        }

        [Test]
        public static void ThenItShouldReturnTimeStampWithSSL()
        {
            server.ClearRequests();
            bool timeReceived1 = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
            };

            server.RunOnHttps(true);
            pubnub1 = createPubNubInstance(config);

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId.ToString())
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent mreTime = new ManualResetEvent(false);
            pubnub1.Time().Execute(new PNTimeResultExt((r, s) => {
                try
                {
                    Debug.WriteLine("PNStatus={0}", pubnub1.JsonPluggableLibrary.SerializeToJsonString(s));
                    if (r != null)
                    {
                        Debug.WriteLine(pubnub1.JsonPluggableLibrary.SerializeToJsonString(r));
                        if (s.StatusCode == 200 && s.Error == false && ((PubnubCommon.EnableStubTest && expectedTime == r.Timetoken) || r.Timetoken > 0))
                        {
                            timeReceived1 = true;
                        }
                    }
                }
                catch { /* ignore */ }
                finally { mreTime.Set(); }
            }));
            mreTime.WaitOne(310 * 1000);

            pubnub1.Destroy();
            pubnub1 = null;

            Assert.IsTrue(timeReceived1, "time() with SSL Failed");
        }

        [Test]
        public static void ThenWithProxyItShouldReturnTimeStamp()
        {
            server.ClearRequests();

            Proxy proxy = new Proxy(new Uri("test.pandu.com:808"));
            proxy.Credentials = new System.Net.NetworkCredential("tuvpnfreeproxy", "Rx8zW78k");

            bool timeReceived1 = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Proxy = (PubnubCommon.EnableStubTest) ? proxy : null,
                Secure = false
            };
            server.RunOnHttps(false);

            pubnub1 = createPubNubInstance(config);

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
                ManualResetEvent mreTime = new ManualResetEvent(false);
                pubnub1.Time().Execute(new PNTimeResultExt((r, s) => {
                    try
                    {
                        Debug.WriteLine("PNStatus={0}", pubnub1.JsonPluggableLibrary.SerializeToJsonString(s));
                        if (r != null)
                        {
                            Debug.WriteLine(pubnub1.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (s.StatusCode == 200 && s.Error == false && ((PubnubCommon.EnableStubTest && expectedTime == r.Timetoken) || r.Timetoken > 0))
                            {
                                timeReceived1 = true;
                            }
                        }
                    }
                    catch { /* ignore */ }
                    finally { mreTime.Set(); }
                }));
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
        public static void ThenWithProxyItShouldReturnTimeStampWithSSL()
        {
            server.ClearRequests();

            Proxy proxy = new Proxy(new Uri("test.pandu.com:808"));
            proxy.Credentials = new System.Net.NetworkCredential("tuvpnfreeproxy", "Rx8zW78k");

            bool timeReceived1 = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Proxy = (PubnubCommon.EnableStubTest) ? proxy : null
            };
            server.RunOnHttps(true);

            pubnub1 = createPubNubInstance(config);

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
                ManualResetEvent mreTime = new ManualResetEvent(false);
                pubnub1.Time().Execute(new PNTimeResultExt((r, s) => {
                    try
                    {
                        Debug.WriteLine("PNStatus={0}", pubnub1.JsonPluggableLibrary.SerializeToJsonString(s));
                        if (r != null)
                        {
                            Debug.WriteLine(pubnub1.JsonPluggableLibrary.SerializeToJsonString(r));
                            if (s.StatusCode == 200 && s.Error == false && ((PubnubCommon.EnableStubTest && expectedTime == r.Timetoken) || r.Timetoken > 0))
                            {
                                timeReceived1 = true;
                            }
                        }
                    }
                    catch { /* ignore */ }
                    finally { mreTime.Set(); }
                }));
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

        

    }
}
