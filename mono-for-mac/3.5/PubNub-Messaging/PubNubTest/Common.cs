using System;
using PubNubMessaging.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;

#if (USE_JSONFX)
using JsonFx.Json;
#elif (USE_DOTNET_SERIALIZATION)
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
#endif

namespace PubNubMessaging.Tests
{
  public class Common
  {
      public object Response { get; set; }
      public bool DeliveryStatus  { get; set; }

      /// <summary>
      /// Blocks the current thread unit the response is received
      /// or timeout occurs
      /// </summary>
      /// <param name="countdownSeconds">seconds to timeout</param>
      public void WaitForResponse(int countdownSeconds = 30)
      {
          Timer timer = new Timer();
          DateTime start = DateTime.UtcNow; 
          DateTime endTime = start.AddSeconds(countdownSeconds); 
          timer.Enabled = true;
          timer.Start();
          timer.Elapsed += delegate(object sender, ElapsedEventArgs e) {
              TimeSpan remainingTime=endTime-DateTime.UtcNow;
              if(remainingTime<TimeSpan.Zero)
              {
                  timer.Enabled=false; 
                  DeliveryStatus = true;
              }
         };
         
         while (!DeliveryStatus);
         timer.Stop();
      }

      public PubnubUnitTest CreateUnitTestInstance(string testClassName, string testCaseName)
      {
        PubnubUnitTest unitTest = new PubnubUnitTest();
        unitTest.TestClassName = testClassName;
        unitTest.TestCaseName = testCaseName;
        return unitTest;
      }

      public void DisplayErrorMessage(PubnubClientError result)
      {
        //Response = result;
          Console.WriteLine(result.ToString());
      }
      
      public void DisplayReturnMessageDummy(object result)
      {
        //deliveryStatus = true;
        //Response = result;
        Console.WriteLine(result.ToString());
      }

      public void DisplayReturnMessage(object result)
      {
        DeliveryStatus = true;
        Response = result;
      }
      public void DisplayReturnMessage(string result)
      {
        DeliveryStatus = true;
        Response = (object)result;
      }

      public long Timestamp(Pubnub pubnub)
      {
          DeliveryStatus = false;

          pubnub.Time(DisplayReturnMessage, DisplayReturnMessage);
          while (!DeliveryStatus) ;

          IList<object> fields = Response as IList<object>;
          return Convert.ToInt64(fields[0].ToString());
      }
      /// <summary>
      /// Deserialize the specified message using either JSONFX or NEWTONSOFT.JSON.
      /// The functionality is based on the pre-compiler flag
      /// </summary>
      /// <param name="message">Message.</param>
      public static T Deserialize<T>(string message)
      {
        object retMessage;
        #if (USE_JSONFX)
        var reader = new JsonFx.Json.JsonReader();
        retMessage= reader.Read<T>(message);
        #else
        retMessage = JsonConvert.DeserializeObject<T>(message);
        #endif
        return (T)retMessage;
      }

      /// <summary>
      /// Serialize the specified message using either JSONFX or NEWTONSOFT.JSON.
      /// The functionality is based on the pre-compiler flag
      /// </summary>
      /// <param name="message">Message.</param>
      public static string Serialize(object message)
      {
        string retMessage;
        #if (USE_JSONFX)
        var writer = new JsonFx.Json.JsonWriter();
        retMessage= writer.Write(message);
        retMessage = ConvertHexToUnicodeChars(retMessage);
        #else
        retMessage = JsonConvert.SerializeObject(message);
        #endif
        return retMessage;
      }

      /// <summary>
      /// Converts the upper case hex to lower case hex.
      /// </summary>
      /// <returns>The lower case hex.</returns>
      /// <param name="value">Hex Value.</param>
      private static string ConvertHexToUnicodeChars( string value ) {
        //if(;
        return Regex.Replace(
          value,
          @"\\u(?<Value>[a-zA-Z0-9]{4})",
          m => {
          return ((char) int.Parse( m.Groups["Value"].Value, NumberStyles.HexNumber )).ToString();
        }     
        );
      }
  }

  /// <summary>
  /// Custom class for testing the encryption and decryption 
  /// </summary>
  class CustomClass
  {
    public string foo = "hi!"; 
    public int [] bar = {1, 2, 3, 4, 5};
  }

  class PubnubDemoObject
  {
    public double VersionID = 3.4;
    public long Timetoken = 13601488652764619;
    public string OperationName = "Publish";
    public string[] Channels = { "ch1" };
    public PubnubDemoMessage DemoMessage = new PubnubDemoMessage();
    public PubnubDemoMessage CustomMessage = new PubnubDemoMessage("This is a demo message");

    public XmlDocument SampleXml = new PubnubDemoMessage().TryXmlDemo();
  }

  class PubnubDemoMessage
  {
    public string DefaultMessage = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";
    //public string DefaultMessage = "\"";
    public PubnubDemoMessage()
    {
    }

    public PubnubDemoMessage(string message)
    {
      DefaultMessage = message;
    }

    public XmlDocument TryXmlDemo()
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml("<DemoRoot><Person ID='ABCD123'><Name><First>John</First><Middle>P.</Middle><Last>Doe</Last></Name><Address><Street>123 Duck Street</Street><City>New City</City><State>New York</State><Country>United States</Country></Address></Person><Person ID='ABCD456'><Name><First>Peter</First><Middle>Z.</Middle><Last>Smith</Last></Name><Address><Street>12 Hollow Street</Street><City>Philadelphia</City><State>Pennsylvania</State><Country>United States</Country></Address></Person></DemoRoot>");

      return xmlDocument;
    }
  }
}

