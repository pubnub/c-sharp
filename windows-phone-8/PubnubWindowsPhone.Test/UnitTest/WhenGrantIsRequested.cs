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
using PubNubMessaging.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;

namespace PubnubWindowsPhone.Test.UnitTest
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

        [TestMethod, Asynchronous]
        public void ThenSubKeyLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess<string>("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenSubKeyLevelWithReadShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess<string>("", true, false, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenSubKeyLevelWithWriteShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess<string>("", false, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenChannelLevelWithReadWriteShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess<string>(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenChannelLevelWithReadShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess<string>(channel, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenChannelLevelWithWriteShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.GrantAccess<string>(channel, false, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenUserLevelWithReadWriteShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_authchannel";
                    string authKey = "hello_my_authkey";
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.AuthenticationKey = authKey;
                        pubnub.GrantAccess<string>(channel, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenUserLevelWithReadShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_authchannel";
                    string authKey = "hello_my_authkey";
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.AuthenticationKey = authKey;
                        pubnub.GrantAccess<string>(channel, true, false, 5, AccessToUserLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenUserLevelWithWriteShouldReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_authchannel";
                    string authKey = "hello_my_authkey";
                    if (PubnubCommon.PAMEnabled)
                    {
                        mreGrant = new ManualResetEvent(false);
                        pubnub.AuthenticationKey = authKey;
                        pubnub.GrantAccess<string>(channel, false, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

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
                        pubnub.GrantAccess<string>(channel, true, true, 5, AccessToMultiChannelGrantCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

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
                        pubnub.AuthenticationKey = auth;
                        pubnub.GrantAccess<string>(channel, true, true, 5, AccessToMultiAuthGrantCallback, DummyErrorCallback);
                        mreGrant.WaitOne(60 * 1000);

                        Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess failed.");
                    }
                    else
                    {
                        Assert.Inconclusive("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess.");
                    }
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenGrantIsRequested";
                    unitTest.TestCaseName = "ThenRevokeAtSubKeyLevelReturnSuccess";
                    pubnub.PubnubUnitTest = unitTest;
                    if (PubnubCommon.PAMEnabled)
                    {
                        if (!unitTest.EnableStubTest)
                        {
                            mreGrant = new ManualResetEvent(false);
                            pubnub.GrantAccess<string>("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                            mreGrant.WaitOne(60 * 1000);
                        }
                        else
                        {
                            receivedGrantMessage = true;
                        }
                        if (receivedGrantMessage)
                        {
                            mreGrant = new ManualResetEvent(false);
                            pubnub.GrantAccess<string>("", false, false, 5, RevokeToSubKeyLevelCallback, DummyErrorCallback);
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

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
                            pubnub.GrantAccess<string>(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                            mreGrant.WaitOne(60 * 1000);
                        }
                        else
                        {
                            receivedGrantMessage = true;
                        }
                        if (receivedGrantMessage)
                        {
                            mreGrant = new ManualResetEvent(false);
                            pubnub.GrantAccess<string>("", false, false, 5, RevokeToChannelLevelCallback, DummyErrorCallback);
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
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

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
                            pubnub.AuthenticationKey = authKey;
                            pubnub.GrantAccess<string>(channel, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                            mreGrant.WaitOne(60 * 1000);
                        }
                        else
                        {
                            receivedGrantMessage = true;
                        }
                        if (receivedGrantMessage)
                        {
                            mreGrant = new ManualResetEvent(false);
                            pubnub.GrantAccess<string>("", false, false, 5, RevokeToUserLevelCallback, DummyErrorCallback);
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
                        TestComplete();
                    });
                });
        }

        [Asynchronous]
        void AccessToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
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
                                string level = payload.Value<string>("level");
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
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void AccessToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                string level = payload.Value<string>("level");
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null)
                                {
                                    var channelContainer = channels.Value<JContainer>(currentChannel);
                                    if (channelContainer != null)
                                    {
                                        bool read = channelContainer.Value<bool>("r");
                                        bool write = channelContainer.Value<bool>("w");
                                        if (level == "channel")
                                        {
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
        void AccessToUserLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                string level = payload.Value<string>("level");
                                string channel = payload.Value<string>("channel");
                                var auths = payload.Value<JContainer>("auths");
                                if (auths != null && auths.Count > 0)
                                {
                                    foreach(JToken auth in auths.Children())
                                    {
                                        if (auth is JProperty)
                                        {
                                            var authProperty = auth as JProperty;
                                            if (authProperty != null)
                                            {
                                                string authKey = authProperty.Name;
                                                var authKeyContainer = auths.Value<JContainer>(authKey);
                                                if (authKeyContainer != null && authKeyContainer.Count > 0)
                                                {
                                                    bool read = authKeyContainer.Value<bool>("r");
                                                    bool write = authKeyContainer.Value<bool>("w");
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
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void AccessToMultiChannelGrantCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                string level = payload.Value<string>("level");
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null)
                                {
                                    Console.WriteLine("{0} - AccessToMultiChannelGrantCallback - Grant MultiChannel Count (Received/Sent) = {1}/{2}", currentUnitTestCase, channels.Count, multipleChannelGrantCount);
                                    if (channels.Count == multipleChannelGrantCount)
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
        void AccessToMultiAuthGrantCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                string level = payload.Value<string>("level");
                                string channel = payload.Value<string>("channel");
                                var auths = payload.Value<JContainer>("auths");
                                if (auths != null)
                                {
                                    Console.WriteLine("{0} - AccessToMultiAuthGrantCallback - Grant Auth Count (Received/Sent) = {1}/{2}", currentUnitTestCase, auths.Count, multipleAuthGrantCount);
                                    if (auths.Count == multipleAuthGrantCount)
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
        void RevokeToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
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
                                string level = payload.Value<string>("level");
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

                        //if (dictionary.
                        //if (status == "200")
                        //{
                        //    receivedGrantMessage = true;
                        //}
                    }
                    //var level = dictionary["level"].ToString();
                }
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void RevokeToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                string level = payload.Value<string>("level");
                                var channels = payload.Value<JContainer>("channels");
                                if (channels != null)
                                {
                                    var channelContainer = channels.Value<JContainer>(currentChannel);
                                    if (channelContainer == null)
                                    {
                                        receivedRevokeMessage = true;
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
            }
            catch { }
            finally
            {
                mreGrant.Set();
            }
        }

        [Asynchronous]
        void RevokeToUserLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    string currentChannel = serializedMessage[1].ToString();
                    if (dictionary != null)
                    {
                        int statusCode = dictionary.Value<int>("status");
                        string statusMessage = dictionary.Value<string>("message");
                        if (statusCode == 200 && statusMessage.ToLower() == "success")
                        {
                            var payload = dictionary.Value<JContainer>("payload");
                            if (payload != null)
                            {
                                string level = payload.Value<string>("level");
                                string channel = payload.Value<string>("channel");
                                var auths = payload.Value<JContainer>("auths");
                                if (auths != null && auths.Count > 0)
                                {
                                    receivedRevokeMessage = true;
                                    foreach (JToken auth in auths.Children())
                                    {
                                        if (auth is JProperty)
                                        {
                                            var authProperty = auth as JProperty;
                                            if (authProperty != null)
                                            {
                                                string authKey = authProperty.Name;
                                                var authKeyContainer = auths.Value<JContainer>(authKey);
                                                if (authKeyContainer != null)
                                                {
                                                    receivedRevokeMessage = false;
                                                    break;
                                                }

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

        }

    }
}
