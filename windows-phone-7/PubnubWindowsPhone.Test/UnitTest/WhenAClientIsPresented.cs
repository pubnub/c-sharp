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
        ManualResetEvent mreSubscribe = new ManualResetEvent(false);
        ManualResetEvent mrePresence = new ManualResetEvent(false);
        ManualResetEvent mreConnect = new ManualResetEvent(false);
        ManualResetEvent mreWhereNow = new ManualResetEvent(false);
        ManualResetEvent mreGlobalHereNow = new ManualResetEvent(false);

        ManualResetEvent unsubscribeManualEvent = new ManualResetEvent(false);

        ManualResetEvent unsubscribeUUIDManualEvent = new ManualResetEvent(false);

        ManualResetEvent mreHereNow = new ManualResetEvent(false);
        ManualResetEvent presenceUnsubscribeEvent = new ManualResetEvent(false);
        ManualResetEvent presenceUnsubscribeUUIDEvent = new ManualResetEvent(false);

        ManualResetEvent mreGrant = new ManualResetEvent(false);

        static bool receivedPresenceMessage = false;
        static bool receivedHereNowMessage = false;
        static bool receivedWhereNowMessage = false;
        static bool receivedGlobalHereNowMessage = false;
        static bool receivedCustomUUID = false;
        static bool receivedGrantMessage = false;

        string customUUID = "mylocalmachine.mydomain.com";

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            receivedGrantMessage = false;

            if (!PubnubCommon.PAMEnabled)
            {
                TestComplete();
                return;
            }

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "GrantRequestUnitTest";
                    unitTest.TestCaseName = "Init2";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel,hello_my_channel-pnpres";
                    mreGrant = new ManualResetEvent(false);
                    pubnub.GrantAccess<string>(channel, true, true, 20, ThenPresenceInitializeShouldReturnGrantMessage, DummyErrorCallback);
                    mreGrant.WaitOne();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenPresenceShouldReturnReceivedMessage()
        {
            receivedPresenceMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
                    pubnub.PubnubUnitTest = unitTest;

                    mreConnect = new ManualResetEvent(false);
                    mrePresence = new ManualResetEvent(false);
                    pubnub.Presence<string>(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, DummyErrorCallback);
                    mreConnect.WaitOne(310 * 1000);

                    //since presence expects from stimulus from sub/unsub...
                    mreSubscribe = new ManualResetEvent(false);
                    pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, DummyErrorCallback);
                    mreSubscribe.WaitOne(310 * 1000);

                    mrePresence.WaitOne(310 * 1000);

                    pubnub.EndPendingRequests();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(receivedPresenceMessage, "Presence message not received");
                           pubnub.PubnubUnitTest = null;
                           pubnub = null;
                           TestComplete();
                       });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenPresenceShouldReturnCustomUUID()
        {
            receivedCustomUUID = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    mrePresence = new ManualResetEvent(false);
                    mreConnect = new ManualResetEvent(false);
                    pubnub.Presence<string>(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, DummyErrorCallback);
                    mreConnect.WaitOne(310 * 1000);

                    //since presence expects from stimulus from sub/unsub...
                    pubnub.SessionUUID = customUUID;
                    pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, DummyErrorCallback);
                    mreSubscribe.WaitOne(310 * 1000);

                    mrePresence.WaitOne(310 * 1000);
                    pubnub.EndPendingRequests();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedCustomUUID, "Custom UUID not received");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                });
        }

        [Asynchronous]
        void ThenPresenceInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                    });
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void ThenPresenceShouldReturnMessage(string receivedMessage)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                    });
            }
            catch { }
            finally
            {
                mrePresence.Set();
            }
        }

        [Asynchronous]
        void ThenPresenceWithCustomUUIDShouldReturnMessage(string receivedMessage)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                    });
            }
            catch { }
            finally
            {
                mrePresence.Set();
            }
        }

        [TestMethod,Asynchronous]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            receivedHereNowMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAClientIsPresented";
                    unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    mreHereNow = new ManualResetEvent(false);
                    pubnub.HereNow<string>(channel, ThenHereNowShouldReturnMessage, DummyErrorCallback);
                    mreHereNow.WaitOne(60 * 1000);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                       {
                           Assert.IsTrue(receivedHereNowMessage, "here_now message not received");
                           pubnub.PubnubUnitTest = null;
                           pubnub = null;
                           TestComplete();
                       });
                });
        }

        [TestMethod, Asynchronous]
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

                mreGlobalHereNow = new ManualResetEvent(false);
                pubnub.GlobalHereNow<string>(true, true, ThenGlobalHereNowShouldReturnMessage, DummyErrorCallback);
                mreGlobalHereNow.WaitOne(60 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedGlobalHereNowMessage, "global_here_now message not received");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    TestComplete();
                });
            });
        }

        [TestMethod, Asynchronous]
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
                string uuid = customUUID;

                mreWhereNow = new ManualResetEvent(false);
                pubnub.WhereNow<string>(uuid, ThenWhereNowShouldReturnMessage, DummyErrorCallback);
                mreWhereNow.WaitOne(60 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedWhereNowMessage, "where_now message not received");
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    TestComplete();
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
                               var dictionary = ((JContainer)serializedMessage[0])["uuids"];
                               if (dictionary != null)
                               {
                                   receivedHereNowMessage = true;
                               }
                           }
                       });
            }
            catch { }
            finally
            {
                mreHereNow.Set();
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
                            if (channels != null && channels.Count >= 0)
                            {
                                receivedGlobalHereNowMessage = true;
                            }
                        }
                    }
                });
            }
            catch { }
            finally
            {
                mreGlobalHereNow.Set();
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
                mreWhereNow.Set();
            }
        }

        [Asynchronous]
        void DummyMethodForSubscribe(string receivedMessage)
        {
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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
                });
            }
            catch { }
            finally
            {
                mrePresence.Set();
            }
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void DummyMethodForSubscribeUUID(string receivedMessage)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        var uuid = dictionary["uuid"].ToString();
                        if (uuid != null)
                        {
                            receivedCustomUUID = true;
                        }
                    }
                }
            });
            //Dummary callback method for subscribe and unsubscribe to test presence
            mrePresence.Set();
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
            mreConnect.Set();
        }

        [Asynchronous]
        void PresenceUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
            mreConnect.Set();
        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(string receivedMessage)
        {
            mreSubscribe.Set();
        }

        [Asynchronous]
        void SubscribeUUIDDummyMethodForConnectCallback(string receivedMessage)
        {
            mreSubscribe.Set();
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
        }

    }
}
