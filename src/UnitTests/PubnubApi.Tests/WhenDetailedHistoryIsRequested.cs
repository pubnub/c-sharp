using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubnubApi;

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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = PubnubCommon.SecretKey;
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.GrantAccess(channel, true, true, 20, ThenDetailedHistoryInitializeShouldReturnGrantMessage, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryNoStoreShouldNotGetMessage";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForNoStorePublish;

            mrePublish = new ManualResetEvent(false);
            pubnub.Publish(channel, message, false, "", ReturnRegularPublishCodeCallback, DummyErrorCallback);
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
                pubnub.DetailedHistory(channel, -1, publishTimetokenForHistory, -1, false, false, CaptureNoStoreDetailedHistoryCallback, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "enigma";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnDecryptMessage";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForPublish;

            mrePublish = new ManualResetEvent(false);
            pubnub.Publish(channel, message, true, "", ReturnRegularPublishCodeCallback, DummyErrorCallback);
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
                pubnub.DetailedHistory(channel, publishTimetokenForHistory - 1, publishTimetokenForHistory, 1, false, false, CaptureRegularDetailedHistoryCallback, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReturnsRecords";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory(channel, 10, false, DetailedHistoryCount10Callback, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReverseTrueReturnsRecords";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory(channel, -1, -1, 10, true, false, DetailedHistoryCount10ReverseTrueCallback, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = "";
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryStartWithReverseTrue";

            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";
            //startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(new DateTime(2012, 12, 1));
            startTimeWithReverseTrue = 0;
            pubnub.Time((s) => { startTimeWithReverseTrue = s; }, (e) => { });
            Thread.Sleep(2000);
            for (int index = 0; index < 10; index++)
            {
                mrePublish = new ManualResetEvent(false);
                pubnub.Publish(channel, 
                    string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index),
                    DetailedHistorySamplePublishCallback, DummyErrorCallback);
                mrePublish.WaitOne();
            }

            Thread.Sleep(2000);

            mreMessageStartReverseTrue = new ManualResetEvent(false);
            pubnub.DetailedHistory(channel, startTimeWithReverseTrue, false, DetailedHistoryStartWithReverseTrueCallback, DummyErrorCallback, true);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = null;
            config.PublishKey = null;
            config.SecretKey = null;
            config.CiperKey = null;
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryWithNullKeysReturnsError";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory(channel, -1, -1, 10, true, false, DetailHistoryWithNullKeyseDummyCallback, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = secretKey;
            config.CiperKey = cipherKey;
            config.Secure = ssl;

            config.Uuid = "myuuid";
            pubnub = new Pubnub(config);

            string channel = "hello_my_channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime1";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time((s) => { starttime = s; }, (e) => { });
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
                pubnub.Publish(channel, message, true, "", ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime2";
            pubnub.PubnubUnitTest = unitTest;


            pubnub.Time((s) => { midtime = s; }, (e) => { });
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
                pubnub.Publish(channel, message, true, "", ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime3";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time((s) => { endtime = s; }, (e) => { });
            Thread.Sleep(1000);
            Console.WriteLine(string.Format("End Time = {0}", endtime));

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnEncryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            Console.WriteLine("Detailed History with Start & End");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory(channel, starttime, midtime, totalMessages / 2, true, false, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            if (messageReceived)
            {
                Console.WriteLine("DetailedHistory with start & reverse = true");
                mreDetailedHistory = new ManualResetEvent(false);
                pubnub.DetailedHistory(channel, midtime - 1, -1, totalMessages / 2, true, false, CaptureSecondPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
                mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
            }
            if (messageReceived)
            {
                Console.WriteLine("DetailedHistory with start & reverse = false");
                mreDetailedHistory = new ManualResetEvent(false);
                pubnub.DetailedHistory(channel, midtime - 1, -1, totalMessages / 2, false, false, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = secretKey;
            config.CiperKey = cipherKey;
            config.Secure = ssl;

            config.Uuid = "myuuid";
            pubnub = new Pubnub(config);

            string channel = "hello_my_channel";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime1";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time((s) => { starttime = s; }, (e) => { });
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
                pubnub.Publish(channel, message, true, "", ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime2";
            pubnub.PubnubUnitTest = unitTest;


            pubnub.Time((s) => { midtime = s; }, (e) => { });
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
                pubnub.Publish(channel, message, true, "", ReturnRegularPublishCodeCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout);
                Console.WriteLine(string.Format("Message #{0} publish {1}", index, (isPublished) ? "SUCCESS" : "FAILED"));
            }

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryShouldReturnServerTime3";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.Time((s) => { endtime = s; }, (e) => { });
            Console.WriteLine(string.Format("End Time = {0}", endtime));

            Thread.Sleep(1000);

            unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryShouldReturnUnencryptedMessageBasedOnParams";
            pubnub.PubnubUnitTest = unitTest;

            Console.WriteLine("Detailed History with Start & End");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory(channel, starttime, midtime, totalMessages / 2, true, false, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            Console.WriteLine("DetailedHistory with start & reverse = true");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory(channel, midtime - 1, -1, totalMessages / 2, true, false, CaptureSecondPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            Console.WriteLine("DetailedHistory with start & reverse = false");
            mreDetailedHistory = new ManualResetEvent(false);
            pubnub.DetailedHistory(channel, midtime - 1, -1, totalMessages / 2, false, false, CaptureFirstPublishSetRegularDetailedHistoryCallback, DummyErrorCallback);
            mreDetailedHistory.WaitOne(manualResetEventsWaitTimeout);

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }



        void ThenDetailedHistoryInitializeShouldReturnGrantMessage(GrantAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    var status = receivedMessage.StatusCode;
                    if (status == 200)
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

        void ReturnRegularPublishCodeCallback(PublishAck result)
        {
            try
            {
                Console.WriteLine(string.Format("ReturnRegularPublishCodeCallback result = {0}", result));

                if (result != null)
                {
                    int statusCode = result.StatusCode;
                    string statusMessage = result.StatusMessage;
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        publishTimetokenForHistory = result.Timetoken;
                        isPublished = true;
                    }
                }
            }
            catch { }
            finally
            {
                mrePublish.Set();
            }
        }

        void CaptureNoStoreDetailedHistoryCallback(DetailedHistoryAck result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length >= 0)
                {
                    if (!message.Contains(messageForNoStorePublish))
                    {
                        messageReceived = false;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void CaptureRegularDetailedHistoryCallback(DetailedHistoryAck result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null && message.Length >= 0)
                {
                    if (message.Contains(messageForPublish))
                    {
                        messageReceived = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void DetailedHistoryCount10Callback(DetailedHistoryAck result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null)
                {
                    if (message.Length >= 0)
                    {
                        messageReceived = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        void DetailedHistoryCount10ReverseTrueCallback(DetailedHistoryAck result)
        {
            if (result != null)
            {
                object[] message = result.Message;
                if (message != null)
                {
                    if (message.Length >= 0)
                    {
                        message10ReverseTrueReceived = true;
                    }
                }
            }

            mreMessageCount10ReverseTrue.Set();
        }

        void DetailedHistoryStartWithReverseTrueCallback(DetailedHistoryAck result)
        {
            int actualCountAtStartTimeWithReverseFalse = 0;
            if (result != null)
            {
                Console.WriteLine(string.Format("DetailedHistoryStartWithReverseTrueCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
                object[] message = result.Message;
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
            mreMessageStartReverseTrue.Set();
        }

        void DetailedHistorySamplePublishCallback(PublishAck result)
        {
            if (result != null)
            {
                Console.WriteLine(string.Format("DetailedHistorySamplePublishCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
                int statusCode = result.StatusCode;
                string statusMessage = result.StatusMessage;
                if (statusCode == 1 && statusMessage.ToLower() == "sent")
                {
                    expectedCountAtStartTimeWithReverseTrue++;
                }
            }

            mrePublish.Set();
        }

        void DetailHistoryWithNullKeyseDummyCallback(DetailedHistoryAck result)
        {
            mreDetailedHistory.Set();
        }

        void CaptureFirstPublishSetRegularDetailedHistoryCallback(DetailedHistoryAck result)
        {
            if (result != null)
            {
                Console.WriteLine(string.Format("CaptureFirstPublishSetRegularDetailedHistoryCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
                object[] message = result.Message;
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

            mreDetailedHistory.Set();
        }

        void CaptureSecondPublishSetRegularDetailedHistoryCallback(DetailedHistoryAck result)
        {
            if (result != null)
            {
                Console.WriteLine(string.Format("CaptureSecondPublishSetRegularDetailedHistoryCallback result = {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result)));
                object[] message = result.Message;
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
