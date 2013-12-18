using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;
using System.Reflection;

namespace PubNubMessaging.Tests
{
    public class WhenAMessageIsPublished: UUnitTestCase
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

        [UUnitTest]
        public void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            Debug.Log("Running ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()");
            isUnencryptPublished = false;
            Pubnub pubnub = new Pubnub("demo","demo","","",false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            string channel = "hello_my_channel";
            string message = messageForUnencryptPublish;

            pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptPublishCodeCallback, DummyErrorCallback);
            mreUnencryptedPublish.WaitOne(310*1000);
            Thread.Sleep(1000);

            if (!isUnencryptPublished)
            {
                UUnitAssert.True(isUnencryptPublished, "Unencrypt Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, unEncryptPublishTimetoken, -1, false, CaptureUnencryptDetailedHistoryCallback, DummyErrorCallback);
                mreUnencryptDetailedHistory.WaitOne(310 * 1000);
                UUnitAssert.True(isUnencryptDetailedHistory, "Unable to match the successful unencrypt Publish");
            }
        }

        [UUnitTest]
        public void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            Debug.Log("Running ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()");
            isUnencryptObjectPublished = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            object message = new CustomClass();
            
            messageObjectForUnencryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);
            pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptObjectPublishCodeCallback, DummyErrorCallback);
            mreUnencryptObjectPublish.WaitOne(310 * 1000);
            Thread.Sleep(1000);
            
            if (!isUnencryptObjectPublished)
            {
                UUnitAssert.True(isUnencryptObjectPublished, "Unencrypt Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, unEncryptObjectPublishTimetoken, -1, false, CaptureUnencryptObjectDetailedHistoryCallback, DummyErrorCallback);
                mreUnencryptObjectDetailedHistory.WaitOne(310 * 1000);
                UUnitAssert.True(isUnencryptObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
            }
        }

        [UUnitTest]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            Debug.Log("Running ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()");
            isEncryptObjectPublished = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "enigma", false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            object message = new SecretCustomClass();
            messageObjectForEncryptPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);

            pubnub.Publish<string>(channel, message, ReturnSuccessEncryptObjectPublishCodeCallback, DummyErrorCallback);
            mreEncryptObjectPublish.WaitOne(310 * 1000);
            Thread.Sleep(1000);

            if (!isEncryptObjectPublished)
            {
                UUnitAssert.True(isEncryptObjectPublished, "Encrypt Object Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, encryptObjectPublishTimetoken, -1, false, CaptureEncryptObjectDetailedHistoryCallback, DummyErrorCallback);
                mreEncryptObjectDetailedHistory.WaitOne(310 * 1000);
                UUnitAssert.True(isEncryptObjectDetailedHistory, "Unable to match the successful encrypt object Publish");
            }
        }

        [UUnitTest]
        public void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            Debug.Log("Running ThenEncryptPublishShouldReturnSuccessCodeAndInfo()");
            isEncryptPublished = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "enigma", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenEncryptPublishShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForEncryptPublish;

            pubnub.Publish<string>(channel, message, ReturnSuccessEncryptPublishCodeCallback, DummyErrorCallback);
            mreEncryptPublish.WaitOne(310 * 1000);
            Thread.Sleep(1000);

            if (!isEncryptPublished)
            {
                UUnitAssert.True(isEncryptPublished, "Encrypt Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, encryptPublishTimetoken, -1, false, CaptureEncryptDetailedHistoryCallback, DummyErrorCallback);
                mreEncryptDetailedHistory.WaitOne(310 * 1000);
                UUnitAssert.True(isEncryptDetailedHistory, "Unable to decrypt the successful Publish");
            }
        }

        [UUnitTest]
        public void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            Debug.Log("Running ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()");
            isSecretEncryptPublished = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "key", "enigma", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageForSecretEncryptPublish;

            pubnub.Publish<string>(channel, message, ReturnSuccessSecretEncryptPublishCodeCallback, DummyErrorCallback);
            mreSecretEncryptPublish.WaitOne(310 * 1000);
            Thread.Sleep(1000);

            if (!isSecretEncryptPublished)
            {
                UUnitAssert.True(isSecretEncryptPublished, "Secret Encrypt Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, secretEncryptPublishTimetoken, -1, false, CaptureSecretEncryptDetailedHistoryCallback, DummyErrorCallback);
                mreSecretEncryptDetailedHistory.WaitOne(310 * 1000);
                UUnitAssert.True(isSecretEncryptDetailedHistory, "Unable to decrypt the successful Secret key Publish");
            }
        }

        [UUnitTest]
        public void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            Debug.Log("Running ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()");
            isComplexObjectPublished = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo";
            pubnub.PubnubUnitTest = unitTest;
            
            if (pubnub.PubnubUnitTest != null)
            {
                Debug.Log("** ATTENTION ** : TEST CASE = \"ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo\" cannot run with local unit test stubs. You can run this test case with pubnub.PubnubUnitTest = null  ");
                return;
            }

            string channel = "hello_my_channel";
            object message = new PubnubDemoObject();
            messageComplexObjectForPublish = pubnub.JsonPluggableLibrary.SerializeToJsonString(message);
            Debug.Log("messageComplexObjectForPublish = " + messageComplexObjectForPublish);
            pubnub.Publish<string>(channel, message, ReturnSuccessComplexObjectPublishCodeCallback, DummyErrorCallback);
            mreComplexObjectPublish.WaitOne(310 * 1000);
            Thread.Sleep(1000);

            if (!isComplexObjectPublished)
            {
                UUnitAssert.True(isComplexObjectPublished, "Complex Object Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, complexObjectPublishTimetoken, -1, false, CaptureComplexObjectDetailedHistoryCallback, DummyErrorCallback);
                mreComplexObjectDetailedHistory.WaitOne(310 * 1000);
                UUnitAssert.True(isComplexObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
            }
        }
        
        [UUnitTest]
        public void ThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        {
            Debug.Log("Running ThenDisableJsonEncodeShouldSendSerializedObjectMessage()");
            isSerializedObjectMessagePublished = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            pubnub.EnableJsonEncodingForPublish = false;

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenDisableJsonEncodeShouldSendSerializedObjectMessage";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            object message = "{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}";
            serializedObjectMessageForPublish = message.ToString();

            pubnub.Publish<string>(channel, message, ReturnSuccessSerializedObjectMessageForPublishCallback, DummyErrorCallback);
            mreSerializedObjectMessageForPublish.WaitOne(310 * 1000);
            Thread.Sleep(1000);

            if (!isSerializedObjectMessagePublished)
            {
                UUnitAssert.True(isSerializedObjectMessagePublished, "Serialized Object Message Publish Failed");
            }
            else
            {
                pubnub.DetailedHistory<string>(channel, -1, serializedMessagePublishTimetoken, -1, false, CaptureSerializedMessagePublishDetailedHistoryCallback, DummyErrorCallback);
                mreSerializedMessagePublishDetailedHistory.WaitOne(310 * 1000);
                UUnitAssert.True(isSerializedObjectMessageDetailedHistory, "Unable to match the successful serialized object message Publish");
            }
        }

        [UUnitTest]
        public void ThenLargeMessageShoudFailWithMessageTooLargeInfo()
        {
            Debug.Log("Running ThenLargeMessageShoudFailWithMessageTooLargeInfo()");
            isLargeMessagePublished = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenLargeMessageShoudFailWithMessageTooLargeInfo";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = messageLarge2K;

            pubnub.Publish<string>(channel, message, ReturnPublishMessageTooLargeInfoCallback, DummyErrorCallback);
            mreLaregMessagePublish.WaitOne(310 * 1000);

            UUnitAssert.True(isLargeMessagePublished, "Message Too Large is not failing as expected.");
        }


        private void ReturnSuccessUnencryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    object[] message = deserializedMessage[0] as object[];
                    if (message != null && message.Length > 0)
                    {
                        string receivedUnencryptString = message[0].ToString();
                        if (receivedUnencryptString != null && receivedUnencryptString == messageForUnencryptPublish)
                        {
                            isUnencryptDetailedHistory = true;
                        }
                    }
                }
            }

            mreUnencryptDetailedHistory.Set();
        }

        private void CaptureUnencryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    object[] message = deserializedMessage[0] as object[];
                    if (message != null && message.Length > 0)
                    {
                        string receivedObjectString = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                        if (receivedObjectString != null && receivedObjectString == messageObjectForUnencryptPublish)
                        {
                            isUnencryptObjectDetailedHistory = true;
                        }
                    }
                }
            }

            mreUnencryptObjectDetailedHistory.Set();
        }

        private void CaptureEncryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    object[] message = deserializedMessage[0] as object[];
                    if (message != null && message.Length > 0)
                    {
                        string receivedObjectString = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                        if (receivedObjectString != null && receivedObjectString == messageObjectForEncryptPublish)
                        {
                            isEncryptObjectDetailedHistory = true;
                        }
                    }
                }
            }

            mreEncryptObjectDetailedHistory.Set();
        }

        private void CaptureEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[] && deserializedMessage.Length > 0)
                {
                    object[] message = deserializedMessage[0] as object[];
                    if (message != null && message.Length > 0 && message[0].ToString() == messageForEncryptPublish)
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedResult = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedResult is object[] && deserializedResult.Length > 0)
                {
                    object[] message = deserializedResult[0] as object[];
                    if (message != null && message.Length > 0 && message[0].ToString() == messageForSecretEncryptPublish)
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[] && deserializedMessage.Length > 0)
                {
                    object[] message = deserializedMessage[0] as object[];
                    if (message != null && message.Length > 0)
                    {
                        string receivedObjectString = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                        if (receivedObjectString != null && receivedObjectString == messageComplexObjectForPublish)
                        {
                            isComplexObjectDetailedHistory = true;
                        }
                    }
                }
            }

            mreComplexObjectDetailedHistory.Set();
        }

        private void ReturnPublishMessageTooLargeInfoCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 0 && statusMessage.ToLower() == "message too large")
                    {
                        isLargeMessagePublished = true;
                    }
                }
            }
            mreLaregMessagePublish.Set();
        }

        [UUnitTest]
        public void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            Debug.Log("Running ThenPubnubShouldGenerateUniqueIdentifier()");
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
            
            UUnitAssert.NotNull(pubnub.GenerateGuid());
        }

        [UUnitTest]
        public void ThenPublishKeyShouldNotBeEmpty()
        {
            Debug.Log("Running ThenPublishKeyShouldNotBeEmpty()");
            bool isExpectedException = false;
            
            Pubnub pubnub = new Pubnub("", "demo", "", "", false);
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenPublishKeyShouldNotBeEmpty";

            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            try
            {
                pubnub.Publish<string>(channel, message, null, DummyErrorCallback);
            }
            catch (MissingFieldException)
            {
                isExpectedException = true;
            }
            catch (Exception)
            {
                isExpectedException = false;
            }
            
            UUnitAssert.True(isExpectedException);
        }


        [UUnitTest]
        public void ThenOptionalSecretKeyShouldBeProvidedInConstructor()
        {
            Debug.Log("Running ThenOptionalSecretKeyShouldBeProvidedInConstructor()");
            isPublished2 = false;
            Pubnub pubnub = new Pubnub("demo","demo","key");
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "ThenOptionalSecretKeyShouldBeProvidedInConstructor";
            pubnub.PubnubUnitTest = unitTest;


            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish<string>(channel, message, ReturnSecretKeyPublishCallback, DummyErrorCallback);
            mreOptionalSecretKeyPublish.WaitOne(310 * 1000);

            UUnitAssert.True(isPublished2, "Publish Failed with secret key");
        }

        private void ReturnSecretKeyPublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedResult = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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

        [UUnitTest]
        public void IfSSLNotProvidedThenDefaultShouldBeFalse()
        {
            Debug.Log("Running IfSSLNotProvidedThenDefaultShouldBeFalse()");
            isPublished3 = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "");
            
            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAMessageIsPublished";
            unitTest.TestCaseName = "IfSSLNotProvidedThenDefaultShouldBeFalse";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish<string>(channel, message, ReturnNoSSLDefaultFalseCallback, DummyErrorCallback);
            mreNoSslPublish.WaitOne(310 * 1000);
            UUnitAssert.True(isPublished3, "Publish Failed with no SSL");
        }

        private void ReturnNoSSLDefaultFalseCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedResult = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedResult = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
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
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    object[] message = deserializedMessage[0] as object[];
                    if (message != null && message.Length > 0)
                    {
                        string receivedObjectString = pubnub.JsonPluggableLibrary.SerializeToJsonString(message[0]);
                        if (receivedObjectString != null && receivedObjectString == serializedObjectMessageForPublish)
                        {
                            isSerializedObjectMessageDetailedHistory = true;
                        }
                    }
                }
            }

            mreSerializedMessagePublishDetailedHistory.Set();
        }

        void DummyErrorCallback(string result)
        {
            Debug.Log("WhenAMessageIsPublished ErrorCallback : " + result);
        }
    }
}
