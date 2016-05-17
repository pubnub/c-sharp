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
using PubnubApi;
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

        [TestMethod, Asynchronous]
        [Description("Gets the Server Time in Unix time nanosecond format")]
        public void ThenItShouldReturnTimeStamp()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGetRequestServerTime";
                    unitTest.TestCaseName = "ThenItShouldReturnTimeStamp";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Time(ReturnTimeStampCallback, DummyErrorCallback);
                    mreTime.WaitOne(60 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(timeReceived, "time() Failed");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                });
        }

        [Asynchronous]
        private void ReturnTimeStampCallback(long result)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (result > 0)
                    {
                        timeReceived = true;
                    }
                });
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
    }
}
