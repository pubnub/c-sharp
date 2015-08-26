using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading;

namespace PubnubWindowsStore.Test
{
    [TestClass]
    public class WhenGetRequestServerTime
    {
        ManualResetEvent mreTime = new ManualResetEvent(false);
        ManualResetEvent mreProxy = new ManualResetEvent(false);
        bool timeReceived = false;
        bool timeReceivedWhenProxy = false;

        [TestMethod]
        public void ThenItShouldReturnTimeStamp()
        {
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenItShouldReturnTimeStamp";

            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>(ReturnTimeStampCallback, DummyErrorCallback);
            mreTime.WaitOne(310 * 1000);
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(timeReceived, "time() Failed");
        }

        [TestMethod]
        public void ThenWithProxyItShouldReturnTimeStamp()
        {
            bool proxyConfigured = false;

            PubnubProxy proxy = new PubnubProxy();
            proxy.ProxyServer = "test.pandu.com";
            proxy.ProxyPort = 808;
            proxy.ProxyUserName = "tuvpnfreeproxy";
            proxy.ProxyPassword = "Rx8zW78k";

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenWithProxyItShouldReturnTimeStamp";
            pubnub.PubnubUnitTest = unitTest;

            if (proxyConfigured)
            {
                pubnub.Proxy = proxy;
                pubnub.Time<string>(ReturnProxyPresenceTimeStampCallback, DummyErrorCallback);
                mreProxy.WaitOne(310 * 1000);
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(timeReceivedWhenProxy, "time() Failed");
            }
            else
            {
                Assert.Inconclusive("Proxy setup not configured. After setup Set proxyConfigured to true");
            }

        }

        private void ReturnTimeStampCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
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
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
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

        [TestMethod]
        public void TranslateDateTimeToUnixTime()
        {
            //Test for 26th June 2012 GMT
            DateTime dt = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(dt);
            Assert.IsTrue(13406688000000000 == nanoSecondTime);
        }

        [TestMethod]
        public void TranslateUnixTimeToDateTime()
        {
            //Test for 26th June 2012 GMT
            DateTime expectedDate = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateTime actualDate = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(13406688000000000);
            Assert.IsTrue(expectedDate == actualDate);
        }

        void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
