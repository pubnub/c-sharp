using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Microsoft.Silverlight.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PubnubSilverlight.UnitTest
{
    [TestClass]
    public class WhenGetRequestServerTime : SilverlightTest
    {
        bool timeReceived = false;
        ManualResetEvent mreTime = new ManualResetEvent(false);

        [TestMethod]
        [Asynchronous]
        public void ThenItShouldReturnTimeStamp()
        {
            mreTime = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGetRequestServerTime";
                    unitTest.TestCaseName = "ThenItShouldReturnTimeStamp";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Time<string>(ReturnTimeStampCallback, DummyErrorCallback));
                    mreTime.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(timeReceived, "time() Failed"));
                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        private void ReturnTimeStampCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    string time = deserializedMessage[0].ToString();
                    if (time.Length > 0)
                    {
                        timeReceived = true;
                    }
                }
            }
            mreTime.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

        [TestMethod]
        public void TranslateDateTimeToUnixTime()
        {
            //Test for 26th June 2012 GMT
            DateTime dt = new DateTime(2012,6,26,0,0,0,DateTimeKind.Utc);
            long nanosecTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(dt);
            Assert.AreEqual<long>(13406688000000000, nanosecTime);
        }

        [TestMethod]
        public void TranslateUnixTimeToDateTime()
        {
            //Test for 26th June 2012 GMT
            DateTime expectedDt = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateTime actualDt = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(13406688000000000);
            Assert.AreEqual<DateTime>(expectedDt, actualDt);
        }
    }
}
