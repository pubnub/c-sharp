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
            };
            admin = new Pubnub(adminConfig);

            var creatorConfig = new PNConfiguration(new UserId(EntityCreatorId))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
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
                            { $"^{prefix}-.*", new PNTokenAuthValues { Get = true } }
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
            var update = await client.DataSync.SetEntity(new SetEntityParameters
            {
                Id = entityId,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = "updated",
                Payload = new Dictionary<string, object> { { "make", "Honda" } }
            });

            Assert.That(read.Status.Error, Is.False, "Get should be allowed");
            Assert.That(update.Status.Error, Is.True, "Update should be denied without Update permission");
        }

        private async Task<Pubnub> CreateClientWithEntityGrant(string entityId, PNTokenAuthValues permissions)
        {
            var grant = await admin.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId(ClientUserId))
                .Resources(new PNTokenResources
                {
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues>
                        {
                            { entityId, permissions }
                        }
                    }
                })
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);

            var client = CreateClientWithToken(grant.Result.Token);
            await Task.Delay(1000); // allow token propagation
            return client;
        }

        private async Task<PNResult<PNDataSyncEntityResult>> SetEntityAsync(Pubnub client, string entityId,
            string status, string ifMatch = null)
        {
            var response = await client.DataSync.SetEntity(new SetEntityParameters
            {
                Id = entityId,
                EntityClass = DataSyncCommon.IntegrationTestEntityClass,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = status,
                Payload = new Dictionary<string, object> { { "make", "Honda" } },
                IfMatch = ifMatch
            });
            if (response.Result != null && !createdEntityIds.Contains(entityId))
            {
                createdEntityIds.Add(entityId);
            }
            return response;
        }

        [Test]
        public async Task ThenGetAndCreateGrantShouldAllowSetToCreateEntity()
        {
            var entityId = UniqueId();
            var client = await CreateClientWithEntityGrant(entityId,
                new PNTokenAuthValues { Get = true, Create = true });

            var response = await SetEntityAsync(client, entityId, "created-by-set");

            Assert.That(response.Status.Error, Is.False,
                $"Set without IfMatch should create with Get and Create: {response.Status.ErrorData?.Information}");
            Assert.That(response.Status.StatusCode, Is.EqualTo(201));
            Assert.That(response.Result.Status, Is.EqualTo("created-by-set"));
        }

        [Test]
        public async Task ThenGetAndCreateGrantShouldDenySetOnExistingEntity()
        {
            var entityId = await CreateEntityAsync(UniqueId());
            var client = await CreateClientWithEntityGrant(entityId,
                new PNTokenAuthValues { Get = true, Create = true });

            var response = await SetEntityAsync(client, entityId, "updated");

            Assert.That(response.Status.Error, Is.True,
                "Set on an existing entity should be denied without Update permission");
        }

        [Test]
        public async Task ThenGetAndUpdateGrantShouldAllowSetWithIfMatch()
        {
            var entityId = UniqueId();
            var created = await creator.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = entityId,
                EntityClass = DataSyncCommon.IntegrationTestEntityClass,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = "active"
            });
            Assert.That(created.Status.Error, Is.False, created.Status.ErrorData?.Information);
            createdEntityIds.Add(entityId);

            var client = await CreateClientWithEntityGrant(entityId,
                new PNTokenAuthValues { Get = true, Update = true });

            var response = await SetEntityAsync(client, entityId, "updated-with-etag", created.Result.ETag);

            Assert.That(response.Status.Error, Is.False,
                $"Set with IfMatch should update with Get and Update: {response.Status.ErrorData?.Information}");
            Assert.That(response.Status.StatusCode, Is.EqualTo(200));
            Assert.That(response.Result.Status, Is.EqualTo("updated-with-etag"));
        }

        [Test]
        public async Task ThenGetAndUpdateGrantShouldDenySetCreatingEntity()
        {
            var entityId = UniqueId();
            var client = await CreateClientWithEntityGrant(entityId,
                new PNTokenAuthValues { Get = true, Update = true });

            var response = await SetEntityAsync(client, entityId, "should-not-exist");

            Assert.That(response.Status.Error, Is.True,
                "Set creating an entity should be denied without Create permission");
            var getResponse = await creator.DataSync.GetEntity(new GetEntityParameters { Id = entityId });
            Assert.That(getResponse.Status.Error, Is.True, "Entity should not have been created");
        }

        [Test]
        public async Task ThenGetCreateAndUpdateGrantShouldAllowSetToCreateAndUpdate()
        {
            var entityId = UniqueId();
            var client = await CreateClientWithEntityGrant(entityId,
                new PNTokenAuthValues { Get = true, Create = true, Update = true });

            var createResponse = await SetEntityAsync(client, entityId, "first");
            Assert.That(createResponse.Status.Error, Is.False,
                $"Set should create the entity: {createResponse.Status.ErrorData?.Information}");
            Assert.That(createResponse.Status.StatusCode, Is.EqualTo(201));

            var updateResponse = await SetEntityAsync(client, entityId, "second");
            Assert.That(updateResponse.Status.Error, Is.False,
                $"Set should update the entity: {updateResponse.Status.ErrorData?.Information}");
            Assert.That(updateResponse.Status.StatusCode, Is.EqualTo(200));
            Assert.That(updateResponse.Result.Status, Is.EqualTo("second"));
        }
    }
}
