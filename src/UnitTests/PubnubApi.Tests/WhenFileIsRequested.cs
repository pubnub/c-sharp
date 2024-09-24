using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using System.Collections.Generic;
using MockServer;
using System.Threading.Tasks;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenFileIsRequested : TestHarness
    {
        private static string channelName = "hello_my_channel";
        private static string token;

        private static Pubnub pubnub;
        private static Server server;

        [SetUp]
        public static async Task Init()
        {
            UnitTestLog unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = Server.Instance();
            MockServer.LoggingMethod.MockServerLog = unitLog;
            if (PubnubCommon.EnableStubTest)
            {
                server.Start();
            }

            if (!PubnubCommon.PAMServerSideGrant)
            {
                return;
            }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid_file_tests"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Secure = false
            };

            pubnub = createPubNubInstance(config);

            var grantResult = await pubnub.GrantToken().TTL(20).AuthorizedUuid(config.UserId).Resources(
                new PNTokenResources()
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {
                            channelName, new PNTokenAuthValues()
                            {
                                Read = true,
                                Write = true,
                                Create = true,
                                Get = true,
                                Delete = true,
                                Join = true, 
                                Update = true, 
                                Manage = true
                            }
                        }
                    }
                }).ExecuteAsync();
            
            await Task.Delay(3000);

            token = grantResult.Result?.Token;
            
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(grantResult.Status.Error == false && grantResult.Result != null,
                "WhenFileUploadIsRequested Grant access failed.");
        }

        [TearDown]
        public static void Exit()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }

            server.Stop();
        }

        [Test]
        public static void ThenSendAndDeleteFileShouldReturnSuccess()
        {
            server.ClearRequests();

            var eventReset = new ManualResetEvent(false);

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate(Pubnub pnObj, PNFileEventResult eventResult)
                {
                    eventReset.Set();
                    System.Diagnostics.Debug.WriteLine("FILE EVENT: " +
                                                       pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                },
                delegate(Pubnub pnObj, PNStatus status) { }
            );

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid_file_tests"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(token);
            
            pubnub.AddListener(eventListener);


            pubnub.Subscribe<string>().Channels(new string[] { channelName }).Execute();

            var messageReset = new ManualResetEvent(false);

            string fileId = "";
            string fileName = "";

            string targetFileUpload = @"fileupload.txt";
            pubnub.SendFile().Channel(channelName).File(targetFileUpload).CipherKey("enigma")
                .Message("This is my sample file")
                .Execute(new PNFileUploadResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine("SendFile result = " +
                                                           pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        fileId = result.FileId;
                        fileName = result.FileName;
                        messageReset.Set();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("SendFile failed = " +
                                                           pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                    }
                }));
            var receivedMessage = messageReset.WaitOne(3 * 1000 * 60);

            if (receivedMessage)
            {
                messageReset = new ManualResetEvent(false);
                pubnub.ListFiles().Channel(channelName)
                    .Execute(new PNListFilesResultExt((result, status) =>
                    {
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine("ListFiles result = " +
                                                               pubnub.JsonPluggableLibrary
                                                                   .SerializeToJsonString(result));
                            messageReset.Set();
                        }
                    }));
                receivedMessage = messageReset.WaitOne(2 * 1000 * 60);
            }

            if (receivedMessage)
            {
                System.Net.ServicePointManager.SecurityProtocol =
                    (System.Net.SecurityProtocolType)3072; //Need this line for .net 3.5/4.0/4.5
                messageReset = new ManualResetEvent(false);
                pubnub.DownloadFile().Channel(channelName).FileId(fileId).FileName(fileName).Execute(
                    new PNDownloadFileResultExt((result, status) =>
                    {
                        if (result != null && result.FileBytes != null && result.FileBytes.Length > 0)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "DownloadFile result = " + result.FileBytes.Length);
                            messageReset.Set();
                        }
                    }));
                receivedMessage = messageReset.WaitOne(2 * 1000 * 60);
            }

            if (receivedMessage)
            {
                messageReset = new ManualResetEvent(false);
                pubnub.DeleteFile().Channel(channelName).FileId(fileId).FileName(fileName)
                    .Execute(new PNDeleteFileResultExt((result, status) =>
                    {
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine("DeleteFile result = " +
                                                               pubnub.JsonPluggableLibrary
                                                                   .SerializeToJsonString(result));
                            messageReset.Set();
                        }
                    }));
                receivedMessage = messageReset.WaitOne(2 * 1000 * 60);
            }

            var receivedEvent = false;
            if (receivedMessage)
            {
                receivedEvent = eventReset.WaitOne(2 * 1000 * 60);
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedEvent);
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> TheSendFileShouldReturnSuccess failed.");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncSendFileShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncSendFileShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            var eventReset = new ManualResetEvent(false);

            SubscribeCallbackExt eventListener = new SubscribeCallbackExt(
                delegate(Pubnub pnObj, PNFileEventResult eventResult)
                {
                    eventReset.Set();
                    System.Diagnostics.Debug.WriteLine("FILE EVENT: " +
                                                       pubnub.JsonPluggableLibrary.SerializeToJsonString(eventResult));
                },
                delegate(Pubnub pnObj, PNStatus status) { }
            );

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid_file_tests"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(token);
            pubnub.AddListener(eventListener);

            pubnub.Subscribe<string>().Channels(new string[] { channelName }).Execute();

            string fileId = "";
            string fileName = "";
            string targetFileUpload = @"fileupload.txt";
            Dictionary<string, object> myInternalMsg = new Dictionary<string, object>();
            myInternalMsg.Add("color", "red");
            myInternalMsg.Add("name", "John Doe");
