#define USE_JSONFX
using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
    
#if (USE_JSONFX)
using JsonFx.Json;

#elif (USE_JSONFX_FOR_UNITY)
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
                [Test]
                public void ThenItShouldReturnReceivedMessage ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
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
                        Thread.Sleep (1000);
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
                                /*if (dictionary != null)
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
                }

                [Test]
                public void ThenItShouldReturnReceivedMessageSSL ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "",
                                      "",
                                      true
                      );
                        string channel = "hello_world2";
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenAClientIsPresented", "ThenPresenceShouldReturnReceivedMessage");

                        pubnub.Presence<string> (channel, common.DisplayReturnMessage, common.DisplayReturnMessage, common.DisplayErrorMessage);
                        Thread.Sleep (1000);
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
                                Assert.True (("hello_world2").Equals (serializedMessage [2]));
                        }
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfo ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "",
                                      "",
                                      false
                      );
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;
            
                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayErrorMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfoCipher ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "",
                                      "enigma",
                                      false
                      );
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecret ()
                {
                        Pubnub pubnub = new Pubnub (
                                  "demo",
                                  "demo",
                                  "secret",
                                  "enigma",
                                  false
                  );
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "secret",
                                      "enigma",
                                      false
                      );
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = Common.ValidateServerCertificate;

                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfoCipherSSL ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "",
                                      "enigma",
                                      true
                      );
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfoSecret ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "secret",
                                      "",
                                      false
                      );
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfoSecretSSL ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "secret",
                                      "",
                                      true
                      );
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                [Test]
                public void IfHereNowIsCalledThenItShouldReturnInfoSSL ()
                {
                        Pubnub pubnub = new Pubnub (
                                      "demo",
                                      "demo",
                                      "",
                                      "",
                                      true
                      );
                        Common common = new Common ();
                        common.DeliveryStatus = false;
                        common.Response = null;

                        HereNow (pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage, common.DisplayReturnMessage);
                        common.WaitForResponse ();

                        ParseResponse (common.Response);
                }

                void HereNow (Pubnub pubnub, string unitTestCaseName, 
                                                  Action<PubnubClientError> errorCallback, Action<object> userCallback)
                {
                        string channel = "hello_world";

                        PubnubUnitTest unitTest = new PubnubUnitTest ();
                        unitTest.TestClassName = "WhenAClientIsPresented";
                        unitTest.TestCaseName = unitTestCaseName;
                        pubnub.PubnubUnitTest = unitTest;

                        pubnub.HereNow (channel, userCallback, errorCallback);
                }

                public void ParseResponse (object commonResponse)
                {
                        string response = "";
                        if (commonResponse.Equals (null)) {
                                Assert.Fail ("Null response");
                        } else {
                                IList<object> responseFields = commonResponse as IList<object>;
                                foreach (object item in responseFields) {
                                        response = item.ToString ();
                                        Console.WriteLine ("Response:" + response);
                                        Assert.NotNull (response);
                                }
                                Dictionary<string, object> message = (Dictionary<string, object>)responseFields [0];
                                foreach (KeyValuePair<String, object> entry in message) {
                                        Console.WriteLine ("value:" + entry.Value + "  " + "key:" + entry.Key);
                                }
            
                                /*object[] objUuid = (object[])message["uuids"];
                                foreach (object obj in objUuid)
                                {
                                    Console.WriteLine(obj.ToString()); 
                                }*/
                                //Assert.AreNotEqual(0, message["occupancy"]);
                        }
                }

                [Test]
                public void ThenPresenceShouldReturnCustomUUID ()
                {
                        Pubnub pubnub = new Pubnub ("demo", "demo", "", "", false);
          
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
                        Thread.Sleep (10000);

                        pubnub.HereNow<string> (channel, commonHereNow.DisplayReturnMessage, commonHereNow.DisplayReturnMessage);

                        commonHereNow.WaitForResponse (30);
                        pubnub.Unsubscribe<string> (channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage);

                        if (commonHereNow.Response != null) {
                                #if (USE_JSONFX || USE_JSONFX_FOR_UNITY || USE_MiniJSON)
                                #if (USE_JSONFX)
                                                IList<object> fields = new JsonFXDotNet ().DeserializeToObject (commonHereNow.Response.ToString ()) as IList<object>;
                                #elif (USE_JSONFX_FOR_UNITY)
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

                }
        }
}

