using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToWildcardChannel
    {
        ManualResetEvent mreSubscribeConnect = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreUnsubscribe = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);
        ManualResetEvent mreSubscribe = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedGrantMessage = false;
        
        //3 channels for multiplex subscribe
        bool receivedChannelGroupMessage = false;
        bool receivedChannelMessage = false;
        bool receivedWildcardChannelMessage = false;

        int manualResetEventsWaitTimeout = 310 * 1000;
        object publishedMessage = null;
        bool isPublished = false;

        string channelGroupName = "";

        Pubnub pubnub = null;
        string currentTestCase = "";
        bool endOfPublish = false;

        [SetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;
            currentTestCase = "Init";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "foo.*";
            mreGrant = new ManualResetEvent(false);
            pubnub.GrantAccess<string>(channel, true, true, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);
            mreGrant.WaitOne();

            if (receivedGrantMessage)
            {
                channel = "hello_my_channel";
                mreGrant = new ManualResetEvent(false);
                pubnub.GrantAccess<string>(channel, true, true, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }

            if (receivedGrantMessage)
            {
                channel = "hello_my_channel1";
                mreGrant = new ManualResetEvent(false);
                pubnub.GrantAccess<string>(channel, true, true, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }

            if (receivedMessage)
            {
                channelGroupName = "hello_my_group";
                mreGrant = new ManualResetEvent(false);
                pubnub.ChannelGroupGrantAccess<string>(channelGroupName, true, true, 20, ThenSubscribeInitializeShouldReturnGrantMessage, DummyErrorCallback);
                Thread.Sleep(1000);
                mreGrant.WaitOne();
            }
            pubnub.EndPendingRequests();
            pubnub = null;
            Assert.True(receivedGrantMessage, "WhenSubscribedToWildcardChannel Grant access failed.");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenItShouldReturnReceivedMessage Failed");
        }

        private void CommonSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnReceivedMessage" : "ThenSubscribeShouldReturnReceivedCipherMessage";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(wildCardSubscribeChannel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout, false);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";
            pubnub.Publish<string>(publishChannel, publishedMessage, dummyPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout, false);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout, false);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(wildCardSubscribeChannel, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout, false);
            }
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

            Thread.Sleep(2000);
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipherSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipher()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnReceivedMessageCipher()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipher Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessage()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", false);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessage Failed");
        }

        private void CommonSubscribeShouldReturnEmojiMessageBasedOnParams(string secretKey, string cipherKey, bool ssl)
        {
            receivedMessage = false;
            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, secretKey, cipherKey, ssl);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = (string.IsNullOrEmpty(cipherKey)) ? "ThenSubscribeShouldReturnReceivedEmojiMessage" : "ThenSubscribeShouldReturnReceivedCipherEmojiMessage";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(wildCardSubscribeChannel, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout, false);

            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Text with 😜 emoji 🎉.";
            pubnub.Publish<string>(publishChannel, publishedMessage, dummyPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout, false);

            if (isPublished)
            {
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout, false);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(wildCardSubscribeChannel, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout, false);
            }
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", true);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", false);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecret()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecret Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL Failed");
        }

        [Test]
        public void ThenSubscribeShouldReturnEmojiMessageSecretSSL()
        {
            receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", true);
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSecretSSL Failed");
        }

        [Test]
        public void ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage()
        {
            receivedMessage = false;
            currentTestCase = "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";
            string subChannelName = "hello_my_channel";
            string commaDelimitedChannel = string.Format("{0},{1}", subChannelName, wildCardSubscribeChannel);
            channelGroupName = "hello_my_group";
            string channelAddForGroup = "hello_my_channel1";
            string pubWildChannelName = "foo.a";

            mreSubscribe = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelAddForGroup }, channelGroupName, ChannelGroupAddCallback, DummyErrorCallback);
            mreSubscribe.WaitOne(manualResetEventsWaitTimeout, false);

            mreSubscribe = new ManualResetEvent(false);

            mreSubscribeConnect = new ManualResetEvent(false);
            pubnub.Subscribe<string>(commaDelimitedChannel, channelGroupName, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, null, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout, false);

            Thread.Sleep(3000);
            mrePublish = new ManualResetEvent(false);
            publishedMessage = "Test for cg";
            pubnub.Publish<string>(channelAddForGroup, publishedMessage, dummyPublishCallback, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
            mrePublish.WaitOne(manualResetEventsWaitTimeout, false);

            if (isPublished)
            {
                Thread.Sleep(3000);
                mrePublish = new ManualResetEvent(false);
                publishedMessage = "Test for wc";
                pubnub.Publish<string>(pubWildChannelName, publishedMessage, dummyPublishCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout, false);
            }

            if (isPublished)
            {
                Thread.Sleep(3000);
                mrePublish = new ManualResetEvent(false);
                publishedMessage = "Test for normal ch";
                pubnub.Publish<string>(subChannelName, publishedMessage, dummyPublishCallback, DummyErrorCallback);
                manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 310 * 1000;
                mrePublish.WaitOne(manualResetEventsWaitTimeout, false);
            }

            endOfPublish = true;
            if (isPublished)
            {
                Thread.Sleep(3000);
                mreSubscribe.WaitOne(manualResetEventsWaitTimeout, false);

                mreUnsubscribe = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>(commaDelimitedChannel, channelGroupName, dummyUnsubscribeCallback, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, null, DummyErrorCallback);
                mreUnsubscribe.WaitOne(manualResetEventsWaitTimeout, false);
            }
            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage Failed");
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback()
        {
            receivedMessage = false;
            currentTestCase = "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";

            mreSubscribe = new ManualResetEvent(false);
            mreSubscribeConnect = new ManualResetEvent(false);

            pubnub.Subscribe<string>(wildCardSubscribeChannel, channelGroupName, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, ReceivedWildcardPresenceCallbackWhenSubscribed, DummyErrorCallback);
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout, false);

            mreSubscribe.WaitOne(manualResetEventsWaitTimeout, false);

            Assert.True(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback Failed");
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        [Test]
        public void ThenSubscribeShouldNotReturnWildCardPresenceEventWhenNoCallback()
        {
            receivedMessage = false;
            currentTestCase = "ThenSubscribeShouldNotReturnWildCardPresenceEventWhenNoCallback";

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
            pubnub.SessionUUID = "myuuid";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenSubscribedToWildcardChannel";
            unitTest.TestCaseName = "ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback";

            pubnub.PubnubUnitTest = unitTest;

            string wildCardSubscribeChannel = "foo.*";

            mreSubscribe = new ManualResetEvent(false);
            mreSubscribeConnect = new ManualResetEvent(false);

            pubnub.Subscribe<string>(wildCardSubscribeChannel, channelGroupName, ReceivedMessageCallbackWhenSubscribed, SubscribeDummyMethodForConnectCallback, null, DummyErrorCallback);
            manualResetEventsWaitTimeout = (unitTest.EnableStubTest) ? 1000 : 10 * 1000;
            mreSubscribeConnect.WaitOne(manualResetEventsWaitTimeout, false);

            mreSubscribe.WaitOne(manualResetEventsWaitTimeout, false);

            Assert.True(!receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldNotReturnWildCardPresenceEventWhenNoCallback Failed");
            pubnub.PubnubUnitTest = null;
            pubnub.EndPendingRequests();
            pubnub = null;

        }

        void ThenSubscribeInitializeShouldReturnGrantMessage(string receivedMessage)
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
                mreGrant.Set();
            }
        }

        private void ReceivedMessageCallbackWhenSubscribed(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Console.WriteLine("ReceivedMessageCallbackWhenSubscribed -> result = " + result);
                List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                if (deserializedMessage != null && deserializedMessage.Count > 0)
                {
                    object subscribedObject = (object)deserializedMessage[0];
                    if (subscribedObject != null)
                    {
                        string serializedResultMessage = pubnub.JsonPluggableLibrary.SerializeToJsonString(subscribedObject);
                        string serializedPublishMesage = pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage);
                        Console.WriteLine(serializedPublishMesage);
                        Console.WriteLine(serializedResultMessage);
                        if (serializedResultMessage == serializedPublishMesage)
                        {
                            receivedMessage = true;
                        }

                    }
                }
            }
            if (currentTestCase == "ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessageBased")
            {
                if (endOfPublish)
                {
                    mreSubscribe.Set();
                }
            }
            else
            {
                mreSubscribe.Set();
            }
        }

        private void ReceivedWildcardPresenceCallbackWhenSubscribed(string result)
        {
            Console.WriteLine(string.Format("ReceivedWildcardPresenceCallbackWhenSubscribed  = {0}", result));
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                receivedMessage = true;
            }
            mreSubscribe.Set();
        }

        private void dummyPublishCallback(string result)
        {
            //Console.WriteLine("dummyPublishCallback -> result = " + result);
            try
            {
                if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
                {
                    List<object> deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result);
                    if (deserializedMessage != null && deserializedMessage.Count > 0)
                    {
                        long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                        string statusMessage = (string)deserializedMessage[1];
                        if (statusCode == 1 && statusMessage.ToLower() == "sent")
                        {
                            isPublished = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            mrePublish.Set();
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
            if (result != null)
            {
                Console.WriteLine("DummyErrorCallback result = " + result.Message);
            }
            if (currentTestCase == "Init")
            {
                mreGrant.Set();
            }
        }

        private void dummyUnsubscribeCallback(string result)
        {

        }

        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            mreSubscribeConnect.Set();
        }

        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            mreUnsubscribe.Set();
        }

        void ChannelGroupAddCallback(string receivedMessage)
        {
            Console.WriteLine(string.Format("ChannelGroupAddCallback = {0}", receivedMessage));
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
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string serviceType = dictionary["service"].ToString();
                            bool errorStatus = (bool)dictionary["error"];
                            string currentChannelGroup = serializedMessage[1].ToString().Substring(1); //assuming no namespace for channel group
                            string statusMessage = dictionary["message"].ToString();

                            if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
                            {
                                if (currentChannelGroup == channelGroupName)
                                {
                                    receivedChannelGroupMessage = true;
                                }
                            }
                        }

                    }

                }
            }
            catch { }
            finally
            {
                mreSubscribe.Set();
            }

        }

    }
}
