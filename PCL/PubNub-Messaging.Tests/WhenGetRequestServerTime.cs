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
using PubNubMessaging.Core;

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
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenItShouldReturnTimeStamp";

            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>(ReturnTimeStampCallback, DummyErrorCallback);
            mreTime.WaitOne(310 * 1000);
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(timeReceived, "time() Failed");
        }

        [Test]
        public void ThenItShouldReturnTimeStampWithSSL()
        {
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", true);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenItShouldReturnTimeStamp";

            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>(ReturnTimeStampCallback, DummyErrorCallback);
            mreTime.WaitOne(310 * 1000);
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(timeReceived, "time() with SSL Failed");
        }


        [Test]
        public void ThenWithProxyItShouldReturnTimeStamp()
        {
            bool proxyConfigured = false;

            PubnubProxy proxy = new PubnubProxy();
            proxy.ProxyServer = "test.pandu.com";
            proxy.ProxyPort = 808;
            proxy.ProxyUserName = "tuvpnfreeproxy";
            proxy.ProxyPassword = "Rx8zW78k";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenWithProxyItShouldReturnTimeStamp";
            pubnub.PubnubUnitTest = unitTest;
            
            if (proxyConfigured)
            {
                pubnub.Proxy = proxy;
                pubnub.Time<string>(ReturnProxyPresenceTimeStampCallback, DummyErrorCallback);
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

            PubnubProxy proxy = new PubnubProxy();
            proxy.ProxyServer = "test.pandu.com";
            proxy.ProxyPort = 808;
            proxy.ProxyUserName = "tuvpnfreeproxy";
            proxy.ProxyPassword = "Rx8zW78k";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", true);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenWithProxyItShouldReturnTimeStamp";
            pubnub.PubnubUnitTest = unitTest;

            if (proxyConfigured)
            {
                pubnub.Proxy = proxy;
                pubnub.Time<string>(ReturnProxyPresenceTimeStampCallback, DummyErrorCallback);
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


        private void ReturnTimeStampCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    string time = deserializedMessage[0].ToString();
                    Int64 nanoTime;
                    if (time.Length > 2 && Int64.TryParse(time, out nanoTime))
                    {
                        timeReceived = true;
                    }
                }
            }
            mreTime.Set();
        }

        private void ReturnProxyPresenceTimeStampCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    string time = deserializedMessage[0].ToString();
                    Int64 nanoTime;
                    if (time.Length > 2 && Int64.TryParse(time, out nanoTime))
                    {
                        timeReceivedWhenProxy = true;
                    }
                }
            }
            mreProxy.Set();
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

        void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
