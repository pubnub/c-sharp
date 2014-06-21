using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace PubNubMessaging.Tests
{
		[TestFixture]
		public class WhenAMessageIsPublished
		{
				public void NullMessage ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
												"",
								                "",
								                false
						                );
						string channel = "hello_world";
						string message = null;

						Common common = new Common ();
						common.DeliveryStatus = false;
						common.Response = null;

						pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage);
						//wait till the response is received from the server
						common.WaitForResponse ();
						if (common.Response != null) {
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
				}
				//[Test]
				public void Test0ForComplexMessage2 ()
				{
						Pubnub pubnub = new Pubnub (
								"demo",
								"demo",
								"",
								"enigma",
								false
						);
						string channel = "hello_world";
						//pubnub.NonSubscribeTimeout = int.Parse (operationTimeoutInSeconds);
						object message = new PubnubDemoObject ();
						Common common = new Common ();
						DeliveryStatus = false;
						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoForEncryptedComplexMessage2");
						pubnub.Publish<string>(channel, message, DisplayReturnMessageECM2, DisplayErrorMessageECM2);


						Pubnub pubnub2 = new Pubnub (
								"demo",
								"demo",
								"",
								"",
								true
						);
						object message2 = new PubnubDemoObject ();

						string json = Common.Serialize (message2);
						DeliveryStatus = false;
						pubnub2.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoForComplexMessage2WithSsl");


						pubnub2.Publish (channel, message2, DisplayReturnMessageECM2Ssl, DisplayErrorMessageECM2Ssl);
						//common.WaitForResponse ();

						Pubnub pubnub3 = new Pubnub (
								"demo",
								"demo",
								"",
								"",
								false
						);
						object message3 = new PubnubDemoObject ();
						//object message = new CustomClass2();

						string json3 = Common.Serialize (message3);

						pubnub3.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoForComplexMessage2");
						pubnub3.Publish<string> (channel, message3, DisplayReturnMessage, DisplayErrorMessage);
				}
				object ResponseECM2;

				object ResponseECM2Ssl;

				public void DisplayReturnMessageECM2 (string result)
				{
						UnityEngine.Debug.Log ("DisplayReturnMessageS:" + result.ToString ());
						UnityEngine.Debug.Log("DisplayReturnMessageS:" + DateTime.UtcNow.ToLongTimeString());
						ResponseECM2 = (object)result.ToString();
						DeliveryStatus = true;
						UnityEngine.Debug.Log("DisplayReturnMessageS2:" + DateTime.UtcNow.ToLongTimeString());
				}

				public void DisplayReturnMessageECM2 (object result)
				{
						UnityEngine.Debug.Log ("DisplayReturnMessageS1:" + result.ToString ());
						UnityEngine.Debug.Log ("DisplayReturnMessageS1:" + DateTime.UtcNow.ToLongTimeString ());
						ResponseECM2 = result;
						DeliveryStatus = true;
						UnityEngine.Debug.Log ("DisplayReturnMessageS12:" + DateTime.UtcNow.ToLongTimeString ());
				}

				public void DisplayErrorMessageECM2 (PubnubClientError result)
				{
						UnityEngine.Debug.Log ("DisplayErrorMessage1:" + result.ToString ());
				}
				public void DisplayReturnMessageECM2Ssl (object result)
				{
						UnityEngine.Debug.Log ("DisplayReturnMessageS1:" + result.ToString ());
						UnityEngine.Debug.Log ("DisplayReturnMessageS1:" + DateTime.UtcNow.ToLongTimeString ());
						ResponseECM2Ssl = result;
						DeliveryStatus = true;
						UnityEngine.Debug.Log ("DisplayReturnMessageS12:" + DateTime.UtcNow.ToLongTimeString ());
				}

				public void DisplayErrorMessageECM2Ssl (PubnubClientError result)
				{
						UnityEngine.Debug.Log ("DisplayErrorMessage1:" + result.ToString ());
				}
				#if(UNITY_ANDROID)
				[Ignore]
				#else
				[Test]
				#endif
				public void ThenItShouldReturnSuccessCodeAndInfoForEncryptedComplexMessage2 ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "",
								                "enigma",
								                false
						                );
						string channel = "hello_world";
						//pubnub.NonSubscribeTimeout = int.Parse (operationTimeoutInSeconds);
						object message = new PubnubDemoObject ();
          
						Common common = new Common ();
          
						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoForEncryptedComplexMessage2");
          
						common.DeliveryStatus = false;
						common.Response = null;
          
						pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage);
						//wait till the response is received from the server

						common.WaitForResponse ();
						//while (!DeliveryStatus)
								;
						//UnityEngine.Debug.Log ("IN:ThenItShouldReturnSuccessCodeAndInfoForEncryptedComplexMessage2");
						if (common.Response != null) {
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
						/*if (ResponseECM2 != null) {
								IList<object> fields = ResponseECM2 as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}*/

				}
				#if((UNITY_ANDROID) || (UNITY_STANDALONE_WIN))
				[Ignore]
				#else
				[Test]
				#endif
				public void ThenItShouldReturnSuccessCodeAndInfoForComplexMessage2WithSsl ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "",
								                "",
								                true
						                );
						string channel = "hello_world";
						object message = new PubnubDemoObject ();
						//object message = new CustomClass2();
          
						string json = Common.Serialize (message);
						Common common = new Common ();
          
						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoForComplexMessage2WithSsl");
          
						common.DeliveryStatus = false;
						common.Response = null;
          
						pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage);
						//wait till the response is received from the server
						common.WaitForResponse ();
						if (common.Response != null) {
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
						/*if (ResponseECM2Ssl != null) {
								IList<object> fields = ResponseECM2Ssl as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}*/
				}

				object Response;
				bool DeliveryStatus;

				public void DisplayReturnMessage (string result)
				{
						UnityEngine.Debug.Log ("DisplayReturnMessageS1:" + result.ToString ());
						UnityEngine.Debug.Log ("DisplayReturnMessageS1:" + DateTime.UtcNow.ToLongTimeString ());
						Response = (object)result.ToString ();
						DeliveryStatus = true;
						UnityEngine.Debug.Log ("DisplayReturnMessageS12:" + DateTime.UtcNow.ToLongTimeString ());
				}

				public void DisplayErrorMessage (PubnubClientError result)
				{
						UnityEngine.Debug.Log ("DisplayErrorMessage1:" + result.ToString ());
				}

				//[Test]
				public void ThenItShouldReturnSuccessCodeAndInfoForComplexMessage2 ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "",
								                "",
								                false
						                );
						string channel = "hello_world";
						object message = new PubnubDemoObject ();
						//object message = new CustomClass2();

						string json = Common.Serialize (message);
						Common common = new Common ();

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoForComplexMessage2");
						DeliveryStatus = false;
						pubnub.Publish<string> (channel, message, DisplayReturnMessage, DisplayErrorMessage);
						//wait till the response is received from the server
						common.WaitForResponse ();
						//while (!DeliveryStatus)
						//; 
						if (common.Response != null) {
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
						/*if (Response != null) {
								#if(!UNITY_ANDROID)
								IList<object> fields = Response as IList<object>;
								#else
								UnityEngine.Debug.Log ("cm2: " + Response.ToString ());
								IList<object> fields = Response as IList<object>;
								#endif
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}*/

				}
				[Test]
				public void ThenItShouldReturnSuccessCodeAndInfoForComplexMessage ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "",
								                "",
								                false
						                );
						string channel = "hello_world";
						object message = new CustomClass ();
          
						Common common = new Common ();
          
						//pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoForComplexMessage");
          
						common.DeliveryStatus = false;
						common.Response = null;
          
						pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayErrorMessage);
						//wait till the response is received from the server
						common.WaitForResponse ();
						if (common.Response != null) {
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
				}
				[Test]
				public void ThenItShouldReturnSuccessCodeAndInfoWhenEncrypted ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "",
								                "enigma",
								                false
						                );
						string channel = "hello_world";
						string message = "Pubnub API Usage Example";
          
						Common common = new Common ();
          
						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoWhenEncrypted");
          
						common.DeliveryStatus = false;
						common.Response = null;
          
						pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage);
						//wait till the response is received from the server
						common.WaitForResponse ();
						if (common.Response != null) {
								Console.WriteLine (common.Response.ToString ());
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
				}
				[Test]
				public void ThenItShouldReturnSuccessCodeAndInfoWhenEncryptedAndSecretKeyed ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "secret",
								                "enigma",
								                false
						                );
						string channel = "hello_world";
						string message = "Pubnub API Usage Example";
          
						Common common = new Common ();
          
						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfoWhenEncryptedAndSecretKeyed");
          
						common.DeliveryStatus = false;
						common.Response = null;
          
						pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage);
						//wait till the response is received from the server
						common.WaitForResponse ();
						if (common.Response != null) {
								Console.WriteLine (common.Response.ToString ());
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
				}
				[Test]
				public void ThenItShouldReturnSuccessCodeAndInfo ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "",
								                "",
								                false
						                );
						string channel = "hello_world";
						string message = "Pubnub API Usage Example";

						Common common = new Common ();
            
						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenItShouldReturnSuccessCodeAndInfo");

						common.DeliveryStatus = false;
						common.Response = null;

						pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage);
						//wait till the response is received from the server
						common.WaitForResponse ();
						if (common.Response != null) {
								IList<object> fields = common.Response as IList<object>;
								string sent = fields [1].ToString ();
								string one = fields [0].ToString ();
								Assert.AreEqual ("Sent", sent);
								Assert.AreEqual ("1", one);
						} else {
								Assert.Fail ("Null response");
						}
				}
				[Test]
				public void ThenItShouldGenerateUniqueIdentifier ()
				{
						Pubnub pubnub = new Pubnub (
								                "demo",
								                "demo",
								                "",
								                "",
								                false
						                );

						Assert.IsNotNull (pubnub.GenerateGuid ());
				}
				[Test]
				public void Then1PublishKeyShouldBeOverriden ()
				{
						Pubnub pubnub = new Pubnub (
								                "",
								                "demo",
								                "",
								                "",
								                false
						                );
						string channel = "mychannel";
						string message = "Pubnub API Usage Example";

						pubnub = new Pubnub (
								"demo",
								"demo",
								"",
								"",
								false
						);
						Common common = new Common ();
						Assert.AreEqual (true, pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage));
				}
				[Test]
				[ExpectedException (typeof(MissingFieldException))]
				public void Then2PublishKeyShouldNotBeEmptyAfterOverriden ()
				{
						Pubnub pubnub = new Pubnub (
								                "",
								                "demo",
								                "",
								                "",
								                false
						                );
						string channel = "mychannel";
						string message = "Pubnub API Usage Example";
						Common common = new Common ();
						Assert.AreEqual (false, pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage));
				}
				[Test]
				public void Then3SecretKeyShouldBeProvidedOptionally ()
				{
						Pubnub pubnub = new Pubnub (
								                "demo",
								                "demo"
						                );
						string channel = "mychannel";
						string message = "Pubnub API Usage Example";
						Common common = new Common ();
						Assert.AreEqual (true, pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage));
						pubnub = new Pubnub (
								"demo",
								"demo",
								"key"
						);
						Assert.AreEqual (true, pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage));
				}
				[Test]
				public void TestIfSSLNotProvidedThenDefaultShouldBeFalse ()
				{
						Pubnub pubnub = new Pubnub (
								                "demo",
								                "demo",
								                ""
						                );
						string channel = "hello_world";
						string message = "Pubnub API Usage Example";
						Common common = new Common ();
						Assert.AreEqual (true, pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage));
				}
				[Test]
				[ExpectedException (typeof(MissingFieldException))]
				public void TestNullShouldBeTreatedAsEmpty ()
				{
						Pubnub pubnub = new Pubnub (
								                null,
								                "demo",
								                null,
								                null,
								                false
						                );
						string channel = "mychannel";
						string message = "Pubnub API Usage Example";
						Common common = new Common ();
						Assert.AreEqual (false, pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessage));
				}
				//[Test]
				public void ThenLargeMessageShoudFailWithMessageTooLargeInfo ()
				{
						Pubnub pubnub = new Pubnub (
								Common.PublishKey,
								Common.SubscribeKey,
								                "",
								                "",
								                false
						                );
						string channel = "hello_world";
						string messageLarge2K = "This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. This is a large message test which will return an error message. ";
            
						Common common = new Common ();
            
						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAMessageIsPublished", "ThenLargeMessageShoudFailWithMessageTooLargeInfo");
            
						common.DeliveryStatus = false;
						common.Response = null;
            
						pubnub.Publish<string> (channel, messageLarge2K, common.DisplayReturnMessage, common.DisplayErrorMessage2);
						//wait till the response is received from the server
						common.WaitForResponse ();
						if (common.Response != null) {
								UnityEngine.Debug.Log ("lm2: " + common.Response.ToString ());
								IList<object> fields = common.Response as IList<object>;

								string sent = fields [1].ToString ();
								Assert.AreEqual ("Message Too Large", sent);
						} else {
								Assert.Fail ("Null response");
						}
				}
		}
}

