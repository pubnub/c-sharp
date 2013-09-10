using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAMessageIsPublished
    {
        ManualResetEvent mreUnencryptedPublish = new ManualResetEvent(false);
        ManualResetEvent mreOptionalSecretKeyPublish = new ManualResetEvent(false);
        ManualResetEvent mreNoSslPublish = new ManualResetEvent(false);

        ManualResetEvent mreUnencryptObjectPublish = new ManualResetEvent(false);
        ManualResetEvent mreEncryptObjectPublish = new ManualResetEvent(false);
        ManualResetEvent mreEncryptPublish = new ManualResetEvent(false);
        ManualResetEvent mreSecretEncryptPublish = new ManualResetEvent(false);
        ManualResetEvent mreComplexObjectPublish = new ManualResetEvent(false);
        ManualResetEvent mreLaregMessagePublish = new ManualResetEvent(false);

        ManualResetEvent mreEncryptDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreSecretEncryptDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreUnencryptDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreUnencryptObjectDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreEncryptObjectDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreComplexObjectDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreSerializedObjectMessageForPublish = new ManualResetEvent(false);
        ManualResetEvent mreSerializedMessagePublishDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool isPublished2 = false;
        bool isPublished3 = false;

        bool isUnencryptPublished = false;
        bool isUnencryptObjectPublished = false;
        bool isEncryptObjectPublished = false;
        bool isUnencryptDetailedHistory = false;
        bool isUnencryptObjectDetailedHistory = false;
        bool isEncryptObjectDetailedHistory = false;
        bool isEncryptPublished = false;
        bool isSecretEncryptPublished = false;
        bool isEncryptDetailedHistory = false;
        bool isSecretEncryptDetailedHistory = false;
        bool isComplexObjectPublished = false;
        bool isComplexObjectDetailedHistory = false;
        bool isSerializedObjectMessagePublished = false;
        bool isSerializedObjectMessageDetailedHistory = false;
        bool isLargeMessagePublished = false;
        bool receivedGrantMessage = false;

        long unEncryptPublishTimetoken = 0;
        long unEncryptObjectPublishTimetoken = 0;
        long encryptObjectPublishTimetoken = 0;
        long encryptPublishTimetoken = 0;
        long secretEncryptPublishTimetoken = 0;
        long complexObjectPublishTimetoken = 0;
        long serializedMessagePublishTimetoken = 0;

        const string messageForUnencryptPublish = "Pubnub Messaging API 1";
        const string messageForEncryptPublish = "漢語";
        const string messageForSecretEncryptPublish = "Pubnub Messaging API 2";
        const string messageLarge2K = "Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime.\" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time";
        string messageObjectForUnencryptPublish = "";
        string messageObjectForEncryptPublish = "";
        string messageComplexObjectForPublish = "";
        string serializedObjectMessageForPublish;

        int manualResetEventsWaitTimeout = 310 * 1000;

        [TestFixtureSetUp]
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

            pubnub.GrantAccess<string>(channel, true, true, 20, ThenPublishInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            Assert.IsTrue(receivedGrantMessage, "WhenAMessageIsPublished Grant access failed.");
        }

        [Test]
        public void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptPublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;
            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

            pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreUnencryptedPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isUnencryptPublished)
            {
                Assert.IsTrue(isUnencryptPublished, "Unencrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);
                pubnub.DetailedHistory<string>(channel, -1, unEncryptPublishTimetoken, -1, false, CaptureUnencryptDetailedHistoryCallback, DummyErrorCallback);
                mreUnencryptDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isUnencryptDetailedHistory, "Unable to match the successful unencrypt Publish");
            }
        }

        [Test]
        public void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptObjectPublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;
            string channel = "hello_my_channel";
            object message = new CustomClass();
            messageObjectForUnencryptPublish = JsonConvert.SerializeObject(message);

            pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptObjectPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreUnencryptObjectPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isUnencryptObjectPublished)
            {
                Assert.IsTrue(isUnencryptObjectPublished, "Unencrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);
                pubnub.DetailedHistory<string>(channel, -1, unEncryptObjectPublishTimetoken, -1, false, CaptureUnencryptObjectDetailedHistoryCallback, DummyErrorCallback);
                mreUnencryptObjectDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isUnencryptObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
            }
        }

        [Test]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            isEncryptObjectPublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            object message = new SecretCustomClass();
            messageObjectForEncryptPublish = JsonConvert.SerializeObject(message);

            pubnub.Publish<string>(channel, message, ReturnSuccessEncryptObjectPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreEncryptObjectPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isEncryptObjectPublished)
            {
                Assert.IsTrue(isEncryptObjectPublished, "Encrypt Object Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);
                pubnub.DetailedHistory<string>(channel, -1, encryptObjectPublishTimetoken, -1, false, CaptureEncryptObjectDetailedHistoryCallback, DummyErrorCallback);
                mreEncryptObjectDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isEncryptObjectDetailedHistory, "Unable to match the successful encrypt object Publish");
            }
        }

        [Test]
        public void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isEncryptPublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenEncryptPublishShouldReturnSuccessCodeAndInfo";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForEncryptPublish;

            pubnub.Publish<string>(channel, message, ReturnSuccessEncryptPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreEncryptPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isEncryptPublished)
            {
                Assert.IsTrue(isEncryptPublished, "Encrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);
                pubnub.DetailedHistory<string>(channel, -1, encryptPublishTimetoken, -1, false, CaptureEncryptDetailedHistoryCallback, DummyErrorCallback);
                mreEncryptDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isEncryptDetailedHistory, "Unable to decrypt the successful Publish");
            }
        }

        [Test]
        public void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isSecretEncryptPublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "key", "enigma", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForSecretEncryptPublish;

            pubnub.Publish<string>(channel, message, ReturnSuccessSecretEncryptPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreSecretEncryptPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isSecretEncryptPublished)
            {
                Assert.IsTrue(isSecretEncryptPublished, "Secret Encrypt Publish Failed");
            }
            else
            {
                Thread.Sleep(1000);
                pubnub.DetailedHistory<string>(channel, -1, secretEncryptPublishTimetoken, -1, false, CaptureSecretEncryptDetailedHistoryCallback, DummyErrorCallback);
                mreSecretEncryptDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isSecretEncryptDetailedHistory, "Unable to decrypt the successful Secret key Publish");
            }
        }

        [Test]
        public void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            isComplexObjectPublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            object message = new PubnubDemoObject();
            messageComplexObjectForPublish = JsonConvert.SerializeObject(message);

            pubnub.Publish<string>(channel, message, ReturnSuccessComplexObjectPublishCodeCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreComplexObjectPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isComplexObjectPublished)
            {
                Assert.IsTrue(isComplexObjectPublished, "Complex Object Publish Failed");
            }
            else
            {
                Console.WriteLine("WhenAMessageIsPublished-ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo - Publish OK. Now checking detailed history");
                pubnub.DetailedHistory<string>(channel, -1, complexObjectPublishTimetoken, -1, false, CaptureComplexObjectDetailedHistoryCallback, DummyErrorCallback);
                mreComplexObjectDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isComplexObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
            }
        }

        [Test]
        public void ThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        {
            isSerializedObjectMessagePublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.EnableJsonEncodingForPublish = false;

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenDisableJsonEncodeShouldSendSerializedObjectMessage";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            object message = "{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}";
            serializedObjectMessageForPublish = message.ToString();

            pubnub.Publish<string>(channel, message, ReturnSuccessSerializedObjectMessageForPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreSerializedObjectMessageForPublish.WaitOne(manualResetEventsWaitTimeout);

            if (!isSerializedObjectMessagePublished)
            {
                Assert.IsTrue(isSerializedObjectMessagePublished, "Serialized Object Message Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, serializedMessagePublishTimetoken, -1, false, CaptureSerializedMessagePublishDetailedHistoryCallback, DummyErrorCallback);
                mreSerializedMessagePublishDetailedHistory.WaitOne(manualResetEventsWaitTimeout);
                Assert.IsTrue(isSerializedObjectMessageDetailedHistory, "Unable to match the successful serialized object message Publish");
            }
        }

        [Test]
        public void ThenLargeMessageShoudFailWithMessageTooLargeInfo()
        {
            isLargeMessagePublished = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenLargeMessageShoudFailWithMessageTooLargeInfo";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageLarge2K;

            pubnub.Publish<string>(channel, message, DummyPublishMessageTooLargeInfoCallback, ReturnPublishMessageTooLargeErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mreLaregMessagePublish.WaitOne(manualResetEventsWaitTimeout);

            Assert.IsTrue(isLargeMessagePublished, "Message Too Large is not failing as expected.");
        }

        void ThenPublishInitializeShouldReturnGrantMessage(string receivedMessage)
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

        private void ReturnSuccessUnencryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isUnencryptPublished = true;
                        unEncryptPublishTimetoken = Convert.ToInt64(deserializedMessage[2].ToString());
                    }
                }
            }
            mreUnencryptedPublish.Set();
        }

        private void ReturnSuccessUnencryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isUnencryptObjectPublished = true;
                        unEncryptObjectPublishTimetoken = Convert.ToInt64(deserializedMessage[2].ToString());
                    }
                }
            }
            mreUnencryptObjectPublish.Set();
        }

        private void ReturnSuccessEncryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isEncryptObjectPublished = true;
                        encryptObjectPublishTimetoken = Convert.ToInt64(deserializedMessage[2].ToString());
                    }
                }
            }
            mreEncryptObjectPublish.Set();
        }

        private void ReturnSuccessEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isEncryptPublished = true;
                        encryptPublishTimetoken = Convert.ToInt64(deserializedMessage[2].ToString());
                    }
                }
            }
            mreEncryptPublish.Set();
        }

        private void ReturnSuccessSecretEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isSecretEncryptPublished = true;
                        secretEncryptPublishTimetoken = Convert.ToInt64(deserializedMessage[2].ToString());
                    }
                }
            }
            mreSecretEncryptPublish.Set();
        }

        private void CaptureUnencryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString() == messageForUnencryptPublish)
                    {
                        isUnencryptDetailedHistory = true;
                    }
                }
            }

            mreUnencryptDetailedHistory.Set();
        }

        private void CaptureUnencryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString(Formatting.None) == messageObjectForUnencryptPublish)
                    {
                        isUnencryptObjectDetailedHistory = true;
                    }
                }
            }

            mreUnencryptObjectDetailedHistory.Set();
        }

        private void CaptureEncryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString(Formatting.None) == messageObjectForEncryptPublish)
                    {
                        isEncryptObjectDetailedHistory = true;
                    }
                }
            }

            mreEncryptObjectDetailedHistory.Set();
        }

        private void CaptureEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString() == messageForEncryptPublish)
                    {
                        isEncryptDetailedHistory = true;
                    }
                }
            }

            mreEncryptDetailedHistory.Set();
        }

        private void CaptureSecretEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[])
                {
                    JArray message = deserializedResult[0] as JArray;
                    if (message != null && message[0].ToString() == messageForSecretEncryptPublish)
                    {
                        isSecretEncryptDetailedHistory = true;
                    }
                }
            }

            mreSecretEncryptDetailedHistory.Set();
        }

        private void ReturnSuccessComplexObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isComplexObjectPublished = true;
                        complexObjectPublishTimetoken = Convert.ToInt64(deserializedMessage[2].ToString());
                    }
                }
            }

            mreComplexObjectPublish.Set();

        }

        private void CaptureComplexObjectDetailedHistoryCallback(string result)
        {
            Console.WriteLine("CaptureComplexObjectDetailedHistoryCallback = \n" + result);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString(Formatting.None) == messageComplexObjectForPublish)
                    {
                        isComplexObjectDetailedHistory = true;
                    }
                }
            }

            mreComplexObjectDetailedHistory.Set();
        }

        private void ReturnPublishMessageTooLargeErrorCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString().Trim());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 0 && statusMessage.ToLower() == "message too large")
                    {
                        isLargeMessagePublished = true;
                    }
                }
            }
            mreLaregMessagePublish.Set();
        }

        private void DummyPublishMessageTooLargeInfoCallback(string result)
        {
        }

        [Test]
        public void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            Assert.IsNotNull(pubnub.GenerateGuid());
        }

        [Test]
        [ExpectedException(typeof(MissingFieldException))]
        public void ThenPublishKeyShouldNotBeEmpty()
        {
            Pubnub pubnub = new Pubnub("", PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenPublishKeyShouldNotBeEmpty";

            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish<string>(channel, message, null, DummyErrorCallback);
        }


        [Test]
        public void ThenOptionalSecretKeyShouldBeProvidedInConstructor()
        {
            isPublished2 = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "key");

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenOptionalSecretKeyShouldBeProvidedInConstructor";

            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish<string>(channel, message, ReturnSecretKeyPublishCallback, DummyErrorCallback);
            mreOptionalSecretKeyPublish.WaitOne(310 * 1000);

            Assert.IsTrue(isPublished2, "Publish Failed with secret key");
        }

        private void ReturnSecretKeyPublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[])
                {
                    long statusCode = Int64.Parse(deserializedResult[0].ToString());
                    string statusMessage = (string)deserializedResult[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isPublished2 = true;
                    }
                }
            }
            mreOptionalSecretKeyPublish.Set();
        }

        [Test]
        public void IfSSLNotProvidedThenDefaultShouldBeFalse()
        {
            isPublished3 = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "");

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "IfSSLNotProvidedThenDefaultShouldBeFalse";

            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish<string>(channel, message, ReturnNoSSLDefaultFalseCallback, DummyErrorCallback);
            mreNoSslPublish.WaitOne(310 * 1000);
            Assert.IsTrue(isPublished3, "Publish Failed with no SSL");
        }

        private void ReturnNoSSLDefaultFalseCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[])
                {
                    long statusCode = Int64.Parse(deserializedResult[0].ToString());
                    string statusMessage = (string)deserializedResult[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isPublished3 = true;
                    }
                }
            }
            mreNoSslPublish.Set();
        }

        private void ReturnSuccessSerializedObjectMessageForPublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[])
                {
                    long statusCode = Int64.Parse(deserializedResult[0].ToString());
                    string statusMessage = (string)deserializedResult[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isSerializedObjectMessagePublished = true;
                        serializedMessagePublishTimetoken = Convert.ToInt64(deserializedResult[2].ToString());
                    }
                }
            }
            mreSerializedObjectMessageForPublish.Set();
        }

        private void CaptureSerializedMessagePublishDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString(Formatting.None) == serializedObjectMessageForPublish)
                    {
                        isSerializedObjectMessageDetailedHistory = true;
                    }
                }
            }

            mreSerializedMessagePublishDetailedHistory.Set();
        }


        private void DummyErrorCallback(string result)
        {

        }


    }
}
