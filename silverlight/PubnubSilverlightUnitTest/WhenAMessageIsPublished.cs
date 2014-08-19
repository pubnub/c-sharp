using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Silverlight.Testing;
using PubNubMessaging.Core;

namespace PubnubSilverlight.UnitTest
{
    [TestClass]
    public class WhenAMessageIsPublished : SilverlightTest
    {
        bool isPublished2 = false;
        bool isPublished3 = false;

        bool isUnencryptPublished = false;
        bool isUnencryptObjectPublished = false;
        bool isEncryptObjectPublished = false;
        bool isUnencryptDH = false;
        bool isUnencryptObjectDH = false;
        bool isEncryptObjectDH = false;
        bool isEncryptPublished = false;
        bool isSecretEncryptPublished = false;
        bool isEncryptDH = false;
        bool isSecretEncryptDH = false;
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

        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreDetailedHistory = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);

        const string messageForUnencryptPublish = "Pubnub Messaging API 1";
        const string messageForEncryptPublish = "漢語";
        const string messageForSecretEncryptPublish = "Pubnub Messaging API 2";
        const string messageLarge32K = "Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say. \"We need all the facts. We need all the information, \" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need. \" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin. \" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime. \" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. ONE..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. TWO..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. THREE..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. FOUR..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. FIVE..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. SIX..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. SEVEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. EIGHT..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. NINE..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. TEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. ELEVEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. THIRTEEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. FOURTEEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. FIFTEEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. SIXTEEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. SEVENTEEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. EIGHTEEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. NINETEEN..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. TWENTY..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. TWENTY ONE..Sen. John McCain, one of the lawmakers who received the letter, said the use of chemical weapons was only a matter of time. alpha beta 12";
        string messageObjectForUnencryptPublish = "";
        string messageObjectForEncryptPublish = "";
        string messageComplexObjectForPublish = "";
        string serializedObjectMessageForPublish;

        bool receivedGrantMessage = false;
        bool grantInitCallbackInvoked = false;

