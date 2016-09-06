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
        private ManualResetEvent mreTime = new ManualResetEvent(false);
        private ManualResetEvent mreProxy = new ManualResetEvent(false);
        private bool timeReceived = false;
        private bool timeReceivedWhenProxy = false;

        private Pubnub pubnub = null;
        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = new Server(new Uri("https://" + PubnubCommon.StubOrign));
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
            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            long expectedTime = 14271224264234400;

            pubnub.Time()
                .Async(new PNCallback<PNTimeResult>()
                {
                    Result = (actual) =>
                                {
                                    if (PubnubCommon.EnableStubTest)
                                    {
                                        if (expectedTime == actual.Timetoken)
                                        {
                                            timeReceived = true;
                                        }
                                    }
                                    else if (actual.Timetoken > 0)
                                    {
                                        timeReceived = true;
                                    }

                                    mreTime.Set();
                                },
                    Error = (error) => { }
                }
            );

            mreTime.WaitOne(310 * 1000);

            pubnub.EndPendingRequests(); 
            pubnub = null;

            Assert.IsTrue(timeReceived, "time() Failed");
        }

        [Test]
        public void ThenItShouldReturnTimeStampWithSSL()
        {
            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "[14725889985315301]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath("/time/0")
                    .WithParameter("uuid", config.Uuid)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            long expectedTime = 14271224264234400;

            pubnub.Time()
                .Async(new PNCallback<PNTimeResult>()
                {
                    Result = (actual) =>
                            {
                                if (PubnubCommon.EnableStubTest)
                                {
                                    if (expectedTime == actual.Timetoken)
                                    {
                                        timeReceived = true;
                                    }
                                }
                                else if (actual.Timetoken > 0)
                                {
                                    timeReceived = true;
                                }

                                mreTime.Set();
                            },
                    Error = (error) => { }
                }
            );

            mreTime.WaitOne(310 * 1000);

            pubnub.EndPendingRequests(); 
            pubnub = null;

            Assert.IsTrue(timeReceived, "time() with SSL Failed");
        }

        [Test]
        public void ThenWithProxyItShouldReturnTimeStamp()
        {
            bool proxyConfigured = true;

            IPubnubProxy proxy = new PubnubProxy();
            proxy.Server = "test.pandu.com";
            proxy.Port = 808;
            proxy.UserName = "tuvpnfreeproxy";
            proxy.Password = "Rx8zW78k";

            timeReceivedWhenProxy = false;
            mreProxy = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                PNProxy = proxy
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            long expected = 14271224264234400;


            if (proxyConfigured)
            {
                pubnub.Time()
                    .Async(
                    new PNCallback<PNTimeResult>()
                    {
                        Result = (actual) =>
                                    {
                                        if (PubnubCommon.EnableStubTest)
                                        {
                                            if (expected == actual.Timetoken)
                                            {
                                                timeReceivedWhenProxy = true;
                                            }
                                        }
                                        else if (actual.Timetoken > 0)
                                        {
                                            timeReceivedWhenProxy = true;
                                        }

                                        mreProxy.Set();
                                    },
                        Error = (error) => { }
                    }
                );

                mreProxy.WaitOne(310 * 1000);

                pubnub.EndPendingRequests(); 

                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(timeReceivedWhenProxy, "time() Failed");
            }
            else
            {
                Assert.Ignore("Proxy setup not configured. After setup Set proxyConfigured to true");
            }
        }

        [Test]
        public void ThenWithProxyItShouldReturnTimeStampWithSSL()
        {
            bool proxyConfigured = false;

            IPubnubProxy proxy = new PubnubProxy();
            proxy.Server = "test.pandu.com";
            proxy.Port = 808;
            proxy.UserName = "tuvpnfreeproxy";
            proxy.Password = "Rx8zW78k";

            timeReceivedWhenProxy = false;
            mreProxy = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid",
                PNProxy = proxy
            };

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/time/0?uuid={2}&pnsdk={3}", (config.Secure) ? "s" : "", config.Origin, config.Uuid, config.SdkVersion)).ToString(), "[14271224264234400]");

            long expected = 14271224264234400;

            if (proxyConfigured)
            {
                pubnub = new Pubnub(config, unitTest);

                pubnub.Time()
                    .Async(
                    new PNCallback<PNTimeResult>()
                    {
                        Result = (actual) =>
                                    {
                                        if (unitTest.EnableStubTest)
                                        {
                                            if (expected == actual.Timetoken)
                                            {
                                                timeReceivedWhenProxy = true;
                                            }
                                        }
                                        else if (actual.Timetoken > 0)
                                        {
                                            timeReceivedWhenProxy = true;
                                        }

                                        mreProxy.Set();
                                    },
                        Error = (error) => { }
                    }
                );

                mreProxy.WaitOne(310 * 1000);

                pubnub.EndPendingRequests();

                pubnub.PubnubUnitTest = null;
                pubnub = null;

                Assert.IsTrue(timeReceivedWhenProxy, "time() with SSL through proxy Failed");
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
            DateTime dt = new DateTime(2012,6,26,0,0,0,DateTimeKind.Utc);
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

    }
}
