using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenGetRequestServerTime
    {
        ManualResetEvent mreTime = new ManualResetEvent(false);
        ManualResetEvent mreProxy = new ManualResetEvent(false);
        bool timeReceived = false;
        bool timeReceivedWhenProxy = false;

        Pubnub pubnub = null;

        [Test]
        public void ThenItShouldReturnTimeStamp()
        {
            timeReceived = false;
            mreTime = new ManualResetEvent(false);

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Uuid = "mytestuuid"
            };

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/time/0?uuid={2}&pnsdk={3}", (config.Secure) ? "s" : "", config.Origin, config.Uuid, config.SdkVersion)).ToString(), "[14271224264234400]");

            long expected = 14271224264234400;

            pubnub = new Pubnub(config, unitTest);

            pubnub.Time(
                (actual) => 
                {
                    if (unitTest.EnableStubTest)
                    {
                        if (expected == actual)
                        {
                            timeReceived = true;
                        }
                    }
                    else if (actual > 0)
                    {
                        timeReceived = true;
                    }

                    mreTime.Set();
                }, 
                (error) => { }
            );

            mreTime.WaitOne(310 * 1000);

            unitTest = null;

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
                Secure = true
            };

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/time/0?uuid={2}&pnsdk={3}", (config.Secure) ? "s" : "", config.Origin, config.Uuid, config.SdkVersion)).ToString(), "[14271224264234400]");

            long expected = 14271224264234400;

            pubnub = new Pubnub(config, unitTest);

            pubnub.Time(
                (actual) =>
                {
                    if (unitTest.EnableStubTest)
                    {
                        if (expected == actual)
                        {
                            timeReceived = true;
                        }
                    }
                    else if (actual > 0)
                    {
                        timeReceived = true;
                    }

                    mreTime.Set();
                },
                (error) => { }
            );

            mreTime.WaitOne(310 * 1000);

            unitTest = null;

            pubnub.EndPendingRequests(); 
            pubnub = null;

            Assert.IsTrue(timeReceived, "time() with SSL Failed");
        }

        [Test]
        public void ThenWithProxyItShouldReturnTimeStamp()
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

                pubnub.Time(
                    (actual) =>
                    {
                        if (unitTest.EnableStubTest)
                        {
                            if (expected == actual)
                            {
                                timeReceivedWhenProxy = true;
                            }
                        }
                        else if (actual > 0)
                        {
                            timeReceivedWhenProxy = true;
                        }

                        mreProxy.Set();
                    },
                    (error) => { }
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

                pubnub.Time(
                    (actual) =>
                    {
                        if (unitTest.EnableStubTest)
                        {
                            if (expected == actual)
                            {
                                timeReceivedWhenProxy = true;
                            }
                        }
                        else if (actual > 0)
                        {
                            timeReceivedWhenProxy = true;
                        }

                        mreProxy.Set();
                    },
                    (error) => { }
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
