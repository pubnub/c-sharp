using System;
using NUnit.Framework;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using PubNubMessaging.Core;
using System.Threading;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenDetailedHistoryIsRequested
    {
        [Test]
        public void ItShouldReturnDetailedHistoryNoStore ()
        {
            Pubnub pubnub = new Pubnub (
                Common.PublishKey,
                Common.SubscribeKey,
                "",
                "",
                false
            );
            string channel = "hello_world_de1";
            string message = "Test Message No Store";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "ItShouldReturnDetailedHistory");

            //publish a test message. 
            pubnub.Publish (channel, message, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            common.DeliveryStatus = false;
            common.Response = null;
            //Thread.Sleep (2000);
            pubnub.DetailedHistory<string> (channel, 1, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
            common.WaitForResponse ();

            ParseResponseNoStore (common.Response.ToString(), message);
            pubnub.EndPendingRequests ();
        }

        public void ParseResponseNoStore (string commonResponse, string message)
        {
            if (commonResponse.Equals (null)) {
                Assert.Fail ("Null response");
            } else {
                Console.WriteLine ("resp:" + commonResponse);
                Assert.IsTrue (!commonResponse.ToString ().Contains (message));
            }        
        }
        [Test]
        public void ItShouldReturnDetailedHistory ()
        {
            GC.Collect ();
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false
                      );
            string channel = "hello_world_de1";
            string message = "Test Message";
      
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "ItShouldReturnDetailedHistory");
      
            //publish a test message. 
            pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      
            common.WaitForResponse ();

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, 1, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
            common.WaitForResponse ();

            ParseResponse (common.Response, 0, 0, message);
            pubnub.EndPendingRequests ();
        }

        public void SendMultipleIntMessages (int messageStart, int messageEnd, string channel, Pubnub pubnub)
        {
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;   

            for (int i = messageStart; i < messageEnd; i++) {
                
                common.DeliveryStatus = false;
                string msg = i.ToString ();
                pubnub.Publish (channel, msg, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

                common.WaitForResponse ();
								
                Console.WriteLine ("Message # " + i.ToString () + " published");
				Thread.Sleep (1000);
            }
        }

        public void ParseResponse (object commonResponse, int messageStart, int messageEnd, string message)
        {
            if (commonResponse.Equals (null)) {
                Assert.Fail ("No response: ");
            } else {
                IList<object> fields = commonResponse as IList<object>;

                if (fields [0] != null) {
                    ParseFields (fields, messageStart, messageEnd, message);
                }
            }        
        }

        public void ParseFields (IList<object> fields, int messageStart, int messageEnd, string message)
        {
            string response = "";

            var myObjectArray = (from item in fields
                                    select item as object).ToArray ();
            IList<object> enumerable = myObjectArray [0] as IList<object>;
            if ((enumerable != null) && (enumerable.Count > 0)) {
                int j = messageStart;
                foreach (object element in enumerable) {
                    response = element.ToString ();
                    if (messageStart != messageEnd) {
                        Console.WriteLine (String.Format ("response :{0} :: j: {1}", response, j));
                        if (j < messageEnd)
                            Assert.AreEqual (j.ToString (), response);
                        j++;
                    } else if (!message.Equals ("")) {
                        Console.WriteLine ("Response:" + response);
                        Assert.AreEqual (message, response);
                    } else {
                        Console.WriteLine ("Response:" + response);
                        Assert.IsNotEmpty (response);
                    }
                }
            } else {
                Assert.Fail ("No response");
            }
        }

        [Test]
        public void DetailedHistoryExample ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false);
            string channel = "hello_world_de2";

            string message = "Test Message";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
            common.WaitForResponse ();

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "DetailHistoryCount10ReturnsRecords");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, 10, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();
            Console.WriteLine ("\n*********** DetailedHistory Messages Received*********** ");

            ParseResponse (common.Response, 0, 0, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void DetailedHistoryDecryptedExample ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "enigma",
                          false);
            string channel = "hello_world_de3";

            string message = "Test Message";

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            pubnub.Publish (channel, message, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
            common.WaitForResponse ();

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "DetailedHistoryDecryptedExample");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, 1, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();
            Console.WriteLine ("\n*********** DetailedHistory Messages Received*********** ");

            ParseResponse (common.Response, 0, 0, message);
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestEncryptedSecretDetailedHistoryParams ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "enigma",
                          false);

            string channel = "hello_world_de4";

            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedSecretDetailedHistoryParams1");

            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = true");

            ParseResponse (common.Response, 0, totalMessages / 2, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedSecretDetailedHistoryParams2");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = false");

            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedSecretDetailedHistoryParams3");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestUnencryptedSecretDetailedHistoryParams ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "",
                          false);

            string channel = "hello_world_de5";

            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedSecretDetailedHistoryParams1");

            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = true");

            ParseResponse (common.Response, 0, totalMessages / 2, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedSecretDetailedHistoryParams2");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = false");

            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedSecretDetailedHistoryParams3");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestUnencryptedDetailedHistoryParams ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false);
      
            string channel = "hello_world_de6";
      
            int totalMessages = 10;
      
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        
      
            long starttime = common.Timestamp (pubnub);
      
            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);
      
            long midtime = common.Timestamp (pubnub);
      
            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);
      
            long endtime = common.Timestamp (pubnub);
      
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistoryParams1");
      
            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      
            common.WaitForResponse ();
      
            Console.WriteLine ("DetailedHistory with start & reverse = true");
      
            ParseResponse (common.Response, 0, totalMessages / 2, "");
      
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistoryParams2");
      
            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      
            common.WaitForResponse ();
      
            Console.WriteLine ("DetailedHistory with start & reverse = false");
      
            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");
      
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistoryParams3");
      
            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      
            common.WaitForResponse ();
      
            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");
      
            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestEncryptedDetailedHistoryParams ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "enigma",
                          false);
      
            string channel = "hello_world_de7";

            int totalMessages = 10;
      
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        
      
            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistoryParams1");
      
            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      
            common.WaitForResponse ();
      
            Console.WriteLine ("DetailedHistory with start & reverse = true");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
      
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistoryParams2");
      
            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      
            common.WaitForResponse ();
      
            Console.WriteLine ("DetailedHistory with start & reverse = false");

            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");
      
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistoryParams3");
      
            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
      
            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestUnencryptedDetailedHistory ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          false);

            string channel = "hello_world_de8";
            int totalMessages = 10;
      
            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);
            common.WaitForResponse ();
      
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistory");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, totalMessages, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
            common.WaitForResponse ();
      
            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestEncryptedDetailedHistory ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "enigma",
                          false);
            string channel = "hello_world_de9";
      
            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages, totalMessages / 2, channel, pubnub);

            long endtime = common.Timestamp (pubnub);
            common.WaitForResponse ();
      
            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistory");
      
            common.Response = null;
            common.DeliveryStatus = false;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, totalMessages, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();
            Console.WriteLine ("\n*********** DetailedHistory Messages Received*********** ");

            ParseResponse (common.Response, 0, totalMessages, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestEncryptedSecretDetailedHistoryParamsSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "enigma",
                          true);

            string channel = "hello_world_de10";

            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedSecretDetailedHistoryParams1");

            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = true");

            ParseResponse (common.Response, 0, totalMessages / 2, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedSecretDetailedHistoryParams2");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = false");

            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedSecretDetailedHistoryParams3");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestUnencryptedSecretDetailedHistoryParamsSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          Common.SecretKey,
                          "",
                          true);

            string channel = "hello_world_de11";

            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedSecretDetailedHistoryParams1");

            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = true");

            ParseResponse (common.Response, 0, totalMessages / 2, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedSecretDetailedHistoryParams2");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = false");

            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedSecretDetailedHistoryParams3");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestUnencryptedDetailedHistoryParamsSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          true);

            string channel = "hello_world_de12";

            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistoryParams1");

            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = true");

            ParseResponse (common.Response, 0, totalMessages / 2, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistoryParams2");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = false");

            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistoryParams3");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestEncryptedDetailedHistoryParamsSSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "enigma",
                          true);

            string channel = "hello_world_de13";

            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;        

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistoryParams1");

            common.DeliveryStatus = false;
            common.Response = null;
            Console.WriteLine ("DetailedHistory with start & end");
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, starttime, midtime, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = true");

            ParseResponse (common.Response, 0, totalMessages / 2, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistoryParams2");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, true, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("DetailedHistory with start & reverse = false");

            ParseResponse (common.Response, totalMessages / 2, totalMessages, "");

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistoryParams3");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, midtime, -1, totalMessages / 2, false, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages / 2, "");
            pubnub.EndPendingRequests ();

        }

        [Test]
        public void TestUnencryptedDetailedHistorySSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "",
                          true);

            string channel = "hello_world_de14";
            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages / 2, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages / 2, totalMessages, channel, pubnub);

            long endtime = common.Timestamp (pubnub);
            common.WaitForResponse ();

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestUnencryptedDetailedHistory");

            common.DeliveryStatus = false;
            common.Response = null;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, totalMessages, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);
            common.WaitForResponse ();

            Console.WriteLine ("\n******* DetailedHistory Messages Received ******* ");

            ParseResponse (common.Response, 0, totalMessages, "");
            pubnub.EndPendingRequests ();
        }

        [Test]
        public void TestEncryptedDetailedHistorySSL ()
        {
            Pubnub pubnub = new Pubnub (
                          Common.PublishKey,
                          Common.SubscribeKey,
                          "",
                          "enigma",
                          true);
            string channel = "hello_world_de15";

            int totalMessages = 10;

            Common common = new Common ();
            common.DeliveryStatus = false;
            common.Response = null;

            long starttime = common.Timestamp (pubnub);

            SendMultipleIntMessages (0, totalMessages, channel, pubnub);

            long midtime = common.Timestamp (pubnub);

            SendMultipleIntMessages (totalMessages, totalMessages / 2, channel, pubnub);

            long endtime = common.Timestamp (pubnub);
            common.WaitForResponse ();

            pubnub.PubnubUnitTest = common.CreateUnitTestInstance ("WhenDetailedHistoryIsRequested", "TestEncryptedDetailedHistory");

            common.Response = null;
            common.DeliveryStatus = false;
            Thread.Sleep (1000);
            pubnub.DetailedHistory (channel, totalMessages, common.DisplayReturnMessage, common.DisplayReturnMessageDummy);

            common.WaitForResponse ();
            Console.WriteLine ("\n*********** DetailedHistory Messages Received*********** ");

            ParseResponse (common.Response, 0, totalMessages, "");
            pubnub.EndPendingRequests ();
        }
    }
}

