using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Collections;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
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

        Pubnub pubnub = null;

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

                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                pubnub.AuditAccess<string>(UserCallbackForCleanUpAccessAtUserLevel, ErrorCallbackForCleanUpAccessAtUserLevel);
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

                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                pubnub.AuditAccess<string>(UserCallbackForCleanUpAccessAtChannelLevel, ErrorCallbackForCleanUpAccessAtChannelLevel);
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

        void UserCallbackForCleanUpAccessAtUserLevel(string receivedMessage)
        {
            try
            {
                Console.WriteLine(receivedMessage);
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null && dictionary.Count > 0)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    Dictionary<string, object> channels = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]);
                                    if (channels != null && channels.Count > 0)
                                    {
                                        Console.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Count);
                                        foreach (string channelName in channels.Keys)
                                        {
                                            Dictionary<string, object> channelContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channels[channelName]);
                                            if (channelContainer != null && channelContainer.Count > 0 && channelContainer.ContainsKey("auths"))
                                            {
                                                Dictionary<string, object> auths = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channelContainer["auths"]);
                                                if (auths != null && auths.Count > 0)
                                                {
                                                    foreach (string authKey in auths.Keys)
                                                    {
                                                        receivedRevokeMessage = false;
                                                        Console.WriteLine("Auth Key = " + authKey);
                                                        pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                                                        pubnub.GrantAccess<string>(channelName, authKey, false, false, UserCallbackForRevokeAccess, ErrorCallbackForRevokeAccess);
                                                        revokeManualEvent.WaitOne();

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    string level = payload["level"].ToString();
                                    if (level == "subkey")
                                    {
                                        receivedAuditMessage = true;
                                    }
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
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        if (dictionary != null && dictionary.Count > 0)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    Dictionary<string, object> channels = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]);
                                    if (channels != null && channels.Count > 0)
                                    {
                                        Console.WriteLine("CleanupGrant / AtUserLevel / UserCallbackForCleanUpAccess - Channel Count = {0}", channels.Count);
                                        foreach (string channelName in channels.Keys)
                                        {
                                            //Dictionary<string, object> channelContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channels[channelName]);
                                            Console.WriteLine(channelName);
                                            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);
                                            pubnub.GrantAccess<string>(channelName, false, false, UserCallbackForRevokeAccess, ErrorCallbackForRevokeAccess);
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
                                    string level = payload["level"].ToString();
                                    if (level == "subkey")
                                    {
                                        receivedAuditMessage = true;
                                    }
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
