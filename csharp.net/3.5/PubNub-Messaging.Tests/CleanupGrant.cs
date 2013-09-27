using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class CleanupGrant
    {
        ManualResetEvent auditManualEvent = new ManualResetEvent(false);
        ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
        bool receivedAuditMessage = false;
        bool receivedRevokeMessage = false;

        [Test]
        public void AtUserLevel()
        {
            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;

                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                pubnub.AuditAccess<string>(UserCallbackForCleanUpAccessAtUserLevel, ErrorCallbackForCleanUpAccessAtUserLevel);
                auditManualEvent.WaitOne();

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
            if (!PubnubCommon.EnableStubTest)
            {
                receivedAuditMessage = false;

                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                pubnub.AuditAccess<string>(UserCallbackForCleanUpAccessAtChannelLevel, ErrorCallbackForCleanUpAccessAtChannelLevel);
                auditManualEvent.WaitOne();

                Assert.IsTrue(receivedAuditMessage, "CleanupGrant -> AtChannelLevel failed.");
            }
            else
            {
                Assert.Ignore("Only for live test; CleanupGrant -> AtChannelLevel.");
            }
        }

        void UserCallbackForCleanUpAccessAtUserLevel(string receivedMessage)
        {
            try
            {
                Console.WriteLine(receivedMessage);
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                bool read = payload.Value<bool>("r");
                                bool write = payload.Value<bool>("w");
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null && channels.Count > 0)
                                {
                                    Console.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Count);
                                    foreach (JToken channel in channels.Children())
                                    {
                                        if (channel is JProperty)
                                        {
                                            var channelProperty = channel as JProperty;
                                            if (channelProperty != null)
                                            {
                                                string channelName = channelProperty.Name;
                                                Console.WriteLine(channelName);
                                                var channelContainer = channels.Value<JContainer>(channelName);
                                                if (channelContainer != null && channelContainer.Count > 0)
                                                {
                                                    var auths = channelContainer.Value<JContainer>("auths");
                                                    if (auths != null && auths.Count > 0)
                                                    {
                                                        foreach (JToken auth in auths.Children())
                                                        {
                                                            if (auth is JProperty)
                                                            {
                                                                var authProperty = auth as JProperty;
                                                                if (authProperty != null)
                                                                {
                                                                    receivedRevokeMessage = false;
                                                                    string authKey = authProperty.Name;
                                                                    Console.WriteLine("Auth Key = " + authKey);
                                                                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                                                                    pubnub.AuthenticationKey = authKey;
                                                                    pubnub.GrantAccess<string>(channelName, false, false, UserCallbackForRevokeAccess, ErrorCallbackForRevokeAccess);
                                                                    revokeManualEvent.WaitOne();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                string level = payload.Value<string>("level");
                                if (level == "subkey")
                                {
                                    receivedAuditMessage = true;
                                }
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

        void UserCallbackForRevokeAccess(string receivedMessage)
        {
            if (receivedMessage != null)
            {
                Console.WriteLine(receivedMessage);
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

        void UserCallbackForCleanUpAccessAtChannelLevel(string receivedMessage)
        {
            try
            {
                Console.WriteLine(receivedMessage);
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                bool read = payload.Value<bool>("r");
                                bool write = payload.Value<bool>("w");
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null && channels.Count > 0)
                                {
                                    Console.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Count);
                                    foreach (JToken channel in channels.Children())
                                    {
                                        if (channel is JProperty)
                                        {
                                            var channelProperty = channel as JProperty;
                                            if (channelProperty != null)
                                            {
                                                string channelName = channelProperty.Name;
                                                Console.WriteLine(channelName);
                                                Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                                                pubnub.GrantAccess<string>(channelName, false, false, UserCallbackForRevokeAccess, ErrorCallbackForRevokeAccess);
                                                revokeManualEvent.WaitOne();
                                            }
                                        }
                                    }
                                }
                                string level = payload.Value<string>("level");
                                if (level == "subkey")
                                {
                                    receivedAuditMessage = true;
                                }
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
