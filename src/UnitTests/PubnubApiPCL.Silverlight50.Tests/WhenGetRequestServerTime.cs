using System;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubnubApi;
using System.Threading;
using Microsoft.Silverlight.Testing;

namespace PubnubApiPCL.Silverlight50.Tests
{
    [TestClass]
    public class WhenGetRequestServerTime : WorkItemTest
    {
        Pubnub pubnub;
        ManualResetEvent mreTime = new ManualResetEvent(false);
        bool timeReceived = false;

        [TestMethod, Asynchronous]
        [Description("Gets the Server Time in Unix time nanosecond format")]
        public void ThenItShouldReturnTimeStamp()
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

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


        [TestMethod]
        public void TranslateUnixTimeToDateTime()
        {
            //Test for 26th June 2012 GMT
            DateTime expectedDate = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            DateTime actualDate = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime(13406688000000000);
            Assert.IsTrue(expectedDate == actualDate);
        }

        [TestMethod]
        public void TranslateDateTimeToUnixTime()
        {
            DateTime dt = new DateTime(2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
            long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(dt);
            //Test for 26th June 2012 GMT
            Assert.IsTrue(13406688000000000 == nanoSecondTime);
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
    }
}
