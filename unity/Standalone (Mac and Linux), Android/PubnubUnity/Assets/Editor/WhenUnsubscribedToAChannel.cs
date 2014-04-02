using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;

namespace PubNubMessaging.Tests
{
        [TestFixture]
        public class WhenUnsubscribedToAChannel
        {
                [Test]
                public void ThenNonExistentChannelShouldReturnNotSubscribed ()
                {
						Pubnub pubnub = new Pubnub (Common.PublishKey,
								Common.SubscribeKey, "", "", false);

                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;
      
                        pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenUnsubscribedToAChannel", "ThenNonExistentChannelShouldReturnNotSubscribed");
      
                        string channel = "hello_world";
      
                        pubnub.Unsubscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessage);

                        common.WaitForResponse ();

                        Console.WriteLine ("Response:" + common.Response);
                        if (common.Response.ToString ().ToLower ().Contains ("not subscribed")) {
                                Assert.Pass ();
                        } else {
                                Assert.Fail ();
                        }
                }

                [Test]
                public void ThenShouldReturnUnsubscribedMessage ()
                {
						Pubnub pubnub = new Pubnub (Common.PublishKey,
								Common.SubscribeKey, "", "", false);
      
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;
      
                        pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenUnsubscribedToAChannel", "ThenShouldReturnUnsubscribedMessage");
      
                        string channel = "hello_world";

                        pubnub.Subscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

                        common.WaitForResponse ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        pubnub.Unsubscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
                        common.WaitForResponse ();

                        if (common.Response.ToString ().Contains ("Unsubscribed from")) {
                                Console.WriteLine ("Response:" + common.Response);
                                Assert.Pass ();
                        } else {
                                Assert.Fail ();
                        }    
                }
                [Test]
                public void ThenShouldReturnUnsubscribedMessageSSL()
                {
						Pubnub pubnub = new Pubnub(Common.PublishKey,
								Common.SubscribeKey, "", "", true);

                    Common common = new Common();
                    common.DeliveryStatus = false;
                    common.Response = null;

                    pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenUnsubscribedToAChannel", "ThenShouldReturnUnsubscribedMessage");

                    string channel = "hello_world2";

                    pubnub.Subscribe<string>(channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

                    common.WaitForResponse();
                    common.DeliveryStatus = false;
                    common.Response = null;

                    pubnub.Unsubscribe<string>(channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
                    common.WaitForResponse();

                    if (common.Response.ToString().Contains("Unsubscribed from"))
                    {
                        Console.WriteLine("Response:" + common.Response);
                        Assert.Pass();
                    } else
                    {
                        Assert.Fail();
                    }    
                }

				//[Test]
                public void TestUnsubscribePresence()
                {
                    Pubnub pubnub = new Pubnub(
						Common.PublishKey,
						Common.SubscribeKey,
                        "",
                        "",
                        false
                    );
                    string channel = "hello_world12";
                    Common common = new Common();
                    common.DeliveryStatus = false;
                    common.Response = null;

                    pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenAClientIsPresented", "ThenPresenceShouldReturnReceivedMessage");

                    pubnub.Presence<string>(channel, common.DisplayReturnMessage, common.DisplayReturnMessage, common.DisplayErrorMessage);
                        //Thread.Sleep(13000);
					common.WaitForResponse();
                    Common commonSubscribe = new Common();
                    common.DeliveryStatus = false;
                    common.Response = null;

                    pubnub.Subscribe<string>(channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

                    commonSubscribe.DeliveryStatus = false;
                    commonSubscribe.Response = null;

                    common.WaitForResponse();

                    string response = "";
                    if (common.Response == null)
                    {
                        Assert.Fail("Null response");
                    } else
                    {
                        //IList<object> responseFields = common.Response as IList<object>;
                        object[] responseFields = Common.Deserialize<object[]>(common.Response.ToString());
                        foreach (object item in responseFields)
                        {
                            response = item.ToString();
                            Console.WriteLine("Response:" + response);
                        }
                        if (channel.Equals(responseFields [2]))
                        {
                            Unsub(common, pubnub, channel);
                        }
                    }
                }

                private void Unsub(Common common, Pubnub pubnub, string channel)
                {
                    Common commonUnsubscribe = new Common();

                    common.DeliveryStatus = false;
                    common.Response = null;
                    pubnub.Unsubscribe<string>(channel, commonUnsubscribe.DisplayReturnMessageDummy, commonUnsubscribe.DisplayReturnMessageDummy, commonUnsubscribe.DisplayReturnMessage, common.DisplayReturnMessageDummy);

                    common.WaitForResponse();
                    string response = "";
                    if (common.Response == null)
                    {
                        Assert.Fail("Null response");
                    } else
                    {
                        object[] responseFields2 = Common.Deserialize<object[]>(common.Response.ToString());
                        int count = 0;
                        foreach (object item in responseFields2)
                        {
                            count++;
                            response = item.ToString();
                            UnityEngine.Debug.Log("Response:" + response);
                            if (count == 0){
                                Assert.True(item.ToString().Contains("leave"));
                            }
                        }
                                //Assert.True(responseFields2 [0].ToString().Contains("leave"));
                    }
                }

				//[Test]
                public void TestUnsubscribePresenceSSL()
                {
                    Pubnub pubnub = new Pubnub(
						Common.PublishKey,
						Common.SubscribeKey,                        
						"",
                        "",
                        true
                    );
                    string channel = "hello_world3";
                    Common common = new Common();
                    common.DeliveryStatus = false;
                    common.Response = null;

                    pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenAClientIsPresented", "ThenPresenceShouldReturnReceivedMessage");

                    pubnub.Presence<string>(channel, common.DisplayReturnMessage, common.DisplayReturnMessage, common.DisplayErrorMessage);
					common.WaitForResponse();
                    Common commonSubscribe = new Common();
                    common.DeliveryStatus = false;
                    common.Response = null;

                    pubnub.Subscribe<string>(channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

                    commonSubscribe.DeliveryStatus = false;
                    commonSubscribe.Response = null;


                    common.WaitForResponse();

                    string response = "";
                    if (common.Response == null)
                    {
                        Assert.Fail("Null response");
                    } else
                    {
                        //IList<object> responseFields = common.Response as IList<object>;
                        object[] responseFields = Common.Deserialize<object[]>(common.Response.ToString());
                        if (channel.Equals(responseFields [2]))
                        {
                            Unsub(common, pubnub, channel);
                        }
                    }
                }
        }
}

