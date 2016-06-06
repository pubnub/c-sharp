using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubnubApi;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using System;

namespace PubnubApiPCL.Silverlight50.Tests
{
    [TestClass]
    public class WhenGrantIsRequested : WorkItemTest
    {
        ManualResetEvent mreGrant = new ManualResetEvent(false);

        bool receivedGrantMessage = false;
        bool receivedRevokeMessage = false;
        int multipleChannelGrantCount = 5;
        int multipleAuthGrantCount = 5;
        string currentUnitTestCase = "";
        string currentChannel = "";

        Pubnub pubnub = null;

        [TestMethod, Asynchronous]
        public void ThenSubKeyLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    pubnub.GrantAccess("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess.");
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
        public void ThenSubKeyLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenSubKeyLevelWithReadShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    pubnub.GrantAccess("", true, false, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess.");
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
        public void ThenSubKeyLevelWithWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenSubKeyLevelWithWriteShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    pubnub.GrantAccess("", false, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess.");
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
        public void ThenChannelLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenChannelLevelWithReadWriteShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    currentChannel = channel;
                    pubnub.GrantAccess(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess.");
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
        public void ThenChannelLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenChannelLevelWithReadShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    currentChannel = channel;
                    pubnub.GrantAccess(channel, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess.");
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
        public void ThenChannelLevelWithWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;
            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenChannelLevelWithWriteShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    currentChannel = channel;
                    pubnub.GrantAccess(channel, false, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess.");
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
        public void ThenUserLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenUserLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenUserLevelWithReadWriteShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_authchannel";
                string authKey = "hello_my_authkey";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    currentChannel = channel;
                    pubnub.GrantAccess(channel, authKey, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess.");
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
        public void ThenUserLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenUserLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenUserLevelWithReadShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_authchannel";
                string authKey = "hello_my_authkey";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    currentChannel = channel;
                    pubnub.GrantAccess(channel, authKey, true, false, 5, AccessToUserLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess.");
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
        public void ThenUserLevelWithWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenUserLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenUserLevelWithWriteShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_authchannel";
                string authKey = "hello_my_authkey";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    currentChannel = channel;
                    pubnub.GrantAccess(channel, authKey, false, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess.");
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
        public void ThenMultipleChannelGrantShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenMultipleChannelGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenMultipleChannelGrantShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                System.Text.StringBuilder channelBuilder = new System.Text.StringBuilder();
                for (int index = 0; index < multipleChannelGrantCount; index++)
                {
                    if (index == multipleChannelGrantCount - 1)
                    {
                        channelBuilder.AppendFormat("csharp-hello_my_channel-{0}", index);
                    }
                    else
                    {
                        channelBuilder.AppendFormat("csharp-hello_my_channel-{0},", index);
                    }
                }
                string channel = "";
                if (!unitTest.EnableStubTest)
                {
                    channel = channelBuilder.ToString();
                }
                else
                {
                    multipleChannelGrantCount = 5;
                    channel = "csharp-hello_my_channel-0,csharp-hello_my_channel-1,csharp-hello_my_channel-2,csharp-hello_my_channel-3,csharp-hello_my_channel-4";
                }
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    currentChannel = channel;
                    pubnub.GrantAccess(channel, true, true, 5, AccessToMultiChannelGrantCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess.");
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
        public void ThenMultipleAuthGrantShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenMultipleAuthGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenMultipleAuthGrantShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                System.Text.StringBuilder authKeyBuilder = new System.Text.StringBuilder();
                for (int index = 0; index < multipleAuthGrantCount; index++)
                {
                    if (index == multipleAuthGrantCount - 1)
                    {
                        authKeyBuilder.AppendFormat("csharp-auth_key-{0}", index);
                    }
                    else
                    {
                        authKeyBuilder.AppendFormat("csharp-auth_key-{0},", index);
                    }
                }
                string channel = "hello_my_channel";
                string auth = "";
                if (!unitTest.EnableStubTest)
                {
                    auth = authKeyBuilder.ToString();
                }
                else
                {
                    multipleAuthGrantCount = 5;
                    auth = "csharp-auth_key-0,csharp-auth_key-1,csharp-auth_key-2,csharp-auth_key-3,csharp-auth_key-4";
                }
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    pubnub.GrantAccess(channel, auth, true, true, 5, AccessToMultiAuthGrantCallback, DummyErrorCallback);
                    mreGrant.WaitOne(60 * 1000);

                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess failed.");
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess.");
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
        public void ThenRevokeAtSubKeyLevelReturnSuccess()
        {
            currentUnitTestCase = "ThenRevokeAtSubKeyLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenRevokeAtSubKeyLevelReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;
                if (PubnubCommon.PAMEnabled)
                {
                    if (!unitTest.EnableStubTest)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);
                    }
                    else
                    {
                        receivedGrantMessage = true;
                    }
                    if (receivedGrantMessage)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess("", false, false, 5, RevokeToSubKeyLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);
                        Assert.IsTrue(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess -> Grant success but revoke failed.");
                    }
                    else
                    {
                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                    }
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess.");
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
        public void ThenRevokeAtChannelLevelReturnSuccess()
        {
            currentUnitTestCase = "ThenRevokeAtChannelLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenRevokeAtChannelLevelReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_channel";
                if (PubnubCommon.PAMEnabled)
                {
                    if (!unitTest.EnableStubTest)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);
                    }
                    else
                    {
                        receivedGrantMessage = true;
                    }
                    if (receivedGrantMessage)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess("", false, false, 5, RevokeToChannelLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);
                        Assert.IsTrue(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess -> Grant success but revoke failed.");
                    }
                    else
                    {
                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                    }
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess.");
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
        public void ThenRevokeAtUserLevelReturnSuccess()
        {
            currentUnitTestCase = "ThenRevokeAtUserLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenRevokeAtUserLevelReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channel = "hello_my_authchannel";
                string authKey = "hello_my_authkey";
                if (PubnubCommon.PAMEnabled)
                {
                    if (!unitTest.EnableStubTest)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess(channel, authKey, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);
                    }
                    else
                    {
                        receivedGrantMessage = true;
                    }
                    if (receivedGrantMessage)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess(channel, authKey, false, false, 5, RevokeToUserLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);
                        Assert.IsTrue(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant success but revoke failed.");
                    }
                    else
                    {
                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                    }
                }
                else
                {
                    Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess.");
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
        public void ThenChannelGroupLevelWithReadManageShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelGroupLevelWithReadManageShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenChannelGroupLevelWithReadManageShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channelgroup = "hello_my_group";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    pubnub.ChannelGroupGrantAccess(channelgroup, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    mreGrant.WaitOne();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelGroupLevelWithReadManageShouldReturnSuccess failed.");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelGroupLevelWithReadManageShouldReturnSuccess.");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                }
            });
        }

        [TestMethod, Asynchronous]
        public void ThenChannelGroupLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelGroupLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
            {
                pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                PubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.TestClassName = "WhenGrantIsRequested";
                unitTest.TestCaseName = "ThenChannelGroupLevelWithReadShouldReturnSuccess";
                pubnub.PubnubUnitTest = unitTest;

                string channelgroup = "hello_my_group";
                if (PubnubCommon.PAMEnabled)
                {
                    mreGrant = new ManualResetEvent(false);
                    pubnub.ChannelGroupGrantAccess(channelgroup, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);

                    mreGrant.WaitOne();

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelGroupLevelWithReadShouldReturnSuccess failed.");
                        pubnub.PubnubUnitTest = null;
                        pubnub = null;
                        TestComplete();
                    });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelGroupLevelWithReadShouldReturnSuccess.");
                            pubnub.PubnubUnitTest = null;
                            pubnub = null;
                            TestComplete();
                        });
                }
            });
        }

