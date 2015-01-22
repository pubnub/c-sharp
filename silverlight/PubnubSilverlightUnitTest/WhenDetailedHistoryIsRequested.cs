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
        bool messageReceived = false;
        bool message10ReverseTrueReceived = false;
        bool messageStartReverseTrue = false;

        ManualResetEvent mreDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);

        int expectedCountAtStartTimeWithReverseTrue=0;
        long startTimeWithReverseTrue = 0;

        bool receivedGrantMessage = false;
        bool grantInitCallbackInvoked = false;

        string currentTestCase = "";

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            grantInitCallbackInvoked = false;

            if (!PubnubCommon.PAMEnabled)
            {
                EnqueueTestComplete();
                return;
            }

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "GrantRequestUnitTest";
                    unitTest.TestCaseName = "Init";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenDetailedHistoryInitializeShouldReturnGrantMessage, DummyErrorCallback));
                    mreGrant.WaitOne(310 * 1000);

                    EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenDetailedHistoryIsRequested Grant access failed."));
                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        void ThenDetailedHistoryInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    if (serializedMessage.Length > 0)
                    {
                        JContainer dictionary = serializedMessage[0] as JContainer;
                        var status = dictionary["status"].ToString();
                        if (status == "200")
                        {
                            receivedGrantMessage = true;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [TestMethod, Asynchronous]
        public void DetailHistoryCount10ReturnsRecords()
        {
            messageReceived = false;
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
                    unitTest.TestCaseName = "DetailHistoryCount10ReturnsRecords";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.DetailedHistory<string>(channel, 10, DetailedHistoryCount10Callback, DummyErrorCallback));
                    mreDetailedHistory.WaitOne(310 * 1000);
                    
                    EnqueueCallback(() => Assert.IsTrue(messageReceived, "Detailed History Failed"));
                    EnqueueTestComplete();
                });
            
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
                            messageReceived = true;
                        }
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void DetailHistoryCount10ReverseTrueReturnsRecords()
        {
            message10ReverseTrueReceived = false;
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
                    unitTest.TestCaseName = "DetailHistoryCount10ReverseTrueReturnsRecords";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailedHistoryCount10ReverseTrueCallback, DummyErrorCallback));
                    mreDetailedHistory.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(message10ReverseTrueReceived, "Detailed History Failed"));
                    EnqueueTestComplete();
                });
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

            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void DetailedHistoryStartWithReverseTrue()
        {
            mreDetailedHistory = new ManualResetEvent(false);

            expectedCountAtStartTimeWithReverseTrue = 0;
            messageStartReverseTrue = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
                    unitTest.TestCaseName = "DetailedHistoryStartWithReverseTrue";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow);

                    for (int index = 0; index < 10; index++)
                    {
                        mrePublish = new ManualResetEvent(false);
                        EnqueueCallback(() => pubnub.Publish<string>(channel,
                                            string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index),
                                            DetailedHistorySamplePublishCallback, DummyErrorCallback));
                        mrePublish.WaitOne(310 * 1000);
                    }


                    EnqueueCallback(() => pubnub.DetailedHistory<string>(channel, startTimeWithReverseTrue, DetailedHistoryStartWithReverseTrueCallback, DummyErrorCallback, true));
                    mreDetailedHistory.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(messageStartReverseTrue, "Detailed History with Start and Reverse True Failed"));
                    EnqueueTestComplete();
                });
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
            mreDetailedHistory.Set();
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
            mrePublish.Set();
        }

        [TestMethod, Asynchronous]
        public void DetailHistoryWithNullKeysReturnsError()
        {
            currentTestCase = "DetailHistoryWithNullKeysReturnsError";

            messageReceived = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub(null, null, null, null, false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
                unitTest.TestCaseName = "DetailHistoryWithNullKeysReturnsError";

                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";

                mreDetailedHistory = new ManualResetEvent(false);
                EnqueueCallback(() => pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailHistoryWithNullKeyseDummyCallback, DummyErrorCallback));
                mreDetailedHistory.WaitOne(310 * 1000);

                EnqueueCallback(() => Assert.IsTrue(messageReceived, "Detailed History With Null Keys Failed"));
                EnqueueTestComplete();
            });
        }

        [Asynchronous]
        void DetailHistoryWithNullKeyseDummyCallback(string result)
        {
            mreDetailedHistory.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
            if (currentTestCase == "DetailHistoryWithNullKeysReturnsError")
            {
                messageReceived = true;
                mreDetailedHistory.Set();
            }
            else
            {
                if (result != null)
                {
                    Console.WriteLine(result.Description);
                }
            }
        }

    }
}
