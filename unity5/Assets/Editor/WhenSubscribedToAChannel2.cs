using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;


namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToAChannel2
    {
        void SubscribePublishAndParse (string message, Pubnub pubnub, Common common, string channel)
        {
            Random r = new Random ();
            channel = "hello_world_sub" + r.Next (1000);
			Thread.Sleep (1000);
            pubnub.Subscribe<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy); 
            Thread.Sleep (2500);
            pubnub.NonSubscribeTimeout = 30;
            pubnub.Publish (channel, message, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);
            pubnub.NonSubscribeTimeout = 15;
            common.WaitForResponse (35);

            if (common.Response != null) {
                object[] deserializedMessage = Common.Deserialize<object[]> (common.Response.ToString ());
                if (deserializedMessage != null) {
                    Assert.True (message.Equals (deserializedMessage [0].ToString ()));
                } else {
                    Assert.Fail ("Test not successful");
                }
            } else {
                Assert.Fail ("No response: " + common.ErrorResponse);
            }
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.Unsubscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse (20);

            pubnub.EndPendingRequests ();
        }

        [Test]
        public void ThenItShouldReturnReceivedMessage ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Test Message";

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenSubscribeShouldReturnReceivedMessage");

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageSSL ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Test Message";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageCipherSSL ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "enigma",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Test Message";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageSecret ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                Common.SecretKey,
                                "",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Test Message";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageSecretSSL ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                Common.SecretKey,
                                "",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Test Message";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageSecretCipher ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                Common.SecretKey,
                                "engima",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Test Message";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageSecretSSLCipher ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                Common.SecretKey,
                                "enigma",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Test Message";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageCipher ()
        {

            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "enigma",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenSubscribeShouldReturnReceivedMessageCipher");

            string message = "Test Message";
            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForEmojiSSL ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForEmojiCipher ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "enigma",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForEmoji ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForEmojiCipherSSL ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "enigma",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForEmojiSecret ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "secret",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForEmojiCipherSecret ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                Common.SecretKey,
                                "enigma",
                                false);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForEmojiCipherSecretSSL ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                Common.SecretKey,
                                "enigma",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForEmojiSecretSSL ()
        {
            Pubnub pubnub = new Pubnub (
                                Common.PublishKey,
                                Common.SubscribeKey,
                                Common.SecretKey,
                                "",
                                true);
            string channel = "hello_world";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string message = "Text with 😜 emoji 🎉.";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }
    }
}

