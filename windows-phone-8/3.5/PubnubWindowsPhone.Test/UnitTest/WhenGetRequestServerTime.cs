using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
//using System.Collections.Generic;

namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenGetRequestServerTime : WorkItemTest
    {
        ManualResetEvent mreTime = new ManualResetEvent(false);
        bool timeReceived = false;

        [TestMethod]
        [Description("Gets the Server Time in Unix time nanosecond format")]
        public void ThenItShouldReturnTimeStamp()
        {
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGetRequestServerTime";
            unitTest.TestCaseName = "ThenItShouldReturnTimeStamp";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>(ReturnTimeStampCallback, DummyErrorCallback);
            mreTime.WaitOne(310 * 1000);
            Assert.IsTrue(timeReceived, "time() Failed");
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
            DateTime dt = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(dt);
            //Test for 26th June 2012 GMT
            Assert.AreEqual<long>(13406688000000000, nanoSecondTime);
        }

        [TestMethod]
        public void TranslateUnixTimeToDateTime()
        {
            //Test for 26th June 2012 GMT
            DateTime expectedDate = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateTime actualDate = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(13406688000000000);
            Assert.AreEqual<DateTime>(expectedDate, actualDate);
        }
    }
}