        [Asynchronous]
        void AccessToSubKeyLevelCallback(GrantAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        var payload = receivedMessage.Payload;
                        if (payload != null)
                        {
                            bool read = payload.Access.read;
                            bool write = payload.Access.write;
                            string level = payload.Level;
                            if (level == "subkey")
                            {
                                switch (currentUnitTestCase)
                                {
                                    case "ThenSubKeyLevelWithReadWriteShouldReturnSuccess":
                                    case "ThenRevokeAtSubKeyLevelReturnSuccess":
                                        if (read && write) receivedGrantMessage = true;
                                        break;
                                    case "ThenSubKeyLevelWithReadShouldReturnSuccess":
                                        if (read && !write) receivedGrantMessage = true;
                                        break;
                                    case "ThenSubKeyLevelWithWriteShouldReturnSuccess":
                                        if (!read && write) receivedGrantMessage = true;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void AccessToChannelLevelCallback(GrantAck receivedMessage)
        {
            try
            {
                if (receivedMessage != null)
                {
                    int statusCode = receivedMessage.StatusCode;
                    string statusMessage = receivedMessage.StatusMessage;
                    if (statusCode == 200 && statusMessage.ToLower() == "success")
                    {
                        var payload = receivedMessage.Payload;
                        if (payload != null)
                        {
                            string level = payload.Level;
                            if (level == "channel")
                            {
                                var channels = payload.channels;
                                if (channels != null)
                                {
                                    string currentChannel = channels.Keys.ToList()[0];
                                    var channelContainer = channels[currentChannel];
                                    if (channelContainer != null)
                                    {
                                        bool read = channelContainer.Access.read;
                                        bool write = channelContainer.Access.write;
                                        switch (currentUnitTestCase)
                                        {
                                            case "ThenChannelLevelWithReadWriteShouldReturnSuccess":
                                            case "ThenRevokeAtChannelLevelReturnSuccess":
                                                if (read && write) receivedGrantMessage = true;
                                                break;
                                            case "ThenChannelLevelWithReadShouldReturnSuccess":
                                                if (read && !write) receivedGrantMessage = true;
                                                break;
                                            case "ThenChannelLevelWithWriteShouldReturnSuccess":
                                                if (!read && write) receivedGrantMessage = true;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                            else if (level == "channel-group")
                            {
                                var channelgroups = payload.channelgroups;
                                if (channelgroups != null)
                                {
                                    string currentChannelGroup = channelgroups.Keys.ToList()[0];
                                    var channelgroupContainer = channelgroups[currentChannelGroup];
                                    if (channelgroupContainer != null)
                                    {
                                        bool read = channelgroupContainer.Access.read;
                                        bool manage = channelgroupContainer.Access.manage;
                                        switch (currentUnitTestCase)
                                        {
                                            case "ThenChannelGroupLevelWithReadManageShouldReturnSuccess":
                                                if (read && manage) receivedGrantMessage = true;
                                                break;
                                            case "ThenChannelGroupLevelWithReadShouldReturnSuccess":
                                                if (read && !manage) receivedGrantMessage = true;
                                                break;
                                            default:
                                                break;
                                        }
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
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void AccessToUserLevelCallback(GrantAck receivedMessage)
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
                            Dictionary<string, GrantAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
                            foreach (string channel in channels.Keys)
                            {
                                GrantAck.Data.ChannelData channelData = channels[channel];
                                if (channelData.auths != null)
                                {
                                    Dictionary<string, GrantAck.Data.ChannelData.AuthData> authDataDic = channelData.auths;
                                    if (authDataDic != null)
                                    {
                                        foreach (string key in authDataDic.Keys)
                                        {
                                            GrantAck.Data.ChannelData.AuthData authData = authDataDic[key];
                                            if (authData != null && authData.Access != null)
                                            {
                                                bool read = authData.Access.read;
                                                bool write = authData.Access.write;
                                                if (level == "user")
                                                {
                                                    switch (currentUnitTestCase)
                                                    {
                                                        case "ThenUserLevelWithReadWriteShouldReturnSuccess":
                                                        case "ThenRevokeAtUserLevelReturnSuccess":
                                                            if (read && write) receivedGrantMessage = true;
                                                            break;
                                                        case "ThenUserLevelWithReadShouldReturnSuccess":
                                                            if (read && !write) receivedGrantMessage = true;
                                                            break;
                                                        case "ThenUserLevelWithWriteShouldReturnSuccess":
                                                            if (!read && write) receivedGrantMessage = true;
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                            }
                                        }
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
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void AccessToMultiChannelGrantCallback(GrantAck receivedMessage)
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
                            Dictionary<string, GrantAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
                            if (channels != null && channels.Count >= 0)
                            {
                                if (channels.Count == multipleChannelGrantCount)
                                {
                                    receivedGrantMessage = true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void AccessToMultiAuthGrantCallback(GrantAck receivedMessage)
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
                            Dictionary<string, GrantAck.Data.ChannelData> channelsData = receivedMessage.Payload.channels;
                            if (channelsData != null && channelsData.Count > 0)
                            {
                                List<string> channels = channelsData.Keys.ToList();
                                string channel = channels[0];
                                //string channel = 
                                GrantAck.Data.ChannelData channelData = channelsData[channel];
                                if (channelData != null && channelData.auths != null)
                                {
                                    //Console.WriteLine("{0} - AccessToMultiAuthGrantCallback - Grant Auth Count (Received/Sent) = {1}/{2}", currentUnitTestCase, channelData.auths.Count, multipleAuthGrantCount);
                                    if (channelData.auths.Count == multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
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
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void RevokeToSubKeyLevelCallback(GrantAck receivedMessage)
        {
            try
            {
                int statusCode = receivedMessage.StatusCode;
                string statusMessage = receivedMessage.StatusMessage;
                if (statusCode == 200 && statusMessage.ToLower() == "success")
                {
                    if (receivedMessage.Payload != null && receivedMessage.Payload.Access != null)
                    {
                        bool read = receivedMessage.Payload.Access.read;
                        bool write = receivedMessage.Payload.Access.write;
                        string level = receivedMessage.Payload.Level;
                        if (level == "subkey")
                        {
                            switch (currentUnitTestCase)
                            {
                                case "ThenRevokeAtSubKeyLevelReturnSuccess":
                                    if (!read && !write) receivedRevokeMessage = true;
                                    break;
                                case "ThenSubKeyLevelWithReadShouldReturnSuccess":
                                    //if (read && !write) receivedGrantMessage = true;
                                    break;
                                case "ThenSubKeyLevelWithWriteShouldReturnSuccess":
                                    //if (!read && write) receivedGrantMessage = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void RevokeToChannelLevelCallback(GrantAck receivedMessage)
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
                            Dictionary<string, GrantAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
                            if (channels != null && channels.Count > 0)
                            {
                                receivedRevokeMessage = true;
                                foreach (string ch in channels.Keys)
                                {
                                    if (channels.ContainsKey(ch))
                                    {
                                        Dictionary<string, object> channelContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channels[ch]);
                                        if (channels[ch].Access != null)
                                        {
                                            bool read = channels[ch].Access.read;
                                            bool write = channels[ch].Access.write;
                                            if (!read && !write)
                                            {
                                                receivedRevokeMessage = true;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                receivedRevokeMessage = true;
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void RevokeToUserLevelCallback(GrantAck receivedMessage)
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
                            Dictionary<string, GrantAck.Data.ChannelData> channelsDataDic = receivedMessage.Payload.channels;
                            if (channelsDataDic != null && channelsDataDic.Count > 0)
                            {
                                List<string> channelKeyList = channelsDataDic.Keys.ToList();
                                string channel = channelKeyList[0];

                                GrantAck.Data.ChannelData channelData = channelsDataDic[channel];
                                if (channelData != null)
                                {
                                    Dictionary<string, GrantAck.Data.ChannelData.AuthData> authDataDic = channelData.auths;
                                    if (authDataDic != null && authDataDic.Count > 0)
                                    {
                                        receivedRevokeMessage = true;
                                        foreach (string key in authDataDic.Keys)
                                        {
                                            GrantAck.Data.ChannelData.AuthData authData = authDataDic[key];
                                            if (authData != null && authData.Access != null)
                                            {
                                                receivedRevokeMessage = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        receivedRevokeMessage = false;
                                    }
                                }
                            }
                        } //end of if payload
                    }
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
            Console.WriteLine(result.Description);
        }

    }
}
