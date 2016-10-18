using System;
using NUnit.Framework;
using System.Threading;
using PubnubApi;
using MockServer;
using System.Text;
using System.Linq;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenGrantIsRequested : TestHarness
    {
        private static ManualResetEvent grantManualEvent = new ManualResetEvent(false);
        private static ManualResetEvent revokeManualEvent = new ManualResetEvent(false);
        private static bool receivedGrantMessage = false;
        private static bool receivedRevokeMessage = false;
        private static int multipleChannelGrantCount = 5;
        private static int multipleAuthGrantCount = 5;
        private static string currentUnitTestCase = "";
        private static string channel = "hello_my_channel";
        private static string channelGroup = "myChannelGroup";
        private static string authKey = "hello_my_authkey";
        private static string[] channelBuilder;
        private static string[] authKeyBuilder;

        private static Pubnub pubnub = null;

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
        public void ThenUserLevelWithReadWriteShouldReturnSuccess()
        {
            currentUnitTestCase = "ThenUserLevelWithReadWriteShouldReturnSuccess";

            receivedGrantMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(false).Manage(false).TTL(5).Async(new GrantResult());
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(false).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            channelBuilder = new string[multipleChannelGrantCount];

            for (int index = 0; index < multipleChannelGrantCount; index++)
            {
                channelBuilder[index] = String.Format("csharp-hello_my_channel-{0}", index);
            }

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().AuthKeys(new string[] { authKey }).Channels(channelBuilder).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            channelBuilder = new string[multipleChannelGrantCount];
            authKeyBuilder = new string[multipleChannelGrantCount];

            for (int index = 0; index < multipleChannelGrantCount; index++)
            {
                channelBuilder[index] = String.Format("csharp-hello_my_channel-{0}", index);
                authKeyBuilder[index] = String.Format("AuthKey-csharp-hello_my_channel-{0}", index);
            }

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(channelBuilder).AuthKeys(authKeyBuilder).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
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
        public void ThenRevokeAtUserLevelReturnSuccess()
        {
            currentUnitTestCase = "ThenRevokeAtUserLevelReturnSuccess";

            receivedGrantMessage = false;
            receivedRevokeMessage = false;

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(false).TTL(5).Async(new GrantResult());
                Thread.Sleep(1000);
                grantManualEvent.WaitOne();

                if (receivedGrantMessage)
                {
                    revokeManualEvent = new ManualResetEvent(false);
                    Console.WriteLine("WhenGrantIsRequested -> ThenRevokeAtUserLevelReturnSuccess -> Grant ok..Now trying Revoke");
                    pubnub.Grant().Channels(new string[] { channel }).AuthKeys(new string[] { authKey }).Read(false).Write(false).Manage(false).TTL(5).Async(new RevokeGrantResult());
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().ChannelGroups(new string[] { channelGroup }).AuthKeys(new string[] { authKey }).Read(true).Write(true).Manage(true).TTL(5).Async(new GrantResult());
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

            PNConfiguration config = new PNConfiguration()
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
                Uuid = "mytestuuid",
            };

            pubnub = this.createPubNubInstance(config);

            if (PubnubCommon.PAMEnabled)
            {
                grantManualEvent = new ManualResetEvent(false);
                pubnub.Grant().ChannelGroups(new string[] { channelGroup }).AuthKeys(new string[] { authKey }).Read(true).Write(false).Manage(false).TTL(5).Async(new GrantResult());
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

        private class GrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        switch (currentUnitTestCase)
                        {
                            case "ThenUserLevelWithReadWriteShouldReturnSuccess":
                            case "ThenRevokeAtUserLevelReturnSuccess":
                                {
                                    if (result.Channels != null && result.Channels.Count > 0)
                                    {
                                        var read = result.Channels[channel][authKey].ReadEnabled;
                                        var write = result.Channels[channel][authKey].WriteEnabled;
                                        if (read && write)
                                        {
                                            receivedGrantMessage = true;
                                        }
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithReadShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (read && !write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithWriteShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (!read && write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleChannelGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count==multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleAuthGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count == multipleAuthGrantCount)
                                    {
                                        if (result.Channels.ToList()[0].Value.Count == multipleAuthGrantCount)
                                        {
                                            receivedGrantMessage = true;
                                        }
                                    }
                                    break;
                                }
                            case "ThenChannelGroupLevelWithReadManageShouldReturnSuccess":
                                {
                                    var read = result.ChannelGroups[channelGroup][authKey].ReadEnabled;
                                    var write = result.ChannelGroups[channelGroup][authKey].WriteEnabled;
                                    var manage = result.ChannelGroups[channelGroup][authKey].ManageEnabled;
                                    if (read && write && manage)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenChannelGroupLevelWithReadShouldReturnSuccess":
                                {
                                    var read = result.ChannelGroups[channelGroup][authKey].ReadEnabled;
                                    var write = result.ChannelGroups[channelGroup][authKey].WriteEnabled;
                                    var manage = result.ChannelGroups[channelGroup][authKey].ManageEnabled;
                                    if (read && !write && !manage)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    grantManualEvent.Set();
                }
            }
        }


        private class RevokeGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                try
                {
                    Console.WriteLine("PNStatus={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(status));

                    if (result != null)
                    {
                        Console.WriteLine("PNAccessManagerAuditResult={0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                        switch (currentUnitTestCase)
                        {
                            case "ThenRevokeAtUserLevelReturnSuccess":
                                {
                                    if (result.Channels != null && result.Channels.Count > 0)
                                    {
                                        var read = result.Channels[channel][authKey].ReadEnabled;
                                        var write = result.Channels[channel][authKey].WriteEnabled;
                                        if (!read && !write)
                                        {
                                            receivedRevokeMessage = true;
                                        }
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithReadShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (read && !write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenUserLevelWithWriteShouldReturnSuccess":
                                {
                                    var read = result.Channels[channel][authKey].ReadEnabled;
                                    var write = result.Channels[channel][authKey].WriteEnabled;
                                    if (!read && write)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleChannelGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count == multipleAuthGrantCount)
                                    {
                                        receivedGrantMessage = true;
                                    }
                                    break;
                                }
                            case "ThenMultipleAuthGrantShouldReturnSuccess":
                                {
                                    if (result.Channels.Count == multipleAuthGrantCount)
                                    {
                                        if (result.Channels.ToList()[0].Value.Count == multipleAuthGrantCount)
                                        {
                                            receivedGrantMessage = true;
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    revokeManualEvent.Set();
                }
            }
        }

    }
}
