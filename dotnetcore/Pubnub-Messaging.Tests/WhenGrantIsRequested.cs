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
                pubnub.GrantAccess<string>("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>("", true, false, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>("", false, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, false, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, authKey, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, authKey, true, false, 5, AccessToUserLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, authKey, false, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, true, true, 5, AccessToMultiChannelGrantCallback, DummyErrorCallback);
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
                pubnub.GrantAccess<string>(channel, auth, true, true, 5, AccessToMultiAuthGrantCallback, DummyErrorCallback);
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
                    pubnub.GrantAccess<string>("", true, true, 5, AccessToSubKeyLevelCallback, DummyErrorCallback);
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
                    pubnub.GrantAccess<string>("", false, false, 5, RevokeToSubKeyLevelCallback, DummyErrorCallback);
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
                    pubnub.GrantAccess<string>(channel, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
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
                    pubnub.GrantAccess<string>("", false, false, 5, RevokeToChannelLevelCallback, DummyErrorCallback);
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
                    pubnub.GrantAccess<string>(channel, authKey, true, true, 5, AccessToUserLevelCallback, DummyErrorCallback);
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
                    pubnub.GrantAccess<string>(channel, authKey, false, false, 5, RevokeToUserLevelCallback, DummyErrorCallback);
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
                pubnub.ChannelGroupGrantAccess<string>(channelgroup, true, true, 5, AccessToChannelLevelCallback, DummyErrorCallback);
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
                pubnub.ChannelGroupGrantAccess<string>(channelgroup, true, false, 5, AccessToChannelLevelCallback, DummyErrorCallback);
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


        void AccessToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
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
                                    bool read = Convert.ToBoolean(payload["r"]);
                                    bool write = Convert.ToBoolean(payload["w"]);
                                    string level = payload["level"].ToString();
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
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void AccessToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        string currentChannel = serializedMessage[1].ToString();
                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    string level = payload["level"].ToString();
                                    if (level == "channel")
                                    {
                                        Dictionary<string, object> channels = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]);
                                        if (channels != null && channels.Count > 0)
                                        {
                                            Dictionary<string, object> channelContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channels[currentChannel]);
                                            if (channelContainer != null && channelContainer.Count > 0)
                                            {
                                                bool read = Convert.ToBoolean(channelContainer["r"]);
                                                bool write = Convert.ToBoolean(channelContainer["w"]);
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
                                        Dictionary<string, object> channelgroups = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channel-groups"]);
                                        if (channelgroups != null && channelgroups.Count > 0)
                                        {
                                            Dictionary<string, object> channelgroupContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channelgroups[currentChannel]);
                                            if (channelgroupContainer != null && channelgroupContainer.Count > 0)
                                            {
                                                bool read = Convert.ToBoolean(channelgroupContainer["r"]);
                                                bool manage = Convert.ToBoolean(channelgroupContainer["m"]);
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
                    
                    
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void AccessToUserLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        string currentChannel = serializedMessage[1].ToString();
                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    string level = payload["level"].ToString();
                                    string channel = payload["channel"].ToString();

                                    Dictionary<string, object> auths = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["auths"]);
                                    
                                    if (auths != null && auths.Count >= 0)
                                    {
                                        foreach (string key in auths.Keys)
                                        {
                                            Dictionary<string, object> authKeyContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(auths[key]);
                                            if (authKeyContainer != null && authKeyContainer.Count > 0)
                                            {
                                                bool read = Convert.ToBoolean(authKeyContainer["r"]);
                                                bool write = Convert.ToBoolean(authKeyContainer["w"]);
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

        void AccessToMultiChannelGrantCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        string currentChannel = serializedMessage[1].ToString();

                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    string level = payload["level"].ToString();
                                    Dictionary<string, object> channels = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]);
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
                }
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void AccessToMultiAuthGrantCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        string currentChannel = serializedMessage[1].ToString();

                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null)
                                {
                                    string level = payload["level"].ToString();
                                    string channel = payload["channel"].ToString();

                                    Dictionary<string, object> auths = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["auths"]);
                                    if (auths != null && auths.Count >= 0)
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
            }
            catch { }
            finally
            {
                grantManualEvent.Set();
            }
        }

        void RevokeToSubKeyLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);

                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    bool read = Convert.ToBoolean(payload["r"]);
                                    bool write = Convert.ToBoolean(payload["w"]);
                                    string level = payload["level"].ToString();
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
                    }
                    

                }
            }
            catch { }
            finally
            {
                revokeManualEvent.Set();
            }
        }

        void RevokeToChannelLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        string currentChannel = serializedMessage[1].ToString();

                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    string level = payload["level"].ToString();
                                    Dictionary<string, object> channels = (payload.ContainsKey("channels")) ? pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["channels"]) : null;
                                    if (channels != null && channels.Count > 0)
                                    {
                                        Dictionary<string, object> channelContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(channels[currentChannel]);
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
            }
            catch { }
            finally
            {
                revokeManualEvent.Set();
            }
        }

        void RevokeToUserLevelCallback(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    List<object> serializedMessage = pubnub.JsonPluggableLibrary.DeserializeToListOfObject(receivedMessage);
                    if (serializedMessage != null && serializedMessage.Count > 0)
                    {
                        Dictionary<string, object> dictionary = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(serializedMessage[0]);
                        string currentChannel = serializedMessage[1].ToString();

                        if (dictionary != null)
                        {
                            int statusCode = Convert.ToInt32(dictionary["status"]);
                            string statusMessage = dictionary["message"].ToString();
                            if (statusCode == 200 && statusMessage.ToLower() == "success")
                            {
                                Dictionary<string, object> payload = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(dictionary["payload"]);
                                if (payload != null && payload.Count > 0)
                                {
                                    string level = payload["level"].ToString();
                                    string channel = payload["channel"].ToString();

                                    Dictionary<string, object> auths = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(payload["auths"]);
                                    if (auths != null && auths.Count >= 0)
                                    {
                                        receivedRevokeMessage = true;
                                        foreach (string key in auths.Keys)
                                        {
                                            Dictionary<string, object> authKeyContainer = pubnub.JsonPluggableLibrary.ConvertToDictionaryObject(auths[key]);
                                            if (authKeyContainer != null && authKeyContainer.Count >= 0)
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
                                } //end of if payload
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

        private void DummyErrorCallback(PubnubClientError result)
        {
            if (currentUnitTestCase == "ThenRevokeAtChannelLevelReturnSuccess")
            {
                grantManualEvent.Set();
            }
        }

    }
}
