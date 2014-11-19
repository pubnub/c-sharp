using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenPushIsRequested : WorkItemTest
    {
        string currentUnitTestCase = "";
        bool receivedSuccessMessage = false;
        static bool receivedGrantMessage = false;

        ManualResetEvent mrePush = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                TestComplete();
                return;
            }

            receivedGrantMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "GrantRequestUnitTest";
                unitTest.TestCaseName = "Init";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";

                mreGrant = new ManualResetEvent(false);
                pubnub.GrantAccess<string>(channel, true, true, 20, ThenPushInitializeShouldReturnGrantMessage, DummyErrorCallback);
                mreGrant.WaitOne(60 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedGrantMessage, "WhenPushIsRequested Grant access failed.");
                    TestComplete();
                });
            });
        }

        [TestMethod, Asynchronous]
        public void ThenRegisterDeviceShouldReturnSuccess()
        {
            string channel = "hello_my_channel";
            string pushToken = "http://sn1.notify.live.net/throttledthirdparty/01.00/AQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ";
            receivedSuccessMessage = false;
            currentUnitTestCase = "ThenRegisterDeviceShouldReturnSuccess";
            
            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenRegisterDeviceShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                mrePush = new ManualResetEvent(false);
                pubnub.RegisterDeviceForPush<string>(channel, PushTypeService.MPNS, pushToken, DeviceRegisterCallback, DummyErrorCallback);
                mrePush.WaitOne(60 * 1000);

                Assert.IsTrue(receivedSuccessMessage, "WhenPushIsRequested -> ThenRegisterDeviceShouldReturnSuccess failed.");
                
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TestComplete();
                });
            });

        }

        [TestMethod, Asynchronous]
        public void ThenUnregisterDeviceShouldReturnSuccess()
        {
            string pushToken = "http://sn1.notify.live.net/throttledthirdparty/01.00/AQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ";
            receivedSuccessMessage = false;
            currentUnitTestCase = "ThenRegisterDeviceShouldReturnSuccess";

            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenUnregisterDeviceShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                mrePush = new ManualResetEvent(false);
                pubnub.UnregisterDeviceForPush<string>(PushTypeService.MPNS, pushToken, DeviceUnregisterCallback, DummyErrorCallback);
                mrePush.WaitOne(60 * 1000);

                Assert.IsTrue(receivedSuccessMessage, "WhenPushIsRequested -> ThenUnregisterDeviceShouldReturnSuccess failed.");

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TestComplete();
                });
            });

        }

        [TestMethod, Asynchronous]
        public void ThenRemoveChannelForDeviceShouldReturnSuccess()
        {
            string channel = "hello_my_channel";
            string pushToken = "http://sn1.notify.live.net/throttledthirdparty/01.00/AQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ";
            receivedSuccessMessage = false;
            currentUnitTestCase = "ThenRemoveChannelForDeviceShouldReturnSuccess";

            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenRemoveChannelForDeviceShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                mrePush = new ManualResetEvent(false);
                pubnub.RemoveChannelForDevicePush<string>(channel, PushTypeService.MPNS, pushToken, ChannelRemoveFromDeviceCallback, DummyErrorCallback);
                mrePush.WaitOne(60 * 1000);

                Assert.IsTrue(receivedSuccessMessage, "WhenPushIsRequested -> ThenRemoveChannelForDeviceShouldReturnSuccess failed.");

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TestComplete();
                });
            });

        }

        [TestMethod, Asynchronous]
        public void ThenGetAllChannelsForDeviceShouldReturnSuccess()
        {
            string channel = "hello_my_channel";
            string pushToken = "http://sn1.notify.live.net/throttledthirdparty/01.00/AQG2MdvoLlZFT7-VJ2TJ5LnbAgAAAAADAQAAAAQUZm52OkRFNzg2NTMxMzlFMEZFNkMFBlVTU0MwMQ";
            receivedSuccessMessage = false;
            currentUnitTestCase = "ThenGetAllChannelsForDeviceShouldReturnSuccess";

            ThreadPool.QueueUserWorkItem((s) =>
            {
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenGetAllChannelsForDeviceShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                if (!PubnubCommon.EnableStubTest)
                {
                    mrePush = new ManualResetEvent(false);
                    pubnub.RegisterDeviceForPush<string>(channel, PushTypeService.MPNS, pushToken, DeviceRegisterCallback, DummyErrorCallback);
                    mrePush.WaitOne(60 * 1000);
                }
                else
                {
                    receivedSuccessMessage = true;
                }
                if (receivedSuccessMessage)
                {
                    mrePush = new ManualResetEvent(false);
                    pubnub.GetChannelsForDevicePush<string>(PushTypeService.MPNS, pushToken, GetChannelsFromDeviceCallback, DummyErrorCallback);
                    mrePush.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedSuccessMessage, "WhenPushIsRequested -> ThenGetAllChannelsForDeviceShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.IsTrue(receivedSuccessMessage, "WhenPushIsRequested -> RegisterDeviceForPush failed in ThenGetAllChannelsForDeviceShouldReturnSuccess.");
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    TestComplete();
                });
            });

        }

        [TestMethod, Asynchronous]
        public void ThenPublishMpnsToastShouldReturnSuccess()
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                receivedSuccessMessage = false;
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                string channel = "hello_my_channel";

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenPublishMpnsToastShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                mrePublish = new ManualResetEvent(false);

                MpnsToastNotification toast = new MpnsToastNotification();
                toast.text1 = "hardcode message";
                Dictionary<string, object> dicToast = new Dictionary<string, object>();
                dicToast.Add("pn_mpns", toast);

                pubnub.EnableDebugForPushPublish = true;
                pubnub.Publish<string>(channel, dicToast, PublishCallbackResult, DummyErrorCallback);
                mrePublish.WaitOne(60 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedSuccessMessage, "Toast Publish Failed");
                    TestComplete();
                });
            });

        }

        [TestMethod, Asynchronous]
        public void ThenPublishMpnsFlipTileShouldReturnSuccess()
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                receivedSuccessMessage = false;
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                string channel = "hello_my_channel";

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenPublishMpnsFlipTileShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                mrePublish = new ManualResetEvent(false);

                MpnsFlipTileNotification tile = new MpnsFlipTileNotification();
                tile.title = "front title";
                tile.count = 6;
                tile.back_title = "back title";
                tile.back_content = "back message";
                tile.back_background_image = "Assets/Tiles/pubnub3.png";
                tile.background_image = "http://cdn.flaticon.com/png/256/37985.png";
                Dictionary<string, object> dicTile = new Dictionary<string, object>();
                dicTile.Add("pn_mpns", tile);

                pubnub.EnableDebugForPushPublish = true;
                pubnub.Publish<string>(channel, dicTile, PublishCallbackResult, DummyErrorCallback);
                mrePublish.WaitOne(60 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedSuccessMessage, "Flip Tile Publish Failed");
                    TestComplete();
                });
            });

        }

        [TestMethod, Asynchronous]
        public void ThenPublishMpnsCycleTileShouldReturnSuccess()
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                receivedSuccessMessage = false;
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                string channel = "hello_my_channel";

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenPublishMpnsCycleTileShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                mrePublish = new ManualResetEvent(false);

                MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
                tile.title = "front title";
                tile.count = 2;
                tile.images = new string[] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };

                Dictionary<string, object> dicTile = new Dictionary<string, object>();
                dicTile.Add("pn_mpns", tile);

                pubnub.EnableDebugForPushPublish = true;
                pubnub.Publish<string>(channel, dicTile, PublishCallbackResult, DummyErrorCallback);
                mrePublish.WaitOne(60 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedSuccessMessage, "Cycle Tile Publish Failed");
                    TestComplete();
                });
            });

        }

        [TestMethod, Asynchronous]
        public void ThenPublishMpnsIconicTileShouldReturnSuccess()
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                receivedSuccessMessage = false;
                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                string channel = "hello_my_channel";

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenPushIsRequested";
                unitTest.TestCaseName = "ThenPublishMpnsIconicTileShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                mrePublish = new ManualResetEvent(false);

                MpnsIconicTileNotification tile = new MpnsIconicTileNotification();
                tile.title = "front title";
                tile.count = 2;
                tile.wide_content_1 = "my wide content";

                Dictionary<string, object> dicTile = new Dictionary<string, object>();
                dicTile.Add("pn_mpns", tile);

                pubnub.EnableDebugForPushPublish = true;
                pubnub.Publish<string>(channel, dicTile, PublishCallbackResult, DummyErrorCallback);
                mrePublish.WaitOne(60 * 1000);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Assert.IsTrue(receivedSuccessMessage, "Iconic Tile Publish Failed");
                    TestComplete();
                });
            });

        }

        [Asynchronous]
        void DeviceRegisterCallback(string receivedMessage)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    try
                    {
                        object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(receivedMessage) as object[];
                        if (deserializedResult is object[])
                        {
                            long statusCode = Int64.Parse(deserializedResult[0].ToString());
                            string statusMessage = (string)deserializedResult[1];
                            if (statusCode == 1 && statusMessage.ToLower() == "modified channels")
                            {
                                receivedSuccessMessage = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
                mrePush.Set();
            });

        }

        [Asynchronous]
        void DeviceUnregisterCallback(string receivedMessage)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    try
                    {
                        object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(receivedMessage) as object[];
                        if (deserializedResult is object[])
                        {
                            long statusCode = Int64.Parse(deserializedResult[0].ToString());
                            string statusMessage = (string)deserializedResult[1];
                            if (statusCode == 1 && statusMessage.ToLower() == "removed device")
                            {
                                receivedSuccessMessage = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
                mrePush.Set();
            });

        }

        [Asynchronous]
        void ChannelRemoveFromDeviceCallback(string receivedMessage)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    try
                    {
                        object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(receivedMessage) as object[];
                        if (deserializedResult is object[])
                        {
                            long statusCode = Int64.Parse(deserializedResult[0].ToString());
                            string statusMessage = (string)deserializedResult[1];
                            if (statusCode == 1 && statusMessage.ToLower() == "modified channels")
                            {
                                receivedSuccessMessage = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
                mrePush.Set();
            });

        }

        [Asynchronous]
        void GetChannelsFromDeviceCallback(string receivedMessage)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    try
                    {
                        object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(receivedMessage) as object[];
                        if (deserializedResult is object[] && deserializedResult.Length > 0)
                        {
                            receivedSuccessMessage = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
                mrePush.Set();
            });

        }

        [Asynchronous]
        private void PublishCallbackResult(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        receivedSuccessMessage = true;
                    }
                }
            }
            mrePublish.Set();
        }


        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {

        }

        [Asynchronous]
        void ThenPushInitializeShouldReturnGrantMessage(string receivedMessage)
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
                mreGrant.Set();
            }
        }

    }
}
