using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;
using PubNubMessaging.Tests;

namespace PubnubApi.Tests.DataSync
{
    [TestFixture]
    public class WhenDataSyncGrantIsRequested : TestHarness
    {
        private const string AdminUserId = "ds-pam-admin";
        private const string EntityCreatorId = "ds-pam-creator";
        private const string ClientUserId = "ds-pam-client";

        private Pubnub admin;
        private Pubnub creator;
        private readonly List<Pubnub> clients = new();
        private readonly List<string> createdEntityIds = new();

        [SetUp]
        public async Task Init()
        {
            var adminConfig = new PNConfiguration(new UserId(AdminUserId))
            {
                SecretKey = PubnubCommon.DataSyncSecretKey,
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
                PublishKey = PubnubCommon.DataSyncPublishKey,
                Origin = PubnubCommon.DataSyncOrigin
            };
            admin = new Pubnub(adminConfig);

            var creatorConfig = new PNConfiguration(new UserId(EntityCreatorId))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
                Origin = PubnubCommon.DataSyncOrigin
            };
            creator = new Pubnub(creatorConfig);
            
            clients.Clear();
            createdEntityIds.Clear();

            await GenerateDataSyncTestToken(creator);
            
            await Task.Delay(1000); // allow token propagation
        }

        [TearDown]
        public async Task Cleanup()
        {
            foreach (var id in createdEntityIds)
            {
                try
                {
                    await creator.DataSync.DeleteEntity(new DeleteEntityParameters { Id = id });
                }
                catch
                {
                    // best-effort cleanup
                }
            }

            foreach (var client in clients)
            {
                try { client.Destroy(); } catch { /* ignore */ }
            }
            clients.Clear();

            if (admin != null)
            {
                admin.Destroy();
                admin = null;
            }
            if (creator != null)
            {
                creator.Destroy();
                creator = null;
            }
        }

        private string UniqueId() => $"test-{Guid.NewGuid():N}";

        private Pubnub CreateClientWithToken(string token)
        {
            var config = new PNConfiguration(new UserId(ClientUserId))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
                PublishKey = PubnubCommon.DataSyncPublishKey,
            };
            var client = createPubNubInstance(config);
            config.Origin = PubnubCommon.DataSyncOrigin;
            client.SetAuthToken(token);
            clients.Add(client);
            return client;
        }

        private async Task<string> CreateEntityAsync(string id)
        {
            var result = await creator.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = id,
                EntityClass = DataSyncCommon.IntegrationTestEntityClass,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "make", "Toyota" } },
                
            });
            Assert.That(result.Status.Error, Is.False, result.Status.ErrorData?.Information);
            createdEntityIds.Add(result.Result.Id);
            return result.Result.Id;
        }

        [Test]
        public async Task ThenEntityResourceGrantShouldAllowGrantedAndDenyOthers()
        {
            var grantedId = await CreateEntityAsync(UniqueId());
            var otherId = await CreateEntityAsync(UniqueId());

            var grant = await admin.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId(ClientUserId))
                .Resources(new PNTokenResources
                {
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues>
                        {
                            { grantedId, new PNTokenAuthValues { Get = true } }
                        }
                    }
                })
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);

            var client = CreateClientWithToken(grant.Result.Token);
            await Task.Delay(1000); // allow token propagation

            var allowed = await client.DataSync.GetEntity(new GetEntityParameters { Id = grantedId });
            var denied = await client.DataSync.GetEntity(new GetEntityParameters { Id = otherId });

            Assert.That(allowed.Status.Error, Is.False, "Granted entity should be readable");
            Assert.That(denied.Status.Error, Is.True, "Non-granted entity should be denied");
        }

        [Test]
        public async Task ThenEntityPatternGrantShouldAllowMatchingAndDenyNonMatching()
        {
            var prefix = $"grant{Guid.NewGuid():N}".Substring(0, 16);
            var matchingId = await CreateEntityAsync($"{prefix}-match");
            var nonMatchingId = await CreateEntityAsync(UniqueId());

            var grant = await admin.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId(ClientUserId))
                .Patterns(new PNTokenPatterns
                {
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues>
                        {
                            { $"^{prefix}-.*$", new PNTokenAuthValues { Get = true } }
                        }
                    }
                })
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);

            var client = CreateClientWithToken(grant.Result.Token);
            await Task.Delay(1000);

            var allowed = await client.DataSync.GetEntity(new GetEntityParameters { Id = matchingId });
            var denied = await client.DataSync.GetEntity(new GetEntityParameters { Id = nonMatchingId });

            Assert.That(allowed.Status.Error, Is.False, "Pattern-matching entity should be readable");
            Assert.That(denied.Status.Error, Is.True, "Non-matching entity should be denied");
        }

        [Test]
        public async Task ThenGetOnlyGrantShouldDenyUpdate()
        {
            var entityId = await CreateEntityAsync(UniqueId());

            var grant = await admin.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId(ClientUserId))
                .Resources(new PNTokenResources
                {
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues>
                        {
                            { entityId, new PNTokenAuthValues { Get = true } }
                        }
                    }
                })
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);

            var client = CreateClientWithToken(grant.Result.Token);
            await Task.Delay(1000);

            var read = await client.DataSync.GetEntity(new GetEntityParameters { Id = entityId });
            var update = await client.DataSync.UpdateEntity(new UpdateEntityParameters
            {
                Id = entityId,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = "updated",
                Payload = new Dictionary<string, object> { { "make", "Honda" } }
            });

            Assert.That(read.Status.Error, Is.False, "Get should be allowed");
            Assert.That(update.Status.Error, Is.True, "Update should be denied without Update permission");
        }
    }
}
