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
		public class WhenSubscribedToAChannel
		{
				[Test]
				public void TestForComplexMessage ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				[Test]
				public void TestForComplexMessageCipher ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageCipherForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				[Test]
				public void TestForComplexMessageCipherSecret ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				[Test]
				public void TestForComplexMessageCipherSecretSSL ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageCipherForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				[Test]
				public void TestForComplexMessageCipherSSL ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				[Test]
				public void TestForComplexMessageSecret ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageCipherForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				[Test]
				public void TestForComplexMessageSecretSSL ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				[Test]
				public void TestForComplexMessageSSL ()
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

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenItShouldReturnReceivedMessageCipherForComplexMessage");

						SubscribePublishAndParseComplex (pubnub, common, channel);
				}

				void SubscribePublishAndParseComplex (Pubnub pubnub, Common common, string channel)
				{
						CustomClass message = new CustomClass ();

						pubnub.Subscribe<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy); 
						Thread.Sleep (3000);

						pubnub.Publish (channel, (object)message, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);

						common.WaitForResponse ();

						if (common.Response != null) {
								object[] fields = Common.Deserialize<object[]> (common.Response.ToString ());

								if (fields [0] != null) {
										var myObjectArray = (from item in fields
										                               select item as object).ToArray ();

										CustomClass cc = new CustomClass ();

										//If the custom class is serialized with jsonfx the response is received as a dictionary and
										//on deserialization with Newtonsoft.Json we get an error.
										//As a work around we parse the dictionary object.   
										var dict = myObjectArray [0] as IDictionary;

										if ((dict != null) && (dict.Count > 1)) {
												cc.foo = (string)dict ["foo"];
												cc.bar = (int[])dict ["bar"];
										} else {
												cc = Common.Deserialize<CustomClass> (myObjectArray [0].ToString ());
												/*                                                            #if (USE_JSONFX)
                        var reader = new JsonFx.Json.JsonReader();
                        cc = reader.Read<CustomClass>(myObjectArray[0].ToString());
            #else
                        cc = JsonConvert.DeserializeObject<CustomClass>(myObjectArray[0].ToString());
            #endif*/
										}  
										if (cc.bar.SequenceEqual (message.bar) && cc.foo.Equals (message.foo)) {
												Assert.Pass ("Complex message test successful");
										} else {
												Assert.Fail ("Complex message test not successful");
										}
								} else {
										Assert.Fail ("No response");
								}
						} else {
								Assert.Fail ("No response");
						}
				}

				[Test]
				public void ThenSubscribeShouldReturnConnectStatus ()
				{
						Pubnub pubnub = new Pubnub (Common.PublishKey,
								             Common.SubscribeKey,
								             "", "", false);

						Common common = new Common ();
						common.DeliveryStatus = false;
						common.Response = null;

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenSubscribeShouldReturnConnectStatus");

						string channel = "hello_world";

						pubnub.Subscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

						common.WaitForResponse ();         

						if (ParseResponse (common.Response)) {
								Assert.Pass ("Connected and status code received");
						} else {
								Assert.Fail ("Test failed");
						}
				}

				bool ParseResponse (object response)
				{
						bool retVal = false;
						if (response != null) {
								object[] deserializedMessage = Common.Deserialize<object[]> (response.ToString ());
								/*                        #if (USE_JSONFX)
            var reader = new JsonFx.Json.JsonReader();
            deserializedMessage = reader.Read<object[]>(response.ToString());
        #else
            deserializedMessage = JsonConvert.DeserializeObject<object[]>(response.ToString());
        #endif*/
								if (deserializedMessage is object[]) {
										long statusCode = Int64.Parse (deserializedMessage [0].ToString ());
										string statusMessage = (string)deserializedMessage [1];
										if (statusCode == 1 && statusMessage.ToLower () == "connected") {
												retVal = true;           
										}
								}
						}
						return retVal;
				}
				//[Test]
				public void ThenMultiSubscribeShouldReturnConnectStatus ()
				{
						Pubnub pubnub = new Pubnub (Common.PublishKey,
								             Common.SubscribeKey,
								             "", "", false);

						Common common = new Common ();
						common.DeliveryStatus = false;
						common.Response = null;

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenMultiSubscribeShouldReturnConnectStatus");

						string channel1 = "testChannel1";
						pubnub.Subscribe<string> (channel1, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

						common.WaitForResponse ();

						bool receivedChannel1ConnectMessage = ParseResponse (common.Response);
						common.DeliveryStatus = false;
						common.Response = null;

						string channel2 = "testChannel2";
						pubnub.Subscribe<string> (channel2, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
						common.WaitForResponse (); 

						bool receivedChannel2ConnectMessage = ParseResponse (common.Response);

						if (receivedChannel1ConnectMessage && receivedChannel2ConnectMessage) {
								Assert.Pass ("Connected and status code received");
						} else {
								Assert.Fail ("Test failed");
						}
				}
				//[Test]
				public void ThenMultiSubscribeShouldReturnConnectStatusSSL ()
				{
						Pubnub pubnub = new Pubnub (Common.PublishKey,
								             Common.SubscribeKey,
								             "", "", true);

						Common common = new Common ();
						common.DeliveryStatus = false;
						common.Response = null;

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenMultiSubscribeShouldReturnConnectStatus");

						string channel1 = "testChannel1";
						pubnub.Subscribe<string> (channel1, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

						common.WaitForResponse ();

						bool receivedChannel1ConnectMessage = ParseResponse (common.Response);
						common.DeliveryStatus = false;
						common.Response = null;

						string channel2 = "testChannel2";
						pubnub.Subscribe<string> (channel2, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
						common.WaitForResponse (); 

						bool receivedChannel2ConnectMessage = ParseResponse (common.Response);

						if (receivedChannel1ConnectMessage && receivedChannel2ConnectMessage) {
								Assert.Pass ("Connected and status code received");
						} else {
								Assert.Fail ("Test failed");
						}
				}

				[Test]
				public void ThenDuplicateChannelShouldReturnAlreadySubscribed ()
				{
						Pubnub pubnub = new Pubnub (Common.PublishKey,
								             Common.SubscribeKey,
								             "", "", false);

						Common common = new Common ();
						common.DeliveryStatus = false;
						common.Response = null;

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenSubscribedToAChannel", "ThenDuplicateChannelShouldReturnAlreadySubscribed");

						string channel = "testChannel";

						pubnub.Subscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);
						Thread.Sleep (100);

						pubnub.Subscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessage);
						common.WaitForResponse ();  

						Console.WriteLine ("Response:" + common.Response);
						if (common.Response.ToString ().ToLower ().Contains ("already subscribed")) {
								Assert.Pass ("Test passed");
						} else {
								Assert.Fail ("Test failed");
						}
				}
				//[Test]
				public void ThenSubscriberShouldBeAbleToReceiveManyMessages ()
				{
						Pubnub pubnub = new Pubnub (Common.PublishKey,
								             Common.SubscribeKey,
								             "", "", false);

						Common common = new Common ();
						common.DeliveryStatus = false;
						common.Response = null;

						//pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenSubscribedToAChannel", "ThenSubscriberShouldBeAbleToReceiveManyMessages");

						string channel = "testChannel";

						pubnub.Subscribe<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);
						Thread.Sleep (1000);

						int iPassCount = 0;
						for (int i = 0; i < 10; i++) {
								/*                if(pubnub.PubnubUnitTest.EnableStubTest)
            {
              if(common.Response!=null)
              {
                iPassCount++;
              }
            }
            else
            {*/
								string message = "Test Message " + i;
								pubnub.Publish (channel, message, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);
								Console.WriteLine (string.Format ("Sent {0}", message)); 

								common.WaitForResponse ();

								if (common.Response.ToString ().Contains (message)) {
										iPassCount++;
										Console.WriteLine (string.Format ("Received {0}", message)); 
								}
								//}
								common.DeliveryStatus = false;
								common.Response = null;            
						}
						Console.WriteLine (string.Format ("passcount {0}", iPassCount)); 
						if (iPassCount >= 10) {
								Assert.Pass ("Test passed");
						} else {
								Assert.Fail ("Test failed");
						}
				}
		}
}

