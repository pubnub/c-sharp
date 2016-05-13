using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using PubnubApi;


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

        Pubnub pubnub = null;

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = PubnubCommon.SecretKey;
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init3";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.ChannelGroupGrantAccess(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyErrorCallback);
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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = PubnubCommon.SecretKey;
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenAddChannelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            string channelName = "hello_my_channel";

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

            receivedChannelGroupMessage = false;

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = PubnubCommon.SecretKey;
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenRemoveChannelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            string channelName = "hello_my_channel";

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

            PNConfiguration config = new PNConfiguration();
            config.SubscribeKey = PubnubCommon.SubscribeKey;
            config.PublishKey = PubnubCommon.PublishKey;
            config.SecretKey = PubnubCommon.SecretKey;
            config.CiperKey = "";
            config.Secure = false;

            pubnub = new Pubnub(config);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenGetChannelListShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            //string channelName = "hello_my_channel";

            pubnub.GetChannelsForChannelGroup(channelGroupName, GetChannelGroupCallback, DummyErrorCallback);
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
