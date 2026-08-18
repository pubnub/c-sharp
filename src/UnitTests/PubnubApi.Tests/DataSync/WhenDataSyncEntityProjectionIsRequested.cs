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
    public class WhenDataSyncEntityProjectionIsRequested : TestHarness
    {
        private const string AdminUserId = "ds-proj-admin";
        private const string EntityCreatorId = "ds-proj-creator";
        private const string ClientUserId = "ds-proj-client";

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

        /// <summary>
        /// Creates a vehicle entity through the (full-access) creator with every projected
        /// field populated, so both the "__default__" and "admin" projections have data to
        /// reveal or hide.
        /// </summary>
        private async Task<string> CreateVehicleAsync(string id)
        {
            var result = await creator.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = id,
                EntityClass = DataSyncCommon.IntegrationTestEntityClass,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object>
                {
                    { "model", "Camry" },
                    { "owner", "Alice" },
                    { "dateBought", "2024-01-01" },
                    { "comments", "test vehicle" }
                },
            });
            Assert.That(result.Status.Error, Is.False, result.Status.ErrorData?.Information);
            createdEntityIds.Add(result.Result.Id);
            return result.Result.Id;
        }

        /// <summary>
        /// Grants the client user the given DataSync permissions on a single entity together
        /// with a projection assignment, then returns a client configured with that token.
        /// </summary>
        private async Task<Pubnub> GrantProjectionClientAsync(string entityId, string projection, PNTokenAuthValues perms)
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
                            { entityId, perms }
                        }
                    }
                })
                .DataSyncProjections(new PNDataSyncProjections
                {
                    Resources = new PNDataSyncProjectionScope
                    {
                        Entities = new Dictionary<string, string> { { entityId, projection } }
                    }
                })
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);

            var client = CreateClientWithToken(grant.Result.Token);
            await Task.Delay(1000); // allow token propagation
            return client;
        }

        #region Read visibility

        [Test]
        public async Task ThenDefaultProjectionGetShouldExposeOnlyDefaultFields()
        {
            var id = await CreateVehicleAsync(UniqueId());
            var client = await GrantProjectionClientAsync(id, "__default__",
                new PNTokenAuthValues { Get = true });

            var response = await client.DataSync.GetEntity(new GetEntityParameters { Id = id });

            Assert.That(response.Status.Error, Is.False, response.Status.ErrorData?.Information);
            var payload = response.Result.Payload;
            Assert.That(payload, Is.Not.Null);
            Assert.That(payload.ContainsKey("model"), Is.True, "model should be visible in __default__");
            Assert.That(payload.ContainsKey("owner"), Is.True, "owner should be visible in __default__");
            Assert.That(payload.ContainsKey("dateBought"), Is.False, "dateBought is admin-only");
            Assert.That(payload.ContainsKey("comments"), Is.False, "comments is admin-only");
        }

        [Test]
        public async Task ThenAdminProjectionGetShouldExposeAllFields()
        {
            var id = await CreateVehicleAsync(UniqueId());
            var client = await GrantProjectionClientAsync(id, "admin",
                new PNTokenAuthValues { Get = true });

            var response = await client.DataSync.GetEntity(new GetEntityParameters { Id = id });

            Assert.That(response.Status.Error, Is.False, response.Status.ErrorData?.Information);
            var payload = response.Result.Payload;
            Assert.That(payload, Is.Not.Null);
            Assert.That(payload.ContainsKey("model"), Is.True);
            Assert.That(payload.ContainsKey("owner"), Is.True);
            Assert.That(payload.ContainsKey("dateBought"), Is.True, "dateBought should be visible in admin");
            Assert.That(payload.ContainsKey("comments"), Is.True, "comments should be visible in admin");
        }

        [Test]
        public async Task ThenPatternProjectionShouldApplyToMatchingEntities()
        {
            var prefix = $"proj{Guid.NewGuid():N}".Substring(0, 16);
            var id = await CreateVehicleAsync($"{prefix}-match");

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
                .DataSyncProjections(new PNDataSyncProjections
                {
                    Patterns = new PNDataSyncProjectionScope
                    {
                        Entities = new Dictionary<string, string> { { $"^{prefix}-.*$", "__default__" } }
                    }
                })
                .ExecuteAsync();
            Assert.That(grant.Status.Error, Is.False, grant.Status.ErrorData?.Information);

            var client = CreateClientWithToken(grant.Result.Token);
            await Task.Delay(1000);

            var response = await client.DataSync.GetEntity(new GetEntityParameters { Id = id });

            Assert.That(response.Status.Error, Is.False, response.Status.ErrorData?.Information);
            var payload = response.Result.Payload;
            Assert.That(payload, Is.Not.Null);
            Assert.That(payload.ContainsKey("model"), Is.True);
            Assert.That(payload.ContainsKey("owner"), Is.True);
            Assert.That(payload.ContainsKey("dateBought"), Is.False, "dateBought is admin-only");
            Assert.That(payload.ContainsKey("comments"), Is.False, "comments is admin-only");
        }

        #endregion

        #region Write behavior

        [Test]
        public async Task ThenDefaultProjectionUpdateVisibleFieldShouldSucceed()
        {
            var id = await CreateVehicleAsync(UniqueId());
            var client = await GrantProjectionClientAsync(id, "__default__",
                new PNTokenAuthValues { Get = true, Update = true });

            var update = await client.DataSync.UpdateEntity(new UpdateEntityParameters
            {
                Id = id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/model",
                        Value = "UpdatedModel"
                    }
                }
            });
            Assert.That(update.Status.Error, Is.False, update.Status.ErrorData?.Information);

            // Verify through the full-access creator that the visible field changed while the
            // admin-only fields were left intact.
            var adminView = await creator.DataSync.GetEntity(new GetEntityParameters { Id = id });
            Assert.That(adminView.Status.Error, Is.False, adminView.Status.ErrorData?.Information);
            Assert.That(adminView.Result.Payload["model"]?.ToString(), Is.EqualTo("UpdatedModel"));
            Assert.That(adminView.Result.Payload.ContainsKey("dateBought"), Is.True,
                "admin-only field should remain after a __default__ update");
            Assert.That(adminView.Result.Payload.ContainsKey("comments"), Is.True,
                "admin-only field should remain after a __default__ update");
        }

        [Test]
        public async Task ThenDefaultProjectionWriteToNonVisibleFieldShouldBeRejected()
        {
            var id = await CreateVehicleAsync(UniqueId());
            var client = await GrantProjectionClientAsync(id, "__default__",
                new PNTokenAuthValues { Get = true, Update = true });

            var patch = await client.DataSync.UpdateEntity(new UpdateEntityParameters
            {
                Id = id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/dateBought",
                        Value = "2030-12-31"
                    }
                }
            });

            if (patch.Status.Error)
            {
                Assert.That(patch.Status.Error, Is.True,
                    "Writing an admin-only field through the __default__ projection should be rejected.");
            }
            else
            {
                // If the server accepted the request, the admin-only field must not have changed.
                var adminView = await creator.DataSync.GetEntity(new GetEntityParameters { Id = id });
                Assert.That(adminView.Status.Error, Is.False, adminView.Status.ErrorData?.Information);
                Assert.That(adminView.Result.Payload["dateBought"]?.ToString(), Is.Not.EqualTo("2030-12-31"),
                    "Writing an admin-only field through the __default__ projection must not persist.");
            }
        }

        [Test]
        public async Task ThenAdminProjectionUpdateShouldPersistAllFields()
        {
            var id = await CreateVehicleAsync(UniqueId());
            var client = await GrantProjectionClientAsync(id, "admin",
                new PNTokenAuthValues { Get = true, Update = true });

            var update = await client.DataSync.SetEntity(new SetEntityParameters
            {
                Id = id,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = "updated",
                Payload = new Dictionary<string, object>
                {
                    { "model", "AdminModel" },
                    { "owner", "AdminOwner" },
                    { "dateBought", "2025-05-05" },
                    { "comments", "admin updated" }
                }
            });
            Assert.That(update.Status.Error, Is.False, update.Status.ErrorData?.Information);

            var view = await client.DataSync.GetEntity(new GetEntityParameters { Id = id });
            Assert.That(view.Status.Error, Is.False, view.Status.ErrorData?.Information);
            var payload = view.Result.Payload;
            Assert.That(payload.ContainsKey("model"), Is.True);
            Assert.That(payload.ContainsKey("owner"), Is.True);
            Assert.That(payload.ContainsKey("dateBought"), Is.True);
            Assert.That(payload.ContainsKey("comments"), Is.True);
            Assert.That(payload["dateBought"]?.ToString(), Is.EqualTo("2025-05-05"));
        }

        #endregion
    }
}
