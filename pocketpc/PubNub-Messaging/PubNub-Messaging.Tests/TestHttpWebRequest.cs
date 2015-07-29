using System;

using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class TestHttpWebRequest
    {
        ManualResetEvent mreMain = new ManualResetEvent(false);
        ManualResetEvent mre = new ManualResetEvent(false);
        ManualResetEvent mreSub = new ManualResetEvent(false);
        ManualResetEvent mreWCSub = new ManualResetEvent(false);

        [Test]
        public void RunMyTestsOne()
        {
            mreMain = new ManualResetEvent(false);

            Thread t1 = new Thread(() => RunSubscribeTest(0));
            t1.Start();

            Thread t2 = new Thread(() =>
                        {
                            RunTimeTest();
                            RunPublishTest1();
                        });
            t2.Start();

            Thread t3 = new Thread(() =>
            {
                RunDetailedHistoryTest();
                RunUnsubscribeTest();
            });
            t3.Start();

            Thread t4 = new Thread(() => RunWCSubscribeTest(0));
            t4.Start();

            mreMain.WaitOne(50 * 1000, false);
            
        }

        [Test]
        public void RunMyTestsTwo()
        {
            mreMain = new ManualResetEvent(false);

            Thread t1 = new Thread(() => RunSubscribeTest(0));
            t1.Start();

            Thread t2 = new Thread(() =>
            {
                RunTimeTest();
                RunPublishTest1();
            });
            t2.Start();

            Thread t3 = new Thread(() =>
            {
                RunDetailedHistoryTest();
                RunUnsubscribeTest();
            });
            t3.Start();

            Thread t4 = new Thread(() => RunWCSubscribeTest(0));
            t4.Start();

            mreMain.WaitOne(50 * 1000, false);

        }

        public void RunSubscribeTest(long timetoken)
        {
            string req = string.Format("http://pubsub.pubnub.com/subscribe/demo-36/hello_my_channel/0/{0}?uuid=myuuid", timetoken);
            //mreSub = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultSubCallback, ResponseType.Subscribe);
            //mreSub.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunSubscribeTest completed");
        }
        public void RunWCSubscribeTest(long timetoken)
        {
            string req = string.Format("http://pubsub.pubnub.com/subscribe/demo-36/foo.*/0/{0}?uuid=myuuid", timetoken);
            //mreWCSub = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultWCSubCallback, ResponseType.Subscribe);
            //mreWCSub.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunWCSubscribeTest completed");
        }

        public void RunUnsubscribeTest()
        {
            string req = "http://pubsub.pubnub.com/v2/presence/sub_key/demo-36/channel/hello_my_channel/leave?uuid=myuuid";
            //mre = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultCallback, ResponseType.Leave);
            //mre.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunUnsubscribeTest completed");
        }

        public void RunWCUnsubscribeTest()
        {
            string req = "http://pubsub.pubnub.com/v2/presence/sub_key/demo-36/channel/foo.*/leave?uuid=myuuid";
            //mre = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultCallback, ResponseType.Leave);
            //mre.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunWCUnsubscribeTest completed");
        }

        public void RunPublishTest1()
        {
            string msg = JsonConvert.SerializeObject(DateTime.Now.ToString()); //%22my%20favorite%20message%22
            string req = string.Format("http://pubsub.pubnub.com/publish/demo-36/demo-36/0/hello_my_channel/0/{0}",msg);
            //mre = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultCallback, ResponseType.Publish);
            //mre.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunPublishTest1 completed");
        }
        public void RunPublishTest2()
        {
            string msg = JsonConvert.SerializeObject(DateTime.Now.ToString()); //%22my%20favorite%20message%22
            string req = string.Format("http://pubsub.pubnub.com/publish/demo-36/demo-36/0/foo.a/0/{0}", msg);
            //mre = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultCallback, ResponseType.Publish);
            //mre.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunPublishTest2 completed");
        }

        public void RunDetailedHistoryTest()
        {
            string req = "http://pubsub.pubnub.com/v2/history/sub-key/demo-36/channel/hello_my_channel?count=10&uuid=myuuid";
            //mre = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultCallback, ResponseType.DetailedHistory);
            //mre.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunDetailedHistoryTest completed");
        }

        public void RunTimeTest()
        {
            string req = "http://pubsub.pubnub.com/time/0";
            //mre = new ManualResetEvent(false);
            RunRequest(new Uri(req), ResultCallback, ResponseType.Time);
            //mre.WaitOne(310 * 1000, false);
            System.Diagnostics.Debug.WriteLine("RunTimeTest completed");
        }

        private void RunRequest(Uri uri, Action<string> callback, ResponseType type)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Headers["UserAgent"] = "ua_string=(PubNub-csharp/UnitTest)";
            req.Headers["Cache-Control"] = "no-cache";
            req.Headers["Pragma"] = "no-cache";
            req.Timeout = 310 * 1000;
            req.Pipelined = true;

            MyRequestState requestState = new MyRequestState();
            requestState.MyRequest = req;
            requestState.Callback = callback;
            requestState.Type = type;

            IAsyncResult asyncResult = req.BeginGetResponse(new AsyncCallback(UrlProcessResponseCallback), requestState);
            
        }

        private object _lockUrlProcessCallback = new object();
        private void UrlProcessResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                // State of request is asynchronous.
                MyRequestState myRequestState = (MyRequestState)asynchronousResult.AsyncState;
                HttpWebRequest myHttpWebRequest = myRequestState.MyRequest;
                using (HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult))
                {
                    string jsonString = "";
                    myRequestState.MyResponse = myHttpWebResponse;

                    Stream stream = myRequestState.MyResponse.GetResponseStream();
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        stream.Flush();
                        jsonString = streamReader.ReadToEnd();
                        streamReader.Close();
                        myRequestState.Callback(jsonString);
                    }
                    stream.Close();
                    System.Diagnostics.Debug.WriteLine(string.Format("@@ ** {0} ** Req Uri = {1} => JSON = {2}", myRequestState.Type.ToString(), myRequestState.MyResponse.ResponseUri, jsonString));
                    //System.Diagnostics.Debug.WriteLine("JSON = " + jsonString);
                }


                // Read the response into a Stream object.

                // Begin the Reading of the contents of the HTML page and print it to the console.
                //IAsyncResult asynchronousInputRead = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
                return;
            }
            catch (WebException e)
            {
                Console.WriteLine("\nRespCallback Exception raised!");
                Console.WriteLine("\nMessage:{0}", e.Message);
                Console.WriteLine("\nStatus:{0}", e.Status);
            }
        }

        //private static void ReadCallBack(IAsyncResult asyncResult)
        //{
        //    try
        //    {

        //        RequestState myRequestState = (RequestState)asyncResult.AsyncState;
        //        Stream responseStream = myRequestState.streamResponse;
        //        int read = responseStream.EndRead(asyncResult);
        //        // Read the HTML page and then print it to the console. 
        //        if (read > 0)
        //        {
        //            myRequestState.requestData.Append(Encoding.ASCII.GetString(myRequestState.BufferRead, 0, read));
        //            IAsyncResult asynchronousResult = responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
        //            return;
        //        }
        //        else
        //        {
        //            Console.WriteLine("\nThe contents of the Html page are : ");
        //            if (myRequestState.requestData.Length > 1)
        //            {
        //                string stringContent;
        //                stringContent = myRequestState.requestData.ToString();
        //                Console.WriteLine(stringContent);
        //            }
        //            Console.WriteLine("Press any key to continue..........");
        //            Console.ReadLine();

        //            responseStream.Close();
        //        }

        //    }
        //    catch (WebException e)
        //    {
        //        Console.WriteLine("\nReadCallBack Exception raised!");
        //        Console.WriteLine("\nMessage:{0}", e.Message);
        //        Console.WriteLine("\nStatus:{0}", e.Status);
        //    }
        //    allDone.Set();

        //}

        void ResultCallback(string result)
        {
            System.Diagnostics.Debug.WriteLine(result);
            //mre.Set();
        }

        void ResultSubCallback(string result)
        {
            System.Diagnostics.Debug.WriteLine(result);
            //mreSub.Set();
            List<object> lstObject = JsonConvert.DeserializeObject<List<object>>(result);
            long tt = Int64.Parse(lstObject[1].ToString());
            RunSubscribeTest(tt);
            //Thread.Sleep(2000);
            RunPublishTest1();
            //Thread.Sleep(2000);
            RunPublishTest1();
            RunUnsubscribeTest();
        }

        void ResultWCSubCallback(string result)
        {
            System.Diagnostics.Debug.WriteLine(result);
            //mreWCSub.Set();
            List<object> lstObject = JsonConvert.DeserializeObject<List<object>>(result);
            long tt = Int64.Parse(lstObject[1].ToString());
            RunWCSubscribeTest(tt);
            //Thread.Sleep(2000);
            RunPublishTest2();
            //Thread.Sleep(2000);
            RunPublishTest2();
            RunWCUnsubscribeTest();
        }

    }

    public class MyRequestState
    {
        public MyRequestState()
        {
            MyRequest = null;
            MyResponse = null;
            Callback = null;
        }

        public HttpWebRequest MyRequest;
        public HttpWebResponse MyResponse;
        public Action<string> Callback;
        public ResponseType Type;
    }

    public enum ResponseType
    {
        None,
        Time,
        Publish,
        DetailedHistory,
        Subscribe,
        Presence,
        Leave
    }
}
