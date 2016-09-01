using NUnit.Framework;
using System.Threading;
using PubnubApi;
using HttpMock;
using System.Collections.Generic;
using MockServer;
using System;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenChannelGroupIsRequested : TestHarness
    {
        private ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
        private ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private bool receivedChannelGroupMessage = false;
        private bool receivedGrantMessage = false;

        private string currentUnitTestCase = "";
        private string channelGroupName = "hello_my_group";
        private string channelName = "hello_my_channel";
        private string authKey = "myAuth";

        private Pubnub pubnub = null;
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
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            //var t = pubnub.GetTimeStamp();

            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "ytgdyeV_yhD_a8KRyNsUaumaW4h70SWIsiHuuKE39Fw=")
                    .WithParameter("channel-group", channelGroupName)
                    .WithParameter("m","1")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("r","1")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("ttl","20")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.GrantAccess(null, new string[] { channelGroupName }, null, true, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyErrorCallback);

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenChannelGroupIsRequested Grant access failed.");
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void ThenAddChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenAddChannelShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }


            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelName)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.AddChannelsToChannelGroup(new string[] { channelName }, channelGroupName, AddChannelGroupCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenAddChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenRemoveChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenRemoveChannelShouldReturnSuccess";
            string channelName = "hello_my_channel";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("remove", channelName)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.RemoveChannelsFromChannelGroup(new string[] { channelName }, channelGroupName, RemoveChannelGroupCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests();

            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenRemoveChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenGetChannelListShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "{\"status\": 200, \"payload\": {\"channels\": [\"" + channelName + "\"], \"group\": \"" + channelGroupName + "\"}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.GetChannelsForChannelGroup(channelGroupName, GetChannelGroupCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests(); 
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenGetAllChannelGroupShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenGetAllChannelGroupShouldReturnSuccess";

            if (PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM is enabled; WhenChannelGroupIsRequested -> ThenGetAllChannelGroupShouldReturnSuccess.");
                return;
            }

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string expected = "{\"status\": 200, \"payload\": {\"namespace\": \"\", \"groups\": [\"" + channelGroupName + "\", \"hello_my_group1\"]}, \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group", PubnubCommon.SubscribeKey))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            channelGroupManualEvent = new ManualResetEvent(false);

            pubnub.GetAllChannelGroups(GetAllChannelGroupCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            channelGroupManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        void RemoveChannelGroupCallback(RemoveChannelFromChannelGroupAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string serviceType = receivedMessage.Service;
                    bool errorStatus = receivedMessage.Error;
                    string currentChannelGroup = "";
                    currentChannelGroup = receivedMessage.ChannelGroupName.Substring(1); //assuming no namespace for channel group
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
                    {
                        if (currentChannelGroup == channelGroupName)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }
                }

            }
            catch { }
            finally
            {
                channelGroupManualEvent.Set();
            }

        }

        void AddChannelGroupCallback(AddChannelToChannelGroupAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string serviceType = receivedMessage.Service;
                    bool errorStatus = receivedMessage.Error;
                    string currentChannelGroup = receivedMessage.ChannelGroupName.Substring(1); //assuming no namespace for channel group
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
                    {
                        if (currentChannelGroup == channelGroupName)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }

                }
            }
            catch { }
            finally
            {
                channelGroupManualEvent.Set();
            }

        }

        void GetChannelGroupCallback(GetChannelGroupChannelsAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string serviceType = receivedMessage.Service;
                    bool errorStatus = receivedMessage.Error;
                    string currentChannelGroup = "";
                    GetChannelGroupChannelsAck.Data payload = receivedMessage.Payload;
                    if (payload != null)
                    {
                        currentChannelGroup = payload.ChannelGroupName;
                        string[] channels = payload.ChannelName;
                        if (currentChannelGroup == channelGroupName && channels != null && channels.Length >= 0)
                        {
                            receivedChannelGroupMessage = true;
                        }
                    }

                }
            }
            catch { }
            finally
            {
                channelGroupManualEvent.Set();
            }

        }

        void GetAllChannelGroupCallback(GetAllChannelGroupsAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string serviceType = receivedMessage.Service;
                    bool errorStatus = receivedMessage.Error;
                    string currentChannelGroup = "";
                    GetAllChannelGroupsAck.Data payload = receivedMessage.Payload;
                    if (payload != null)
                    {
                        string[] channelGroups = payload.ChannelGroupName;
                        receivedChannelGroupMessage = true;
                    }

                }
            }
            catch { }
            finally
            {
                channelGroupManualEvent.Set();
            }

        }

        void ThenChannelGroupInitializeShouldReturnGrantMessage(GrantAck receivedMessage)
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

        private void DummyErrorCallback(PubnubClientError result)
        {
            channelGroupManualEvent.Set();
        }
    }
}
