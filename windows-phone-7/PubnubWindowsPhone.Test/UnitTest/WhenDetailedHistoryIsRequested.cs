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
using System.Collections.Generic;


namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenDetailedHistoryIsRequested : WorkItemTest
    {
        ManualResetEvent mreDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);

        bool messageReceived = false;
        bool message10ReverseTrueReceived = false;
        bool messageStartReverseTrue = false;
        bool receivedGrantMessage = false;

        int expectedCountAtStartTimeWithReverseTrue=0;
        long startTimeWithReverseTrue = 0;
        string currentTestCase = "";

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                TestComplete();
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

                    mreGrant = new ManualResetEvent(false);
                    pubnub.GrantAccess<string>(channel, true, true, 20, ThenDetailedHistoryInitializeShouldReturnGrantMessage, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");
                            TestComplete();
                        });
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
                mreGrant.Set();
            }
        }
        
        [TestMethod,Asynchronous]
        public void DetailHistoryCount10ReturnsRecords()
        {
            messageReceived = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
                    unitTest.TestCaseName = "DetailHistoryCount10ReturnsRecords";
                    pubnub.PubnubUnitTest = unitTest;

                    mreDetailedHistory = new ManualResetEvent(false);
                    pubnub.DetailedHistory<string>(channel, 10, DetailedHistoryCount10Callback, DummyErrorCallback);
                    mreDetailedHistory.WaitOne(60 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(messageReceived, "Detailed History Failed");
                            TestComplete();
                        });
                });
        }

        [Asynchronous]
        void DetailedHistoryCount10Callback(string result)
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

        [TestMethod,Asynchronous]
        public void DetailHistoryCount10ReverseTrueReturnsRecords()
        {
            message10ReverseTrueReceived = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
                    unitTest.TestCaseName = "DetailHistoryCount10ReverseTrueReturnsRecords";
                    pubnub.PubnubUnitTest = unitTest;

                    mreDetailedHistory = new ManualResetEvent(false);
                    pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailedHistoryCount10ReverseTrueCallback, DummyErrorCallback);
                    mreDetailedHistory.WaitOne(60 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(message10ReverseTrueReceived, "Detailed History Failed");
                            TestComplete();
                        });
                });
        }

        [Asynchronous]
        void DetailedHistoryCount10ReverseTrueCallback(string result)
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

        [TestMethod,Asynchronous]
        public void DetailedHistoryStartWithReverseTrue()
        {
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
                     if (PubnubCommon.EnableStubTest)
                    {
                        startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(new DateTime(2012, 12, 1));
                    }
                    else
                    {
                        startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow);
                    }
                    for (int index = 0; index < 10; index++)
                    {
                        mrePublish = new ManualResetEvent(false);
                        pubnub.Publish<string>(channel,
                            string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index),
                            DetailedHistorySamplePublishCallback, DummyErrorCallback);
                        mrePublish.WaitOne(60 * 1000);
                    }

                    mreDetailedHistory = new ManualResetEvent(false);
                    pubnub.DetailedHistory<string>(channel, startTimeWithReverseTrue, DetailedHistoryStartWithReverseTrueCallback, DummyErrorCallback, true);
                    mreDetailedHistory.WaitOne(60 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(messageStartReverseTrue, "Detailed History with Start and Reverse True Failed");
                            TestComplete();
                        });
                });
        }

        [Asynchronous]
        private void DetailedHistoryStartWithReverseTrueCallback(string result)
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
        private void DetailedHistorySamplePublishCallback(string result)
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
                    pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailHistoryWithNullKeyseDummyCallback, DummyErrorCallback);
                    mreDetailedHistory.WaitOne(310 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(messageReceived, "Detailed History With Null Keys Failed");
                            TestComplete();
                        });
                });
        }

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
        }

    }
}
