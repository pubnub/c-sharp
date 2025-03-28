﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Diagnostics;
using System.Threading.Tasks;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenSubscribedToWildcardChannel : TestHarness
    {
        private static object publishedMessage;
        private static string channelGroupName = "";

        private static int manualResetEventWaitTimeout = 310 * 1000;
        private static string channel = "hello_my_channel";
        private static string authKey = "myauth";
        private static string authToken;

        private static Pubnub pubnub;
        private static Server server;

        public class TestLog : IPubnubLog
        {
            void IPubnubLog.WriteToLog(string logText)
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
        }

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

            if (!PubnubCommon.PAMServerSideGrant) { return; }

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                AuthKey = authKey,
                Secure = false,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog(),
            };
            server.RunOnHttps(false);

            pubnub = createPubNubInstance(config);
            manualResetEventWaitTimeout = 310 * 1000;
            
            if (string.IsNullOrEmpty(PubnubCommon.GrantToken))
            {
                await GenerateTestGrantToken(pubnub);
            }
            authToken = PubnubCommon.GrantToken;
            
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

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
        public static void ThenSubscribeShouldReturnReceivedMessage()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenItShouldReturnReceivedMessage Failed");
        }

        private static void CommonSubscribeShouldReturnReceivedMessageBasedOnParams(string secretKey, string cipherKey, bool ssl, out bool receivedMessage)
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor(cipherKey), null),
                Secure = ssl,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog()
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            server.RunOnHttps(ssl);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) =>
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage) == m.Message.ToString())
                        {
                            internalReceivedMessage = true;
                        }
                        subscribeManualEvent.Set();
                    }
                },
                (o, p) => {
                    internalReceivedMessage = true;
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        internalReceivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "5yl-J1ci5xJzHDTctEWusTxwvwRPuZ_JNAALf_zBvJU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { wildCardSubscribeChannel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishedMessage = "Test for WhenSubscribedToAChannel ThenItShouldReturnReceivedMessage";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20WhenSubscribedToAChannel%20ThenItShouldReturnReceivedMessage%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22QoHwTga0QtOCtJRQ6sqtyateB%2FVotNt%2F50y23yXW7rpCbZdJLUAVKKbf01SpN6zghA6MqQaaHRXoYqAf84RF56C7Ky6Oi6jLqN2I5%2FlXSCw%3D%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);
            if (!receivedErrorMessage)
            {
                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(publishChannel).Message(publishedMessage)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            internalReceivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);

                pubnub.Unsubscribe<string>().Channels(new[] { wildCardSubscribeChannel }).Execute();

                Thread.Sleep(1000);
            }

            receivedMessage = internalReceivedMessage;
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSSL()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageCipherSSL()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipherSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecret()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecretSSL()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecretCipher()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipher Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageSecretCipherSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnReceivedMessageCipher()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnReceivedMessageBasedOnParams("", "enigma", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnReceivedMessageCipher Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessage()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessage Failed");
        }

        private static void CommonSubscribeShouldReturnEmojiMessageBasedOnParams(string secretKey, string cipherKey, bool ssl, out bool receivedMessage)
        {
            server.ClearRequests();
            if (PubnubCommon.PAMServerSideRun && string.IsNullOrEmpty(secretKey))
            {
                Assert.Ignore("Ignored for Server side run");
            }

            bool internalReceivedMessage = false;
            bool receivedErrorMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                CryptoModule = new CryptoModule(new LegacyCryptor(cipherKey), null),
                Secure = ssl
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = secretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }
            server.RunOnHttps(ssl);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) =>
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage) == m.Message.ToString())
                        {
                            internalReceivedMessage = true;
                        }
                        subscribeManualEvent.Set();
                    }
                },
                (o, p) => {
                    internalReceivedMessage = true;
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        internalReceivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string publishChannel = "foo.bar";

            manualResetEventWaitTimeout = (PubnubCommon.EnableStubTest) ? 1000 : 310 * 1000;

            string expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("auth", authKey)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithParameter("signature", "5yl-J1ci5xJzHDTctEWusTxwvwRPuZ_JNAALf_zBvJU=")
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { wildCardSubscribeChannel }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishedMessage = "Text with 😜 emoji 🎉.";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Text%20with%20%F0%9F%98%9C%20emoji%20%F0%9F%8E%89.%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22vaD98V5XDtEvByw6RrxT9Ya76GKQLhyrEZw9Otrsu1KBVDIqGgWkrAD8X6TM%2FXC6%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, publishChannel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            if (!receivedErrorMessage)
            {
                ManualResetEvent publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(publishChannel).Message(publishedMessage)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            internalReceivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                Debug.WriteLine("publishManualEvent.WaitOne DONE");

                pubnub.Unsubscribe<string>().Channels(new[] { wildCardSubscribeChannel }).Execute();

                Thread.Sleep(1000);
            }

            receivedMessage = internalReceivedMessage;
            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageSSL()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams("", "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageSecret()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageCipherSecret()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", false, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecret Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "enigma", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageCipherSecretSSL Failed");
        }

        [Test]
        public static void ThenSubscribeShouldReturnEmojiMessageSecretSSL()
        {
            bool receivedMessage = false;
            CommonSubscribeShouldReturnEmojiMessageBasedOnParams(PubnubCommon.SecretKey, "", true, out receivedMessage);
            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnEmojiMessageSecretSSL Failed");
        }

        [Test]
        public static void ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage()
        {
            // TODO: this test seems to be unstable...
            server.ClearRequests();

            bool receivedMessage = false;

            PNConfiguration config = new PNConfiguration(new UserId("mytestuuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                AuthKey = authKey,
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
            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) =>
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage) == m.Message.ToString())
                        {
                            receivedMessage = true;
                        }
                        subscribeManualEvent.Set();
                    }
                },
                (o, p) => {
                    receivedMessage = true;
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                    else if (s.StatusCode == 200 && s.Category == PNStatusCategory.PNConnectedCategory)
                    {
                        receivedMessage = true;
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, authToken);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";
            string subChannelName = "hello_my_channel";
            string[] commaDelimitedChannel = new [] { subChannelName, wildCardSubscribeChannel };
            channelGroupName = "hello_my_group";
            string channelAddForGroup = "hello_my_channel_1";
            string pubWildChannelName = "foo.a";
            manualResetEventWaitTimeout = 310 * 1000;

            string expected = "{\"status\": 200, \"message\": \"OK\", \"service\": \"channel-registry\", \"error\": false}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/channel-registration/sub-key/{0}/channel-group/{1}", PubnubCommon.SubscribeKey, channelGroupName))
                    .WithParameter("add", channelAddForGroup)
                    .WithParameter("auth", config.AuthKey)
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            ManualResetEvent channelGroupManualEvent = new ManualResetEvent(false);
            pubnub.AddChannelsToChannelGroup().Channels(new [] { channelAddForGroup }).ChannelGroup(channelGroupName)
                .Execute(new PNChannelGroupsAddChannelResultExt(
                                (r, s) => {
                                    try
                                    {
                                        Debug.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(s));
                                        if (r != null)
                                        {
                                            Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(r));
                                            if (s.StatusCode == 200 && s.Error == false)
                                            {
                                                receivedMessage = true;
                                            }
                                        }
                                    }
                                    catch { /* ignore */ }
                                    finally
                                    {
                                        channelGroupManualEvent.Set();
                                    }
                                }));
            channelGroupManualEvent.WaitOne();

            subscribeManualEvent = new ManualResetEvent(false);

            expected = "{\"t\":{\"t\":\"14839022442039237\",\"r\":7},\"m\":[]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A,hello_my_channel"))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel-group", channelGroupName)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A,hello_my_channel"))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new [] { channelGroupName }).Execute();

            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            publishedMessage = "Test for cg";

            expected = "[1,\"Sent\",\"14722484585147754\"]";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20cg%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, channel))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            Thread.Sleep(1000);

            ManualResetEvent publishManualEvent = new ManualResetEvent(false);
            pubnub.Publish().Channel(channelAddForGroup).Message(publishedMessage)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
            publishManualEvent.WaitOne(manualResetEventWaitTimeout);
            Debug.WriteLine("publishManualEvent.WaitOne DONE");


            if (receivedMessage)
            {
                receivedMessage = false;

                subscribeManualEvent = new ManualResetEvent(false);

                Thread.Sleep(1000);
                publishedMessage = "Test for wc";

                expected = "[1,\"Sent\",\"14722484585147754\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20wc%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, pubWildChannelName))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                publishManualEvent = new ManualResetEvent(false);
                pubnub.Publish().Channel(pubWildChannelName).Message(publishedMessage)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));
                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                Debug.WriteLine("publishManualEvent.WaitOne DONE");
            }

            if (receivedMessage)
            {
                receivedMessage = false;

                publishManualEvent = new ManualResetEvent(false);
                publishedMessage = "Test for normal ch";

                expected = "[1,\"Sent\",\"14722484585147754\"]";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(String.Format("/publish/{0}/{1}/0/{2}/0/%22Test%20for%20normal%20ch%22", PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, subChannelName))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                Thread.Sleep(1000);

                pubnub.Publish().Channel(subChannelName).Message(publishedMessage)
                    .Execute(new PNPublishResultExt((r, s) =>
                    {
                        if (r != null && s.StatusCode == 200 && !s.Error)
                        {
                            receivedMessage = true;
                        }
                        publishManualEvent.Set();
                    }));

                publishManualEvent.WaitOne(manualResetEventWaitTimeout);
                Debug.WriteLine("publishManualEvent.WaitOne DONE");
            }

            pubnub.Unsubscribe<string>().Channels(commaDelimitedChannel).ChannelGroups(new [] { channelGroupName }).Execute();

            Thread.Sleep(1000);

            pubnub.RemoveListener(listenerSubCallack);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ChannelAndChannelGroupAndWildcardChannelSubscribeShouldReturnReceivedMessage Failed");
        }

        [Test]
        public static async Task ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback()
        {
            server.ClearRequests();

            bool receivedMessage = false;
            bool receivedErrorMessage = true;

            string userId = $"user{new Random().Next(10, 100)}";
            
            var fullAccess = new PNTokenAuthValues()
            {
                Read = true,
                Write = true,
                Create = true,
                Get = true,
                Delete = true,
                Join = true,
                Update = true,
                Manage = true
            };
            pubnub = createPubNubInstance(new PNConfiguration(userId)
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey
            });
            var grantResult = await pubnub.GrantToken().TTL(30).AuthorizedUuid(userId)
                .Patterns(new PNTokenPatterns()
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>()
                    {
                        { "foo.*", fullAccess },
                        { "foo.*-pnpres", fullAccess }
                    }, ChannelGroups = new Dictionary<string, PNTokenAuthValues>()
                    {
                        { "hello_my_group", fullAccess },
                        { "hello_my_group-pnpres", fullAccess }
                    }
                })
                .ExecuteAsync();

            await Task.Delay(4000);
            Assert.IsTrue(grantResult.Status.Error == false && grantResult.Result != null, 
                "GrantToken() for wildCardChannelSubscribe Test failed.");
            var accessToken = grantResult.Result.Token;
            PNConfiguration config = new PNConfiguration(new UserId(userId))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                Secure = false,
                LogVerbosity = PNLogVerbosity.BODY,
                PubnubLog = new TestLog(),
                NonSubscribeRequestTimeout = 120
            };
            if (PubnubCommon.PAMServerSideRun)
            {
                config.SecretKey = PubnubCommon.SecretKey;
            }
            else if (!string.IsNullOrEmpty(authKey) && !PubnubCommon.SuppressAuthKey)
            {
                config.AuthKey = authKey;
            }

            server.RunOnHttps(false);

            ManualResetEvent subscribeManualEvent = new ManualResetEvent(false);
            SubscribeCallback listenerSubCallack = new SubscribeCallbackExt(
                (o, m) =>
                {
                    Debug.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(m));
                    if (m != null)
                    {
                        Debug.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(m.Message));
                        if (pubnub.JsonPluggableLibrary.SerializeToJsonString(publishedMessage) == m.Message.ToString())
                        {
                            receivedMessage = true;
                        }
                    }
                    subscribeManualEvent.Set();
                },
                (o, p) => {
                    receivedMessage = true;
                    subscribeManualEvent.Set();
                },
                (o, s) => {
                    Debug.WriteLine(string.Format("{0} {1} {2}", s.Operation, s.Category, s.StatusCode));
                    if (s.StatusCode != 200 || s.Error)
                    {
                        receivedErrorMessage = true;
                        if (s.ErrorData != null) { Debug.WriteLine(s.ErrorData.Information); }
                        subscribeManualEvent.Set();
                    }
                });
            pubnub = createPubNubInstance(config, accessToken);
            pubnub.AddListener(listenerSubCallack);

            string wildCardSubscribeChannel = "foo.*";

            subscribeManualEvent = new ManualResetEvent(false);

            string expected = "{\"t\":{\"t\":\"14843277463968146\",\"r\":7},\"m\":[{\"a\":\"4\",\"f\":512,\"p\":{\"t\":\"14843277462084783\",\"r\":1},\"k\":\"demo-36\",\"c\":\"foo.*-pnpres\",\"d\":{\"action\": \"join\", \"timestamp\": 1484327746, \"uuid\": \"mytestuuid\", \"occupancy\": 1},\"b\":\"foo.*\"}]}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithParameter("auth", authKey)
                    .WithParameter("channel-group",channelGroupName)
                    .WithParameter("heartbeat", "300")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("requestid", "myRequestId")
                    .WithParameter("tt", "0")
                    .WithParameter("uuid", config.UserId)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            expected = "{}";

            server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(String.Format("/v2/subscribe/{0}/{1}/0", PubnubCommon.SubscribeKey, "foo.%2A"))
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

            pubnub.Subscribe<string>().Channels(new [] { wildCardSubscribeChannel }).ChannelGroups(new [] { channelGroupName }).Execute();
            subscribeManualEvent.WaitOne(manualResetEventWaitTimeout);

            if (!receivedErrorMessage)
            {
                Thread.Sleep(1000);

                pubnub.Unsubscribe<string>().Channels(new[] { wildCardSubscribeChannel }).ChannelGroups(new[] { channelGroupName }).Execute();

                Thread.Sleep(1000);
            }
            pubnub.RemoveListener(listenerSubCallack);
            Thread.Sleep(1000);
            pubnub.Destroy();
            pubnub.PubnubUnitTest = null;
            pubnub = null;

            Assert.IsTrue(receivedMessage, "WhenSubscribedToWildcardChannel --> ThenSubscribeShouldReturnWildCardPresenceEventInWildcardPresenceCallback Failed");
        }


    }
}
