using PubnubApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PubNubMessaging.Tests
{
    public class TestHarness
    {
        private static readonly PNTokenAuthValues fullAccess = new PNTokenAuthValues()
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
        
        protected static Pubnub createPubNubInstance(PNConfiguration pnConfiguration, string authToken = "")
        {
            Pubnub pubnub = null;
            if (PubnubCommon.EnableStubTest)
            {
                #pragma warning disable CS0162 // Unreachable code detected
                pnConfiguration.Origin = PubnubCommon.StubOrign;
                #pragma warning restore CS0162 // Unreachable code detected

                IPubnubUnitTest unitTest = new PubnubUnitTest();
                unitTest.Timetoken = 1567581283; //Hardcoded timetoken
                unitTest.RequestId = "myRequestId";
                unitTest.InternetAvailable = true;
                unitTest.SdkVersion = PubnubCommon.EncodedSDK;
                unitTest.IncludePnsdk = true;
                unitTest.IncludeUuid = true;

                pubnub = new Pubnub(pnConfiguration);

                pubnub.PubnubUnitTest = unitTest;
            }
            else
            {
                pnConfiguration.Origin = "ps.pndsn.com";
                pubnub = new Pubnub(pnConfiguration);
            }
            if (!string.IsNullOrEmpty(authToken))
            {
                pubnub.SetAuthToken(authToken);
            }
            return pubnub;
        }

        public static async Task GenerateTestGrantToken(Pubnub pubnub, string presenceTestChannel = "presenceTestChannel")
        {
            string channel = "hello_my_channel";
            string channel1 = "hello_my_channel_1";
            string channel2 = "hello_my_channel_2";
            string channel3 = "hello_my_channel_3";
            string channel4 = "hello_my_channel_4";
            string group = "hello_my_group";
            string channelPattern = "foo.*";
            string channelGroupPattern = "foo.*";
            string uuidPattern = "fuu.*";
            
            var grantResult = await pubnub.GrantToken().TTL(30).AuthorizedUuid(pubnub.PNConfig.UserId).Resources(
                new PNTokenResources()
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {
                            channel, fullAccess
                        },
                        {
                            channel+"-pnpres", fullAccess
                        },
                        {
                            channel1, fullAccess
                        },
                        {
                            channel1+"-pnpres", fullAccess
                        },
                        {
                            channel2, fullAccess
                        },
                        {
                            channel2+"-pnpres", fullAccess
                        },
                        {
                            channel3, fullAccess
                        },
                        {
                            channel3+"-pnpres", fullAccess
                        },
                        {
                            channel4, fullAccess
                        },
                        {
                            channel4+"-pnpres", fullAccess
                        },
                        {
                            presenceTestChannel, fullAccess
                        },
                        {
                            $"{presenceTestChannel}{Constants.Pnpres}", fullAccess
                        },
                    },
                    ChannelGroups = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {group, fullAccess},
                        {group+"-pnpres", fullAccess}
                    }
                })
                .Patterns(new PNTokenPatterns()
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>()
                    {
                        { channelPattern, fullAccess },
                        { channelPattern+"-pnpres", fullAccess }
                    },
                    Uuids = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {uuidPattern, fullAccess}
                    },
                    ChannelGroups = new Dictionary<string, PNTokenAuthValues>()
                    {
                        {channelGroupPattern, fullAccess}
                    }
                })
                .ExecuteAsync();

            await Task.Delay(4000);

            PubnubCommon.GrantToken = grantResult.Result?.Token;
            Assert.IsTrue(grantResult.Status.Error == false && grantResult.Result != null, 
                "GrantToken() failed.");
        }

        protected static async Task GenerateDataSyncTestToken(Pubnub pubnub)
        {
            if (!string.IsNullOrEmpty(PubnubCommon.GrantToken))
            {
                return;
            }
            var config = new PNConfiguration(new UserId("ds_granter"))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
                PublishKey = PubnubCommon.DataSyncPublishKey,
                SecretKey = PubnubCommon.DataSyncSecretKey,
                Origin = PubnubCommon.DataSyncOrigin
            };
            var granter = new Pubnub(config);
            var grant = await granter.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId(pubnub.PNConfig.UserId))
                .Patterns(new PNTokenPatterns
                {
                    Users = new Dictionary<string, PNTokenAuthValues>(){{".*",fullAccess}},
                    Channels = new Dictionary<string, PNTokenAuthValues>(){{".*", fullAccess}},
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues> { { ".*", fullAccess } },
                        Relationships = new Dictionary<string, PNTokenAuthValues> { { ".*", fullAccess } },
                        Memberships = new Dictionary<string, PNTokenAuthValues> { { ".*", fullAccess } }
                    }
                })
                .DataSyncProjections(new PNDataSyncProjections
                {
                    Patterns = new PNDataSyncProjectionScope
                    {
                        Entities = new Dictionary<string, string> { { ".*", "admin" } },
                        Relationships = new Dictionary<string, string> { { ".*", "admin" } },
                        Memberships = new Dictionary<string, string> { { ".*", "admin" } }
                    }
                })
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False,
                $"Admin grant failed: {grant.Status.ErrorData?.Information}");
            pubnub.SetAuthToken(grant.Result.Token);
        }
    }
}