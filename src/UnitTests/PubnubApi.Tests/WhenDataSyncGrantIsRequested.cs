using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenDataSyncGrantIsRequested : TestHarness
    {
        private static Pubnub pubnub;

        [SetUp]
        public void Init()
        {
            PNConfiguration config = new PNConfiguration(new UserId("datasync-grant-uuid"))
            {
                PublishKey = PubnubCommon.PublishKey,
                SubscribeKey = PubnubCommon.SubscribeKey,
                SecretKey = PubnubCommon.SecretKey,
            };
            pubnub = createPubNubInstance(config);
        }

        [TearDown]
        public void Cleanup()
        {
            if (pubnub != null)
            {
                pubnub.Destroy();
                pubnub.PubnubUnitTest = null;
                pubnub = null;
            }
        }

        private static string GetGrantBody(GrantTokenOperation operation)
        {
            MethodInfo method = typeof(GrantTokenOperation).GetMethod(
                "CreateRequestParameter", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "CreateRequestParameter not found via reflection");
            object requestParameter = method.Invoke(operation, null);
            PropertyInfo bodyProperty = requestParameter.GetType().GetProperty("BodyContentString");
            return (string)bodyProperty.GetValue(requestParameter);
        }

        [Test]
        public void ThenDataSyncResourceScopesShouldBeSerialized()
        {
            GrantTokenOperation operation = pubnub.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId("user-123"))
                .Resources(new PNTokenResources
                {
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues>
                        {
                            { "order-456", new PNTokenAuthValues { Get = true, Update = true } } // 32 + 64 = 96
                        },
                        Relationships = new Dictionary<string, PNTokenAuthValues>
                        {
                            { "user.A:channel.X", new PNTokenAuthValues { Get = true } } // 32
                        },
                        Memberships = new Dictionary<string, PNTokenAuthValues>
                        {
                            { "user-123:channel-X", new PNTokenAuthValues { Get = true } } // 32
                        }
                    }
                });

            string body = GetGrantBody(operation);

            Assert.That(body, Does.Contain("\"datasync:entities\":{\"order-456\":96}"));
            Assert.That(body, Does.Contain("\"datasync:relationships\":{\"user.A:channel.X\":32}"));
            Assert.That(body, Does.Contain("\"datasync:memberships\":{\"user-123:channel-X\":32}"));
        }

        [Test]
        public void ThenDataSyncPatternScopesShouldBeSerialized()
        {
            GrantTokenOperation operation = pubnub.GrantToken()
                .TTL(60)
                .Patterns(new PNTokenPatterns
                {
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues>
                        {
                            { "order-*", new PNTokenAuthValues { Get = true } } // 32
                        }
                    }
                });

            string body = GetGrantBody(operation);

            Assert.That(body, Does.Contain("\"patterns\":"));
            Assert.That(body, Does.Contain("\"datasync:entities\":{\"order-*\":32}"));
        }

        [Test]
        public void ThenProjectionsShouldBeSerializedIntoMeta()
        {
            GrantTokenOperation operation = pubnub.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId("user-123"))
                .DataSyncProjections(new PNDataSyncProjections
                {
                    Resources = new PNDataSyncProjectionScope
                    {
                        Entities = new Dictionary<string, string> { { "order-456", "admin" } },
                        Users = new Dictionary<string, string> { { "alice", "admin" } },
                        Channels = new Dictionary<string, string> { { "general", "admin" } },
                        Relationships = new Dictionary<string, string> { { "user.A:channel.X", "admin" } }
                    },
                    Patterns = new PNDataSyncProjectionScope
                    {
                        Entities = new Dictionary<string, string> { { "order-*", "__default__" } },
                        Users = new Dictionary<string, string> { { "user-*", "__default__" } },
                        Channels = new Dictionary<string, string> { { "channel-*", "__default__" } }
                    }
                });

            string body = GetGrantBody(operation);

            Assert.That(body, Does.Contain("\"pn-projections\":"));
            Assert.That(body, Does.Contain("\"datasync:entities:order-456\":\"admin\""));
            Assert.That(body, Does.Contain("\"datasync:users:alice\":\"admin\""));
            Assert.That(body, Does.Contain("\"datasync:channels:general\":\"admin\""));
            Assert.That(body, Does.Contain("\"datasync:relationships:user.A:channel.X\":\"admin\""));
            Assert.That(body, Does.Contain("\"datasync:entities:order-*\":\"__default__\""));
            Assert.That(body, Does.Contain("\"datasync:users:user-*\":\"__default__\""));
            Assert.That(body, Does.Contain("\"datasync:channels:channel-*\":\"__default__\""));
        }

        [Test]
        public void ThenProjectionsShouldPreserveUserMeta()
        {
            GrantTokenOperation operation = pubnub.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId("user-123"))
                .Meta(new Dictionary<string, object> { { "app_version", "2.0" } })
                .DataSyncProjections(new PNDataSyncProjections
                {
                    Resources = new PNDataSyncProjectionScope
                    {
                        Entities = new Dictionary<string, string> { { "user.A", "admin" } }
                    }
                });

            string body = GetGrantBody(operation);

            Assert.That(body, Does.Contain("\"app_version\":\"2.0\""));
            Assert.That(body, Does.Contain("\"pn-projections\":"));
        }

        [Test]
        public void ThenGrantWithoutDataSyncShouldNotContainDataSyncKeys()
        {
            GrantTokenOperation operation = pubnub.GrantToken()
                .TTL(60)
                .Resources(new PNTokenResources
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>
                    {
                        { "my-channel", new PNTokenAuthValues { Read = true, Write = true } }
                    }
                });

            string body = GetGrantBody(operation);

            Assert.That(body, Does.Not.Contain("datasync:"));
            Assert.That(body, Does.Not.Contain("pn-projections"));
        }

        [Test]
        public void ThenDecodeProjectionsShouldMapFlatKeysToScopes()
        {
            // Mirrors the CBOR-decoded shape passed to TokenManager.DecodeProjections:
            // Dictionary<object, object> with "res"/"pat" -> { flat composite key -> projection name }.
            var projectionsCbor = new Dictionary<object, object>
            {
                {
                    "res", new Dictionary<object, object>
                    {
                        { "datasync:entities:order-456", "admin" },
                        { "datasync:users:alice", "admin" },
                        { "datasync:channels:general", "admin" },
                        { "datasync:relationships:user.A:channel.X", "admin" },
                        { "datasync:memberships:user-123:channel-X", "__default__" }
                    }
                },
                {
                    "pat", new Dictionary<object, object>
                    {
                        { "datasync:entities:order-*", "__default__" },
                        { "datasync:users:user-*", "__default__" },
                        { "datasync:channels:channel-*", "__default__" }
                    }
                }
            };

            var tokenManager = new TokenManager(
                new PNConfiguration(new UserId("decode-uuid")),
                pubnub.JsonPluggableLibrary,
                Guid.NewGuid().ToString());

            MethodInfo decode = typeof(TokenManager).GetMethod(
                "DecodeProjections", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(decode, Is.Not.Null, "DecodeProjections not found via reflection");

            var projections = (PNDataSyncProjections)decode.Invoke(tokenManager, new object[] { projectionsCbor });

            Assert.That(projections, Is.Not.Null);
            Assert.That(projections.Resources, Is.Not.Null);
            Assert.That(projections.Resources.Entities["order-456"], Is.EqualTo("admin"));
            Assert.That(projections.Resources.Users["alice"], Is.EqualTo("admin"));
            Assert.That(projections.Resources.Channels["general"], Is.EqualTo("admin"));
            Assert.That(projections.Resources.Relationships["user.A:channel.X"], Is.EqualTo("admin"));
            Assert.That(projections.Resources.Memberships["user-123:channel-X"], Is.EqualTo("__default__"));
            Assert.That(projections.Patterns.Entities["order-*"], Is.EqualTo("__default__"));
            Assert.That(projections.Patterns.Users["user-*"], Is.EqualTo("__default__"));
            Assert.That(projections.Patterns.Channels["channel-*"], Is.EqualTo("__default__"));
        }
    }
}
