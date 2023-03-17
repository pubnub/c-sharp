﻿using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenFileIsRequested : TestHarness
    {
        private static ManualResetEvent mre = new ManualResetEvent(false);
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);

        private static bool receivedMessage = false;
        private static bool receivedGrantMessage = false;

        private static string currentUnitTestCase = "";
        //private static string channelGroupName = "hello_my_group";
        private static string channelName = "hello_my_channel";
        private static string authKey = "myauth";

        private static Pubnub pubnub;
        private static Server server;

        [SetUp]
        public static void Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();

            if (!PubnubCommon.PAMServerSideGrant)
            {
                return;
            }

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Secure = false
            };

            pubnub = createPubNubInstance(config);

            pubnub.Grant().Channels(new[] { channelName }).AuthKeys(new[] { authKey }).Read(true).Write(true).Manage(true).Delete(true).Update(true).Get(true).TTL(20)
                .Execute(new PNAccessManagerGrantResultExt((r,s)=> 
                { 
                    if (r != null)
                    {
                        receivedGrantMessage = true;
                    }
                    grantManualEvent.Set();
                }));

            Thread.Sleep(1000);

            grantManualEvent.WaitOne(2000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedGrantMessage, "WhenFileUploadIsRequested Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            server.Stop();
        }

        [Test]
        public static void ThenSendFileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;
            bool receivedEvent = false;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNFileEventResult eventResult)
                {
                    receivedEvent = true;
                    System.Diagnostics.Debug.WriteLine("FILE EVENT: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                },
                delegate (Pubnub pnObj, PNStatus status)
                {

                }
                );

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);
            pubnub.AddListener(eventListener);

            mre = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channelName }).Execute();
            mre.WaitOne(2000);

            mre = new ManualResetEvent(false);

            string fileId = "";
            string fileName = "";
            receivedMessage = false;
            string targetFileUpload = @"fileupload.txt";
            pubnub.SendFile().Channel(channelName).File(targetFileUpload).CipherKey("enigma").Message("This is my sample file")
                .Execute(new PNFileUploadResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine("SendFile result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        fileId = result.FileId;
                        fileName = result.FileName;
                        receivedMessage = true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("SendFile failed = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    }
                    mre.Set();
                }));
            Thread.Sleep(1000);
            mre.WaitOne(3 * 1000 * 60);

            if (receivedMessage)
            {
                receivedMessage = false;
                mre = new ManualResetEvent(false);
                pubnub.ListFiles().Channel(channelName)
                    .Execute(new PNListFilesResultExt((result, status) =>
                    {
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine("ListFiles result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                            receivedMessage = true;
                        }
                        mre.Set();
                    }));
                Thread.Sleep(1000);
                mre.WaitOne(2 * 1000 * 60);
            }

            if (receivedMessage)
            {
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072; //Need this line for .net 3.5/4.0/4.5
                receivedMessage = false;
                mre = new ManualResetEvent(false);
                pubnub.DownloadFile().Channel(channelName).FileId(fileId).FileName(fileName).Execute(new PNDownloadFileResultExt((result, status) =>
                {
                    if (result != null && result.FileBytes != null && result.FileBytes.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("DownloadFile result = " + result.FileBytes.Length);
                        receivedMessage = true;
                    }
                    mre.Set();
                }));
                mre.WaitOne(2 * 1000 * 60);
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                mre = new ManualResetEvent(false);
                pubnub.DeleteFile().Channel(channelName).FileId(fileId).FileName(fileName)
                    .Execute(new PNDeleteFileResultExt((result, status) =>
                    {
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine("DeleteFile result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                            receivedMessage = true;
                        }
                        mre.Set();
                    }));
                Thread.Sleep(1000);
                mre.WaitOne(2 * 1000 * 60);

            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage && receivedEvent, "WhenFileIsRequested -> TheSendFileShouldReturnSuccess failed.");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncSendFileShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncSendFileShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            receivedMessage = false;
            bool receivedEvent = false;

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate (Pubnub pnObj, PNFileEventResult eventResult)
                {
                    receivedEvent = true;
                    System.Diagnostics.Debug.WriteLine("FILE EVENT: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                },
                delegate (Pubnub pnObj, PNStatus status)
                {

                }
                );

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);
            pubnub.AddListener(eventListener);

            mre = new ManualResetEvent(false);
            pubnub.Subscribe<string>().Channels(new string[] { channelName }).Execute();
            mre.WaitOne(2000);

            mre = new ManualResetEvent(false);

            string fileId = "";
            string fileName = "";
            receivedMessage = false;
            string targetFileUpload = @"fileupload.txt";
            Dictionary<string, object> myInternalMsg = new Dictionary<string, object>();
            myInternalMsg.Add("color", "red");
            myInternalMsg.Add("name", "John Doe");
