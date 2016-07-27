using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenChannelGroupIsRequested
    {
        ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedChannelGroupMessage = false;
        bool receivedGrantMessage = false;
        
        string currentUnitTestCase = "";
        string channelGroupName = "hello_my_group";
        string authKey = "myAuth";

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
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

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/v1/auth/grant/sub-key/pam?signature=XVNGV2Z1QsRCusoEObp3Q6ytfEy6pK_SldphSbECbrg=&auth={2}&channel-group={3}&m=1&pnsdk={4}&r=1&timestamp=1469461667&ttl=20&uuid={5}", config.Secure ? "s" : "", config.Origin, authKey, channelGroupName, System.Net.WebUtility.UrlEncode(config.SdkVersion), config.Uuid)).ToString(),
                    "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group+auth\",\"subscribe_key\":\"" + PubnubCommon.SubscribeKey + "\",\"ttl\":20,\"channel-groups\":\"" + channelGroupName + "\",\"auths\":{\"" + authKey + "\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}"
                );
            pubnub = new Pubnub(config, unitTest);


            pubnub.GrantAccess(new string[] { }, new string[] { channelGroupName }, new string[] { authKey }, true, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyErrorCallback);
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

            IPubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/v1/channel-registration/sub-key/{2}/channel-group/{3}?add={4}", config.Secure ? "s" : "", config.Origin, PubnubCommon.SubscribeKey, channelGroupName, channelName)).ToString(),
                    "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}"
                );
            pubnub = new Pubnub(config, unitTest);

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

            //IPubnubUnitTest unitTest = new PubnubUnitTest();
            //unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            //unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/v1/auth/grant/sub-key/pam?signature=XVNGV2Z1QsRCusoEObp3Q6ytfEy6pK_SldphSbECbrg=&auth={2}&channel-group={3}&m=1&pnsdk={4}&r=1&timestamp=1469461667&ttl=20&uuid={5}", config.Secure ? "s" : "", config.Origin, authKey, channelGroupName, System.Net.WebUtility.UrlEncode(config.SdkVersion), config.Uuid)).ToString(),
            //        "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group+auth\",\"subscribe_key\":\"" + PubnubCommon.SubscribeKey + "\",\"ttl\":20,\"channel-groups\":\"" + channelGroupName + "\",\"auths\":{\"" + authKey + "\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}"
            //    );
            //pubnub = new Pubnub(config, unitTest);
            pubnub = new Pubnub(config);

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
                CiperKey = "",
                Secure = false
            };

            //IPubnubUnitTest unitTest = new PubnubUnitTest();
            //unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            //unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/v1/auth/grant/sub-key/pam?signature=XVNGV2Z1QsRCusoEObp3Q6ytfEy6pK_SldphSbECbrg=&auth={2}&channel-group={3}&m=1&pnsdk={4}&r=1&timestamp=1469461667&ttl=20&uuid={5}", config.Secure ? "s" : "", config.Origin, authKey, channelGroupName, System.Net.WebUtility.UrlEncode(config.SdkVersion), config.Uuid)).ToString(),
            //        "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group+auth\",\"subscribe_key\":\"" + PubnubCommon.SubscribeKey + "\",\"ttl\":20,\"channel-groups\":\"" + channelGroupName + "\",\"auths\":{\"" + authKey + "\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}"
            //    );
            //pubnub = new Pubnub(config, unitTest);
            pubnub = new Pubnub(config);

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

            //IPubnubUnitTest unitTest = new PubnubUnitTest();
            //unitTest.EnableStubTest = PubnubCommon.EnableStubTest;
            //unitTest.StubRequestResponse(new Uri(string.Format("http{0}://{1}/v1/auth/grant/sub-key/pam?signature=XVNGV2Z1QsRCusoEObp3Q6ytfEy6pK_SldphSbECbrg=&auth={2}&channel-group={3}&m=1&pnsdk={4}&r=1&timestamp=1469461667&ttl=20&uuid={5}", config.Secure ? "s" : "", config.Origin, authKey, channelGroupName, System.Net.WebUtility.UrlEncode(config.SdkVersion), config.Uuid)).ToString(),
            //        "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group+auth\",\"subscribe_key\":\"" + PubnubCommon.SubscribeKey + "\",\"ttl\":20,\"channel-groups\":\"" + channelGroupName + "\",\"auths\":{\"" + authKey + "\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}"
            //    );
            //pubnub = new Pubnub(config, unitTest);
            pubnub = new Pubnub(config);

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
