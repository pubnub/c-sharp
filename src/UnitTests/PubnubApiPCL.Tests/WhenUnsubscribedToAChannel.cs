using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenUnsubscribedToAChannel : TestHarness
    {
        private static ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        //ManualResetEvent meChannelSubscribed = new ManualResetEvent(false);
        //ManualResetEvent meChannelUnsubscribed = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        //bool receivedUnsubscribedMessage = false;
        //bool receivedChannelConnectedMessage = false;
        private static bool receivedGrantMessage = false;

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string authKey = "myAuth";
        private static string currentTestCase = "";

        private static Pubnub pubnub = null;

        private Server server;
        private UnitTestLog unitLog;


        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = new Server(new Uri("https://" + PubnubCommon.StubOrign));
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            pubnub = this.createPubNubInstance(config);

            pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(20).Async(new UTGrantResult());

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenUnsubscribedToAChannel Grant access failed.");
        }

        [Test]
        public void ThenShouldReturnUnsubscribedMessage()
        {
            receivedMessage = false;
            currentTestCase = "ThenShouldReturnUnsubscribedMessage";

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                AuthKey = authKey,
                Uuid = "mytestuuid",
                Secure = false
            };

            SubscribeCallback listenerSubCallack = new UTSubscribeCallback();
            pubnub = this.createPubNubInstance(config);
            pubnub.AddListener(listenerSubCallack);

            string channel = "hello_my_channel";
            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            subscribeManualEvent = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout); //Wait for Connect Status

            if (receivedMessage)
            {
                receivedMessage = false;

                Thread.Sleep(1000);
                subscribeManualEvent = new ManualResetEvent(false);
                pubnub.Unsubscribe<string>().Channels(new string[] { channel }).Execute();
                subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);
                Thread.Sleep(1000);
            }

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenUnsubscribedToAChannel --> ThenShouldReturnUnsubscribedMessage Failed");
        }

        //void ThenUnsubscribeInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
        //{
        //    try
        //    {
        //        if (receivedMessage != null)
        //        {
        //            var status = receivedMessage.StatusCode;
        //            if (status == 200)
        //            {
        //                receivedGrantMessage = true;
        //            }

        //        }
        //    }
        //    catch { }
        //    finally
        //    {
        //        grantManualEvent.Set();
        //    }
        //}

        //private void DummyMethodChannelSubscribeUserCallback(PNMessageResult<string> result)
        //{
        //}

        //private void DummyMethodChannelSubscribeConnectCallback(ConnectOrDisconnectAck result)
        //{
        //    if (result.StatusMessage.Contains("Connected"))
        //    {
        //        receivedChannelConnectedMessage = true;
        //    }
        //    meChannelSubscribed.Set();
        //}

        //private void DummyMethodUnsubscribeChannelUserCallback(string result)
        //{
        //}

        //private void DummyMethodUnsubscribeChannelConnectCallback(ConnectOrDisconnectAck result)
        //{
        //}

        //private void DummyMethodUnsubscribeChannelDisconnectCallback(ConnectOrDisconnectAck result)
        //{
        //    if (result.StatusMessage.Contains("Unsubscribed from"))
        //    {
        //        receivedUnsubscribedMessage = true;
        //    }
        //    meChannelUnsubscribed.Set();
        //}

        //private void DummyMethodNoExistChannelUnsubscribeChannelUserCallback(string result)
        //{
        //}

        //private void DummyMethodNoExistChannelUnsubscribeChannelConnectCallback(ConnectOrDisconnectAck result)
        //{
        //}

        //private void DummyMethodNoExistChannelUnsubscribeChannelDisconnectCallback1(ConnectOrDisconnectAck result)
        //{
        //}

        //private void DummyErrorCallback(PubnubClientError result)
        //{
        //}

        //private void NoExistChannelErrorCallback(PubnubClientError result)
        //{
        //    if (result != null && result.Message.ToLower().Contains("not subscribed"))
        //    {
        //        receivedNotSubscribedMessage = true;
        //    }
        //    meNotSubscribed.Set();
        //}

        private class UTGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerGrantResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        if (result.Channels != null && result.Channels.Count > 0)
                        {
                            foreach (KeyValuePair<string, Dictionary<string, PNAccessManagerKeyData>> channelKP in result.Channels)
                            {
                                string channel = channelKP.Key;
                                var read = result.Channels[channel][authKey].ReadEnabled;
                                var write = result.Channels[channel][authKey].WriteEnabled;
                                if (read && write)
                                {
                                    receivedGrantMessage = true;
                                }
                                else
                                {
                                    receivedGrantMessage = false;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    grantManualEvent.Set();
                }
            }
        }

        public class UTSubscribeCallback : SubscribeCallback
        {
            public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
            {
                if (message != null)
                {
                    Console.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
                }
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                //Console.WriteLine("SubscribeCallback: PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Console.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200 || status.Error)
                {
                    switch (currentTestCase)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    if (status.ErrorData != null)
                    {
                        Console.WriteLine(status.ErrorData.Information);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    switch (currentTestCase)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            receivedMessage = true;
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }
                else if (status.StatusCode == 200 && status.Category == PNStatusCategory.PNDisconnectedCategory)
                {
                    switch (currentTestCase)
                    {
                        case "ThenShouldReturnUnsubscribedMessage":
                            receivedMessage = true;
                            subscribeManualEvent.Set();
                            break;
                        default:
                            break;
                    }
                }


            }
        }

    }
}
