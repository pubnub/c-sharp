using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    public class WhenDetailedHistoryIsRequested: UUnitTestCase
    {
        ManualResetEvent mreMessageCount10 = new ManualResetEvent(false);
        ManualResetEvent mreMessageCount10ReverseTrue = new ManualResetEvent(false);
        ManualResetEvent mreMessageStartReverseTrue = new ManualResetEvent(false);
        ManualResetEvent mrePublishStartReverseTrue = new ManualResetEvent(false);


        bool message10Received = false;
        bool message10ReverseTrueReceived = false;
        bool messageStartReverseTrue = false;

        int expectedCountAtStartTimeWithReverseTrue=0;
        long startTimeWithReverseTrue = 0;

        [UUnitTest]
        public void DetailHistoryCount10ReturnsRecords()
        {
            Debug.Log("Running DetailHistoryCount10ReturnsRecords()");
            message10Received = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReturnsRecords";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory<string>(channel, 10, DetailedHistoryCount10Callback, DummyErrorCallback);
            mreMessageCount10.WaitOne(310 * 1000);
            UUnitAssert.True(message10Received, "Detailed History Failed");
        }

        void DetailedHistoryCount10Callback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    object[] messages = deserializedMessage[0] as object[];
                    if (messages != null && messages.Length >= 0)
                    {
                        message10Received = true;
                    }
                }
            }

            mreMessageCount10.Set();
        }

        [UUnitTest]
        public void DetailHistoryCount10ReverseTrueReturnsRecords()
        {
            Debug.Log("Running DetailHistoryCount10ReverseTrueReturnsRecords()");
            message10ReverseTrueReceived = false;

            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailHistoryCount10ReverseTrueReturnsRecords";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            pubnub.DetailedHistory<string>(channel, -1, -1, 10, true, DetailedHistoryCount10ReverseTrueCallback, DummyErrorCallback);
            mreMessageCount10ReverseTrue.WaitOne(310 * 1000);
            UUnitAssert.True(message10ReverseTrueReceived, "Detailed History Failed");
        }

        void DetailedHistoryCount10ReverseTrueCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    object[] messages = deserializedMessage[0] as object[];
                    if (messages != null)
                    {
                        if (messages.Length >= 0)
                        {
                            message10ReverseTrueReceived = true;
                        }
                    }
                }
            }

            mreMessageCount10ReverseTrue.Set();
        }

        [UUnitTest]
        public void DetailedHistoryStartWithReverseTrue()
        {
            Debug.Log("Running DetailedHistoryStartWithReverseTrue()");
            bool enableLocalStubTest = false;
            expectedCountAtStartTimeWithReverseTrue = 0;
            messageStartReverseTrue = false;
            Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenDetailedHistoryIsRequested";
            unitTest.TestCaseName = "DetailedHistoryStartWithReverseTrue";
            pubnub.PubnubUnitTest = unitTest;
            if (pubnub.PubnubUnitTest is IPubnubUnitTest && pubnub.PubnubUnitTest.EnableStubTest)
            {
                enableLocalStubTest = true;
            }

            string channel = "hello_my_channel";
            if (enableLocalStubTest)
            {
                startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(new DateTime(2012,12,1));
            }
            else
            {
                startTimeWithReverseTrue = Pubnub.TranslateDateTimeToPubnubUnixNanoSeconds(DateTime.UtcNow);
            }
            
            for (int index = 0; index < 10; index++)
            {
                pubnub.Publish<string>(channel, 
                    string.Format("DetailedHistoryStartTimeWithReverseTrue {0}", index), 
                    DetailedHistorySamplePublishCallback, DummyErrorCallback);
                mrePublishStartReverseTrue.WaitOne(310 * 1000);
            }

            Thread.Sleep(2000);

            pubnub.DetailedHistory<string>(channel, startTimeWithReverseTrue, DetailedHistoryStartWithReverseTrueCallback, DummyErrorCallback, true);
            Thread.Sleep(2000);
            mreMessageStartReverseTrue.WaitOne(310 * 1000);
            UUnitAssert.True(messageStartReverseTrue, "Detailed History with Start and Reverse True Failed");
        }

        private void DetailedHistoryStartWithReverseTrueCallback(string result)
        {
            int actualCountAtStartTimeWithReverseFalse = 0;
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);

                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    object[] messages = deserializedMessage[0] as object[];
                    if (messages != null && messages.Length >= expectedCountAtStartTimeWithReverseTrue)
                    {
                            foreach (object item in messages)
                            {
                                if (item.ToString().Contains("DetailedHistoryStartTimeWithReverseTrue"))
                                {
                                    actualCountAtStartTimeWithReverseFalse++;
                                }
                            }
                            if (actualCountAtStartTimeWithReverseFalse == expectedCountAtStartTimeWithReverseTrue)
                            {
                                messageStartReverseTrue = true;
                            }
                    }
                }
            }
            mreMessageStartReverseTrue.Set();
        }

        private void DetailedHistorySamplePublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                Pubnub pubnub = new Pubnub("demo", "demo", "", "", false);
                object[] deserializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(result).ToArray();
                if (deserializedMessage is object[])
                {
                    int statusCode = Int32.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        expectedCountAtStartTimeWithReverseTrue++;
                    }
                }
            }
            mrePublishStartReverseTrue.Set();
        }
        
        void DummyErrorCallback(string result)
        {
            Debug.Log("WhenDetailedHistoryIsRequested ErrorCallback = " + result);
        }
    }
}
