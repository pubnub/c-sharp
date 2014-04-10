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
    public class WhenSubscribedToAChannel3
    {
        void SubscribePublishAndParse (string message, Pubnub pubnub, Common common, string channel)
        {
            pubnub.Subscribe<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy); 
            Thread.Sleep (1500);

            pubnub.Publish (channel, message, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            if (common.Response != null) {
                object[] deserializedMessage = Common.Deserialize<object[]> (common.Response.ToString ());
                if (deserializedMessage != null) {
                    Assert.True (message.Equals(deserializedMessage [0].ToString ()));
                } else {
                    Assert.Fail ("Test not successful");
                }
            } else {
                Assert.Fail ("No response");
            }
        }

		[Test]
		public void TestForUnicodeSSL ()
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

				string message = "Text with ÜÖ漢語";

				SubscribePublishAndParse (message, pubnub, common, channel);
		}

		[Test]
		public void TestForUnicode()
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

				string message = "Text with ÜÖ漢語";

				SubscribePublishAndParse (message, pubnub, common, channel);
		}

        [Test]
        public void TestForForwardSlashSSL ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForForwardSlashCipher ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForForwardSlash ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForForwardSlashCipherSSL ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForForwardSlashSecret ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForForwardSlashCipherSecret ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForForwardSlashCipherSecretSSL ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForForwardSlashSecretSSL ()
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

            string message = "Text with /";

            SubscribePublishAndParse (message, pubnub, common, channel);
        }

        [Test]
        public void TestForSpecialCharSSL ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForSpecialCharCipher ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForSpecialChar ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForSpecialCharCipherSSL ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForSpecialCharSecret ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForSpecialCharCipherSecret ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForSpecialCharCipherSecretSSL ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }

        [Test]
        public void TestForSpecialCharSecretSSL ()
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

            string message = "Text with '\"";

            SubscribePublishAndParse (message, pubnub, common, channel);

        }
    }
}