#if NET40
            PNResult<PNFileUploadResult> sendFileResult = Task.Factory.StartNew(async () => await pubnub.SendFile().Channel(channelName).File(targetFileUpload).Message(myInternalMsg).ExecuteAsync()).Result.Result;
#else
            PNResult<PNFileUploadResult> sendFileResult = await pubnub.SendFile().Channel(channelName).File(targetFileUpload).Message(myInternalMsg).ExecuteAsync();
#endif
            if (sendFileResult.Result != null && !string.IsNullOrEmpty(sendFileResult.Result.FileId) && sendFileResult.Result.Timetoken > 0)
            {
                System.Diagnostics.Debug.WriteLine("SendFile result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(sendFileResult.Result));
                fileId = sendFileResult.Result.FileId;
                fileName = sendFileResult.Result.FileName;
                receivedMessage = true;
            }

            receivedMessage = false;
            mre = new ManualResetEvent(false);
            pubnub.ListFiles().Channel(channelName)
                .Execute(new PNListFilesResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine("ListFiles result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        receivedMessage = true;
                    }
                    mre.Set();
                }));
            Thread.Sleep(1000);
            mre.WaitOne(2 * 1000 * 60);


            if (receivedMessage)
            {
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072; //Need this line for .net 3.5/4.0/4.5
                receivedMessage = false;
                mre = new ManualResetEvent(false);
                pubnub.DownloadFile().Channel(channelName).FileId(fileId).FileName(fileName).Execute(new PNDownloadFileResultExt((result, status) =>
                {
                    if (result != null && result.FileBytes != null && result.FileBytes.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("DownloadFile result = " + result.FileBytes.Length);
                        receivedMessage = true;
                    }
                    mre.Set();
                }));
                mre.WaitOne(2 * 1000 * 60);
            }

            if (receivedMessage)
            {
                receivedMessage = false;
                mre = new ManualResetEvent(false);
                pubnub.DeleteFile().Channel(channelName).FileId(fileId).FileName(fileName)
                    .Execute(new PNDeleteFileResultExt((result, status) =>
                    {
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine("DeleteFile result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                            receivedMessage = true;
                        }
                        mre.Set();
                    }));
                Thread.Sleep(1000);
                mre.WaitOne(2 * 1000 * 60);

            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage && receivedEvent, "WhenFileIsRequested -> ThenWithAsyncSendFileShouldReturnSuccess failed.");
        }

        //[Test]
        public static void ThenDownloadFileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            mre = new ManualResetEvent(false);
            string fileId = "b0a5c0df-7523-432e-8ea9-01567c93da7d";
            string fileName = "pandu_test.gif";

            receivedMessage = false;
            pubnub.DownloadFile().Channel(channelName).FileId(fileId).FileName(fileName).CipherKey("enigma").Execute(new PNDownloadFileResultExt((result, status) =>
            {
                if (result != null)
                {
                    //result.SaveToLocal(@"C:\Pandu\temp\new\output\hi_file.gif");
                    //result.SaveToLocal(@"C:\Pandu\temp\new\");
                    result.SaveFileToLocal("what_is_hi_file.gif");
                    receivedMessage = true;
                }
                mre.Set();
            }));
            Thread.Sleep(1000);
            mre.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenListFilesShouldReturnSuccess failed.");

        }

        [Test]
        public static void ThenGetFileUrlShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            mre = new ManualResetEvent(false);
            string fileId = "bc03db55-6345-4a0f-aa58-beac970b2c5b";
            string fileName = "whoami.txt";

            receivedMessage = false;
            pubnub.GetFileUrl().Channel(channelName).FileId(fileId).FileName(fileName).Execute(new PNFileUrlResultExt((result, status) =>
            {
                if (result != null)
                {
                    System.Diagnostics.Debug.WriteLine(result.Url);
                    receivedMessage = true;
                }
                mre.Set();
            }));
            Thread.Sleep(1000);
            mre.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenListFilesShouldReturnSuccess failed.");

        }


        [Test]
        public static void ThenListFilesShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            receivedMessage = false;
            mre = new ManualResetEvent(false);
            pubnub.ListFiles().Channel(channelName)
                .Execute(new PNListFilesResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine("result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        receivedMessage = true;
                    }
                    mre.Set();
                }));
            Thread.Sleep(1000);
            mre.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenListFilesShouldReturnSuccess failed.");

        }


        [Test]
        public static void ThenDeleteFileShouldReturnSuccess()
        {
            server.ClearRequests();

            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            receivedMessage = false;
            PNResult<PNListFilesResult> listFilesResponse = pubnub.ListFiles().Channel(channelName).ExecuteAsync().Result;
            if (listFilesResponse.Result != null && listFilesResponse.Result.FilesList != null && listFilesResponse.Result.FilesList.Count > 0 && !listFilesResponse.Status.Error)
            {
                List<PNFileResult> filesList = listFilesResponse.Result.FilesList;
                foreach (var file in filesList)
                {
                    PNResult<PNDeleteFileResult> deleteFileResponse = pubnub.DeleteFile().Channel(channelName).FileId(file.Id).FileName(file.Name).ExecuteAsync().Result;
                    PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
                    if (deleteFileResult != null)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("File Id={0}, Name={1} => deleted successfully", file.Id, file.Name));
                    }
                }
                receivedMessage = true;
            }
            else
            {
                PNResult<PNDeleteFileResult> deleteFileResponse = pubnub.DeleteFile().Channel(channelName).FileId("test_file_id").FileName("test_file_name.test").ExecuteAsync().Result;
                PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
                if (deleteFileResult != null)
                {
                    System.Diagnostics.Debug.WriteLine("File Id=test_file_id, Name=test_file_name.test => deleted successfully");
                    receivedMessage = true;
                }
            }


            mre = new ManualResetEvent(false);
            receivedMessage = false;
            pubnub.DeleteFile().Channel(channelName).FileId("8f83d951-7850-40fb-9688-d2d825b14722").FileName("word_test.txt")
                .Execute(new PNDeleteFileResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine("result = " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        receivedMessage = true;
                    }
                    mre.Set();
                }));
            Thread.Sleep(1000);
            mre.WaitOne();

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenListFilesShouldReturnSuccess failed.");

        }

        [Test]
