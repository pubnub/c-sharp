//#define USE_JSONFX
using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;

#if (USE_JSONFX)
using JsonFx.Json;
#elif (USE_DOTNET_SERIALIZATION)
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif
namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenAClientIsPresented
    {
        #if (USE_JSONFX)
        [Test]
        #else
        [Ignore]
        #endif
        public void UsingJsonFx ()
        {
            Console.Write ("UsingJsonFx");
            Assert.True (true, "UsingJsonFx");
        }

        #if (USE_JSONFX)
        [Ignore]
        #else
        [Test]
        #endif
        public void UsingNewtonSoft ()
        {
            Console.Write ("UsingNewtonSoft");
            Assert.True (true, "UsingNewtonSoft");
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
            Thread.Sleep (3000);
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
                //IList<object> responseFields = common.Response as IList<object>;
                object[] responseFields = Common.Deserialize<object[]> (common.Response.ToString ());
                foreach (object item in responseFields) {
                    response = item.ToString ();
                    Console.WriteLine ("Response:" + response);
                }
                Assert.AreEqual (channel, responseFields [2]);
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
            Thread.Sleep (3000);
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
                //IList<object> responseFields = common.Response as IList<object>;
                object[] responseFields = Common.Deserialize<object[]> (common.Response.ToString ());
                foreach (object item in responseFields) {
                    response = item.ToString ();
                    Console.WriteLine ("Response:" + response);
                }
                Assert.AreEqual (channel, responseFields [2]);
            }
            pubnub.EndPendingRequests ();
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Subscribe
        /// </summary>
        /// <param name="result"></param>
        static void DisplaySubscribeReturnMessage (string result)
        {
            Console.WriteLine ("SUBSCRIBE REGULAR CALLBACK:");
            Console.WriteLine (result);
            Console.WriteLine ();
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Presence
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceReturnMessage (string result)
        {
            Console.WriteLine ("PRESENCE REGULAR CALLBACK:");
            Console.WriteLine (result);
            Console.WriteLine ();
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        static void DisplaySubscribeConnectStatusMessage (string result)
        {
            Console.WriteLine ("SUBSCRIBE CONNECT CALLBACK:");
            Console.WriteLine (result);
            Console.WriteLine ();
        }

        /// <summary>
        /// Callback method to provide the connect status of Presence call
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceConnectStatusMessage (string result)
        {
            Console.WriteLine ("PRESENCE CONNECT CALLBACK:");
            Console.WriteLine (result);
            Console.WriteLine ();
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
          
            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
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

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
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
                                "secret",
                                "enigma",
                                false
                            );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
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
                                "secret",
                                "enigma",
                                false
                            );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
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

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
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
                                "secret",
                                "",
                                false
                            );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
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
                                "secret",
                                "",
                                true
                            );
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
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

            HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse ();

            ParseResponse (common.Response, pubnub);
            pubnub.EndPendingRequests ();
        }

        void HereNow (Pubnub pubnub, string unitTestCaseName, 
                     Action<object> userCallback)
        {
            Random r = new Random ();
            string channel = "hello_world_hn" + r.Next (100);

            Common commonSubscribe = new Common ();
            pubnub.Subscribe<string> (channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;

            commonSubscribe.WaitForResponse (45);
            Thread.Sleep (3000);
            pubnub.HereNow (channel, userCallback, userCallback);

            //pubnub.Unsubscribe<string>(channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy);
        }

        public void ParseResponse (object commonResponse, Pubnub pubnub)
        {
            if (commonResponse.Equals (null)) {
                Assert.Fail ("Null response");
            } else {
                bool found = false;
                IList<object> responseFields = commonResponse as IList<object>;
                Console.WriteLine ("response:" + commonResponse.ToString ()); 
                foreach (object item in responseFields) {
                    if (item is Dictionary<string, object>) {
                        Dictionary<string, object> message = (Dictionary<string, object>)item;
                        if (message.ContainsKey ("uuids")) {
                            object[] objUuid = null;
                            Console.WriteLine ("uuids:" + message ["uuids"]);
                            Type valueType = message ["uuids"].GetType ();
                            var expectedType = typeof(string[]);
                            var expectedType2 = typeof(object[]);

                            if (expectedType.IsAssignableFrom (valueType)) {
                                objUuid = message ["uuids"] as string[];
                            } else if (expectedType2.IsAssignableFrom (valueType)) {
                                objUuid = message ["uuids"] as object[];
                            } else {
                                objUuid = Common.Deserialize<object[]> (message ["uuids"].ToString ());
                            }
                            foreach (object obj in objUuid) {
                                Console.WriteLine (obj.ToString ()); 
                                if (obj.Equals (pubnub.SessionUUID)) {
                                    found = true;
                                }
                            }
                        }
                    }
                }
                if (found) {
                    Assert.True (found, "Test passed");
                } else {
                    Console.WriteLine ("response:" + commonResponse.ToString ()); 
                    Assert.Fail ("Test failed:" + commonResponse.ToString ());
                }
                /*Dictionary<string, object> message = (Dictionary<string, object>)responseFields [3];

                    foreach (KeyValuePair<String, object> entry in message)
                        {
                            Console.WriteLine("value:" + entry.Value + "  " + "key:" + entry.Key);
                        }
                    if (message.Count <= 0)
                        {
                            Assert.Fail("No UUID");
                        }
                    /**/
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

            //string json = pubnub.GetLocalUserState(channel);

            ParseResponseWithState (common.Response, "{\"testkey\":\"testval\"}", testname);
            pubnub.Unsubscribe<string> (channel, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy, common.DisplayReturnMessageDummy);
            pubnub.EndPendingRequests ();
        }

        public void ParseResponseWithState (object commonResponse, string userState, string testname)
        {
            if (commonResponse.Equals (null)) {
                Assert.Fail ("Null response");
            } else {
                Console.WriteLine ("response:" + commonResponse.ToString ()); 
                if (commonResponse.ToString ().Contains (userState)) {
                    Assert.True (true, "Test passed:" + testname);
                } else {
                    Assert.Fail ("Test failed:" + userState + testname + commonResponse.ToString ());
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

            //pubnub.SetLocalUserState(channel, "testkey", "testval");
            //string json = pubnub.GetLocalUserState(channel);
            pubnub.SetUserState<string> (channel, "{\"testkey\": \"testval\"}", commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);
            commonSubscribe.WaitForResponse (30);
            Thread.Sleep (3000);
            pubnub.HereNow<T> (channel, true, true, userCallback, errorCallback);
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
        public void SetAndDeleteLocalState()
        {
            Pubnub pubnub = new Pubnub(
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "",
                                false
                            );
            string channel = "testChannel";
            pubnub.SetLocalUserState(channel, "testkey", "testval");
            pubnub.SetLocalUserState(channel, "testkey2", "testval2");
            pubnub.SetLocalUserState(channel, "testkey2", null);
            Assert.AreEqual("{\"testkey\":\"testval\"}", pubnub.GetLocalUserState(channel));
            pubnub.EndPendingRequests();
        }

        [Test]
        public void SetAndGetLocalState()
        {
            Pubnub pubnub = new Pubnub(
                                Common.PublishKey,
                                Common.SubscribeKey,
                                "",
                                "",
                                false
                            );
            string channel = "testChannel2";
            pubnub.SetLocalUserState(channel, "testkey", "testval");
            Assert.AreEqual("{\"testkey\":\"testval\"}", pubnub.GetLocalUserState(channel));
            pubnub.EndPendingRequests();
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
            //pubnub.SetLocalUserState(channel, "testkey", "testval");

            Common common = new Common ();

            pubnub.SetUserState<string> (channel, "{\"testkey\": \"testval\"}", common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);

            pubnub.GetUserState<string> (channel, common.DisplayReturnMessage, common.DisplayErrorMessage);
            common.WaitForResponse (30);

            Assert.IsTrue (common.Response.ToString ().Contains ("{\"testkey\":\"testval\"}"));
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
            Assert.IsTrue (common.Response.ToString ().Contains ("{\"k\":\"v\"}"));
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
          
            //while (!commonSubscribe.DeliveryStatus);
            commonSubscribe.WaitForResponse (30);
            Thread.Sleep (10000);

            pubnub.HereNow<string> (channel, commonHereNow.DisplayReturnMessage, commonHereNow.DisplayReturnMessage);

            //while (!commonHereNow.DeliveryStatus);
            commonHereNow.WaitForResponse (30);
            pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage);
            if (commonHereNow.Response != null) {
                Console.WriteLine (commonHereNow.Response.ToString ());
                object[] fields = Common.Deserialize<object[]> (commonHereNow.Response.ToString ());
#if (USE_JSONFX)
                    if (fields [0] != null)
                        {
                            dynamic x = fields [0];
                            string[] strarr = x.uuids;
                            bool found = false;
                            foreach (string s in strarr)
                                {
                                    if (s.Contains(pubnub.SessionUUID))
                                        {
                                            found = true;
                                            break;
                                        }
                                }
                            if (found)
                                {
                                    Assert.True(found, "Customuuid pass");
                                } else
                                {
                                    Assert.Fail("Customuuid fail");
                                }
                        } else
                        {
                            Assert.Fail("Null response");
                        }
#else
                JContainer dictionary = fields [0] as JContainer;
                var uuid = dictionary ["uuids"].ToString ();
                if (uuid != null) {
                    Assert.True (uuid.Contains (pubnub.SessionUUID));
                } else {
                    Assert.Fail ("Custom uuid not found.");
                }
#endif
            } else {
                Assert.Fail ("Null response");
            }
            pubnub.EndPendingRequests ();
        }
    }
}
    
