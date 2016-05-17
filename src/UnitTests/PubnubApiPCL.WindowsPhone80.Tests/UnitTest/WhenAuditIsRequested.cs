using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Phone.Testing;
using PubnubApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace PubnubWindowsPhone.Test.UnitTest
{
    [TestClass]
    public class WhenAuditIsRequested: WorkItemTest
    {
        ManualResetEvent mreAudit = new ManualResetEvent(false);
        bool receivedAuditMessage = false;
        string currentUnitTestCase = "";

        [TestMethod, Asynchronous]
        public void ThenSubKeyLevelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelShouldReturnSuccess";

            receivedAuditMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAuditIsRequested";
                    unitTest.TestCaseName = "ThenSubKeyLevelShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreAudit = new ManualResetEvent(false);
                        pubnub.AuditAccess(AccessToSubKeyLevelCallback, DummyErrorCallback);
                        mreAudit.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenSubKeyLevelShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenAuditIsRequested -> ThenSubKeyLevelShouldReturnSuccess");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        pubnub.PubnubUnitTest = null;
                        pubnub = null;
                        TestComplete();
                    });
                });

        }

        [TestMethod, Asynchronous]
        public void ThenChannelLevelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelShouldReturnSuccess";

            receivedAuditMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAuditIsRequested";
                    unitTest.TestCaseName = "ThenChannelLevelShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";

                    if (PubnubCommon.PAMEnabled)
                    {
                        mreAudit = new ManualResetEvent(false);
                        pubnub.AuditAccess(channel, AccessToChannelLevelCallback, DummyErrorCallback);
                        mreAudit.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenChannelLevelShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenAuditIsRequested -> ThenChannelLevelShouldReturnSuccess");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        pubnub.PubnubUnitTest = null;
                        pubnub = null;
                        TestComplete();
                    });
                });
        }

        [TestMethod, Asynchronous]
        public void ThenChannelGroupLevelShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelGroupLevelShouldReturnSuccess";

            receivedAuditMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAuditIsRequested";
                    unitTest.TestCaseName = "ThenChannelGroupLevelShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channelgroup = "hello_my_group";

                    if (PubnubCommon.PAMEnabled)
                    {
                        mreAudit = new ManualResetEvent(false);
                        pubnub.ChannelGroupAuditAccess(channelgroup, AccessToChannelLevelCallback, DummyErrorCallback);
                        Thread.Sleep(1000);

                        mreAudit.WaitOne();

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.IsTrue(receivedAuditMessage, "WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess failed.");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Assert.Inconclusive("PAM Not Enabled for WhenAuditIsRequested -> ThenChannelGroupLevelShouldReturnSuccess");
                                pubnub.PubnubUnitTest = null;
                                pubnub = null;
                                TestComplete();
                            });
                    }
                });
        }

        [Asynchronous]
        void AccessToSubKeyLevelCallback(AuditAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        if (receivedMessage.Payload != null)
                        {
                            Dictionary<string, AuditAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
                            if (channels != null && channels.Count >= 0)
                            {
                                //Console.WriteLine("{0} - AccessToSubKeyLevelCallback - Audit Count = {1}", currentUnitTestCase, channels.Count);
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
                mreAudit.Set();
            }
        }

        [Asynchronous]
        void AccessToChannelLevelCallback(AuditAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        if (receivedMessage.Payload != null)
                        {
                            string level = receivedMessage.Payload.Level;
                            if (currentUnitTestCase == "ThenChannelLevelShouldReturnSuccess")
                            {
                                Dictionary<string, AuditAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
                                if (channels != null && channels.Count >= 0)
                                {
                                    Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit Channel Count = {1}", currentUnitTestCase, channels.Count);
                                }
                                if (level == "channel")
                                {
                                    receivedAuditMessage = true;
                                }
                            }
                            else if (currentUnitTestCase == "ThenChannelGroupLevelShouldReturnSuccess")
                            {
                                Dictionary<string, AuditAck.Data.ChannelGroupData> channelgroups = receivedMessage.Payload.channelgroups;
                                if (channelgroups != null && channelgroups.Count >= 0)
                                {
                                    Console.WriteLine("{0} - AccessToChannelLevelCallback - Audit ChannelGroup Count = {1}", currentUnitTestCase, channelgroups.Count);
                                }
                                if (level == "channel-group")
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
                mreAudit.Set();
            }
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {

        }
    }
}
