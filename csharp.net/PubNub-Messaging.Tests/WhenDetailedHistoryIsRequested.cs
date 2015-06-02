using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenDetailedHistoryIsRequested
    {
        ManualResetEvent mreDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreMessageCount10ReverseTrue = new ManualResetEvent(false);
        ManualResetEvent mreMessageStartReverseTrue = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        const string messageForNoStorePublish = "Pubnub Messaging With No Storage";
        const string messageForPublish = "Pubnub Messaging API 1";

        bool messageReceived = false;
        bool message10ReverseTrueReceived = false;
        bool messageStartReverseTrue = false;
        bool receivedGrantMessage = false;

        int expectedCountAtStartTimeWithReverseTrue=0;
        long startTimeWithReverseTrue = 0;
        bool isPublished = false;
        long publishTimetokenForHistory = 0;

        string currentTestCase = "";
        int manualResetEventsWaitTimeout = 310 * 1000;
        int[] firstPublishSet;
        double[] secondPublishSet;

        long starttime = Int64.MaxValue;
        long midtime = Int64.MaxValue;
        long endtime = Int64.MaxValue;

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.GrantAccess<string>(channel, true, true, 20, ThenDetailedHistoryInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenDetailedHistoryIsRequested Grant access failed.");
        }

        [Test]
        public void DetailHistoryNoStoreShouldNotGetMessage()
        {
            messageReceived = true;
            isPublished = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryNoStoreShouldNotGetMessage";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForNoStorePublish;

            mrePublish = new ManualResetEvent(false);
            pubnub.Publish<string>(channel, message, false, ReturnRegularPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isPublished)
            {
                Assert.IsTrue(isPublished, "No Store Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);
                mreDetailedHistory = new ManualResetEvent(false);
                pubnub.DetailedHistory<string>(channel, -1, publishTimetokenForHistory, -1, false, CaptureNoStoreDetailedHistoryCallback, DummyErrorCallback);
                mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(!messageReceived, "Message stored for Publish when no store is expected");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void DetailHistoryShouldReturnDecryptMessage()
        {
            messageReceived = false;
            isPublished = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnDecryptMessage";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForPublish;

            mrePublish = new ManualResetEvent(false);
            pubnub.Publish<string>(channel, message, true, ReturnRegularPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isPublished)
            {
                Assert.IsTrue(isPublished, "Encrypted message Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);
                mreDetailedHistory = new ManualResetEvent(false);
                pubnub.DetailedHistory<string>(channel, publishTimetokenForHistory-1, publishTimetokenForHistory,1, false, CaptureRegularDetailedHistoryCallback, DummyErrorCallback);
                mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

                Assert.IsTrue(messageReceived, "Encrypted message not showed up in history");
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public void DetailHistoryCount10ReturnsRecords()
        {
            messageReceived = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReturnsRecords";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory<string>(channel, 10, DetailedHistoryCount10Callback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(310 * 1000);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(messageReceived, "Detailed History Failed");
        }

        [Test]
        public void DetailHistoryCount10ReverseTrueReturnsRecords()
        {
            message10ReverseTrueReceived = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReverseTrueReturnsRecords";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailedHistoryCount10ReverseTrueCallback, DummyErrorCallback);
            mreMessageCount10ReverseTrue.WaitOne(310 * 1000);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(message10ReverseTrueReceived, "Detailed History Failed");
        }

        [Test]
        public void DetailedHistoryStartWithReverseTrue()
        {
            expectedCountAtStartTimeWithReverseTrue = 0;
            messageStartReverseTrue = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryStartWithReverseTrue";

            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";
            //startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(new DateTime(2012, 12, 1));
            startTimeWithReverseTrue = 0;
            pubnub.Time<string>((s) => { List<object> m = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s); startTimeWithReverseTrue = Convert.ToInt64(m[0]); }, (e) => { });
            for (int index = 0; index < 10; index++)
            {
                mrePublish = new ManualResetEvent(false);
                pubnub.Publish<string>(channel, 
                    string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index),
                    DetailedHistorySamplePublishCallback, DummyErrorCallback);
                mrePublish.WaitOne();
            }

            Thread.Sleep(2000);

            mreMessageStartReverseTrue = new ManualResetEvent(false);
            pubnub.DetailedHistory<string>(channel, startTimeWithReverseTrue, DetailedHistoryStartWithReverseTrueCallback, DummyErrorCallback, true);
            Thread.Sleep(2000);
            mreMessageStartReverseTrue.WaitOne(310 * 1000);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(messageStartReverseTrue, "Detailed History with Start and Reverse True Failed");
        }

        [Test]
        public void DetailHistoryWithNullKeysReturnsError()
        {
            currentTestCase = "DetailHistoryWithNullKeysReturnsError";

            messageReceived = false;

            pubnub = new Pubnub(null, null, null, null, false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryWithNullKeysReturnsError";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailHistoryWithNullKeyseDummyCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(310 * 1000);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(messageReceived, "Detailed History With Null Keys Failed");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedSecretMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnUnencrypedSecretMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams("", "", false);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnUnencrypedMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedSecretSSLMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnUnencrypedSecretSSLMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnUnencrypedSSLMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams("", "", true);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnUnencrypedSSLMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams("", "enigma", false);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnEncrypedMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedSecretMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnEncrypedSecretMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedSecretSSLMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnEncrypedSecretSSLMessage - Detailed History Result not expected");
        }

        [Test]
        public void DetailHistoryShouldReturnEncrypedSSLMessage()
        {
            messageReceived = false;
            CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams("", "enigma", true);
            Assert.IsTrue(messageReceived, "DetailHistoryShouldReturnEncrypedSSLMessage - Detailed History Result not expected");
        }
        
        private void CommonDetailedHistoryShouldReturnEncryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            messageReceived = false;
            isPublished = false;
            int totalMessages = 10;
            starttime = 0;
            midtime = 0;
            endtime = 0;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);
            pubnub.SessionUUID = "myuuid";

            string channel = "hello_my_channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime1";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>((s) => { List<object> m = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s); starttime = Convert.ToInt64(m[0]); }, (e) => { });
            Thread.Sleep(1000);
            Console.WriteLine(string.Format("Start Time = {0}", starttime));
            firstPublishSet = new int[totalMessages / 2];

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnEncryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            for (int index = 0; index < totalMessages / 2; index++)
            {
                object message = index;
                firstPublishSet[index] = index;
                mrePublish = new ManualResetEvent(false);
                Thread.Sleep(1000);
                pubnub.Publish<string>(channel, message, true, ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime2";
            pubnub.PubnubUnitTest = unitTest;


            pubnub.Time<string>((s) => { List<object> m = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s); midtime = Convert.ToInt64(m[0]); }, (e) => { });
            Thread.Sleep(1000);
            Console.WriteLine(string.Format("Mid Time = {0}", midtime));
            secondPublishSet = new double[totalMessages / 2];
            int arrayIndex = 0;

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnEncryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                object message = (double)index + 0.1D;
                secondPublishSet[arrayIndex] = (double)index + 0.1D;
                arrayIndex++;
                mrePublish = new ManualResetEvent(false);
                Thread.Sleep(1000);
                pubnub.Publish<string>(channel, message, true, ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime3";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>((s) => { List<object> m = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s); endtime = Convert.ToInt64(m[0]); }, (e) => { });
            Thread.Sleep(1000);
            Console.WriteLine(string.Format("End Time = {0}", endtime));

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnEncryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            Console.WriteLine("Detailed History with Start & End");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory<string>(channel, starttime, midtime, totalMessages / 2, true, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            if (messageReceived)
            {
                Console.WriteLine("DetailedHistory with start & reverse = true");
                mreDetailedHistory = new ManualResetEvent(false);
                pubnub.DetailedHistory<string>(channel, midtime - 1, -1, totalMessages / 2, true, CaptureSecondPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
                mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
            }
            if (messageReceived)
            {
                Console.WriteLine("DetailedHistory with start & reverse = false");
                mreDetailedHistory = new ManualResetEvent(false);
                pubnub.DetailedHistory<string>(channel, midtime - 1, -1, totalMessages / 2, false, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
                mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
            }
            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        private void CommonDetailedHistoryShouldReturnUnencryptedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            messageReceived = false;
            isPublished = false;
            int totalMessages = 10;
            starttime = 0;
            midtime = 0;
            endtime = 0;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);
            pubnub.SessionUUID = "myuuid";

            string channel = "hello_my_channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime1";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>((s) => { List<object> m = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s); starttime = Convert.ToInt64(m[0]); }, (e) => { });
            Console.WriteLine(string.Format("Start Time = {0}", starttime));
            firstPublishSet = new int[totalMessages / 2];

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnUnencryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            for (int index = 0; index < totalMessages / 2; index++)
            {
                object message = index;
                firstPublishSet[index] = index;
                mrePublish = new ManualResetEvent(false);
                Thread.Sleep(1000);
                pubnub.Publish<string>(channel, message, true, ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime2";
            pubnub.PubnubUnitTest = unitTest;


            pubnub.Time<string>((s) => { List<object> m = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s); midtime = Convert.ToInt64(m[0]); }, (e) => { });
            Console.WriteLine(string.Format("Mid Time = {0}", midtime));
            secondPublishSet = new double[totalMessages / 2];
            int arrayIndex = 0;

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnUnencryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            for (int index = totalMessages / 2; index < totalMessages; index++)
            {
                object message = (double)index + 0.1D;
                secondPublishSet[arrayIndex] = (double)index + 0.1D;
                arrayIndex++;
                mrePublish = new ManualResetEvent(false);
                Thread.Sleep(1000);
                pubnub.Publish<string>(channel, message, true, ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime3";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time<string>((s) => { List<object> m = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(s); endtime = Convert.ToInt64(m[0]); }, (e) => { });
            Console.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnUnencryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            Console.WriteLine("Detailed History with Start & End");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory<string>(channel, starttime, midtime, totalMessages / 2, true, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            Console.WriteLine("DetailedHistory with start & reverse = true");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory<string>(channel, midtime - 1, -1, totalMessages / 2, true, CaptureSecondPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            Console.WriteLine("DetailedHistory with start & reverse = false");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory<string>(channel, midtime - 1, -1, totalMessages / 2, false, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }



        void ThenDetailedHistoryInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null)
                        {
                            var status = dictionary["status"].ToString();
                            if (status == "200")
                            {
                                receivedGrantMessage = true;
                            }
                        }

                    }

                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void ReturnRegularPublishCodeCallback(string result)
        {
            try
            {
                Console.WriteLine(string.Format("ReturnRegularPublishCodeCallback result = {0}", result));
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
                {
                    List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                    if (deserializedMessage != null && deserializedMessage.Count > 0)
                    {
                        long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                        string statusMessage = (string)deserializedMessage[1];
                        if (statusCode == 1 && statusMessage.ToLower() == "sent")
                        {
                            publishTimetokenForHistory = Convert.ToInt64(deserializedMessage[2].ToString());
                            isPublished = true;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mrePublish.Set();
            }
        }

        void CaptureNoStoreDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    if (deserializedMessage[0].ToString() == "[]")
                    {
                        messageReceived = false;
                    }
                    else
                    {
                        object[] message = pubnub.JsonPluggableLibrary.ConvertToObjectArray(deserializedMessage[0]);
                        if (message != null && message.Length >= 0)
                        {
                            if (!message.Contains(messageForNoStorePublish))
                            {
                                messageReceived = false;
                            }
                        }
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void CaptureRegularDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object[] message = pubnub.JsonPluggableLibrary.ConvertToObjectArray(deserializedMessage[0]);
                    if (message != null && message.Length >= 0)
                    {
                        if (message.Contains(messageForPublish))
                        {
                            messageReceived = true;
                        }
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void DetailedHistoryCount10Callback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object[] message = pubnub.JsonPluggableLibrary.ConvertToObjectArray(deserializedMessage[0]);
                    if (message != null)
                    {
                        if (message.Length >= 0)
                        {
                            messageReceived = true;
                        }
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void DetailedHistoryCount10ReverseTrueCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object[] message = pubnub.JsonPluggableLibrary.ConvertToObjectArray(deserializedMessage[0]);
                    if (message != null)
                    {
                        if (message.Length >= 0)
                        {
                            message10ReverseTrueReceived = true;
                        }
                    }
                }
            }

            mreMessageCount10ReverseTrue.Set();
        }

        void DetailedHistoryStartWithReverseTrueCallback(string result)
        {
            int actualCountAtStartTimeWithReverseFalse = 0;
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Console.WriteLine(string.Format("DetailedHistoryStartWithReverseTrueCallback result = {0}", result));
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object[] message = pubnub.JsonPluggableLibrary.ConvertToObjectArray(deserializedMessage[0]);
                    if (message != null)
                    {
                        if (message.Length >= expectedCountAtStartTimeWithReverseTrue)
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

        void DetailedHistorySamplePublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Console.WriteLine(string.Format("DetailedHistorySamplePublishCallback result = {0}", result));
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
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

        void DetailHistoryWithNullKeyseDummyCallback(string result)
        {
            mreDetailedHistory.Set();
        }

        void CaptureFirstPublishSetRegularDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Console.WriteLine(string.Format("CaptureFirstPublishSetRegularDetailedHistoryCallback result = {0}", result));
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object[] message = pubnub.JsonPluggableLibrary.ConvertToObjectArray(deserializedMessage[0]);
                    if (message != null && message.Length >= 0 && firstPublishSet != null && firstPublishSet.Length == message.Length)
                    {
                        for (int index = 0; index < message.Length; index++)
                        {
                            if (firstPublishSet[index].ToString() != message[index].ToString())
                            {
                                messageReceived = false;
                                break;
                            }
                            messageReceived = true;
                        }
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void CaptureSecondPublishSetRegularDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Console.WriteLine(string.Format("CaptureSecondPublishSetRegularDetailedHistoryCallback result = {0}", result));
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object[] message = pubnub.JsonPluggableLibrary.ConvertToObjectArray(deserializedMessage[0]);
                    if (message != null && message.Length >= 0 && firstPublishSet != null && firstPublishSet.Length == message.Length)
                    {
                        for (int index = 0; index < message.Length; index++)
                        {
                            if (secondPublishSet[index].ToString() != message[index].ToString())
                            {
                                messageReceived = false;
                                break;
                            }
                            messageReceived = true;
                        }
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void DummyErrorCallback(PubnubClientError result)
        {
            if (currentTestCase == "DetailHistoryWithNullKeysReturnsError")
            {
                messageReceived = true;
                mreDetailedHistory.Set();
            }
        }

    }
}
