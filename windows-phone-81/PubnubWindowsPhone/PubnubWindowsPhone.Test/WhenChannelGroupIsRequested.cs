using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.Threading;

namespace PubnubWindowsStore.Test
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

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init3";
            pubnub.PubnubUnitTest = unitTest;

            pubnub.ChannelGroupGrantAccess<string>(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Task.Delay(1000);

            grantManualEvent.WaitOne();

            Assert.IsTrue(receivedGrantMessage, "WhenChannelGroupIsRequested Grant access failed.");
        }

        [Test]
        public void ThenAddChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenAddChannelShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenAddChannelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            string channelName = "hello_my_channel";

            pubnub.AddChannelsToChannelGroup<string>(new string[] { channelName }, channelGroupName, ChannelGroupCRUDCallback, DummyErrorCallback);
            Task.Delay(1000);

            channelGroupManualEvent.WaitOne();

            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenAddChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenRemoveChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenRemoveChannelShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenRemoveChannelShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            string channelName = "hello_my_channel";

            pubnub.RemoveChannelsFromChannelGroup<string>(new string[] { channelName }, channelGroupName, ChannelGroupCRUDCallback, DummyErrorCallback);
            Task.Delay(1000);

            channelGroupManualEvent.WaitOne();

            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenRemoveChannelShouldReturnSuccess failed.");

        }

        [Test]
        public void ThenGetChannelListShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenChannelGroupIsRequested";
            unitTest.TestCaseName = "ThenGetChannelListShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            channelGroupManualEvent = new ManualResetEvent(false);
            //string channelName = "hello_my_channel";

            pubnub.GetChannelsForChannelGroup<string>(channelGroupName, ChannelGroupCRUDCallback, DummyErrorCallback);
            Task.Delay(1000);

            channelGroupManualEvent.WaitOne();

            Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");

        }

        void ChannelGroupCRUDCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string serviceType = dictionary.Value<string>("service");
                        bool errorStatus = dictionary.Value<bool>("error");
                        string currentChannelGroup = "";
                        switch (currentUnitTestCase)
                        {
                            case "ThenAddChannelShouldReturnSuccess":
                            case "ThenRemoveChannelShouldReturnSuccess":
                                currentChannelGroup = serializedMessage[1].ToString().Substring(1); //assuming no namespace for channel group
                                string statusMessage = dictionary.Value<string>("message");
                                if (statusCode == 200 && statusMessage.ToLower() == "ok" && serviceType == "channel-registry" && !errorStatus)
                                {
                                    if (currentChannelGroup == channelGroupName)
                                    {
                                        receivedChannelGroupMessage = true;
                                    }
                                }
                                break;
                            case "ThenGetChannelListShouldReturnSuccess":
                                var payload = dictionary.Value<JContainer>("payload");
                                if (payload != null)
                                {
                                    currentChannelGroup = payload.Value<string>("group");
                                    JArray channels = payload.Value<JArray>("channels");
                                    if (currentChannelGroup == channelGroupName && channels != null && channels.Count >= 0)
                                    {
                                        receivedChannelGroupMessage = true;
                                    }
                                }
                                break;
                            default:
                                break;
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

        void ThenChannelGroupInitializeShouldReturnGrantMessage(string receivedMessage)
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

        private void DummyErrorCallback(PubnubClientError result)
        {
            channelGroupManualEvent.Set();
        }
    }
}
