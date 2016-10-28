//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using System.ComponentModel;
//using System.Threading;
//using System.Collections;
////using Newtonsoft.Json;
////using Newtonsoft.Json.Linq;
//using PubnubApi;
//namespace PubNubMessaging.Tests
//{
//    [TestFixture]
//    public class WhenUnsubscribedToAChannelGroup : TestHarness
//    {
//        ManualResetEvent unsubscribeManualEvent = new ManualResetEvent(false);
//        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

//        bool receivedMessage = false;
//        bool receivedGrantMessage = false;
//        bool receivedChannelGroupMessage = false;
//        bool receivedChannelGroupConnectedMessage = false;

//        string currentUnitTestCase = "";
//        string channelGroupName = "hello_my_group";

//        int manualResetEventsWaitTimeout = 310 * 1000;

//        Pubnub pubnub = null;

//        [TestFixtureSetUp]
//        public void Init()
//        {
//            if (!PubnubCommon.PAMEnabled) return;

//            receivedGrantMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                SecretKey = PubnubCommon.SecretKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            pubnub.Grant().ChannelGroups(new string[] { channelGroupName }).Read(true).Write(true).Manage(true).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenChannelGroupInitializeShouldReturnGrantMessage, Error = DummyUnsubscribeErrorCallback });
//            Thread.Sleep(1000);

//            grantManualEvent.WaitOne();

//            pubnub.Destroy();
//            pubnub = null;
//            Assert.IsTrue(receivedGrantMessage, "WhenUnsubscribedToAChannelGroup Grant access failed.");
//        }

//        [Test]
//        public void ThenShouldReturnUnsubscribedMessage()
//        {
//            currentUnitTestCase = "ThenShouldReturnUnsubscribedMessage";
//            receivedMessage = false;
//            receivedChannelGroupMessage = false;
//            receivedChannelGroupConnectedMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);
//            pubnub.SessionUUID = "myuuid";

//            channelGroupName = "hello_my_group";
//            string channelName = "hello_my_channel";

//            unsubscribeManualEvent = new ManualResetEvent(false);
//            pubnub.AddChannelsToChannelGroup().Channels(new string[] { channelName }).ChannelGroup(channelGroupName).Async(new PNCallback<PNChannelGroupsAddChannelResult>() { Result = ChannelGroupAddCallback, Error = DummyErrorCallback });
//            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
//            if (receivedChannelGroupMessage)
//            {
//                unsubscribeManualEvent = new ManualResetEvent(false);
//                pubnub.Subscribe<string>().ChannelGroups(new string[] { channelGroupName }).Execute(new SubscribeCallback<string>() { Message = DummyMethodChannelSubscribeUserCallback, Connect = DummyMethodChannelSubscribeConnectCallback, Disconnect = DummyMethodSubscribeChannelDisconnectCallback, Error = DummyErrorCallback });
//                Thread.Sleep(1000);
//                unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

//                if (receivedChannelGroupConnectedMessage)
//                {
//                    unsubscribeManualEvent = new ManualResetEvent(false);
//                    pubnub.Unsubscribe<string>().ChannelGroups(new string[] { channelGroupName }).Execute(new UnsubscribeCallback() { Error = DummyErrorCallback });
//                    unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
//                }

//                pubnub.Destroy();
//                pubnub = null;

//                Assert.IsTrue(receivedMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
//            }
//            else
//            {
//                Assert.IsTrue(receivedChannelGroupMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
//            }
//        }

//        private void DummyMethodChannelSubscribeUserCallback(PNMessageResult<string> result)
//        {
//        }

//        private void DummyMethodChannelSubscribeConnectCallback(ConnectOrDisconnectAck result)
//        {
//            if (result.StatusMessage.Contains("Connected"))
//            {
//                receivedChannelGroupConnectedMessage = true;
//            }
//            unsubscribeManualEvent.Set();
//        }

//        private void DummyMethodSubscribeChannelDisconnectCallback(ConnectOrDisconnectAck result)
//        {
//            if (result.StatusMessage.Contains("Unsubscribed from"))
//            {
//                receivedMessage = true;
//            }
//            unsubscribeManualEvent.Set();
//        }

//        private void DummyMethodUnsubscribeChannelUserCallback(string result)
//        {
//        }

//        private void DummyMethodUnsubscribeChannelConnectCallback(ConnectOrDisconnectAck result)
//        {
//        }

//        //private void DummyMethodUnsubscribeChannelDisconnectCallback(ConnectOrDisconnectAck result)
//        //{
//        //    if (result.StatusMessage.Contains("Unsubscribed from"))
//        //    {
//        //        receivedMessage = true;
//        //    }
//        //    unsubscribeManualEvent.Set();
//        //}

//        void ChannelGroupAddCallback(PNChannelGroupsAddChannelResult receivedMessage)
//        {
//            try
//            {
//                if (receivedMessage != null)
//                {
//                    int statusCode = receivedMessage.StatusCode;
//                    string serviceType = receivedMessage.Service;
//                    bool errorStatus = receivedMessage.Error;
//                    string currentChannelGroup = receivedMessage.ChannelGroupName.Substring(1); //assuming no namespace for channel group
//                    string statusMessage = receivedMessage.StatusMessage;
//                    if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
//                    {
//                        if (currentChannelGroup == channelGroupName)
//                        {
//                            receivedChannelGroupMessage = true;
//                        }
//                    }
//                }
//            }
//            catch { }
//            finally
//            {
//                unsubscribeManualEvent.Set();
//            }

//        }

//        void ThenChannelGroupInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
//        {
//            try
//            {
//                if (receivedMessage != null)
//                {
//                    var status = receivedMessage.StatusCode;
//                    if (status == 200)
//                    {
//                        receivedGrantMessage = true;
//                    }
//                }
//            }
//            catch { }
//            finally
//            {
//                grantManualEvent.Set();
//            }
//        }

//        private void DummyUnsubscribeErrorCallback(PubnubClientError result)
//        {
//            unsubscribeManualEvent.Set();
//        }

//        private void DummySubscribeErrorCallback(PubnubClientError result)
//        {
//            unsubscribeManualEvent.Set();
//        }

//        private void DummyErrorCallback(PubnubClientError result)
//        {
//        }

//    }
//}
