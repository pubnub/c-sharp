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
    public class WhenDetailedHistoryIsRequested
    {
        ManualResetEvent mreDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreMessageCount10ReverseTrue = new ManualResetEvent(false);
        ManualResetEvent mreMessageStartReverseTrue = new ManualResetEvent(false);
        ManualResetEvent mrePublishStartReverseTrue = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool messageReceived = false;
        bool message10ReverseTrueReceived = false;
        bool messageStartReverseTrue = false;
        bool receivedGrantMessage = false;

        int expectedCountAtStartTimeWithReverseTrue = 0;
        long startTimeWithReverseTrue = 0;
        string currentTestCase = "";

        [TestInitialize]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.GrantAccess<string>(channel, true, true, 20, ThenDetailedHistoryInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Task.Delay(1000);

            grantManualEvent.WaitOne();

            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenDetailedHistoryIsRequested Grant access failed.");
        }

        [TestMethod]
        public void DetailHistoryCount10ReturnsRecords()
        {
            messageReceived = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReturnsRecords";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory<string>(channel, 10, DetailedHistoryCount10Callback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(310 * 1000);
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(messageReceived, "Detailed History Failed");
        }

        void ThenDetailedHistoryInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var status = dictionary["status"].ToString();
                    if (status == "200")
                    {
                        receivedGrantMessage = true;
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void DetailedHistoryCount10Callback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null)
                    {
                        if (message.Count >= 0)
                        {
                            messageReceived = true;
                        }
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod]
        public void DetailHistoryCount10ReverseTrueReturnsRecords()
        {
            message10ReverseTrueReceived = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReverseTrueReturnsRecords";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailedHistoryCount10ReverseTrueCallback, DummyErrorCallback);
            mreMessageCount10ReverseTrue.WaitOne(310 * 1000);
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(message10ReverseTrueReceived, "Detailed History Failed");
        }

        void DetailedHistoryCount10ReverseTrueCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null)
                    {
                        if (message.Count >= 0)
                        {
                            message10ReverseTrueReceived = true;
                        }
                    }
                }
            }

            mreMessageCount10ReverseTrue.Set();
        }

        [TestMethod]
        public void DetailedHistoryStartWithReverseTrue()
        {
            expectedCountAtStartTimeWithReverseTrue = 0;
            messageStartReverseTrue = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryStartWithReverseTrue";

            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";
            startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(new DateTime(2012, 12, 1));
            for (int index = 0; index < 10; index++)
            {
                pubnub.Publish<string>(channel,
                    string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index),
                    DetailedHistorySamplePublishCallback, DummyErrorCallback);
                mrePublishStartReverseTrue.WaitOne();
            }

            Task.Delay(2000);

            pubnub.DetailedHistory<string>(channel, startTimeWithReverseTrue, DetailedHistoryStartWithReverseTrueCallback, DummyErrorCallback, true);
            Task.Delay(2000);
            mreMessageStartReverseTrue.WaitOne(310 * 1000);
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(messageStartReverseTrue, "Detailed History with Start and Reverse True Failed");
        }

        private void DetailedHistoryStartWithReverseTrueCallback(string result)
        {
            int actualCountAtStartTimeWithReverseFalse = 0;
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null)
                    {
                        if (message.Count >= expectedCountAtStartTimeWithReverseTrue)
                        {
                            foreach (object item in message)
                            {
                                if (item.ToString().Contains("DetailedHistoryStartTimeWithReverseTrue"))
                                {
                                    actualCountAtStartTimeWithReverseFalse++;
                                }
                            }
                            if (actualCountAtStartTimeWithReverseFalse >= expectedCountAtStartTimeWithReverseTrue)
                            {
                                messageStartReverseTrue = true;
                            }
                        }
                    }
                }
            }
            mreMessageStartReverseTrue.Set();
        }

        private void DetailedHistorySamplePublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    int statusCode = Int32.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        expectedCountAtStartTimeWithReverseTrue++;
                    }
                }
            }
            mrePublishStartReverseTrue.Set();
        }

        [TestMethod]
        public void DetailHistoryWithNullKeysReturnsError()
        {
            currentTestCase = "DetailHistoryWithNullKeysReturnsError";

            messageReceived = false;

            Pubnub pubnub = new Pubnub(null, null, null, null, false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryWithNullKeysReturnsError";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailHistoryWithNullKeyseDummyCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(310 * 1000);
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(messageReceived, "Detailed History With Null Keys Failed");
        }

        void DetailHistoryWithNullKeyseDummyCallback(string result)
        {
            mreDetailedHistory.Set();
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
            if (currentTestCase == "DetailHistoryWithNullKeysReturnsError")
            {
                messageReceived = true;
                mreDetailedHistory.Set();
            }
        }

    }
}