#if NET40
        public static void ThenWithAsyncDeleteFileShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncDeleteFileShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CipherKey = "enigma",
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            pubnub = createPubNubInstance(config);

            receivedMessage = false;
#if NET40
            PNResult<PNListFilesResult> listFilesResponse = Task.Factory.StartNew(async () => await pubnub.ListFiles().Channel(channelName).ExecuteAsync()).Result.Result;
#else
            PNResult<PNListFilesResult> listFilesResponse = await pubnub.ListFiles().Channel(channelName).ExecuteAsync();
#endif
            if (listFilesResponse.Result != null && listFilesResponse.Result.FilesList != null && listFilesResponse.Result.FilesList.Count > 0 && !listFilesResponse.Status.Error)
            {
                List<PNFileResult> filesList = listFilesResponse.Result.FilesList;
                foreach (var file in filesList)
                {
#if NET40
                    PNResult<PNDeleteFileResult> deleteFileResponse = Task.Factory.StartNew(async () => await pubnub.DeleteFile().Channel(channelName).FileId(file.Id).FileName(file.Name).ExecuteAsync()).Result.Result;
#else
                    PNResult<PNDeleteFileResult> deleteFileResponse = await pubnub.DeleteFile().Channel(channelName).FileId(file.Id).FileName(file.Name).ExecuteAsync();
#endif
                    PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
                    if (deleteFileResult != null)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("File Id={0}, Name={1} => deleted successfully", file.Id, file.Name));
                    }
                }
                receivedMessage = true;
            }
            else
            {
#if NET40
                PNResult<PNDeleteFileResult> deleteFileResponse = Task.Factory.StartNew(async () => await pubnub.DeleteFile().Channel(channelName).FileId("test_file_id").FileName("test_file_name.test").ExecuteAsync()).Result.Result;
#else
                PNResult<PNDeleteFileResult> deleteFileResponse = await pubnub.DeleteFile().Channel(channelName).FileId("test_file_id").FileName("test_file_name.test").ExecuteAsync();
#endif
                PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
                if (deleteFileResult != null)
                {
                    System.Diagnostics.Debug.WriteLine("File Id=test_file_id, Name=test_file_name.test => deleted successfully");
                    receivedMessage = true;
                }
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenWithAsyncDeleteFileShouldReturnSuccess failed.");

        }
    }
}
