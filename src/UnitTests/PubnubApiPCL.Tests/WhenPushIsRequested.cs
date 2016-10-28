//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using System.ComponentModel;
//using System.Threading;
//using System.Collections;
////using Newtonsoft.Json;
////using Newtonsoft.Json.Linq;
//using PubnubApi;


//namespace PubNubMessaging.Tests
//{
//    [TestFixture]
//    public class WhenPushIsRequested : TestHarness
//    {
//        //string currentUnitTestCase = "";
//        bool receivedSuccessMessage = false;
//        static bool receivedGrantMessage = false;

//        ManualResetEvent mrePush = new ManualResetEvent(false);
//        ManualResetEvent mreGrant = new ManualResetEvent(false);
//        ManualResetEvent mrePublish = new ManualResetEvent(false);

//        Pubnub pubnub = null;

//        [TestFixtureSetUp]
//        public void Init()
//        {
//            if (!PubnubCommon.PAMEnabled) return;

//            receivedGrantMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                SecretKey = PubnubCommon.SecretKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channel = "hello_my_channel";

//            pubnub.Grant().Channels(new string[] { channel }).Read(true).Write(true).Manage(false).TTL(20).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = ThenPublishInitializeShouldReturnGrantMessage, Error = DummyErrorCallback });
//            Thread.Sleep(1000);

//            mreGrant.WaitOne();

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;
//            Assert.IsTrue(receivedGrantMessage, "WhenAMessageIsPublished Grant access failed.");
//        }

//        [Test]
//        public void ThenPublishMpnsToastShouldReturnSuccess()
//        {
//            receivedSuccessMessage = false;
//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);
//            string channel = "hello_my_channel";

//            mrePublish = new ManualResetEvent(false);

//            MpnsToastNotification toast = new MpnsToastNotification();
//            toast.text1 = "hardcode message";
//            Dictionary<string, object> dicToast = new Dictionary<string, object>();
//            dicToast.Add("pn_mpns", toast);

//            pubnub.EnableDebugForPushPublish = true;
//            pubnub.Publish().Channel(channel).Message(dicToast).Async(new PNCallback<PNPublishResult>() { Result = PublishCallbackResult, Error = DummyErrorCallback });
//            mrePublish.WaitOne(60 * 1000);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;
//            Assert.IsTrue(receivedSuccessMessage, "Toast Publish Failed");
//        }

//        [Test]
//        public void ThenPublishMpnsFlipTileShouldReturnSuccess()
//        {
//            receivedSuccessMessage = false;
//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channel = "hello_my_channel";

//            mrePublish = new ManualResetEvent(false);

//            MpnsFlipTileNotification tile = new MpnsFlipTileNotification();
//            tile.title = "front title";
//            tile.count = 6;
//            tile.back_title = "back title";
//            tile.back_content = "back message";
//            tile.back_background_image = "Assets/Tiles/pubnub3.png";
//            tile.background_image = "http://cdn.flaticon.com/png/256/37985.png";
//            Dictionary<string, object> dicTile = new Dictionary<string, object>();
//            dicTile.Add("pn_mpns", tile);

//            pubnub.EnableDebugForPushPublish = true;
//            pubnub.Publish().Channel(channel).Message(dicTile).Async(new PNCallback<PNPublishResult>() { Result = PublishCallbackResult, Error = DummyErrorCallback });
//            mrePublish.WaitOne(60 * 1000);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;
//            Assert.IsTrue(receivedSuccessMessage, "Flip Tile Publish Failed");
//        }

//        [Test]
//        public void ThenPublishMpnsCycleTileShouldReturnSuccess()
//        {
//            receivedSuccessMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channel = "hello_my_channel";

//            mrePublish = new ManualResetEvent(false);

//            MpnsCycleTileNotification tile = new MpnsCycleTileNotification();
//            tile.title = "front title";
//            tile.count = 2;
//            tile.images = new string[] { "Assets/Tiles/pubnub1.png", "Assets/Tiles/pubnub2.png", "Assets/Tiles/pubnub3.png", "Assets/Tiles/pubnub4.png" };

//            Dictionary<string, object> dicTile = new Dictionary<string, object>();
//            dicTile.Add("pn_mpns", tile);

//            pubnub.EnableDebugForPushPublish = true;
//            pubnub.Publish().Channel(channel).Message(dicTile).Async(new PNCallback<PNPublishResult>() { Result = PublishCallbackResult, Error = DummyErrorCallback });
//            mrePublish.WaitOne(60 * 1000);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;
//            Assert.IsTrue(receivedSuccessMessage, "Cycle Tile Publish Failed");
//        }

//        [Test]
//        public void ThenPublishMpnsIconicTileShouldReturnSuccess()
//        {
//            receivedSuccessMessage = false;

//            PNConfiguration config = new PNConfiguration()
//            {
//                PublishKey = PubnubCommon.PublishKey,
//                SubscribeKey = PubnubCommon.SubscribeKey,
//                Uuid = "mytestuuid",
//            };

//            pubnub = this.createPubNubInstance(config);

//            string channel = "hello_my_channel";

//            mrePublish = new ManualResetEvent(false);

//            MpnsIconicTileNotification tile = new MpnsIconicTileNotification();
//            tile.title = "front title";
//            tile.count = 2;
//            tile.wide_content_1 = "my wide content";

//            Dictionary<string, object> dicTile = new Dictionary<string, object>();
//            dicTile.Add("pn_mpns", tile);

//            pubnub.EnableDebugForPushPublish = true;
//            pubnub.Publish().Channel(channel).Message(dicTile).Async(new PNCallback<PNPublishResult>() { Result = PublishCallbackResult, Error = DummyErrorCallback });
//            mrePublish.WaitOne(60 * 1000);

//            pubnub.Destroy(); 
//            pubnub.PubnubUnitTest = null;
//            pubnub = null;
//            Assert.IsTrue(receivedSuccessMessage, "Iconic Tile Publish Failed");
//        }

//        private void PublishCallbackResult(PNPublishResult result)
//        {
//            if (result != null)
//            {
//                long statusCode = result.StatusCode;
//                string statusMessage = result.StatusMessage;
//                if (statusCode == 1 && statusMessage.ToLower() == "sent")
//                {
//                    receivedSuccessMessage = true;
//                }
//            }
//            mrePublish.Set();
//        }

//        void ThenPublishInitializeShouldReturnGrantMessage(PNAccessManagerGrantResult receivedMessage)
//        {
//            try
//            {
//                if (receivedMessage != null)
//                {
//                    var status = receivedMessage.StatusCode;
//                    if (status == 200)
//                    {
//                        receivedGrantMessage = true;
//                    }
//                }
//            }
//            catch { }
//            finally
//            {
//                mreGrant.Set();
//            }
//        }

//        private void DummyErrorCallback(PubnubClientError result)
//        {
//            if (result != null)
//            {
//                Console.WriteLine(result.Message);
//            }
//        }

//    }
//}
