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
using PubnubApi;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenGrantIsRequested
    {

        ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
        bool receivedGrantMessage = false;
        bool receivedRevokeMessage = false;
        int multipleChannelGrantCount = 5;
        int multipleAuthGrantCount = 5;
        string currentUnitTestCase = "";

        Pubnub pubnub = null;

        [Test]
        public void ThenSubKeyLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelWithReadWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadWriteShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenSubKeyLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelWithReadShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess("", true, false, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithReadShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenSubKeyLevelWithWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenSubKeyLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenSubKeyLevelWithWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess("", false, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenSubKeyLevelWithWriteShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenChannelLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelWithReadWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadWriteShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenChannelLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelWithReadShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithReadShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenChannelLevelWithWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelLevelWithWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, false, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelLevelWithWriteShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenUserLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenUserLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenUserLevelWithReadWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_authchannel";
            string authKey = "hello_my_authkey";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, authKey, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadWriteShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenUserLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenUserLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenUserLevelWithReadShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_authchannel";
            string authKey = "hello_my_authkey";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, authKey, true, false, 5, AccessToUserLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithReadShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenUserLevelWithWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenUserLevelWithWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenUserLevelWithWriteShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_authchannel";
            string authKey = "hello_my_authkey";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, authKey, false, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenUserLevelWithWriteShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenMultipleChannelGrantShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenMultipleChannelGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenMultipleChannelGrantShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            StringBuilder channelBuilder = new StringBuilder();
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
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, true, true, 5, AccessToMultiChannelGrantCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleChannelGrantShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenMultipleAuthGrantShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenMultipleAuthGrantShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenMultipleAuthGrantShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            StringBuilder authKeyBuilder = new StringBuilder();
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
                grantManualEvent = new ManualResetEvent(false);
                pubnub.GrantAccess(channel, auth, true, true, 5, AccessToMultiAuthGrantCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenMultipleAuthGrantShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenRevokeAtSubKeyLevelReturnSuccess()
        {
            currentUnitTestCase = "ThenRevokeAtSubKeyLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenRevokeAtSubKeyLevelReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;
            if (PubnubCommon.PAMEnabled)
            {
                if (!unitTest.EnableStubTest)
                {
                    grantManualEvent = new ManualResetEvent(false);
                    pubnub.GrantAccess("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    grantManualEvent.WaitOne();
                }
                else
                {
                    receivedGrantMessage = true;
                }
                if (receivedGrantMessage)
                {
                    revokeManualEvent = new ManualResetEvent(false);
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.GrantAccess("", false, false, 5, RevokeToSubKeyLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    revokeManualEvent.WaitOne();
                    
                    pubnub.EndPendingRequests(); 
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    Assert.IsTrue(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess -> Grant success but revoke failed.");
                }
                else
                {
                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                }
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtSubKeyLevelReturnSuccess.");
            }
        }

        [Test]
        public void ThenRevokeAtChannelLevelReturnSuccess()
        {
            currentUnitTestCase = "ThenRevokeAtChannelLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

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
                    grantManualEvent = new ManualResetEvent(false);
                    pubnub.GrantAccess(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    grantManualEvent.WaitOne(310*1000);
                }
                else
                {
                    receivedGrantMessage = true;
                }
                if (receivedGrantMessage)
                {
                    revokeManualEvent = new ManualResetEvent(false);
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.GrantAccess(channel, false, false, RevokeToChannelLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    revokeManualEvent.WaitOne();

                    pubnub.EndPendingRequests(); 
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    Assert.IsTrue(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess -> Grant success but revoke failed.");
                }
                else
                {
                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                }
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtChannelLevelReturnSuccess.");
            }
        }

        [Test]
        public void ThenRevokeAtUserLevelReturnSuccess()
        {
            currentUnitTestCase = "ThenRevokeAtUserLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

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
                    grantManualEvent = new ManualResetEvent(false);
                    pubnub.GrantAccess(channel, authKey, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    grantManualEvent.WaitOne();
                }
                else
                {
                    receivedGrantMessage = true;
                }
                if (receivedGrantMessage)
                {
                    revokeManualEvent = new ManualResetEvent(false);
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.GrantAccess(channel, authKey, false, false, 5, RevokeToUserLevelCallback, DummyErrorCallback);
                    Thread.Sleep(1000);
                    revokeManualEvent.WaitOne();

                    pubnub.EndPendingRequests(); 
                    pubnub.PubnubUnitTest = null;
                    pubnub = null;
                    Assert.IsTrue(receivedRevokeMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant success but revoke failed.");
                }
                else
                {
                    Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess failed. -> Grant not occured, so is revoke");
                }
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess.");
            }
        }

        [Test]
        public void ThenChannelGroupLevelWithReadManageShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelGroupLevelWithReadManageShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelGroupLevelWithReadManageShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channelgroup = "hello_my_group";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.ChannelGroupGrantAccess(channelgroup, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelGroupLevelWithReadManageShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelGroupLevelWithReadManageShouldReturnSuccess.");
            }
        }

        [Test]
        public void ThenChannelGroupLevelWithReadShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenChannelGroupLevelWithReadShouldReturnSuccess";

            receivedGrantMessage = false;

            pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "WhenGrantIsRequested";
            unitTest.TestCaseName = "ThenChannelGroupLevelWithReadShouldReturnSuccess";
            pubnub.PubnubUnitTest = unitTest;

            string channelgroup = "hello_my_group";
            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.ChannelGroupGrantAccess(channelgroup, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
                Thread.Sleep(1000);

                grantManualEvent.WaitOne();

                pubnub.EndPendingRequests(); 
                pubnub.PubnubUnitTest = null;
                pubnub = null;
                Assert.IsTrue(receivedGrantMessage, "WhenGrantIsRequested -> ThenChannelGroupLevelWithReadShouldReturnSuccess failed.");
            }
            else
            {
                Assert.Ignore("PAM Not Enabled for WhenGrantIsRequested -> ThenChannelGroupLevelWithReadShouldReturnSuccess.");
            }
        }


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
                        if (receivedMessage.Payload != null && receivedMessage.Payload.Access != null)
                        {
                            bool read = receivedMessage.Payload.Access.read;
                            bool write = receivedMessage.Payload.Access.write;
                            string level = receivedMessage.Payload.Level;
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
                grantManualEvent.Set();
            }
        }

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
                        if (receivedMessage.Payload != null)
                        {
                            string level = receivedMessage.Payload.Level;
                            if (level == "channel")
                            {
                                Dictionary<string, GrantAck.Data.ChannelData> channels = receivedMessage.Payload.channels;
                                if (channels != null && channels.Count > 0)
                                {
                                    string currentChannel = channels.Keys.ToList()[0];
                                    if (channels[currentChannel].Access != null)
                                    {
                                        bool read = channels[currentChannel].Access.read;
                                        bool write = channels[currentChannel].Access.write;
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
                                Dictionary<string, GrantAck.Data.ChannelGroupData> channelgroups = receivedMessage.Payload.channelgroups;
                                if (channelgroups != null && channelgroups.Count > 0)
                                {
                                    string currentChannelGroup = channelgroups.Keys.ToList()[0];
                                    if (channelgroups[currentChannelGroup].Access != null)
                                    {
                                        bool read = channelgroups[currentChannelGroup].Access.read;
                                        bool manage = channelgroups[currentChannelGroup].Access.manage;
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
                            } //end of if
                        }
                    }
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

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
                grantManualEvent.Set();
            }
        }

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
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

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
                                    Console.WriteLine("{0} - AccessToMultiAuthGrantCallback - Grant Auth Count (Received/Sent) = {1}/{2}", currentUnitTestCase, channelData.auths.Count, multipleAuthGrantCount);
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
                grantManualEvent.Set();
            }
        }

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
                revokeManualEvent.Set();
            }
        }

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
                revokeManualEvent.Set();
            }
        }

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
                revokeManualEvent.Set();
            }
        }

        private void DummyErrorCallback(PubnubClientError result)
        {
            if (currentUnitTestCase == "ThenRevokeAtChannelLevelReturnSuccess")
            {
                grantManualEvent.Set();
            }
        }

    }
}
