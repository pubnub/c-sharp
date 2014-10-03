#define USE_JSONFX_UNITY_IOS
//#define USE_MiniJSON

using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Collections;

#if (USE_JSONFX) || (USE_JSONFX_UNITY)
using JsonFx.Json;

#elif (USE_JSONFX_UNITY_IOS)
using Pathfinding.Serialization.JsonFx;

#elif (USE_DOTNET_SERIALIZATION)
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

#elif (USE_MiniJSON)
using MiniJSON;

#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAClientIsPresented
    {
        #if (UNITY_ANDROID)
		[Test]
		#else
        [Ignore]
        #endif
		public void TestsForUNITYANDROID ()
        {
            Assert.True (true);
        }
	
        #if (UNITY_IOS)
		[Test]
		#else
        [Ignore]
        #endif
		public void TestsForUNITYIOS ()
        {
            Assert.True (true);
        }

        #if (UNITY_STANDALONE)
        [Test]
        #else
		[Ignore]
		#endif
		public void TestsOnUNITY_STANDALONE ()
        {
            Assert.True (true);
        }

        #if (UNITY_STANDALONE_OSX)
        [Test]
        #else
		[Ignore]
		#endif
		public void TestsOnUNITY_STANDALONE_OSX ()
        {
            Assert.True (true);
        }

        #if (UNITY_EDITOR)
        [Test]
        #else
		[Ignore]
		#endif
		public void TestsOnUNITY_EDITOR ()
        {
            Assert.True (true);
        }

        #if (UNITY_EDITOR_OSX)
        [Test]
        #else
		[Ignore]
		#endif
		public void TestsOnUNITY_EDITOR_OSX ()
        {
            Assert.True (true);
        }

        #if (USE_JSONFX_UNITY_IOS)
        [Test]
        #else
		[Ignore]
		#endif
		public void TestsUsingJSONFX_UNITY_IOS ()
        {
            Assert.True (true);
        }

        #if (UNITY_IOS && USE_JSONFX_UNITY_IOS)
		[Test]
		#else
        [Ignore]
        #endif
		public void TestsForUNITYIOSUSE_JSONFX_UNITY_IOS ()
        {
            Assert.True (true);
        }

        #if (UNITY_IOS && USE_MiniJSON)
		[Test]
		#else
        [Ignore]
        #endif
		public void TestsForUNITYIOSUSE_MiniJSON ()
        {
            Assert.True (true);
        }

        [Test]
        public void ThenItShouldReturnReceivedMessage ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );
            string channel = "hello_world2";
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAClientIsPresented", "ThenPresenceShouldReturnReceivedMessage");

            pubnub.Presence<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessage, common.DisplayErrorMessage);
            Thread.Sleep (500);
            Common commonSubscribe = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;

            common.WaitForResponse (30);

            string response = "";

            if (common.Response == null) {
                Assert.Fail ("Null response"); 
            } else {
                UnityEngine.Debug.Log ("ThenItShouldReturnReceivedMessage:" + common.Response);
                object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject (common.Response.ToString ()).ToArray ();
                //Dictionary<string,object> dictionary = serializedMessage[0] as Dictionary<string,object>;
                /*								if (dictionary != null)
                                {
                                            foreach (KeyValuePair<string, object> item in dictionary) {
                                                        //response = dictionary[item].ToString ();
                                                        UnityEngine.Debug.Log ("ThenItShouldReturnReceivedMessage:" + item.Key + " " + item.Value.ToString());
                                            }
                                }

                                UnityEngine.Debug.Log ("ThenItShouldReturnReceivedMessage:" + common.Response);
                                /*IList<object> responseFields = common.Response as IList<object>;
                                foreach (object item in responseFields) {
                                            response = item.ToString ();
                                            Console.WriteLine ("Response:" + response);
                                            //Assert.IsNotEmpty(strResponse);
                                }*/

                Assert.True (("hello_world2").Equals (serializedMessage [2]));
            }
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          true
                      );
            string channel = "hello_world3";
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAClientIsPresented", "ThenPresenceShouldReturnReceivedMessage");

            pubnub.Presence<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessage, common.DisplayErrorMessage);
            Thread.Sleep (500);
            Common commonSubscribe = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;

            common.WaitForResponse (30);

            string response = "";
            if (common.Response == null) {
                Assert.Fail ("Null response");
            } else {
                object[] serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject (common.Response.ToString ()).ToArray ();
                Assert.True (channel.Equals (serializedMessage [2]));
            }
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfo ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayErrorMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipher ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "enigma",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecret ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "enigma",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "enigma",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "enigma",
                          true
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecret ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecretSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "",
                          true
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          true
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        void HereNow (Pubnub pubnub, string unitTestCaseName, 
                  Action<PubnubClientError> errorCallback, Action<object> userCallback)
        {
            Random r = new Random ();
            string channel = "hello_world_hn" + r.Next (100);

            Common commonSubscribe = new Common ();
            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;


            commonSubscribe.WaitForResponse (30);

            PubnubUnitTest unitTest = new PubnubUnitTest ();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = unitTestCaseName;
            pubnub.PubnubUnitTest = unitTest;
            Thread.Sleep (1500);
            pubnub.HereNow (channel, userCallback, errorCallback);

            //pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy);
        }

        public void ParseResponse (object commonResponse, Pubnub pubnub)
        {
            if (commonResponse.Equals (null)) {
                Assert.Fail ("No response: ");
            } else {
                bool found = false;
                IList<object> responseFields = commonResponse as IList<object>;
                UnityEngine.Debug.Log ("responseFields:" + responseFields.ToString ()); 
                foreach (object item in responseFields) {
                    UnityEngine.Debug.Log ("item:" + item.ToString ());
                    if (item is Dictionary<string, object>) {
                        Dictionary<string, object> message = (Dictionary<string, object>)item;
                        #if (USE_MiniJSON)
						foreach (KeyValuePair<string, object> k in message) {
								UnityEngine.Debug.Log (string.Format ("objs:{0} {1}", k.Key, k.Value));
								if (k.Key.Equals ("uuids")) {
										found = ParseDict (pubnub, k.Value);
								}
						}
                        #else
                        if (message.ContainsKey ("uuids")) {
                            object uuids = message ["uuids"];
                            found = ParseDict (pubnub, uuids);
                        }
                        #endif
                    }
                }
                if (found) {
                    Assert.True (found, "Test passed");
                } else {
                    Console.WriteLine ("response:" + commonResponse.ToString ()); 
                    Assert.Fail ("Test failed");
                }
                /*								foreach (object item in responseFields) {
                                        response = item.ToString ();
                                        Console.WriteLine ("Response:" + response);
                                        Assert.NotNull (response);
                                }
                                Dictionary<string, object> message = (Dictionary<string, object>)responseFields [0];
                                foreach (KeyValuePair<String, object> entry in message) {
                                        Console.WriteLine ("value:" + entry.Value + "  " + "key:" + entry.Key);
                                }*/

                /*object[] objUuid = (object[])message["uuids"];
								foreach (object obj in objUuid)
								{
										Console.WriteLine(obj.ToString()); 
								}*/
                //Assert.AreNotEqual(0, message["occupancy"]);
            }
        }

        [Test]
        public void IfHereNowIsCalledWithState ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string channel = "testChannelhn1";
            string testname = "IfHereNowIsCalledWithState";

            HereNowWithState<string> (pubnub, channel, testname, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse ();

            ParseResponseWithState (common.Response, "{\"testkey\":\"testval\"}", testname);
            pubnub.Unsubscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);
            pubnub.EndPendingRequests ();
        }

        bool ParseDict (Pubnub pubnub, object uuids)
        {
            object[] objUuid = null;
            UnityEngine.Debug.Log ("uuids:" + uuids);
            Type valueType = uuids.GetType ();
            var expectedType = typeof(string[]);
            var expectedType2 = typeof(object[]);

            if (expectedType.IsAssignableFrom (valueType)) {
                objUuid = uuids as string[];
            } else if (expectedType2.IsAssignableFrom (valueType)) {
                objUuid = uuids as object[];
            } else if (uuids is IList && uuids.GetType ().IsGenericType) {
                objUuid = ((IEnumerable)uuids).Cast<object> ().ToArray ();
            } else {
                objUuid = Common.Deserialize<object[]> (uuids.ToString ());
            }
            foreach (object obj in objUuid) {
                UnityEngine.Debug.Log ("session:" + obj.ToString ()); 
                if (obj.Equals (pubnub.SessionUUID)) {
                    return true;
                }
            }
            return false;
        }

        public void ParseResponseWithState (object commonResponse, string userState, string testname)
        {
            if (commonResponse.Equals (null)) {
                Assert.Fail ("Null response");
            } else {
                if (commonResponse.ToString ().Contains (userState)) {
                    Console.WriteLine ("response:" + commonResponse.ToString ()); 
                    Assert.True (true, "Test passed:" + testname);
                } else {
                    Assert.Fail ("Test failed:" + testname);
                }
            }
        }

        void HereNowWithState<T> (Pubnub pubnub, string channel, string unitTestCaseName, 
                              Action<T> userCallback, Action<PubnubClientError> errorCallback)
        {
            Common commonSubscribe = new Common ();
            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            commonSubscribe.WaitForResponse ();

            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;

            //pubnub.SetLocalUserState (channel, "testkey", "testval");
            pubnub.SetUserState<string> (channel, "{\"testkey\":\"testval\"}", commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            commonSubscribe.WaitForResponse (30);
            Thread.Sleep (500);
            pubnub.HereNow<T> (channel, true, true, userCallback, errorCallback);

            //pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy);
        }

        [Test]
        public void TestGlobalHerewNow ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string channel = "testChannel5";
            string testname = "IfHereNowIsCalledWithState";

            Common commonSubscribe = new Common ();
            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            commonSubscribe.WaitForResponse ();
            Thread.Sleep (5000);
            pubnub.GlobalHereNow<string> (true, true, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse ();   

            if (common.Response.Equals (null)) {
                Assert.Fail ("Null response");
            } else {
                if (common.Response.ToString ().Contains (pubnub.SessionUUID)
                && common.Response.ToString ().Contains (channel)) {
                    Assert.True (true, "Test passed:" + testname);
                } else {
                    Console.WriteLine ("response:" + common.Response.ToString ()); 
                    Assert.Fail ("Test failed:" + testname);
                }
            }
            pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestWhereNow ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            string channel = "testChannel7";
            string testname = "IfHereNowIsCalledWithState";

            Common commonSubscribe = new Common ();
            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            commonSubscribe.WaitForResponse ();

            Thread.Sleep (5000);

            pubnub.WhereNow<string> ("", common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse ();   

            if (common.Response.Equals (null)) {
                Assert.Fail ("Null response");
            } else {
                if (common.Response.ToString ().Contains (pubnub.SessionUUID)
                && common.Response.ToString ().Contains (channel)) {
                    Assert.True (true, "Test passed:" + testname);
                } else {
                    Console.WriteLine ("response:" + common.Response.ToString ()); 
                    Assert.Fail ("Test failed:" + testname);
                }
            }
            pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy);
            pubnub.EndPendingRequests ();
        }

        /*[Test]
				public void SetAndDeleteLocalState ()
				{
						Pubnub pubnub = new Pubnub (
								                Common.PublishKey,
								                Common.SubscribeKey,
								                "",
								                "",
								                false
						                );
						string channel = "testChannel";
						pubnub.SetLocalUserState (channel, "testkey", "testval");
						pubnub.SetLocalUserState (channel, "testkey2", "testval2");
						pubnub.SetLocalUserState (channel, "testkey2", null);
						Assert.True (("{\"testkey\":\"testval\"}").Equals (pubnub.GetLocalUserState (channel)));
						pubnub.EndPendingRequests ();
				}

				[Test]
				public void SetAndGetLocalState ()
				{
						Pubnub pubnub = new Pubnub (
								                Common.PublishKey,
								                Common.SubscribeKey,
								                "",
								                "",
								                false
						                );
						string channel = "testChannel2";
						pubnub.SetLocalUserState (channel, "testkey", "testval");
						Assert.True (("{\"testkey\":\"testval\"}").Equals (pubnub.GetLocalUserState (channel)));
						pubnub.EndPendingRequests ();
				}*/

        [Test]
        public void SetAndGetGlobalState ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "sec-c-NGVlNmRkYjAtY2Q1OS00OWM2LWE4NzktNzM5YzIxNGQxZjg3",
                          "",
                          false
                      );
            string channel = "testChannel3";
            //pubnub.SetLocalUserState (channel, "testkey", "testval");

            Common common = new Common ();

            pubnub.SetUserState<string> (channel, "{\"testkey\": \"testval\"}", common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);

            pubnub.GetUserState<string> (channel, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);

            Assert.True (common.Response.ToString ().Contains ("{\"testkey\":\"testval\"}"));
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void SetAndDeleteGlobalState ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );
            string channel = "testChannel4";
            Common common = new Common ();
            KeyValuePair<string, object> kvp = new KeyValuePair<string, object> ("k", "v");
            pubnub.SetUserState<string> (channel, kvp, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);
            Console.WriteLine ("Response UserStateAfterKvp:" + common.Response.ToString ());
            common.DeliveryStatus = false;
            common.Response = null;

            KeyValuePair<string, object> kvp2 = new KeyValuePair<string, object> ("k2", "v2");
            pubnub.SetUserState<string> (channel, kvp2, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);
            Console.WriteLine ("Response UserStateAfterKvp2:" + common.Response.ToString ());
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.GetUserState<string> (channel, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);

            Console.WriteLine ("Response GetUserStateBeforeDelete:" + common.Response.ToString ());
            Thread.Sleep (5000);
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.SetUserState<string> (channel, new KeyValuePair<string, object> ("k2", null), common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);

            Console.WriteLine ("Response SetUserState:" + common.Response.ToString ());
            common.DeliveryStatus = false;
            common.Response = null;

            Thread.Sleep (5000);
            pubnub.GetUserState<string> (channel, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);
            Console.WriteLine ("Response GetUserStateAfterDelete:" + common.Response.ToString ());
            Assert.True (common.Response.ToString ().Contains ("{\"k\":\"v\"}"));
        }

        [Test]
        public void TestPresenceHeartbeat ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );     
            string channel = "testChannel6";

            Common common = new Common ();
            pubnub.Presence<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse ();   

            Common commonSubscribe = new Common ();
            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            commonSubscribe.WaitForResponse ();

            common.DeliveryStatus = false;
            common.Response = null;
            common.WaitForResponse (); 

            common.DeliveryStatus = false;
            common.Response = null;
            common.WaitForResponse (pubnub.PresenceHeartbeat + 3); 

            if (common.Response == null) {
                Assert.True (true, "Test passed");
            } else {
                if (common.Response.ToString ().Contains ("timeout")
                && common.Response.ToString ().Contains (channel)) {
                    Assert.Fail ("Test failed: timed out");
                } else {
                    Console.WriteLine ("response:" + common.Response.ToString ()); 
                    Assert.True (true, "Test passed");
                }
            }
            pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void ThenPresenceShouldReturnCustomUUID ()
        {
            Pubnub pubnub = new Pubnub (Common.PublishKey,
                          Common.SubscribeKey,
                          "", "", false);

            Common commonHereNow = new Common ();
            commonHereNow.DeliveryStatus = false;
            commonHereNow.Response = null;

            Common commonSubscribe = new Common ();
            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;

            pubnub.PubnubUnitTest = commonHereNow.CreateUnitTestInstance ("WhenAClientIsPresented", "ThenPresenceShouldReturnCustomUUID");
            ;
            pubnub.SessionUUID = "CustomSessionUUIDTest";

            string channel = "hello_world3";
            pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage);
            commonSubscribe.WaitForResponse (30);

            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage);

            commonSubscribe.WaitForResponse (30);
            Thread.Sleep (5000);

            pubnub.HereNow<string> (channel, commonHereNow.DisplayReturnMessage, commonHereNow.DisplayReturnMessage);

            commonHereNow.WaitForResponse (30);
            pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage);

            if (commonHereNow.Response != null) {
                #if (USE_JSONFX || USE_JSONFX_UNITY || USE_JSONFX_UNITY_IOS|| USE_MiniJSON)
                #if (USE_JSONFX) 
				IList<object> fields = new JsonFXDotNet ().DeserializeToObject (commonHereNow.Response.ToString ()) as IList<object>;
                #elif (USE_JSONFX_UNITY)
				IList<object> fields = new JsonFXDotNet ().DeserializeToObject (commonHereNow.Response.ToString ()) as IList<object>;
                #elif (USE_JSONFX_UNITY_IOS)
                IList<object> fields = JsonReader.Deserialize<IList<object>> (commonHereNow.Response.ToString ()) as IList<object>;
                #elif (USE_MiniJSON)
				IList<object> fields = Json.Deserialize (commonHereNow.Response.ToString ()) as IList<object>;
                #endif
                if (fields [0] != null) {
                    bool result = false;
                    Dictionary<string, object> message = (Dictionary<string, object>)fields [0];
                    foreach (KeyValuePair<String, object> entry in message) {
                        Console.WriteLine ("value:" + entry.Value + "  " + "key:" + entry.Key);
                        Type valueType = entry.Value.GetType ();
                        var expectedType = typeof(string[]);
                        if (valueType.IsArray && expectedType.IsAssignableFrom (valueType)) {
                            List<string> uuids = new List<string> (entry.Value as string[]);
                            if (uuids.Contains (pubnub.SessionUUID)) {
                                result = true;
                                break;
                            }
                        }
                    }
                    Assert.True (result);
                } else {
                    Assert.Fail ("Null response");
                }
                #else
								object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(commonHereNow.Response.ToString());
								JContainer dictionary = serializedMessage[0] as JContainer;
								var uuid = dictionary["uuids"].ToString();
								if (uuid != null)
								{
								Assert.True(uuid.Contains(pubnub.SessionUUID));
								} else {
								Assert.Fail("Custom uuid not found.");
								}
                #endif
            } else {
                Assert.Fail ("Null response");
            }
            pubnub.EndPendingRequests ();
        }
    }
}

