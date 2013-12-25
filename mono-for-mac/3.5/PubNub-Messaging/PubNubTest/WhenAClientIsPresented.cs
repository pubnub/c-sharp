using System;
using PubNubMessaging.Core;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
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
            string channel = "hello_world";
            Common commonPresence = new Common();
            commonPresence.DeliveryStatus = false;
            commonPresence.Response = null;
            
            pubnub.PubnubUnitTest = commonPresence.CreateUnitTestInstance("WhenAClientIsPresented", "ThenPresenceShouldReturnReceivedMessage");
            
            pubnub.Presence(channel, commonPresence.DisplayReturnMessage, commonPresence.DisplayReturnMessageDummy, commonPresence.DisplayReturnMessageDummy);
            
            Common commonSubscribe = new Common();
            commonSubscribe.DeliveryStatus = false;
            commonSubscribe.Response = null;
            
            pubnub.Subscribe(channel, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessageDummy, commonPresence.DisplayReturnMessageDummy);
            //while (!commonSubscribe.DeliveryStatus) ;
            
            commonPresence.WaitForResponse(30);
            
            string response = "";
            if (commonPresence.Response == null) {
              Assert.Fail("Null response");
            }
            else
            {
              IList<object> responseFields = commonPresence.Response as IList<object>;
              foreach (object item in responseFields)
              {
                response = item.ToString();
                Console.WriteLine("Response:" + response);
                //Assert.IsNotEmpty(strResponse);
              }
              Assert.AreEqual("hello_world", responseFields[2]);
            }
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
          if (commonResponse.Equals (null)) {
            Assert.Fail("Null response");
          }
          else
          {
            IList<object> responseFields = commonResponse as IList<object>;
            foreach(object item in responseFields)
            {
              response = item.ToString();
              Console.WriteLine("Response:" + response);
              Assert.IsNotEmpty(response);
            }
            Dictionary<string, object> message = (Dictionary<string, object>)responseFields[0];
            foreach(KeyValuePair<String, object> entry in message)
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
        public void IfHereNowIsCalledWithCipherThenItShouldReturnInfo()
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
        public void ThenPresenceShouldReturnCustomUUID()
        {
          Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
          
          Common commonHereNow = new Common();
          commonHereNow.DeliveryStatus = false;
          commonHereNow.Response = null;

          Common commonSubscribe = new Common();
          commonSubscribe.DeliveryStatus = false;
          commonSubscribe.Response = null;

          pubnub.PubnubUnitTest = commonHereNow.CreateUnitTestInstance("WhenAClientIsPresented", "ThenPresenceShouldReturnCustomUUID");;
          pubnub.SessionUUID = "CustomSessionUUIDTest";
          
          string channel = "hello_world";

          pubnub.Subscribe(channel, commonSubscribe.DisplayReturnMessageDummy, commonSubscribe.DisplayReturnMessage, commonSubscribe.DisplayReturnMessage);
            
          //while (!commonSubscribe.DeliveryStatus);
          commonSubscribe.WaitForResponse();

          pubnub.HereNow<string>(channel, commonHereNow.DisplayReturnMessage, commonHereNow.DisplayReturnMessage);

          //while (!commonHereNow.DeliveryStatus);
          commonHereNow.WaitForResponse();
          if (commonHereNow.Response!= null)
          {
#if (USE_JSONFX)
              IList<object> fields = new JsonFXDotNet ().DeserializeToObject (commonHereNow.Response.ToString ()) as IList<object>;
              if (fields [0] != null)
              {
                  bool result = false;
                  Dictionary<string, object> message = (Dictionary<string, object>)fields [0];
                  foreach (KeyValuePair<String, object> entry in message)
                  {
                      Console.WriteLine("value:" + entry.Value + "  " + "key:" + entry.Key);
                      Type valueType = entry.Value.GetType();
                      var expectedType = typeof(string[]);
                      if (valueType.IsArray && expectedType.IsAssignableFrom(valueType))
                      {
                        List<string> uuids = new List<string>(entry.Value as string[]);
                        if(uuids.Contains(pubnub.SessionUUID )){
                            result= true;
                            break;
                        }
                      }
                  }
                  Assert.True(result);
              } 
              else
              {
                Assert.Fail("Null response");
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
              Assert.Fail("Null response");
          }

        }
    }
}

