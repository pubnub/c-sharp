using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class CleanupGrant : TestHarness
    {
        private ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        private ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
        private bool receivedAuditMessage = false;
        private bool receivedRevokeMessage = false;

        private Pubnub pubnub = null;
        private Server server;
        private UnitTestLog unitLog;

        [TestFixtureSetUp]
        public void Init()
        {
            unitLog = new Tests.UnitTestLog();
            unitLog.LogLevel = MockServer.LoggingMethod.Level.Verbose;
            server = new Server(new Uri("https://" + PubnubCommon.StubOrign));
            MockServer.LoggingMethod.MockServerLog = unitLog;
            server.Start();
        }

        [TestFixtureTearDown]
        public void Exit()
        {
            server.Stop();
        }

        [Test]
        public void AtUserLevel()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; CleanupGrant -> AtUserLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;
                auditManualEvent = new ManualResetEvent(false);

                PNConfiguration config = new PNConfiguration()
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Uuid = "mytestuuid",
                };

                pubnub = this.createPubNubInstance(config);

                string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                    .WithMethod("GET")
                    .WithPath(string.Format("/v1/auth/audit/sub-key/{0}", PubnubCommon.SubscribeKey))
                    .WithParameter("signature", "Yxvw2lCm7HL0tB9kj8qFA0YCC3KbxyTKkUcrwti9PKQ=")
                    .WithParameter("pnsdk", PubnubCommon.EncodedSDK)
                    .WithParameter("timestamp", "1356998400")
                    .WithParameter("uuid", config.Uuid)
                    .WithResponse(expected)
                    .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Audit().Async(new PNCallback<PNAccessManagerAuditResult>() { Result = UserCallbackForCleanUpAccessAtUserLevel, Error = ErrorCallbackForCleanUpAccessAtUserLevel });
                auditManualEvent.WaitOne();
                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "CleanupGrant -> AtUserLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; CleanupGrant -> AtUserLevel.");
            }
        }

        [Test]
        public void AtChannelLevel()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                Assert.Ignore("PAM not enabled; CleanupGrant -> AtChannelLevel.");
                return;
            }

            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;
                auditManualEvent = new ManualResetEvent(false);

                PNConfiguration config = new PNConfiguration()
                {
                    PublishKey = PubnubCommon.PublishKey,
                    SubscribeKey = PubnubCommon.SubscribeKey,
                    SecretKey = PubnubCommon.SecretKey,
                    Uuid = "mytestuuid",
                };

                pubnub = this.createPubNubInstance(config);

                string expected = "{\"message\":\"Success\",\"payload\":{\"level\":\"channel-group\",\"subscribe_key\":\"pam\",\"ttl\":20,\"channel-groups\":{\"hello_my_group\":{\"r\":1,\"w\":0,\"m\":1}}},\"service\":\"Access Manager\",\"status\":200}";

                server.AddRequest(new Request()
                        .WithMethod("GET")
                        .WithPath(string.Format("/v1/auth/grant/sub-key/{0}", PubnubCommon.SubscribeKey))
                        .WithResponse(expected)
                        .WithStatusCode(System.Net.HttpStatusCode.OK));

                pubnub.Audit().Async(new PNCallback<PNAccessManagerAuditResult>() { Result = UserCallbackForCleanUpAccessAtChannelLevel, Error = ErrorCallbackForCleanUpAccessAtChannelLevel });
                auditManualEvent.WaitOne();

                pubnub.EndPendingRequests();
                pubnub = null;
                Assert.IsTrue(receivedAuditMessage, "CleanupGrant -> AtChannelLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; CleanupGrant -> AtChannelLevel.");
            }
        }

        void UserCallbackForCleanUpAccessAtUserLevel(PNAccessManagerAuditResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage));
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        if (receivedMessage.Payload != null)
                        {
                            Dictionary<string, PNAccessManagerAuditResult.Data.ChannelData> channels = receivedMessage.Payload.Channels;
                            if (channels != null && channels.Count > 0)
                            {
                                Console.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Count);
                                foreach (string channelName in channels.Keys)
                                {
                                    PNAccessManagerAuditResult.Data.ChannelData channelContainer = channels[channelName];
                                    if (channelContainer != null && channelContainer.Auths != null)
                                    {
                                        Dictionary<string, PNAccessManagerAuditResult.Data.ChannelData.AuthData> auths = channelContainer.Auths;
                                        if (auths != null && auths.Count > 0)
                                        {
                                            foreach (string authKey in auths.Keys)
                                            {
                                                receivedRevokeMessage = false;
                                                Console.WriteLine("Auth Key = " + authKey);
                                                pubnub.Grant().Channels(new string[] { channelName }).AuthKeys(new string[] { authKey }).Read(false).Write(false).Manage(false).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = UserCallbackForRevokeAccess, Error = ErrorCallbackForRevokeAccess });
                                                revokeManualEvent.WaitOne();

                                            }
                                        }
                                    }
                                }
                            }
                            string level = receivedMessage.Payload.Level;
                            if (level == "subkey")
                            {
                                receivedAuditMessage = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                auditManualEvent.Set();
            }
        }

        void ErrorCallbackForCleanUpAccessAtUserLevel(PubnubClientError receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage);
            }
            auditManualEvent.Set();
        }

        void UserCallbackForRevokeAccess(PNAccessManagerGrantResult receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage));
                receivedRevokeMessage = true;
            }
            revokeManualEvent.Set();
        }

        void ErrorCallbackForRevokeAccess(PubnubClientError receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage);
            }
            revokeManualEvent.Set();
        }

        void UserCallbackForCleanUpAccessAtChannelLevel(PNAccessManagerAuditResult receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    Console.WriteLine(pubnub.JsonPluggableLibrary.SerializeToJsonString(receivedMessage));
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        if (receivedMessage.Payload != null)
                        {
                            Dictionary<string, PNAccessManagerAuditResult.Data.ChannelData> channels = receivedMessage.Payload.Channels;
                            if (channels != null && channels.Count > 0)
                            {
                                Console.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Count);
                                foreach (string channelName in channels.Keys)
                                {
                                    //Dictionary<string, object> channelContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channels[channelName]);
                                    Console.WriteLine(channelName);
                                    pubnub.Grant().Channels(new string[] { channelName }).Read(false).Write(false).Manage(false).Async(new PNCallback<PNAccessManagerGrantResult>() { Result = UserCallbackForRevokeAccess, Error = ErrorCallbackForRevokeAccess });
                                    revokeManualEvent.WaitOne();

                                }
                                //foreach (JToken channel in channels.Children())
                                //{
                                //    if (channel is JProperty)
                                //    {
                                //        var channelProperty = channel as JProperty;
                                //        if (channelProperty != null)
                                //        {
                                //            string channelName = channelProperty.Name;
                                //            Console.WriteLine(channelName);
                                //            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                                //            pubnub.GrantAccess<string>(channelName, false, false, UserCallbackForRevokeAccess, ErrorCallbackForRevokeAccess);
                                //            revokeManualEvent.WaitOne();
                                //        }
                                //    }
                                //}
                            }
                            string level = receivedMessage.Payload.Level;
                            if (level == "subkey")
                            {
                                receivedAuditMessage = true;
                            }
                        }
                    }

                }
            }
            catch { }
            finally
            {
                auditManualEvent.Set();
            }
        }

        void ErrorCallbackForCleanUpAccessAtChannelLevel(PubnubClientError receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage.Message);
            }
            auditManualEvent.Set();
        }

    }
}
