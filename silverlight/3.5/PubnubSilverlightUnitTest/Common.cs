using System;
using PubNubMessaging.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;

//using JsonFx.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Windows.Threading;


namespace PubnubSilverlight.UnitTest
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
            DispatcherTimer timer = new DispatcherTimer();

            timer.Interval = new TimeSpan(0, 0, 1); // one second
            timer.Start();
            DateTime start = DateTime.UtcNow; 
            DateTime endTime = start.AddSeconds(countdownSeconds); 
            timer.Start();
            timer.Tick += delegate(object sender, EventArgs e) {
                TimeSpan remainingTime = endTime - DateTime.UtcNow;
                if (remainingTime < TimeSpan.Zero)
                    {
                        DeliveryStatus = true;
                    }
            };
         
            while (!DeliveryStatus)
                ;
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
            Console.WriteLine(result.ToString());
        }

        public void DisplayReturnMessage(string result)
        {
            DeliveryStatus = true;
            Response = (object)result;
            Console.WriteLine(result.ToString());
        }

        public long Timestamp(Pubnub pubnub)
        {
            DeliveryStatus = false;

            pubnub.Time(DisplayReturnMessage, DisplayReturnMessage);
            while (!DeliveryStatus)
                ;

            IList<object> fields = Response as IList<object>;
            return Convert.ToInt64(fields [0].ToString());
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
            retMessage = reader.Read<T>(message);
            #else
            retMessage = JsonConvert.DeserializeObject<T>(message);
            #endif
            return (T)retMessage;
        }
        /*public static T DeserializeUsingJSONFx<T>(string message)
        {
            object retMessage;
            var reader = new JsonFx.Json.JsonReader();
            retMessage = reader.Read<T>(message);
            return (T)retMessage;
        }*/

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
            retMessage = writer.Write(message);
            retMessage = ConvertHexToUnicodeChars(retMessage);
            #else
            retMessage = JsonConvert.SerializeObject(message);
            #endif
            return retMessage;
        }

        /*public static string SerializeUsingJSONFx(object message)
        {
            string retMessage;
            var writer = new JsonFx.Json.JsonWriter();
            retMessage = writer.Write(message);
            retMessage = ConvertHexToUnicodeChars(retMessage);
            return retMessage;
        }*/

        /// <summary>
        /// Converts the upper case hex to lower case hex.
        /// </summary>
        /// <returns>The lower case hex.</returns>
        /// <param name="value">Hex Value.</param>
        private static string ConvertHexToUnicodeChars(string value)
        {
            //if(;
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups ["Value"].Value, NumberStyles.HexNumber)).ToString();
                }     
            );
        }
    }
}

