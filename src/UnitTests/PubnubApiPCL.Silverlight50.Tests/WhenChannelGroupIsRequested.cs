using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubnubApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using Microsoft.Silverlight.Testing;

namespace PubnubApiPCL.Silverlight50.Tests
{
    [TestClass]
    public class WhenChannelGroupIsRequested : WorkItemTest
    {
        Pubnub pubnub;
        ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedChannelGroupMessage = false;
        bool receivedGrantMessage = false;

        string currentUnitTestCase = "";
        string channelGroupName = "hello_my_group";

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "GrantRequestUnitTest";
                unitTest.TestCaseName = "Init3";
                pubnub.PubnubUnitTest = unitTest;

                pubnub.ChannelGroupGrantAccess(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyErrorCallback);

                grantManualEvent.WaitOne(310 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Assert.IsTrue(receivedGrantMessage, "WhenChannelGroupIsRequested Grant access failed.");
                        pubnub.PubnubUnitTest = null;
                        pubnub = null;
                        TestComplete();
                    });
            });
        }

        [TestMethod, Asynchronous]
        public void ThenAddChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenAddChannelShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenChannelGroupIsRequested";
                unitTest.TestCaseName = "ThenAddChannelShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                channelGroupManualEvent = new ManualResetEvent(false);
                string channelName = "hello_my_channel";

                pubnub.AddChannelsToChannelGroup(new string[] { channelName }, channelGroupName, AddChannelGroupCallback, DummyErrorCallback);

                channelGroupManualEvent.WaitOne(310 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenAddChannelShouldReturnSuccess failed.");
                        pubnub.PubnubUnitTest = null;
                        pubnub = null;
                        TestComplete();
                    });
            });
        }

        [TestMethod, Asynchronous]
        public void ThenRemoveChannelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenRemoveChannelShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenChannelGroupIsRequested";
                unitTest.TestCaseName = "ThenRemoveChannelShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                channelGroupManualEvent = new ManualResetEvent(false);
                string channelName = "hello_my_channel";

                pubnub.RemoveChannelsFromChannelGroup(new string[] { channelName }, channelGroupName, RemoveChannelGroupCallback, DummyErrorCallback);

                channelGroupManualEvent.WaitOne(310 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenRemoveChannelShouldReturnSuccess failed.");
                        pubnub.PubnubUnitTest = null;
                        pubnub = null;
                        TestComplete();
                    });
            });
        }

        [TestMethod, Asynchronous]
        public void ThenGetChannelListShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenGetChannelListShouldReturnSuccess";

            receivedChannelGroupMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenChannelGroupIsRequested";
                unitTest.TestCaseName = "ThenGetChannelListShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                channelGroupManualEvent = new ManualResetEvent(false);
                //string channelName = "hello_my_channel";

                pubnub.GetChannelsForChannelGroup(channelGroupName, GetChannelGroupCallback, DummyErrorCallback);

                channelGroupManualEvent.WaitOne(310 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Assert.IsTrue(receivedChannelGroupMessage, "WhenChannelGroupIsRequested -> ThenGetChannelListShouldReturnSuccess failed.");
                        pubnub.PubnubUnitTest = null;
                        pubnub = null;
                        TestComplete();
                    });
            });
        }

        [Asynchronous]
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

        [Asynchronous]
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

        [Asynchronous]
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

        [Asynchronous]
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

        [Asynchronous]
        void ThenChannelGroupInitializeShouldReturnGrantMessage(GrantAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null && receivedMessage.StatusCode == 200)
                {
                    receivedGrantMessage = true;
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
            channelGroupManualEvent.Set();
        }
    }
}
