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
            if (common.Response == null) {
              Assert.Fail("Null response");
            }
            else
            {
                IList<object> responseFields = common.Response as IList<object>;
              foreach (object item in responseFields)
              {
                response = item.ToString();
                Console.WriteLine("Response:" + response);
                //Assert.IsNotEmpty(strResponse);
              }
			  Assert.True (("hello_world").Equals(responseFields[2]));
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
              Assert.NotNull(response);
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

