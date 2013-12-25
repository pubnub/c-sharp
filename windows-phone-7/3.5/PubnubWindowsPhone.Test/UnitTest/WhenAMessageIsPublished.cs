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
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Threading;
//using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenAMessageIsPublished : WorkItemTest
    {
        JsonSerializer serializer = new JsonSerializer();

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
        const string messageLarge2K = "Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime.\" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of";
        string messageObjectForUnencryptPublish = "";
        string messageObjectForEncryptPublish = "";
        string messageComplexObjectForPublish = "";
        string serializedObjectMessageForPublish;

        System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();

        [ClassInitialize]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Inconclusive("WhenAClientIsPresent Grant access failed.");
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

                    pubnub.GrantAccess<string>(channel, true, true, 20, ThenPublishInitializeShouldReturnGrantMessage, DummyErrorCallback);
                    //Thread.Sleep(1000);

                    grantManualEvent.WaitOne();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed.");
                        });
                });
        }

        [Asynchronous]
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

        [TestMethod, Asynchronous]
        public void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isUnencryptPublished = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";
                    string message = messageForUnencryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptPublishCodeCallback, DummyErrorCallback);
                    mreUnencryptedPublish.WaitOne(310 * 1000);

                    if (!isUnencryptPublished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isUnencryptPublished, "Unencrypt Publish Failed");
                           TestComplete();
                       });
                    }
                    else
                    {
                        pubnub.DetailedHistory<string>(channel, -1, unEncryptPublishTimetoken, -1, false, CaptureUnencryptDetailedHistoryCallback, DummyErrorCallback);
                        mreUnencryptDetailedHistory.WaitOne(310 * 1000);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isUnencryptDetailedHistory, "Unable to match the successful unencrypt Publish");
                           TestComplete();
                       });
                    }
                });
        }

        [TestMethod, Asynchronous]
        public void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isUnencryptObjectPublished = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";
                    object message = new CustomClass();
                    messageObjectForUnencryptPublish = JsonConvert.SerializeObject(message);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptObjectPublishCodeCallback, DummyErrorCallback);
                    mreUnencryptObjectPublish.WaitOne(310 * 1000);
                    Thread.Sleep(1000);
                    if (!isUnencryptObjectPublished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isUnencryptObjectPublished, "Unencrypt Publish Failed");
                           TestComplete();
                       });
                    }
                    else
                    {
                        pubnub.DetailedHistory<string>(channel, -1, unEncryptObjectPublishTimetoken, -1, false, CaptureUnencryptObjectDetailedHistoryCallback, DummyErrorCallback);
                        mreUnencryptObjectDetailedHistory.WaitOne(310 * 1000);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isUnencryptObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
                           TestComplete();
                       });
                    }
                });
        }

        [TestMethod, Asynchronous]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isEncryptObjectPublished = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
                    string channel = "hello_my_channel";
                    
                    object message = new SecretCustomClass();

                    messageObjectForEncryptPublish = JsonConvert.SerializeObject(message);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Publish<string>(channel, message, ReturnSuccessEncryptObjectPublishCodeCallback, DummyErrorCallback);
                    mreEncryptObjectPublish.WaitOne(310 * 1000);

                    if (!isEncryptObjectPublished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isEncryptObjectPublished, "Encrypt Object Publish Failed");
                           TestComplete();
                       });
                    }
                    else
                    {
                        pubnub.DetailedHistory<string>(channel, -1, encryptObjectPublishTimetoken, -1, false, CaptureEncryptObjectDetailedHistoryCallback, DummyErrorCallback);
                        mreEncryptObjectDetailedHistory.WaitOne(310 * 1000);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isEncryptObjectDetailedHistory, "Unable to match the successful encrypt object Publish");
                           TestComplete();
                       });
                    }
                });
        }


        [TestMethod, Asynchronous]
        public void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isEncryptPublished = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
                    string channel = "hello_my_channel";
                    string message = messageForEncryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenEncryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Publish<string>(channel, message, ReturnSuccessEncryptPublishCodeCallback, DummyErrorCallback);
                    mreEncryptPublish.WaitOne(310 * 1000);

                    if (!isEncryptPublished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isEncryptPublished, "Encrypt Publish Failed");
                           TestComplete();
                       });
                    }
                    else
                    {
                        pubnub.DetailedHistory<string>(channel, -1, encryptPublishTimetoken, -1, false, CaptureEncryptDetailedHistoryCallback, DummyErrorCallback);
                        mreEncryptDetailedHistory.WaitOne(310 * 1000);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isEncryptDetailedHistory, "Unable to decrypt the successful Publish");
                           TestComplete();
                       });
                    }
                });
        }

        [TestMethod, Asynchronous]
        public void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isSecretEncryptPublished = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "enigma", false);
                    string channel = "hello_my_channel";
                    string message = messageForSecretEncryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Publish<string>(channel, message, ReturnSuccessSecretEncryptPublishCodeCallback, DummyErrorCallback);
                    mreSecretEncryptPublish.WaitOne(310 * 1000);

                    if (!isSecretEncryptPublished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isSecretEncryptPublished, "Secret Encrypt Publish Failed");
                           TestComplete();
                       });
                    }
                    else
                    {
                        pubnub.DetailedHistory<string>(channel, -1, secretEncryptPublishTimetoken, -1, false, CaptureSecretEncryptDetailedHistoryCallback, DummyErrorCallback);
                        mreSecretEncryptDetailedHistory.WaitOne(310 * 1000);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(isSecretEncryptDetailedHistory, "Unable to decrypt the successful Secret key Publish");
                           TestComplete();
                       });
                    }
                });
        }

        [Asynchronous]
        private void ReturnSuccessUnencryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void ReturnSuccessUnencryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void ReturnSuccessEncryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void ReturnSuccessEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void ReturnSuccessSecretEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void CaptureUnencryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void CaptureUnencryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void CaptureEncryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void CaptureEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
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

        [Asynchronous]
        private void CaptureSecretEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString() == messageForSecretEncryptPublish)
                    {
                        isSecretEncryptDetailedHistory = true;
                    }
                }
            }

            mreSecretEncryptDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                               {
                                   Assert.IsNotNull(pubnub.GenerateGuid());
                                   TestComplete();
                               });
                });

        }

        [TestMethod, Asynchronous]
        public void ThenPublishKeyShouldNotBeEmpty()
        {
            bool isExpectedException = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub("", PubnubCommon.SubscribeKey, "", "", false);

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
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(isExpectedException);
                            TestComplete();
                        });
                });
        }


        [TestMethod, Asynchronous]
        public void ThenOptionalSecretKeyShouldBeProvidedInConstructor()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isPublished2 = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey);
                    string channel = "hello_my_channel";
                    string message = "Pubnub API Usage Example";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenOptionalSecretKeyShouldBeProvidedInConstructor";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Publish<string>(channel, message, ReturnSecretKeyPublishCallback, DummyErrorCallback);
                    mreOptionalSecretKeyPublish.WaitOne(310 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                               {
                                   Assert.IsTrue(isPublished2, "Publish Failed with secret key");
                                   TestComplete();
                               });
                });
        }

        [Asynchronous]
        private void ReturnSecretKeyPublishCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isPublished2 = true;
                    }
                }
            }
            mreOptionalSecretKeyPublish.Set();
        }

        [TestMethod,Asynchronous]
        public void IfSSLNotProvidedThenDefaultShouldBeFalse()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isPublished3 = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "");
                    string channel = "hello_my_channel";
                    string message = "Pubnub API Usage Example";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "IfSSLNotProvidedThenDefaultShouldBeFalse";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Publish<string>(channel, message, ReturnNoSSLDefaultFalseCallback, DummyErrorCallback);
                    mreNoSslPublish.WaitOne(310 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                               {
                                   Assert.IsTrue(isPublished3, "Publish Failed with no SSL");
                                   TestComplete();
                               });
                });
        }

        [Asynchronous]
        private void ReturnNoSSLDefaultFalseCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                        if (deserializedMessage is object[])
                        {
                            long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                            string statusMessage = (string)deserializedMessage[1];
                            if (statusCode == 1 && statusMessage.ToLower() == "sent")
                            {
                                isPublished3 = true;
                            }
                        }
                    });
            }
            mreNoSslPublish.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
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
                    mreComplexObjectPublish.WaitOne(310 * 1000);

                    if (!isComplexObjectPublished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(isComplexObjectPublished, "Complex Object Publish Failed");
                                TestComplete();
                            });
                    }
                    else
                    {
                        pubnub.DetailedHistory<string>(channel, -1, complexObjectPublishTimetoken, -1, false, CaptureComplexObjectDetailedHistoryCallback, DummyErrorCallback);
                        mreComplexObjectDetailedHistory.WaitOne(310 * 1000);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(isComplexObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
                                TestComplete();
                            });
                    }
                });
        }

        [Asynchronous]
        private void ReturnSuccessComplexObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                    });
            }


            mreComplexObjectPublish.Set();

        }

        [Asynchronous]
        private void CaptureComplexObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                    });
            }

            mreComplexObjectDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        {
            ThreadPool.QueueUserWorkItem((s) =>
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
                    mreSerializedObjectMessageForPublish.WaitOne(310 * 1000);

                    if (!isSerializedObjectMessagePublished)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(isSerializedObjectMessagePublished, "Serialized Object Message Publish Failed");
                                TestComplete();
                            });
                    }
                    else
                    {
                        pubnub.DetailedHistory<string>(channel, -1, serializedMessagePublishTimetoken, -1, false, CaptureSerializedMessagePublishDetailedHistoryCallback, DummyErrorCallback);
                        mreSerializedMessagePublishDetailedHistory.WaitOne(310 * 1000);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(isSerializedObjectMessageDetailedHistory, "Unable to match the successful serialized object message Publish");
                                TestComplete();
                            });
                    }
                });
        }

        [Asynchronous]
        private void ReturnSuccessSerializedObjectMessageForPublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                    });
            }
            mreSerializedObjectMessageForPublish.Set();
        }

        [Asynchronous]
        private void CaptureSerializedMessagePublishDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                    });
            }

            mreSerializedMessagePublishDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenLargeMessageShoudFailWithMessageTooLargeInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    isLargeMessagePublished = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenLargeMessageShoudFailWithMessageTooLargeInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    string message = messageLarge2K.Substring(0, 1320);

                    pubnub.Publish<string>(channel, message, ReturnPublishMessageTooLargeInfoCallback, ReturnPublishMessageTooLargeErrorCallback);
                    mreLaregMessagePublish.WaitOne(310 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(isLargeMessagePublished, "Message Too Large is not failing as expected.");
                            TestComplete();
                        });
                });
        }

        [Asynchronous]
        private void ReturnPublishMessageTooLargeInfoCallback(string result)
        {
        }

        [Asynchronous]
        private void ReturnPublishMessageTooLargeErrorCallback(PubnubClientError pubnubError)
        {
            if (pubnubError != null)
            {
                if (pubnubError.Message.ToLower().IndexOf("message too large") >= 0)
                {
                    isLargeMessagePublished = true;
                }
            }
            mreLaregMessagePublish.Set();
        }
        
        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
