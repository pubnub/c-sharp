using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using PubnubApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenUnsubscribedToAChannelGroup : WorkItemTest
    {
        ManualResetEvent unsubscribeManualEvent = new ManualResetEvent(false);
        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        bool receivedMessage = false;
        bool receivedGrantMessage = false;
        bool receivedChannelGroupMessage = false;
        bool receivedChannelGroupConnectedMessage = false;

        string currentUnitTestCase = "";
        string channelGroupName = "hello_my_group";

        int manualResetEventsWaitTimeout = 310 * 1000;

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled) return;

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "GrantRequestUnitTest";
                    unitTest.TestCaseName = "Init3";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.ChannelGroupGrantAccess(channelGroupName, true, true, 20, ThenChannelGroupInitializeShouldReturnGrantMessage, DummyUnsubscribeErrorCallback);
                    Thread.Sleep(1000);

                    grantManualEvent.WaitOne();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGrantMessage, "WhenUnsubscribedToAChannelGroup Grant access failed.");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenShouldReturnUnsubscribedMessage()
        {
            currentUnitTestCase = "ThenShouldReturnUnsubscribedMessage";
            receivedMessage = false;
            receivedChannelGroupMessage = false;
            receivedChannelGroupConnectedMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    pubnub.SessionUUID = "myuuid";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenUnsubscribedToAChannelGroup";
                    unitTest.TestCaseName = "ThenShouldReturnUnsubscribedMessage";

                    pubnub.PubnubUnitTest = unitTest;

                    channelGroupName = "hello_my_group";
                    string channelName = "hello_my_channel";

                    unsubscribeManualEvent = new ManualResetEvent(false);
                    pubnub.AddChannelsToChannelGroup(new string[] { channelName }, channelGroupName, ChannelGroupAddCallback, DummySubscribeErrorCallback);
                    unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
                    if (receivedChannelGroupMessage)
                    {
                        unsubscribeManualEvent = new ManualResetEvent(false);
                        pubnub.Subscribe<string>("", channelGroupName, DummyMethodChannelSubscribeUserCallback, DummyMethodChannelSubscribeConnectCallback, DummyMethodSubscribeChannelDisconnectCallback, DummyErrorCallback);
                        Thread.Sleep(1000);
                        unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);

                        if (receivedChannelGroupConnectedMessage)
                        {
                            unsubscribeManualEvent = new ManualResetEvent(false);
                            pubnub.Unsubscribe<string>("", channelGroupName, DummyErrorCallback);
                            unsubscribeManualEvent.WaitOne(manualResetEventsWaitTimeout);
                        }

                        pubnub.EndPendingRequests();

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedChannelGroupMessage, "WhenUnsubscribedToAChannelGroup --> ThenShouldReturnUnsubscribedMessage Failed");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                });
        }

        [Asynchronous]
        private void DummyMethodChannelSubscribeUserCallback(Message<string> result)
        {
        }

        [Asynchronous]
        private void DummyMethodChannelSubscribeConnectCallback(ConnectOrDisconnectAck result)
        {
            if (result.StatusMessage.Contains("Connected"))
            {
                receivedChannelGroupConnectedMessage = true;
            }
            unsubscribeManualEvent.Set();
        }

        [Asynchronous]
        private void DummyMethodSubscribeChannelDisconnectCallback(ConnectOrDisconnectAck result)
        {
            if (result.StatusMessage.Contains("Unsubscribed from"))
            {
                receivedMessage = true;
            }
            unsubscribeManualEvent.Set();
        }

        [Asynchronous]
        private void DummyMethodUnsubscribeChannelUserCallback(string result)
        {
        }

        [Asynchronous]
        private void DummyMethodUnsubscribeChannelConnectCallback(ConnectOrDisconnectAck result)
        {
        }

        [Asynchronous]
        private void DummyMethodUnsubscribeChannelDisconnectCallback(string result)
        {
            if (result.Contains("Unsubscribed from"))
            {
                receivedMessage = true;
            }
            unsubscribeManualEvent.Set();
        }

        [Asynchronous]
        void ChannelGroupAddCallback(AddChannelToChannelGroupAck receivedMessage)
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
                unsubscribeManualEvent.Set();
            }

        }

        [Asynchronous]
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

        [Asynchronous]
        private void DummyUnsubscribeErrorCallback(PubnubClientError result)
        {
            unsubscribeManualEvent.Set();
        }

        [Asynchronous]
        private void DummySubscribeErrorCallback(PubnubClientError result)
        {
            unsubscribeManualEvent.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
