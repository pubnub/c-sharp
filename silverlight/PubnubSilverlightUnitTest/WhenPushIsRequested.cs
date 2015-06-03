using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Silverlight.Testing;
using PubNubMessaging.Core;

namespace PubnubSilverlight.UnitTest
{
    [TestClass]
    public class WhenPushIsRequested : SilverlightTest
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
                EnqueueTestComplete();
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

                EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenPushInitializeShouldReturnGrantMessage, DummyGrantErrorCallback));
                mreGrant.WaitOne(310 * 1000);

                EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenPushIsRequested Grant access failed."));

                EnqueueCallback(() =>
                        {
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                        }
                    );
                EnqueueTestComplete();
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
                EnqueueCallback(() => pubnub.Publish<string>(channel, dicToast, PublishCallbackResult, DummyErrorCallback));
                mrePublish.WaitOne(60 * 1000);

                EnqueueCallback(() => Assert.IsTrue(receivedSuccessMessage, "Toast Publish Failed"));

                EnqueueCallback(() =>
                        {
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                        }
                    );
                EnqueueTestComplete();
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
                EnqueueCallback(() => pubnub.Publish<string>(channel, dicTile, PublishCallbackResult, DummyErrorCallback));
                mrePublish.WaitOne(60 * 1000);

                EnqueueCallback(() => Assert.IsTrue(receivedSuccessMessage, "Flip Tile Publish Failed"));

                EnqueueCallback(() =>
                        {
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                        }
                    );
                EnqueueTestComplete();
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
                EnqueueCallback(() => pubnub.Publish<string>(channel, dicTile, PublishCallbackResult, DummyErrorCallback));
                mrePublish.WaitOne(60 * 1000);

                EnqueueCallback(() => Assert.IsTrue(receivedSuccessMessage, "Cycle Tile Publish Failed"));

                EnqueueCallback(() =>
                        {
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                        }
                    );
                EnqueueTestComplete();
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
                EnqueueCallback(() => pubnub.Publish<string>(channel, dicTile, PublishCallbackResult, DummyErrorCallback));
                mrePublish.WaitOne(60 * 1000);

                EnqueueCallback(() => Assert.IsTrue(receivedSuccessMessage, "Iconic Tile Publish Failed"));

                EnqueueCallback(() =>
                        {
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                        }
                    );
                EnqueueTestComplete();
            });

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
        private void DummyGrantErrorCallback(PubnubClientError result)
        {
            mreGrant.Set();
        }

    }
}
