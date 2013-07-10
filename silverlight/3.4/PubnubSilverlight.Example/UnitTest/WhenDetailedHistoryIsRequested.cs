using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
    public class WhenDetailedHistoryIsRequested : SilverlightTest
    {
        bool message10Received = false;
        bool message10ReverseTrueReceived = false;
        bool messageStartReverseTrue = false;

        int expectedCountAtStartTimeWithReverseTrue=0;
        long startTimeWithReverseTrue = 0;

        bool detailedHistoryCount10CallbackInvoked = false;
        bool detailedHistoryCount10ReverseCallbackInvoked = false;
        bool isDetailedHistoryStartReverseTrue = false;
        bool detailedHistoryPublishCallbackInvoked = false;

        [TestMethod]
        [Asynchronous]
        public void DetailHistoryCount10ReturnsRecords()
        {
            message10Received = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            string channel = "my/channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReturnsRecords";
            pubnub.PubnubUnitTest = unitTest;

            EnqueueCallback(() => pubnub.DetailedHistory<string>(channel, 10, DetailedHistoryCount10Callback, DummyErrorCallback));
            EnqueueConditional(() => detailedHistoryCount10CallbackInvoked);
            EnqueueCallback(() => Assert.IsTrue(message10Received, "Detailed History Failed"));

            EnqueueTestComplete();
        }

        [Asynchronous]
        public void DetailedHistoryCount10Callback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null)
                    {
                        if (message.Count >= 0)
                        {
                            message10Received = true;
                        }
                    }
                }
            }

            detailedHistoryCount10CallbackInvoked = true;
        }

        [TestMethod]
        [Asynchronous]
        public void DetailHistoryCount10ReverseTrueReturnsRecords()
        {
            message10ReverseTrueReceived = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            string channel = "my/channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReverseTrueReturnsRecords";
            pubnub.PubnubUnitTest = unitTest;

            EnqueueCallback(() => pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailedHistoryCount10ReverseTrueCallback, DummyErrorCallback));
            EnqueueConditional(() => detailedHistoryCount10ReverseCallbackInvoked);
            EnqueueCallback(() => Assert.IsTrue(message10ReverseTrueReceived, "Detailed History Failed"));

            EnqueueTestComplete();
        }

        [Asynchronous]
        public void DetailedHistoryCount10ReverseTrueCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

            detailedHistoryCount10ReverseCallbackInvoked = true;
        }

        [TestMethod]
        [Asynchronous]
        public void DetailedHistoryStartWithReverseTrue()
        {
            expectedCountAtStartTimeWithReverseTrue = 0;
            messageStartReverseTrue = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryStartWithReverseTrue";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "my/channel";
            startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow);

            EnqueueCallback(() =>
            {
                for (int index = 0; index < 10; index++)
                {
                    pubnub.Publish<string>(channel,
                                        string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index),
                                        DetailedHistorySamplePublishCallback, DummyErrorCallback);
                    Thread.Sleep(100);
                    EnqueueConditional(() => detailedHistoryPublishCallbackInvoked);
                }
            });

            EnqueueCallback(() => pubnub.DetailedHistory<string>(channel, startTimeWithReverseTrue, DetailedHistoryStartWithReverseTrueCallback, DummyErrorCallback, true));
            EnqueueConditional(() => isDetailedHistoryStartReverseTrue);
            EnqueueCallback(() => Assert.IsTrue(messageStartReverseTrue, "Detailed History with Start and Reverse True Failed"));

            EnqueueTestComplete();
        }

        [Asynchronous]
        public void DetailedHistoryStartWithReverseTrueCallback(string result)
        {
            int actualCountAtStartTimeWithReverseFalse = 0;
            if (!string.IsNullOrWhiteSpace(result))
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
                            if (actualCountAtStartTimeWithReverseFalse == expectedCountAtStartTimeWithReverseTrue)
                            {
                                messageStartReverseTrue = true;
                            }
                        }
                    }
                }
            }
            isDetailedHistoryStartReverseTrue = true;
        }

        [Asynchronous]
        public void DetailedHistorySamplePublishCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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
            detailedHistoryPublishCallbackInvoked = true;
        }

        [Asynchronous]
        private void DummyErrorCallback(string result)
        {
        }

    }
}