#if NET40
            PNResult<PNFileUploadResult> sendFileResult =
 Task.Factory.StartNew(async () => await pubnub.SendFile().Channel(channelName).File(targetFileUpload).Message(myInternalMsg).ExecuteAsync()).Result.Result;
#else
            PNResult<PNFileUploadResult> sendFileResult = await pubnub.SendFile().Channel(channelName)
                .File(targetFileUpload).Message(myInternalMsg).ExecuteAsync();
#endif
            if (sendFileResult.Result != null && !string.IsNullOrEmpty(sendFileResult.Result.FileId) &&
                sendFileResult.Result.Timetoken > 0)
            {
                System.Diagnostics.Debug.WriteLine("SendFile result = " +
                                                   pubnub.JsonPluggableLibrary.SerializeToJsonString(sendFileResult
                                                       .Result));
                fileId = sendFileResult.Result.FileId;
                fileName = sendFileResult.Result.FileName;
            }

            var messageReset = new ManualResetEvent(false);
            pubnub.ListFiles().Channel(channelName)
                .Execute(new PNListFilesResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine("ListFiles result = " +
                                                           pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        messageReset.Set();
                    }
                }));
            var receivedMessage = messageReset.WaitOne(2 * 1000 * 60);

            if (receivedMessage)
            {
                System.Net.ServicePointManager.SecurityProtocol =
                    (System.Net.SecurityProtocolType)3072; //Need this line for .net 3.5/4.0/4.5
                receivedMessage = false;
                messageReset = new ManualResetEvent(false);
                pubnub.DownloadFile().Channel(channelName).FileId(fileId).FileName(fileName).Execute(
                    new PNDownloadFileResultExt((result, status) =>
                    {
                        if (result != null && result.FileBytes != null && result.FileBytes.Length > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("DownloadFile result = " + result.FileBytes.Length);
                            messageReset.Set();
                        }
                    }));
                receivedMessage = messageReset.WaitOne(2 * 1000 * 60);
            }

            if (receivedMessage)
            {
                messageReset = new ManualResetEvent(false);
                pubnub.DeleteFile().Channel(channelName).FileId(fileId).FileName(fileName)
                    .Execute(new PNDeleteFileResultExt((result, status) =>
                    {
                        if (result != null)
                        {
                            System.Diagnostics.Debug.WriteLine("DeleteFile result = " +
                                                               pubnub.JsonPluggableLibrary
                                                                   .SerializeToJsonString(result));
                            messageReset.Set();
                        }
                    }));
                receivedMessage = messageReset.WaitOne(2 * 1000 * 60);
            }

            var receivedEvent = false;
            if (receivedMessage)
            {
                receivedEvent = eventReset.WaitOne(10000);
            }

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage && receivedEvent,
                "WhenFileIsRequested -> ThenWithAsyncSendFileShouldReturnSuccess failed.");
        }

        //[Test]
        public static void ThenDownloadFileShouldReturnSuccess()
        {
            server.ClearRequests();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid_file_tests"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(token);

            var messageReset = new ManualResetEvent(false);
            string fileId = "b0a5c0df-7523-432e-8ea9-01567c93da7d";
            string fileName = "pandu_test.gif";

            pubnub.DownloadFile().Channel(channelName).FileId(fileId).FileName(fileName).CipherKey("enigma").Execute(
                new PNDownloadFileResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        //result.SaveToLocal(@"C:\Pandu\temp\new\output\hi_file.gif");
                        //result.SaveToLocal(@"C:\Pandu\temp\new\");
                        result.SaveFileToLocal("what_is_hi_file.gif");
                        messageReset.Set();
                    }
                }));
            var receivedMessage = messageReset.WaitOne(4000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenListFilesShouldReturnSuccess failed.");
        }

        [Test]
        public static void ThenGetFileUrlShouldReturnSuccess()
        {
            server.ClearRequests();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid_file_tests"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(token);

            var messageReset = new ManualResetEvent(false);
            string fileId = "bc03db55-6345-4a0f-aa58-beac970b2c5b";
            string fileName = "whoami.txt";

            pubnub.GetFileUrl().Channel(channelName).FileId(fileId).FileName(fileName).Execute(new PNFileUrlResultExt(
                (result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine(result.Url);
                        messageReset.Set();
                    }
                }));
            var receivedMessage = messageReset.WaitOne(5000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenListFilesShouldReturnSuccess failed.");
        }

        [Test]
        public static void ThenListFilesShouldReturnSuccess()
        {
            server.ClearRequests();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid_file_tests"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(token);

            var messageReset = new ManualResetEvent(false);
            pubnub.ListFiles().Channel(channelName)
                .Execute(new PNListFilesResultExt((result, status) =>
                {
                    if (result != null)
                    {
                        System.Diagnostics.Debug.WriteLine("result = " +
                                                           pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        messageReset.Set();
                    }
                }));
            var receivedMessage = messageReset.WaitOne(5000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, $"WhenFileIsRequested -> ThenListFilesShouldReturnSuccess failed. DUMP: {PubnubCommon.TEMP_DebugDump()}");
        }

        [Test]
#if NET40
        public static void ThenWithAsyncDeleteFileShouldReturnSuccess()
#else
        public static async Task ThenWithAsyncDeleteFileShouldReturnSuccess()
#endif
        {
            server.ClearRequests();

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid_file_tests"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor("enigma"), null),
                Secure = false
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }

            pubnub = createPubNubInstance(config);
            pubnub.SetAuthToken(token);

            var messageReset = new ManualResetEvent(false);
#if NET40
            PNResult<PNListFilesResult> listFilesResponse =
 Task.Factory.StartNew(async () => await pubnub.ListFiles().Channel(channelName).ExecuteAsync()).Result.Result;
#else
            PNResult<PNListFilesResult> listFilesResponse =
                await pubnub.ListFiles().Channel(channelName).ExecuteAsync();
#endif
            if (listFilesResponse.Result != null && listFilesResponse.Result.FilesList != null &&
                listFilesResponse.Result.FilesList.Count > 0 && !listFilesResponse.Status.Error)
            {
                List<PNFileResult> filesList = listFilesResponse.Result.FilesList;
                foreach (var file in filesList)
                {
#if NET40
                    PNResult<PNDeleteFileResult> deleteFileResponse =
 Task.Factory.StartNew(async () => await pubnub.DeleteFile().Channel(channelName).FileId(file.Id).FileName(file.Name).ExecuteAsync()).Result.Result;
#else
                    PNResult<PNDeleteFileResult> deleteFileResponse = await pubnub.DeleteFile().Channel(channelName)
                        .FileId(file.Id).FileName(file.Name).ExecuteAsync();
#endif
                    PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
                    if (deleteFileResult != null)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            string.Format("File Id={0}, Name={1} => deleted successfully", file.Id, file.Name));
                    }
                }

                messageReset.Set();
            }
            else
            {
#if NET40
                PNResult<PNDeleteFileResult> deleteFileResponse =
 Task.Factory.StartNew(async () => await pubnub.DeleteFile().Channel(channelName).FileId("test_file_id").FileName("test_file_name.test").ExecuteAsync()).Result.Result;
#else
                PNResult<PNDeleteFileResult> deleteFileResponse = await pubnub.DeleteFile().Channel(channelName)
                    .FileId("test_file_id").FileName("test_file_name.test").ExecuteAsync();
#endif
                PNDeleteFileResult deleteFileResult = deleteFileResponse.Result;
                if (deleteFileResult != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "File Id=test_file_id, Name=test_file_name.test => deleted successfully");
                    messageReset.Set();
                }
            }

            var receivedMessage = messageReset.WaitOne(7000);

            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
            Assert.IsTrue(receivedMessage, "WhenFileIsRequested -> ThenWithAsyncDeleteFileShouldReturnSuccess failed.");
        }
    }
}