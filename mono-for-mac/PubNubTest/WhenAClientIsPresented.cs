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
        [Test]
        public void ThenItShouldReturnReceivedMessage()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "",
                                "",
                                false
                            );
            string channel = "hello_world2";
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;
          
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenAClientIsPresented", "ThenPresenceShouldReturnReceivedMessage");

            pubnub.Presence<string>(channel, common.DisplayReturnMessage, common.DisplayReturnMessage, common.DisplayErrorMessage);
            Thread.Sleep(3000);
            Common commonSubscribe = new Common();
            common.DeliveryStatus = false;
            common.Response = null;
          
            pubnub.Subscribe<string>(channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;


            common.WaitForResponse(30);
          
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
                    Assert.AreEqual(channel, responseFields [2]);
                }
        }

        [Test]
        public void ThenItShouldReturnReceivedMessageSSL()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
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
            Thread.Sleep(3000);
            Common commonSubscribe = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.Subscribe<string>(channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayErrorMessage);

            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;


            common.WaitForResponse(30);

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
                    Assert.AreEqual(channel, responseFields [2]);
                }
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Subscribe
        /// </summary>
        /// <param name="result"></param>
        static void DisplaySubscribeReturnMessage(string result)
        {
            Console.WriteLine("SUBSCRIBE REGULAR CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method captures the response in JSON string format for Presence
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceReturnMessage(string result)
        {
            Console.WriteLine("PRESENCE REGULAR CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method to provide the connect status of Subscribe call
        /// </summary>
        /// <param name="result"></param>
        static void DisplaySubscribeConnectStatusMessage(string result)
        {
            Console.WriteLine("SUBSCRIBE CONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        /// <summary>
        /// Callback method to provide the connect status of Presence call
        /// </summary>
        /// <param name="result"></param>
        static void DisplayPresenceConnectStatusMessage(string result)
        {
            Console.WriteLine("PRESENCE CONNECT CALLBACK:");
            Console.WriteLine(result);
            Console.WriteLine();
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfo()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "",
                                "",
                                false
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;
          
            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipher()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "",
                                "enigma",
                                false
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecret()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "secret",
                                "enigma",
                                false
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSecretSSL()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "secret",
                                "enigma",
                                false
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoCipherSSL()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "",
                                "enigma",
                                true
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecret()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "secret",
                                "",
                                false
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSecretSSL()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "secret",
                                "",
                                true
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        [Test]
        public void IfHereNowIsCalledThenItShouldReturnInfoSSL()
        {
            Pubnub pubnub = new Pubnub(
                                "demo",
                                "demo",
                                "",
                                "",
                                true
                            );
            Common common = new Common();
            common.DeliveryStatus = false;
            common.Response = null;

            HereNow(pubnub, "IfHereNowIsCalledThenItShouldReturnInfo", common.DisplayReturnMessage);
            common.WaitForResponse();

            ParseResponse(common.Response);
        }

        void HereNow(Pubnub pubnub, string unitTestCaseName, 
                     Action<object> userCallback)
        {
            string channel = "hello_world";

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenAClientIsPresented";
            unitTest.TestCaseName = unitTestCaseName;
            pubnub.PubnubUnitTest = unitTest;

            pubnub.HereNow(channel, userCallback, userCallback);
        }

        public void ParseResponse(object commonResponse)
        {
            string response = "";
            if (commonResponse.Equals(null))
                {
                    Assert.Fail("Null response");
                } else
                {
                    IList<object> responseFields = commonResponse as IList<object>;
                    foreach (object item in responseFields)
                        {
                            response = item.ToString();
                            Console.WriteLine("Response:" + response);
                            Assert.IsNotEmpty(response);
                        }
                    Dictionary<string, object> message = (Dictionary<string, object>)responseFields [0];
                    foreach (KeyValuePair<String, object> entry in message)
                        {
                            Console.WriteLine("value:" + entry.Value + "  " + "key:" + entry.Key);
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
        public void ThenPresenceShouldReturnCustomUUID()
        {
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
        
            Common commonHereNow = new Common();
            commonHereNow.DeliveryStatus = false;
            commonHereNow.Response = null;

            Common commonSubscribe = new Common();
            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;

            pubnub.PubnubUnitTest = commonHereNow.CreateUnitTestInstance("WhenAClientIsPresented", "ThenPresenceShouldReturnCustomUUID");
            ;
            pubnub.SessionUUID = "CustomSessionUUIDTest";
        
            string channel = "hello_world3";
            pubnub.Unsubscribe<string>(channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage);
            commonSubscribe.WaitForResponse(30);

            pubnub.Subscribe<string>(channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage);
          
            //while (!commonSubscribe.DeliveryStatus);
            commonSubscribe.WaitForResponse(30);
            Thread.Sleep(10000);

            pubnub.HereNow<string>(channel, commonHereNow.DisplayReturnMessage, commonHereNow.DisplayReturnMessage);

            //while (!commonHereNow.DeliveryStatus);
            commonHereNow.WaitForResponse(30);
            pubnub.Unsubscribe<string>(channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage);
            if (commonHereNow.Response != null)
                {
                    Console.WriteLine(commonHereNow.Response.ToString());
                    object[] fields = Common.Deserialize<object[]>(commonHereNow.Response.ToString());
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
                    var uuid = dictionary ["uuids"].ToString();
                    if (uuid != null)
                        {
                            Assert.True(uuid.Contains(pubnub.SessionUUID));
                        } else
                        {
                            Assert.Fail("Custom uuid not found.");
                        }
#endif
                } else
                {
                    Assert.Fail("Null response");
                }
        }
    }
}
    