        [ClassInitialize, Asynchronous]
        public void Init()
        {
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

                    EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenPublishInitializeShouldReturnGrantMessage, DummyGrantErrorCallback));
                    mreGrant.WaitOne(310 * 1000);

                    EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed."));

                    EnqueueTestComplete();
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
                mreGrant.Set();
            }
        }

        [TestMethod, Asynchronous]
        public void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";
                    string message = messageForUnencryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptPublishCodeCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isUnencryptPublished)
                        {
                            Assert.IsTrue(isUnencryptPublished, "Unencrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, unEncryptPublishTimetoken, -1, false, CaptureUnencryptDetailedHistoryCallback, DummyDetailedHistoryErrorCallback);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isUnencryptDH, "Unable to match the successful unencrypt Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessUnencryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isUnencryptPublished = true;
                        unEncryptPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureUnencryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jObj = JArray.Parse(receivedObj[0].ToString());
                    if (jObj.Count > 0 && jObj[0].ToString() == messageForUnencryptPublish)
                    {
                        isUnencryptDH = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptObjectPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    object message = new CustomClass();
                    messageObjectForUnencryptPublish = JsonConvert.SerializeObject(message);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptObjectPublishCodeCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isUnencryptObjectPublished)
                        {
                            Assert.IsTrue(isUnencryptObjectPublished, "Unencrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, unEncryptObjectPublishTimetoken, -1, false, CaptureUnencryptObjectDetailedHistoryCallback, DummyDetailedHistoryErrorCallback);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isUnencryptObjectDH, "Unable to match the successful unencrypt object Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessUnencryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isUnencryptObjectPublished = true;
                        unEncryptObjectPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureUnencryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr.Count > 0 && jArr[0].ToString(Formatting.None) == messageObjectForUnencryptPublish)
                    {
                        isUnencryptObjectDH = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            isEncryptObjectPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
                    string channel = "hello_my_channel";
                    object message = new SecretCustomClass();
                    messageObjectForEncryptPublish = JsonConvert.SerializeObject(message);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessEncryptObjectPublishCodeCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isEncryptObjectPublished)
                        {
                            Assert.IsTrue(isEncryptObjectPublished, "Encrypt Object Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, encryptObjectPublishTimetoken, -1, false, CaptureEncryptObjectDetailedHistoryCallback, DummyDetailedHistoryErrorCallback);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isEncryptObjectDH, "Unable to match the successful encrypt object Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessEncryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isEncryptObjectPublished = true;
                        encryptObjectPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureEncryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr.Count > 0 && jArr[0].ToString(Formatting.None) == messageObjectForEncryptPublish)
                    {
                        isEncryptObjectDH = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isEncryptPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
                    string channel = "hello_my_channel";
                    string message = messageForEncryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenEncryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessEncryptPublishCodeCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isEncryptPublished)
                        {
                            Assert.IsTrue(isEncryptPublished, "Encrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, encryptPublishTimetoken, -1, false, CaptureEncryptDetailedHistoryCallback, DummyDetailedHistoryErrorCallback);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isEncryptDH, "Unable to decrypt the successful Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isEncryptPublished = true;
                        encryptPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr.Count > 0 && jArr[0].ToString() == messageForEncryptPublish)
                    {
                        isEncryptDH = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isSecretEncryptPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "key", "enigma", false);
                    string channel = "hello_my_channel";
                    string message = messageForSecretEncryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessSecretEncryptPublishCodeCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isSecretEncryptPublished)
                        {
                            Assert.IsTrue(isSecretEncryptPublished, "Secret Encrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, secretEncryptPublishTimetoken, -1, false, CaptureSecretEncryptDetailedHistoryCallback, DummyDetailedHistoryErrorCallback);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isSecretEncryptDH, "Unable to decrypt the successful Secret key Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessSecretEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isSecretEncryptPublished = true;
                        secretEncryptPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            mrePublish.Set();
        }

        [Asynchronous]
        private void CaptureSecretEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr.Count > 0 && jArr[0].ToString() == messageForSecretEncryptPublish)
                    {
                        isSecretEncryptDH = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod]
        public void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            Assert.IsNotNull(pubnub.GenerateGuid());
        }

        [TestMethod]
        [ExpectedException(typeof(MissingMemberException))]
        public void ThenPublishKeyShouldNotBeEmpty()
        {
            Pubnub pubnub = new Pubnub("", PubnubCommon.SubscribeKey, "", "", false);

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish<string>(channel, message, null, DummyPublishErrorCallback);
        }

        [TestMethod, Asynchronous]
        public void ThenOptionalSecretKeyShouldBeProvidedInConstructor()
        {
            isPublished2 = false;
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "key");
                    string channel = "hello_my_channel";
                    string message = "Pubnub API Usage Example";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenOptionalSecretKeyShouldBeProvidedInConstructor";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSecretKeyPublishCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(isPublished2, "Publish Failed with secret key"));

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSecretKeyPublishCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isPublished2 = true;
                    }
                }
            }
            mrePublish.Set();
        }

        [TestMethod, Asynchronous]
        public void IfSSLNotProvidedThenDefaultShouldBeFalse()
        {
            isPublished3 = false;
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "");
                    string channel = "hello_my_channel";
                    string message = "Pubnub API Usage Example";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "IfSSLNotProvidedThenDefaultShouldBeFalse";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnNoSSLDefaultFalseCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(isPublished3, "Publish Failed with no SSL"));

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnNoSSLDefaultFalseCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isPublished3 = true;
                    }
                }
            }
            mrePublish.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            isComplexObjectPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    object message = new PubnubDemoObject();
                    messageComplexObjectForPublish = JsonConvert.SerializeObject(message);

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessComplexObjectPublishCodeCallback, ComplexObjectMessagePublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                        {
                            if (!isComplexObjectPublished)
                            {
                                Assert.IsTrue(isComplexObjectPublished, "Complex Object Publish Failed");
                            }
                            else
                            {
                                pubnub.DetailedHistory<string>(channel, -1, complexObjectPublishTimetoken, -1, false, CaptureComplexObjectDetailedHistoryCallback, DummyDetailedHistoryErrorCallback);
                                mreDetailedHistory.WaitOne(310 * 1000);
                                Assert.IsTrue(isComplexObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
                            }
                        });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
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

            mrePublish.Set();
        }

        [Asynchronous]
        private void CaptureComplexObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message.Count > 0 && message[0].ToString(Formatting.None) == messageComplexObjectForPublish)
                    {
                        isComplexObjectDetailedHistory = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        {
            isSerializedObjectMessagePublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    pubnub.EnableJsonEncodingForPublish = false;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenDisableJsonEncodeShouldSendSerializedObjectMessage";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    object message = "{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}";
                    serializedObjectMessageForPublish = message.ToString();

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessSerializedObjectMessageForPublishCallback, DummyPublishErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                        {
                            if (!isSerializedObjectMessagePublished)
                            {
                                EnqueueCallback(() => Assert.IsTrue(isSerializedObjectMessagePublished, "Serialized Object Message Publish Failed"));
                            }
                            else
                            {
                                Thread.Sleep(1000); //Adding 1 sec wait because sometimes I am getting empty array from server
                                pubnub.DetailedHistory<string>(channel, -1, serializedMessagePublishTimetoken, -1, false, CaptureSerializedMessagePublishDetailedHistoryCallback, DummyDetailedHistoryErrorCallback);
                                mreDetailedHistory.WaitOne(310 * 1000);
                                Assert.IsTrue(isSerializedObjectMessageDetailedHistory, "Unable to match the successful serialized object message Publish");
                            }
                        });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
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
            mrePublish.Set();
        }

        [Asynchronous]
        private void CaptureSerializedMessagePublishDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message.Count > 0 && message[0].ToString(Formatting.None) == serializedObjectMessageForPublish)
                    {
                        isSerializedObjectMessageDetailedHistory = true;
                    }
                }
            }

            mreDetailedHistory.Set();
        }

        //[TestMethod, Asynchronous] This test case is commented because Silverlight do not support URL length > 2050 encoded characters
        public void ThenLargeMessageShoudFailWithMessageTooLargeInfo()
        {
            isLargeMessagePublished = false;
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", true);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenLargeMessageShoudFailWithMessageTooLargeInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    //NOTE: Eventhough Pubnub Server allows 32K characters, Silverlight allows max 2050 characters after encoding including http or https
                    string message = messageLarge32K.Substring(0, 1432); 
                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, DummyPublishMessageTooLargeInfoCallback, PublishMessageTooLargeErrorCallback));
                    mrePublish.WaitOne(310 * 100);
                    EnqueueCallback(() => Assert.IsTrue(isLargeMessagePublished, "Message Too Large is not failing as expected."));

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        private void DummyPublishMessageTooLargeInfoCallback(string result)
        {
            mrePublish.Set();
        }

        [Asynchronous]
        private void PublishMessageTooLargeErrorCallback(PubnubClientError result)
        {
            if (result != null && result.StatusCode > 0)
            {
                string message = result.Message;
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(message);
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
            mrePublish.Set();
        }

        [Asynchronous]
        private void ComplexObjectMessagePublishErrorCallback(PubnubClientError result)
        {
            mrePublish.Set();
        }

        [Asynchronous]
        private void DummyPublishErrorCallback(PubnubClientError result)
        {
            mrePublish.Set();
        }

        [Asynchronous]
        private void DummyGrantErrorCallback(PubnubClientError result)
        {
            mreGrant.Set();
        }

        [Asynchronous]
        private void DummyDetailedHistoryErrorCallback(PubnubClientError result)
        {
            mreDetailedHistory.Set();
        }

    }
}
