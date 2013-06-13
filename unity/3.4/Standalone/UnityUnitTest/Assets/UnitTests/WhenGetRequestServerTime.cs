using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class WhenGetRequestServerTime: UUnitTestCase
    {
        ManualResetEvent mreTime = new ManualResetEvent(false);
        ManualResetEvent mreProxy = new ManualResetEvent(false);
        bool timeReceived = false;
        bool timeReceivedWhenProxy = false;

        [UUnitTest]
        public void ThenItShouldReturnTimeStamp()
        {
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenItShouldReturnTimeStamp";

            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>(ReturnTimeStampCallback, DummyErrorCallback);
            mreTime.WaitOne(310 * 1000);
            UUnitAssert.True(timeReceived, "time() Failed");
        }

        [UUnitTest]
        public void ThenWithProxyItShouldReturnTimeStamp()
        {
            PubnubProxy proxy = new PubnubProxy();
            proxy.ProxyServer = "test.pandu.com";
            proxy.ProxyPort = 808;
            proxy.ProxyUserName = "tuvpnfreeproxy";
            proxy.ProxyPassword = "Rx8zW78k";

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            pubnub.Proxy = proxy;

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenWithProxyItShouldReturnTimeStamp";

            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>(ReturnProxyPresenceTimeStampCallback, DummyErrorCallback);
            mreProxy.WaitOne(310 * 1000);
            UUnitAssert.True(timeReceivedWhenProxy, "time() Failed");
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

        [UUnitTest]
        public void TranslateDateTimeToUnixTime()
        {
            //Test for 26th June 2012 GMT
            DateTime dt = new DateTime(2012,6,26,0,0,0,DateTimeKind.Utc);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(dt);
            UUnitAssert.Equals(13406688000000000, nanoSecondTime);
        }

        [UUnitTest]
        public void TranslateUnixTimeToDateTime()
        {
            //Test for 26th June 2012 GMT
            DateTime expectedDate = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateTime actualDate = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(13406688000000000);
            UUnitAssert.Equals(expectedDate.ToString(), actualDate.ToString());
        }
    
        void DummyErrorCallback(string result)
        {
        }
	}
}
