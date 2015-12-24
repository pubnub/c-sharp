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
            mreTime.WaitOne(310 * 1000, false);
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.True(timeReceived, "time() Failed");
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
            mreTime.WaitOne(310 * 1000, false);
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.True(timeReceived, "time() with SSL Failed");
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
                mreProxy.WaitOne(310 * 1000, true);
                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.True(timeReceivedWhenProxy, "time() Failed");
            }
            else
            {
                Assert.That(!proxyConfigured);
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
                mreProxy.WaitOne(310 * 1000, true);
                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.True(timeReceivedWhenProxy, "time() with SSL through proxy Failed");
            }
            else
            {
                Assert.That(!proxyConfigured);
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
                    if (time.Length > 2 && IsPubnubTime(time))
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
                    if (time.Length > 2 && IsPubnubTime(time))
                    {
                        timeReceivedWhenProxy = true;
                    }
                }
            }
            mreProxy.Set();
        }

        public static bool IsPubnubTime(string str)
        {
            bool ret = false;
            try
            {
                long num = Int64.Parse(str);
                ret = true;
            }
            catch { }
            return ret;
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
            mreTime.Set();
        }

    }
}
