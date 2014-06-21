using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;
using NUnit.Framework;

namespace PubNubMessaging.Tests
{
        public class WhenGetRequestServerTime
        {
                ManualResetEvent mreTime = new ManualResetEvent (false);
                ManualResetEvent mreProxy = new ManualResetEvent (false);
                bool timeReceived = false;
                bool timeReceivedWhenProxy = false;

				[Test]
                public void ThenItShouldReturnTimeStamp ()
                {
						Pubnub pubnub = new Pubnub(
								Common.PublishKey,
								Common.SubscribeKey,
								"",
								"",
								false
						);

						Common common = new Common();
						common.DeliveryStatus = false;
						common.Response = null;

						pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenGetRequestServerTime", "ThenItShouldReturnTimeStamp");;

						string response = "";

						pubnub.Time(common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

						common.WaitForResponse();

						IList<object> fields = common.Response as IList<object>;
						response = fields[0].ToString();
						Console.WriteLine("Response:" + response);
						Assert.False(("0").Equals(response));
                }

				[Test]
				public void ThenItShouldReturnTimeStampSSL ()
				{
					Pubnub pubnub = new Pubnub(
						Common.PublishKey,
						Common.SubscribeKey,
						"",
						"",
						false
					);

					Common common = new Common();
					common.DeliveryStatus = false;
					common.Response = null;

					pubnub.PubnubUnitTest = common.CreateUnitTestInstance("WhenGetRequestServerTime", "ThenItShouldReturnTimeStamp");;

					string response = "";

					pubnub.Time(common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

					common.WaitForResponse();

					IList<object> fields = common.Response as IList<object>;
					response = fields[0].ToString();
					Console.WriteLine("Response:" + response);
					Assert.False(("0").Equals(response));
				}
                private void ReturnTimeStampCallback (string result)
                {
                        if (!string.IsNullOrEmpty (result) && !string.IsNullOrEmpty (result.Trim ())) {
                                Pubnub pubnub = new Pubnub ("demo", "demo", "", "", false);
                                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject (result).ToArray ();
                                if (deserializedMessage is object[]) {
                                        string time = deserializedMessage [0].ToString ();
                                        Int64 nanoTime;
                                        if (time.Length > 2 && Int64.TryParse (time, out nanoTime)) {
                                                timeReceived = true;
                                        }
                                }
                        }
                        mreTime.Set ();
                }

				[Test]
                public void TranslateDateTimeToUnixTime ()
                {
                        Debug.Log ("Running TranslateDateTimeToUnixTime()");
                        //Test for 26th June 2012 GMT
                        DateTime dt = new DateTime (2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
                        long nanoSecondTime = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds (dt);
                        Assert.True ((13406688000000000).Equals (nanoSecondTime));
                }

				[Test]
                public void TranslateUnixTimeToDateTime ()
                {
                        Debug.Log ("Running TranslateUnixTimeToDateTime()");
                        //Test for 26th June 2012 GMT
                        DateTime expectedDate = new DateTime (2012, 6, 26, 0, 0, 0, DateTimeKind.Utc);
                        DateTime actualDate = Pubnub.TranslatePubnubUnixNanoSecondsToDateTime (13406688000000000);
                        Assert.True (expectedDate.ToString ().Equals( actualDate.ToString ()));
                }

                void DummyErrorCallback (string result)
                {
                        Debug.Log ("WhenGetRequestServerTime ErrorCallback = " + result);
                }

                void DummyErrorCallback (PubnubClientError result)
                {
                        Debug.Log ("WhenUnsubscribedToAChannel ErrorCallback = " + result.ToString ());
                }
        }
}
