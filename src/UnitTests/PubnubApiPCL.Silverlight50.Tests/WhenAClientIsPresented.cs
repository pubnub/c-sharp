using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubnubApi;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using System;

namespace PubnubApiPCL.Silverlight50.Tests
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
        string jsonUserState = "";
        string currentTestCase = "";
        string whereNowChannel = "";
        int manualResetEventsWaitTimeout = 310 * 1000;

        Pubnub pubnub = null;

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
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "GrantRequestUnitTest";
                unitTest.TestCaseName = "Init2";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel,hello_my_channel-pnpres";
                mreGrant = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, true, true, 20, ThenPresenceInitializeShouldReturnGrantMessage, DummyErrorCallback);
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
            currentTestCase = "ThenPresenceShouldReturnReceivedMessage";
            receivedPresenceMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                string channel = "hello_my_channel";

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenAClientIsPresented";
                unitTest.TestCaseName = "ThenPresenceShouldReturnReceivedMessage";
                pubnub.PubnubUnitTest = unitTest;

                mreConnect = new ManualResetEvent(false);
                mrePresence = new ManualResetEvent(false);
                pubnub.Presence(channel, ThenPresenceShouldReturnMessage, PresenceDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreConnect.WaitOne(310 * 1000);

                //since presence expects from stimulus from sub/unsub...
                mreSubscribe = new ManualResetEvent(false);
                pubnub.Subscribe<string>(channel, DummyMethodForSubscribe, SubscribeDummyMethodForConnectCallback, UnsubscribeDummyMethodForDisconnectCallback, DummyErrorCallback);
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
            currentTestCase = "ThenPresenceShouldReturnCustomUUID";

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenAClientIsPresented";
                unitTest.TestCaseName = "ThenPresenceShouldReturnCustomUUID";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";

                pubnub.SessionUUID = customUUID;

                mrePresence = new ManualResetEvent(false);
                mreConnect = new ManualResetEvent(false);
                pubnub.Presence(channel, ThenPresenceWithCustomUUIDShouldReturnMessage, PresenceUUIDDummyMethodForConnectCallback, UnsubscribeUUIDDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreConnect.WaitOne(310 * 1000);

                //since presence expects from stimulus from sub/unsub...
                pubnub.SessionUUID = customUUID;
                pubnub.Subscribe<string>(channel, DummyMethodForSubscribeUUID, SubscribeUUIDDummyMethodForConnectCallback, UnsubscribeUUIDDummyMethodForDisconnectCallback, DummyErrorCallback);
                mreSubscribe.WaitOne(310 * 1000);
                Thread.Sleep(PubnubCommon.TIMEOUT);
                mrePresence.WaitOne(310 * 1000);

                pubnub.Unsubscribe<string>(channel, DummyErrorCallback);
                Thread.Sleep(PubnubCommon.TIMEOUT);
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


        [TestMethod, Asynchronous]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            receivedHereNowMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                string channel = "hello_my_channel";

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenAClientIsPresented";
                unitTest.TestCaseName = "IfHereNowIsCalledThenItShouldReturnInfo";
                pubnub.PubnubUnitTest = unitTest;

                mreHereNow = new ManualResetEvent(false);
                pubnub.HereNow(new string[] { channel }, ThenHereNowShouldReturnMessage, DummyErrorCallback);
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

                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenAClientIsPresented";
                unitTest.TestCaseName = "IfGlobalHereNowIsCalledThenItShouldReturnInfo";
                pubnub.PubnubUnitTest = unitTest;

                mreGlobalHereNow = new ManualResetEvent(false);
                pubnub.GlobalHereNow(true, true, ThenGlobalHereNowShouldReturnMessage, DummyErrorCallback);
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

                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenAClientIsPresented";
                unitTest.TestCaseName = "IfWhereNowIsCalledThenItShouldReturnInfo";
                pubnub.PubnubUnitTest = unitTest;
                string uuid = customUUID;

                mreWhereNow = new ManualResetEvent(false);
                pubnub.WhereNow(uuid, ThenWhereNowShouldReturnMessage, DummyErrorCallback);
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
        void ThenHereNowShouldReturnMessage(HereNowAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null && receivedMessage.Payload != null)
                {
                    receivedHereNowMessage = true;

                    //string channelName = receivedMessage.ChannelName;

                    ////Console.WriteLine("ThenHereNowShouldReturnMessage -> result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage));
                    //Dictionary<string, HereNowAck.Data.ChannelData> channelDataDic = receivedMessage.Payload.channels;
                    //if (channelDataDic != null && channelDataDic.Count > 0)
                    //{
                    //    HereNowAck.Data.ChannelData.UuidData[] uuidDataArray = channelDataDic["hello_my_channel"].uuids;
                    //    if (uuidDataArray != null && uuidDataArray.Length > 0)
                    //    {
                    //        if (currentTestCase == "IfHereNowIsCalledThenItShouldReturnInfoWithUserState")
                    //        {
                    //            foreach (HereNowAck.Data.ChannelData.UuidData uuidData in uuidDataArray)
                    //            {
                    //                if (uuidData.uuid != null && uuidData.state != null)
                    //                {
                    //                    string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidData.state);
                    //                    string receivedUUID = uuidData.uuid;

                    //                    if (receivedUUID == pubnub.SessionUUID && receivedState == jsonUserState)
                    //                    {
                    //                        receivedHereNowMessage = true;
                    //                        break;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            foreach (HereNowAck.Data.ChannelData.UuidData uuidData in uuidDataArray)
                    //            {
                    //                if (pubnub.PubnubUnitTest != null && pubnub.PubnubUnitTest.EnableStubTest)
                    //                {
                    //                    receivedHereNowMessage = true;
                    //                    break;
                    //                }
                    //                if (uuidData.uuid != null && uuidData.uuid == pubnub.SessionUUID)
                    //                {
                    //                    receivedHereNowMessage = true;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            catch { }
            finally
            {
                mreHereNow.Set();
            }
        }

        [Asynchronous]
        void ThenGlobalHereNowShouldReturnMessage(GlobalHereNowAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    if (receivedMessage.Payload != null)
                    {
                        Dictionary<string, GlobalHereNowAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
                        if (channels != null && channels.Count >= 0)
                        {
                            if (channels.Count == 0)
                            {
                                receivedGlobalHereNowMessage = true;
                            }
                            else
                            {
                                foreach (KeyValuePair<string, GlobalHereNowAck.Data.ChannelData> channelUUID in channels)
                                {
                                    var channelName = channelUUID.Key;
                                    GlobalHereNowAck.Data.ChannelData channelUuidListDictionary = channelUUID.Value;
                                    if (channelUuidListDictionary != null && channelUuidListDictionary.uuids != null)
                                    {
                                        if (pubnub.PubnubUnitTest != null && pubnub.PubnubUnitTest.EnableStubTest)
                                        {
                                            receivedGlobalHereNowMessage = true;
                                            break;
                                        }

                                        GlobalHereNowAck.Data.ChannelData.UuidData[] uuidDataList = channelUuidListDictionary.uuids;
                                        if (currentTestCase == "IfGlobalHereNowIsCalledThenItShouldReturnInfoWithUserState")
                                        {
                                            foreach (GlobalHereNowAck.Data.ChannelData.UuidData uuidData in uuidDataList)
                                            {
                                                if (uuidData.uuid != null && uuidData.state != null)
                                                {
                                                    string receivedState = pubnub.JsonPluggableLibrary.SerializeToJsonString(uuidData.state);
                                                    string receivedUUID = uuidData.uuid;
                                                    if (receivedUUID == pubnub.SessionUUID && receivedState == jsonUserState)
                                                    {
                                                        receivedGlobalHereNowMessage = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            receivedGlobalHereNowMessage = true;
                                            break;
                                        }

                                    }
                                }
                            }


                        }
                    }

                }
            }
            catch { }
            finally
            {
                mreGlobalHereNow.Set();
            }
        }

        [Asynchronous]
        void ThenWhereNowShouldReturnMessage(WhereNowAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    if (receivedMessage.Payload != null)
                    {
                        string[] channels = receivedMessage.Payload.channels;
                        if (channels != null && channels.Length >= 0)
                        {
                            receivedWhereNowMessage = true;

                        }
                    }
                }
            }
            catch { }
            finally
            {
                mreWhereNow.Set();
            }
        }

        [Asynchronous]
        void DummyMethodForSubscribe(Message<string> receivedMessage)
        {
            try
            {
                if (receivedMessage != null && !string.IsNullOrEmpty(receivedMessage.Data) && !string.IsNullOrEmpty(receivedMessage.Data.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage.Data);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null && dictionary.Count > 0)
                        {
                            var uuid = dictionary["uuid"].ToString();
                            if (uuid != null)
                            {
                                receivedPresenceMessage = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mrePresence.Set();
            }
            //Dummary callback method for subscribe and unsubscribe to test presence
        }

        [Asynchronous]
        void DummyMethodForSubscribeUUID(Message<string> receivedMessage)
        {
            if (receivedMessage != null && !string.IsNullOrEmpty(receivedMessage.Data) && !string.IsNullOrEmpty(receivedMessage.Data.Trim()))
            {
                List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage.Data);
                if (serializedMessage != null && serializedMessage.Count > 0)
                {
                    Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                    if (dictionary != null && dictionary.Count > 0)
                    {
                        var uuid = dictionary["uuid"].ToString();
                        if (uuid != null)
                        {
                            receivedCustomUUID = true;
                        }
                    }
                }
            }
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
        void PresenceDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreConnect.Set();
        }

        [Asynchronous]
        void PresenceUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreConnect.Set();
        }

        [Asynchronous]
        void SubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreSubscribe.Set();
        }

        [Asynchronous]
        void SubscribeUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            mreSubscribe.Set();
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForConnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
        }

        [Asynchronous]
        void UnsubscribeDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            unsubscribeManualEvent.Set();
        }

        [Asynchronous]
        void UnsubscribeUUIDDummyMethodForDisconnectCallback(ConnectOrDisconnectAck receivedMessage)
        {
            unsubscribeUUIDManualEvent.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
            Console.WriteLine(result.Description);
        }

        [Asynchronous]
        void ThenPresenceInitializeShouldReturnGrantMessage(GrantAck receivedMessage)
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
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void ThenPresenceShouldReturnMessage(PresenceAck receivedMessage)
        {
            try
            {
                string action = receivedMessage.Action.ToLower();
                if (currentTestCase == "ThenPresenceHeartbeatShouldReturnMessage")
                {
                    if (action == "timeout")
                    {
                        receivedPresenceMessage = false;
                    }
                    else
                    {
                        receivedPresenceMessage = true;
                    }
                }
                else
                {
                    receivedPresenceMessage = true;
                }
            }
            catch { }
            finally
            {
                mrePresence.Set();
            }
        }

        [Asynchronous]
        void ThenPresenceWithCustomUUIDShouldReturnMessage(PresenceAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null && !string.IsNullOrWhiteSpace(receivedMessage.UUID) && receivedMessage.UUID.Contains(customUUID))
                {
                    receivedCustomUUID = true;
                }
            }
            catch { }
            finally
            {
                mrePresence.Set();
            }
        }
    }
}
