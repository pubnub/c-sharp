using PubnubApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PubNubMessaging.Tests
{
    public class TestHarness
    {
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
            string uuidPattern = "fuu.*";
            
            var fullAccess = new PNTokenAuthValues()
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
                    }
                })
                .ExecuteAsync();

            await Task.Delay(4000);

            PubnubCommon.GrantToken = grantResult.Result?.Token;
            Assert.IsTrue(grantResult.Status.Error == false && grantResult.Result != null, 
                "GrantToken() failed.");
        }
    }
}