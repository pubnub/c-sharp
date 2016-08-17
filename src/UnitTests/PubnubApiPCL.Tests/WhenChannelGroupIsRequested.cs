using NUnit.Framework;
using System.Threading;
using PubnubApi;
using HttpMock;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenChannelGroupIsRequested : TestHarness
    {
        IHttpServer stubHttp;

        ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedChannelGroupMessage = false;
        bool receivedGrantMessage = false;
        
        string currentUnitTestCase = "";
        string channelGroupName = "hello_my_group";
        string channelName = "hello_my_channel";
        string authKey = "myAuth";

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
            stubHttp = HttpMockRepository.At("http://" + PubnubCommon.StubOrign);

            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                CiperKey = "",
                Secure = false
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

            string url = string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey);
            string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";
            stubHttp.WithNewContext()
                .Stub(x => x.Get(url))
                .Return(expected)
                .OK();

            pubnub.GrantAccess(null, new string[] { channelGroupName }, null, true, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyErrorCallback);

            Thread.Sleep(1000);

            grantManualEvent.WaitOne();

            pubnub.EndPendingRequests();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenChannelGroupIsRequested Grant access failed.");
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
                CiperKey = "",
                Secure = false
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }


            string url = string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName);
            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";
            ConcurrentDictionary<string, string> parameters = new ConcurrentDictionary<string, string>();
            parameters.Add("add", channelName);
            stubHttp.WithNewContext()
                .Stub(x => x.Get(url))
                .WithParams(parameters)
                .Return(expected)
                .OK();

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
                CiperKey = "",
                Secure = false
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string url = string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName);
            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";
            ConcurrentDictionary<string, string> parameters = new ConcurrentDictionary<string, string>();
            parameters.Add("remove", channelName);

            stubHttp.WithNewContext()
                .Stub(x => x.Get(url))
                .WithParams(parameters)
                .Return(expected)
                .OK();

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
            stubHttp = HttpMockRepository.At("http://" + PubnubCommon.StubOrign);

            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
                CiperKey = "",
                Secure = false
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string url = string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName);
            string expected = "{\"status\": 200, \"payload\": {\"channels\": [\"" + channelName + "\"], \"group\": \"" + channelGroupName + "\"}, \"service\": \"channel-registry\", \"error\": false}";

            stubHttp.WithNewContext()
                .Stub(x => x.Get(url))
                .Return(expected)
                .OK();

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
                CiperKey = "",
                Secure = false
            };

            if (PubnubCommon.EnableStubTest)
            {
                pubnub = this.createPubNubInstance(config);
            }
            else
            {
                pubnub = new Pubnub(config);
            }

            string url = string.Format("/v1/channel-registration/sub-key/{0}/channel-group", PubnubCommon.SubscribeKey);
            string expected = "{\"status\": 200, \"payload\": {\"namespace\": \"\", \"groups\": [\"" + channelGroupName + "\", \"hello_my_group1\"]}, \"service\": \"channel-registry\", \"error\": false}";
            stubHttp.WithNewContext()
                .Stub(x => x.Get(url))
                .Return(expected)
                .OK();

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
