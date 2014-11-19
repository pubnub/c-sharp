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
    public class WhenPushIsRequested
    {
        string currentUnitTestCase = "";
        bool receivedSuccessMessage = false;
        static bool receivedGrantMessage = false;

        ManualResetEvent mrePush = new ManualResetEvent(false);
        ManualResetEvent mreGrant = new ManualResetEvent(false);
        ManualResetEvent mrePublish = new ManualResetEvent(false);

        [TestFixtureSetUp]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                return;
            }

            receivedGrantMessage = false;
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            mreGrant = new ManualResetEvent(false);
            pubnub.GrantAccess<string>(channel, true, true, 20, ThenPushInitializeShouldReturnGrantMessage, DummyErrorCallback);
            mreGrant.WaitOne(60 * 1000);

            Assert.IsTrue(receivedGrantMessage, "WhenPushIsRequested Grant access failed.");
        }

        [Test]
        public void ThenPublishMpnsToastShouldReturnSuccess()
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

            Assert.IsTrue(receivedSuccessMessage, "Toast Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsFlipTileShouldReturnSuccess()
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

            Assert.IsTrue(receivedSuccessMessage, "Flip Tile Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsCycleTileShouldReturnSuccess()
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

            Assert.IsTrue(receivedSuccessMessage, "Cycle Tile Publish Failed");
        }

        [Test]
        public void ThenPublishMpnsIconicTileShouldReturnSuccess()
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

            Assert.IsTrue(receivedSuccessMessage, "Iconic Tile Publish Failed");
        }

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


        private void DummyErrorCallback(PubnubClientError result)
        {

        }

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
