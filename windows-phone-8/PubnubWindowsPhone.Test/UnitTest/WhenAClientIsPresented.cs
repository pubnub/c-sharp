using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenAClientIsPresented : WorkItemTest
    {
        ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceManualEvent = new ManualResetEvent(false);
        ManualResetEvent unsubscribeManualEvent = new ManualResetEvent(false);

        ManualResetEvent subscribeUUIDManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUUIDManualEvent = new ManualResetEvent(false);
        ManualResetEvent unsubscribeUUIDManualEvent = new ManualResetEvent(false);

        ManualResetEvent hereNowManualEvent = new ManualResetEvent(false);
        ManualResetEvent globalHereNowManualEvent = new ManualResetEvent(false);
        ManualResetEvent whereNowManualEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUnsubscribeEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUnsubscribeUUIDEvent = new ManualResetEvent(false);

        ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        static bool receivedPresenceMessage = false;
        static bool receivedHereNowMessage = false;
        static bool receivedGlobalHereNowMessage = false;
        static bool receivedWhereNowMessage = false;
        static bool receivedCustomUUID = false;
        static bool receivedGrantMessage = false;

        string customUUID = "mylocalmachine.mydomain.com";

        [ClassInitialize]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Inconclusive("WhenAClientIsPresent Grant access failed");
                //TestComplete();
                return;
            }

            receivedGrantMessage = false;
            //receivedGrantMessage2 = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init2";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel,hello_my_channel-pnpres";
            pubnub.GrantAccess<string>(channel, true, true, 20, ThenPresenceInitializeShouldReturnGrantMessage, DummyErrorCallback);
            Thread.Sleep(1000);
            grantManualEvent.WaitOne();

            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed");
            //TestComplete();
        }

        [TestMethod, Asynchronous]
        public void ThenPresenceShouldReturnReceivedMessage()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    receivedPresenceMessage = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.Presence<string>(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, DummyErrorCallback);
                    Thread.Sleep(1000);

                    //since presence expects from stimulus from sub/unsub...
                    pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    subscribeManualEvent.WaitOne(310 * 1000);

                    //pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribe, UnsubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                    //Thread.Sleep(1000);
                    //unsubscribeManualEvent.WaitOne(2000);

                    presenceManualEvent.WaitOne(310 * 1000);

                    pubnub.EndPendingRequests();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedPresenceMessage, "Presence message not received");
                            TestComplete();
                        });
                });
        }

        [TestMethod]
        public void ThenPresenceShouldReturnCustomUUID()
        {
            receivedCustomUUID = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.Presence<string>(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            Thread.Sleep(1000);

            //since presence expects from stimulus from sub/unsub...
            pubnub.SessionUUID = customUUID;
            pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, DummyErrorCallback);
            subscribeUUIDManualEvent.WaitOne();
            Thread.Sleep(1000);

            //pubnub.Unsubscribe<string>(channel, DummyMethodForUnSubscribeUUID, UnsubscribeUUIDDummyMethodForConnectCallback, UnsubscribeUUIDDummyMethodForDisconnectCallback, DummyErrorCallback);
            //Thread.Sleep(1000);
            //unsubscribeUUIDManualEvent.WaitOne(2000);

            presenceUUIDManualEvent.WaitOne();
            pubnub.EndPendingRequests();

            Assert.IsTrue(receivedCustomUUID, "Custom UUID not received");
        }

        [Asynchronous]
        void ThenPresenceInitializeShouldReturnGrantMessage(string receivedMessage)
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

        [Asynchronous]
        void ThenPresenceShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var uuid = dictionary["uuid"].ToString();
                    if (uuid != null)
                    {
                        receivedPresenceMessage = true;
                    }
                }
            }
            catch { }
            finally
            {
                presenceManualEvent.Set();
            }
        }

        [Asynchronous]
        void ThenPresenceWithCustomUUIDShouldReturnMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var uuid = dictionary["uuid"].ToString();
                    if (uuid != null && uuid.Contains(customUUID))
                    {
                        receivedCustomUUID = true;
                    }
                }
            }
            catch { }
            finally
            {
                presenceUUIDManualEvent.Set();
            }
        }

        [TestMethod]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    receivedHereNowMessage = false;
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    pubnub.HereNow<string>(channel, true, false, ThenHereNowShouldReturnMessage, DummyErrorCallback);
                    hereNowManualEvent.WaitOne();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedHereNowMessage, "here_now message not received");
                            //TestComplete();
                            //EnqueueTestComplete();
                        });
                });
        }

        [TestMethod]
        public void IfGlobalHereNowIsCalledThenItShouldReturnInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    receivedGlobalHereNowMessage = false;

                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "IfGlobalHereNowIsCalledThenItShouldReturnInfo";
                    pubnub.PubnubUnitTest = unitTest;
                    pubnub.GlobalHereNow<string>(true, true, ThenGlobalHereNowShouldReturnMessage, DummyErrorCallback);
                    globalHereNowManualEvent.WaitOne();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGlobalHereNowMessage, "global_here_now message not received");
                        });
                });
        }

        [TestMethod]
        public void IfWhereNowIsCalledThenItShouldReturnInfo()
        {
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    receivedWhereNowMessage = false;

                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "IfWhereNowIsCalledThenItShouldReturnInfo";
                    pubnub.PubnubUnitTest = unitTest;
                    string uuid = "hello_my_uuid";
                    pubnub.WhereNow<string>(uuid, ThenWhereNowShouldReturnMessage, DummyErrorCallback);
                    whereNowManualEvent.WaitOne();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedWhereNowMessage, "where_now message not received");
                        });
                });
        }

        [Asynchronous]
        void ThenHereNowShouldReturnMessage(string receivedMessage)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (!string.IsNullOrWhiteSpace(receivedMessage))
                    {
                        object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                        if (serializedMessage != null && serializedMessage.Length > 0)
                        {
                            var dictionary = ((JContainer)serializedMessage[0])["uuids"];
                            if (dictionary != null)
                            {
                                receivedHereNowMessage = true;
                            }
                        }
                    }
                });
            }
            catch{}
            finally
            {
                hereNowManualEvent.Set();
            }
        }

        [Asynchronous]
        void ThenGlobalHereNowShouldReturnMessage(string receivedMessage)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                        {
                            object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                            JContainer dictionary = serializedMessage[0] as JContainer;
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null && channels.Count > 0)
                                {
                                    var channelsContainer = channels["hello_my_channel"];
                                    if (channelsContainer != null)
                                    {
                                        var uuids = channelsContainer.Value<JArray>("uuids");
                                        if (uuids != null && uuids.Count > 0)
                                        {
                                            receivedGlobalHereNowMessage = true;
                                        }
                                    }
                                }
                            }
                        }
                    });
            }
            catch { }
            finally
            {
                globalHereNowManualEvent.Set();
            }
        }

        [Asynchronous]
        void ThenWhereNowShouldReturnMessage(string receivedMessage)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                        {
                            object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                            JContainer dictionary = serializedMessage[0] as JContainer;
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null && channels.Count >= 0)
                                {
                                    receivedWhereNowMessage = true;
                                }
                            }
                        }
                    });
            }
            catch { }
            finally
            {
                whereNowManualEvent.Set();
            }
        }


        [Asynchronous]
        void DummyMethodForSubscribe(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var uuid = dictionary["uuid"].ToString();
                    if (uuid != null)
                    {
                        receivedPresenceMessage = true;
                    }
                }
            }
            catch { }
            finally
            {
                presenceManualEvent.Set();
            }
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void DummyMethodForSubscribeUUID(string receivedMessage)
        {
            if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
            {
                object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                JContainer dictionary = serializedMessage[0] as JContainer;
                if (dictionary != null)
                {
                    var uuid = dictionary["uuid"];
                    if (uuid != null)
                    {
                        receivedCustomUUID = true;
                        presenceUUIDManualEvent.Set();
                    }
                }
            }
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void DummyMethodForUnSubscribe(string receivedMessage)
        {
            //Dummary callback method for unsubscribe to test presence
        }

        [Asynchronous]
        void DummyMethodForUnSubscribeUUID(string receivedMessage)
        {
            //Dummary callback method for unsubscribe to test presence
        }

        [Asynchronous]
        void PresenceDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void PresenceUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            subscribeManualEvent.Set();
        }

        [Asynchronous]
        void SubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
            subscribeUUIDManualEvent.Set();
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForDisconnectCallback(string receivedMessage)
        {
            unsubscribeManualEvent.Set();
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForDisconnectCallback(string receivedMessage)
        {
            unsubscribeUUIDManualEvent.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
            System.Diagnostics.Debug.WriteLine(result.ToString());
        }

    }
}
