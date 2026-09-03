using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;
using PubNubMessaging.Tests;

namespace PubnubApi.Tests.DataSync
{
    [TestFixture]
    public class WhenDataSyncEntityEventIsReceived : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdEntityIds = new();
        
        private const int EventWaitTimeoutMs = 30 * 1000;
        private const int SubscribeSettleMs = 3000;
        
        public async Task InitWithoutProjections()
        {
            var config = new PNConfiguration(new UserId($"ds-test-{Guid.NewGuid():N}".Substring(0, 30)))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
            };
            pubnub = createPubNubInstance(config);
            config.Origin = PubnubCommon.DataSyncOrigin;
            await GenerateDataSyncTestToken(pubnub);
            createdEntityIds.Clear();
        }
        
        public async Task InitWithProjections()
        {
            var config = new PNConfiguration(new UserId($"ds-test-{Guid.NewGuid():N}".Substring(0, 30)))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
            };
            pubnub = createPubNubInstance(config);
            config.Origin = PubnubCommon.DataSyncOrigin;
            await GenerateDataSyncTestToken(pubnub, true);
            createdEntityIds.Clear();
        }

        [TearDown]
        public async Task Cleanup()
        {
            if (pubnub != null)
            {
                foreach (var id in createdEntityIds)
                {
                    try
                    {
                        await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters { Id = id });
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }
                pubnub.Destroy();
                pubnub = null;
            }
        }

        private string UniqueId() => $"test-{Guid.NewGuid():N}";

        private async Task<PNDataSyncEntityResult> CreateTestEntity(
            string id = null,
            string status = "active",
            Dictionary<string, object> payload = null)
        {
            id ??= UniqueId();
            var result = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = id,
                EntityClass = DataSyncCommon.IntegrationTestEntityClass,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = status,
                Payload = payload ?? new Dictionary<string, object>
                {
                    { "make", "Toyota" },
                    { "model", "Camry" },
                    { "year", 2025 }
                },
                
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateEntity failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdEntityIds.Add(result.Result.Id);
            return result.Result;
        }

        /// <summary>
        /// Subscribes to the given channel, runs the trigger action, and waits for a DataSync
        /// event matching the predicate. Events for an entity are published on a channel named
        /// after the entity id.
        /// </summary>
        private async Task<PNDataSyncEventResult> CaptureEventAsync(
            string channel,
            Func<PNDataSyncEventResult, bool> predicate,
            Func<Task> triggerAction)
        {
            var eventReceived = new ManualResetEvent(false);
            PNDataSyncEventResult captured = null;

            var listener = new SubscribeCallbackExt(
                (Pubnub _, PNDataSyncEventResult dataSyncEvent) =>
                {
                    if (dataSyncEvent != null && predicate(dataSyncEvent))
                    {
                        captured = dataSyncEvent;
                        eventReceived.Set();
                    }
                },
                (Pubnub _, PNStatus _) => { });

            pubnub.AddListener(listener);
            pubnub.Subscribe<object>().Channels(new[] { channel }).Execute();

            // Give the subscribe connection time to establish before triggering the change.
            await Task.Delay(SubscribeSettleMs);

            await triggerAction();

            var received = eventReceived.WaitOne(EventWaitTimeoutMs);

            pubnub.Unsubscribe<object>().Channels(new[] { channel }).Execute();
            pubnub.RemoveListener(listener);

            Assert.That(received, Is.True,
                $"Did not receive expected DataSync event on channel '{channel}' within timeout.");
            return captured;
        }

        /// <summary>
        /// Same as <see cref="CaptureEventAsync"/> but subscribes/listens on the supplied client
        /// instance instead of the fixture pubnub. Used to observe events through a projection-scoped
        /// subscriber token.
        /// </summary>
        private static async Task<PNDataSyncEventResult> CaptureEventWithClientAsync(
            Pubnub client,
            string channel,
            Func<PNDataSyncEventResult, bool> predicate,
            Func<Task> triggerAction)
        {
            var eventReceived = new ManualResetEvent(false);
            PNDataSyncEventResult captured = null;

            var listener = new SubscribeCallbackExt(
                (Pubnub _, PNDataSyncEventResult dataSyncEvent) =>
                {
                    if (dataSyncEvent != null && predicate(dataSyncEvent))
                    {
                        captured = dataSyncEvent;
                        eventReceived.Set();
                    }
                },
                (Pubnub _, PNStatus _) => { });

            client.AddListener(listener);
            client.Subscribe<object>().Channels(new[] { channel }).Execute();

            await Task.Delay(SubscribeSettleMs);

            await triggerAction();

            var received = eventReceived.WaitOne(EventWaitTimeoutMs);

            client.Unsubscribe<object>().Channels(new[] { channel }).Execute();
            client.RemoveListener(listener);

            Assert.That(received, Is.True,
                $"Did not receive expected DataSync event on channel '{channel}' within timeout.");
            return captured;
        }

        private static Pubnub BuildAdmin()
        {
            var adminConfig = new PNConfiguration(new UserId("ds-proj-evt-admin"))
            {
                SecretKey = PubnubCommon.DataSyncSecretKey,
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
                PublishKey = PubnubCommon.DataSyncPublishKey,
                Origin = PubnubCommon.DataSyncOrigin
            };
            return new Pubnub(adminConfig);
        }

        private Pubnub BuildProjectionSubscriber(string userId, string token)
        {
            var config = new PNConfiguration(new UserId(userId))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
                PublishKey = PubnubCommon.DataSyncPublishKey,
            };
            var client = createPubNubInstance(config);
            config.Origin = PubnubCommon.DataSyncOrigin;
            client.SetAuthToken(token);
            return client;
        }

        /// <summary>
        /// Resolves the channel a projected entity event is published on. The base ("__default__")
        /// projection uses the plain entity id; any non-default projection is routed to a channel
        /// prefixed with the projection name: "__&lt;projection&gt;__&lt;entityId&gt;".
        /// </summary>
        private static string ProjectionEventChannel(string projection, string entityId)
            => projection == "__default__" ? entityId : $"__{projection}__{entityId}";

        /// <summary>
        /// Grants the given user Read on the projection's event channel plus Get on the entity, with the
        /// requested projection assignment, and returns the resulting token.
        /// </summary>
        private static async Task<string> GrantProjectionTokenAsync(
            Pubnub admin, string userId, string entityId, string projection)
        {
            var eventChannel = ProjectionEventChannel(projection, entityId);
            var grant = await admin.GrantToken()
                .TTL(60)
                .AuthorizedUserId(new UserId(userId))
                .Resources(new PNTokenResources
                {
                    Channels = new Dictionary<string, PNTokenAuthValues>
                    {
                        { eventChannel, new PNTokenAuthValues { Read = true, Get = true, Join = true } }
                    },
                    DataSync = new PNDataSyncTokenScopes
                    {
                        Entities = new Dictionary<string, PNTokenAuthValues>
                        {
                            { entityId, new PNTokenAuthValues { Get = true } }
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
            return grant.Result.Token;
        }

        /// <summary>
        /// Creates a vehicle entity (through the full-access fixture pubnub) with every projected
        /// field populated so projections have data to reveal or hide.
        /// </summary>
        private async Task<PNDataSyncEntityResult> CreateProjectedVehicleAsync(string id)
        {
            var result = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = id,
                EntityClass = DataSyncCommon.IntegrationTestEntityClassWithProjections,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Payload = new Dictionary<string, object>
                {
                    { "model", "Camry" },
                    { "owner", "Alice" },
                    { "dateBought", "2024-01-01" },
                    { "comments", "test vehicle" }
                },
            });
            Assert.That(result.Status.Error, Is.False,
                $"CreateEntity failed: {result.Status.ErrorData?.Information}");
            createdEntityIds.Add(result.Result.Id);
            return result.Result;
        }

        [Test]
        public async Task ThenCreatingEntityShouldDeliverCreateEvent()
        {
            await InitWithoutProjections();
            
            var entityId = UniqueId();

            var dataSyncEvent = await CaptureEventAsync(
                entityId,
                e => string.Equals(e.Event, "create", StringComparison.OrdinalIgnoreCase)
                     && e.EntityData?.Id == entityId,
                async () =>
                {
                    var response = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
                    {
                        Id = entityId,
                        EntityClass = DataSyncCommon.IntegrationTestEntityClass,
                        EntityClassVersion = DataSyncCommon.EntityClassVersion,
                        Status = "active",
                        Payload = new Dictionary<string, object>
                        {
                            { "make", "Toyota" },
                            { "model", "Camry" },
                            { "year", 2025 }
                        },
                        
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"CreateEntity failed: {response.Status.ErrorData?.Information}");
                    createdEntityIds.Add(response.Result.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("create").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(entityId));
            Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
            Assert.That(dataSyncEvent.EntityData.Id, Is.EqualTo(entityId));
            Assert.That(dataSyncEvent.EntityData.Payload, Is.Not.Null);
        }

        [Test]
        public async Task ThenUpdatingEntityShouldDeliverUpdateEvent()
        {
            await InitWithoutProjections();
            
            var created = await CreateTestEntity(status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.EntityData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.SetEntity(new SetEntityParameters
                    {
                        Id = created.Id,
                        EntityClassVersion = DataSyncCommon.EntityClassVersion,
                        Status = "updated",
                        Payload = new Dictionary<string, object>
                        {
                            { "make", "Ford" },
                            { "model", "Focus" }
                        }
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"UpdateEntity failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
            Assert.That(dataSyncEvent.EntityData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenPatchingEntityShouldDeliverUpdateEvent()
        {
            await InitWithoutProjections();
            
            var created = await CreateTestEntity(status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.EntityData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
                    {
                        Id = created.Id,
                        Operations = new List<JsonPatchOperation>
                        {
                            new JsonPatchOperation
                            {
                                Op = JsonPatchOperationType.Replace,
                                Path = "/status",
                                Value = "patched"
                            }
                        },
                        
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"PatchEntity failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
            Assert.That(dataSyncEvent.EntityData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenDeletingEntityShouldDeliverDeleteEvent()
        {
            await InitWithoutProjections();
            
            var created = await CreateTestEntity(status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "delete", StringComparison.OrdinalIgnoreCase)
                     && e.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters
                    {
                        Id = created.Id
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"DeleteEntity failed: {response.Status.ErrorData?.Information}");
                    createdEntityIds.Remove(created.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("delete").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.Id, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.DeletedAt, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenDefaultProjectionUpdateEventShouldContainOnlyDefaultFields()
        {
            await InitWithProjections();
            
            var created = await CreateProjectedVehicleAsync(UniqueId());

            var admin = BuildAdmin();
            Pubnub subscriber = null;
            try
            {
                var userId = $"ds-proj-evt-def-{Guid.NewGuid():N}".Substring(0, 30);
                var token = await GrantProjectionTokenAsync(admin, userId, created.Id, "__default__");
                subscriber = BuildProjectionSubscriber(userId, token);
                await Task.Delay(1000); // allow token propagation

                // The base "__default__" projection is routed to the plain entity id channel.
                var channel = ProjectionEventChannel("__default__", created.Id);
                var dataSyncEvent = await CaptureEventWithClientAsync(
                    subscriber,
                    channel,
                    e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                         && e.EntityData?.Id == created.Id,
                    async () =>
                    {
                        var response = await pubnub.DataSync.SetEntity(new SetEntityParameters
                        {
                            Id = created.Id,
                            EntityClassVersion = DataSyncCommon.EntityClassVersion,
                            Payload = new Dictionary<string, object>
                            {
                                { "model", "Focus" },
                                { "owner", "Bob" },
                                { "dateBought", "2025-02-02" },
                                { "comments", "changed" }
                            }
                        });
                        Assert.That(response.Status.Error, Is.False,
                            $"UpdateEntity failed: {response.Status.ErrorData?.Information}");
                    });

                Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
                var payload = dataSyncEvent.EntityData.Payload;
                Assert.That(payload, Is.Not.Null);
                Assert.That(payload.ContainsKey("model"), Is.True, "model should be visible in __default__");
                Assert.That(payload.ContainsKey("owner"), Is.True, "owner should be visible in __default__");
                Assert.That(payload.ContainsKey("dateBought"), Is.False, "dateBought is admin-only");
                Assert.That(payload.ContainsKey("comments"), Is.False, "comments is admin-only");
            }
            finally
            {
                try { subscriber?.Destroy(); } catch { /* ignore */ }
                try { admin.Destroy(); } catch { /* ignore */ }
            }
        }

        [Test]
        public async Task ThenAdminProjectionUpdateEventShouldContainAllFields()
        {
            await InitWithProjections();
            
            var created = await CreateProjectedVehicleAsync(UniqueId());

            var admin = BuildAdmin();
            Pubnub subscriber = null;
            try
            {
                var userId = $"ds-proj-evt-adm-{Guid.NewGuid():N}".Substring(0, 30);
                var token = await GrantProjectionTokenAsync(admin, userId, created.Id, "admin");
                subscriber = BuildProjectionSubscriber(userId, token);
                await Task.Delay(1000); // allow token propagation

                // Non-default projections publish to a projection-prefixed channel, not the plain entity id.
                var channel = ProjectionEventChannel("admin", created.Id);
                var dataSyncEvent = await CaptureEventWithClientAsync(
                    subscriber,
                    channel,
                    e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                         && e.EntityData?.Id == created.Id,
                    async () =>
                    {
                        var response = await pubnub.DataSync.SetEntity(new SetEntityParameters
                        {
                            Id = created.Id,
                            EntityClassVersion = DataSyncCommon.EntityClassVersion,
                            Payload = new Dictionary<string, object>
                            {
                                { "model", "Focus" },
                                { "owner", "Bob" },
                                { "dateBought", "2025-02-02" },
                                { "comments", "changed" }
                            }
                        });
                        Assert.That(response.Status.Error, Is.False,
                            $"UpdateEntity failed: {response.Status.ErrorData?.Information}");
                    });

                Assert.That(dataSyncEvent.Channel, Is.EqualTo(channel),
                    "admin projection events should be routed to the projection-prefixed channel");
                Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
                var payload = dataSyncEvent.EntityData.Payload;
                Assert.That(payload, Is.Not.Null);
                Assert.That(payload.ContainsKey("model"), Is.True);
                Assert.That(payload.ContainsKey("owner"), Is.True);
                Assert.That(payload.ContainsKey("dateBought"), Is.True, "dateBought should be visible in admin");
                Assert.That(payload.ContainsKey("comments"), Is.True, "comments should be visible in admin");
            }
            finally
            {
                try { subscriber?.Destroy(); } catch { /* ignore */ }
                try { admin.Destroy(); } catch { /* ignore */ }
            }
        }

        [Test]
        public async Task ThenDefaultProjectionCreateEventShouldContainOnlyDefaultFields()
        {
            await InitWithProjections();
            
            var entityId = UniqueId();

            var admin = BuildAdmin();
            Pubnub subscriber = null;
            try
            {
                var userId = $"ds-proj-evt-crt-{Guid.NewGuid():N}".Substring(0, 30);
                var token = await GrantProjectionTokenAsync(admin, userId, entityId, "__default__");
                subscriber = BuildProjectionSubscriber(userId, token);
                await Task.Delay(1000); // allow token propagation

                // The base "__default__" projection is routed to the plain entity id channel.
                var channel = ProjectionEventChannel("__default__", entityId);
                var dataSyncEvent = await CaptureEventWithClientAsync(
                    subscriber,
                    channel,
                    e => string.Equals(e.Event, "create", StringComparison.OrdinalIgnoreCase)
                         && e.EntityData?.Id == entityId,
                    async () =>
                    {
                        await CreateProjectedVehicleAsync(entityId);
                    });

                Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
                var payload = dataSyncEvent.EntityData.Payload;
                Assert.That(payload, Is.Not.Null);
                Assert.That(payload.ContainsKey("model"), Is.True, "model should be visible in __default__");
                Assert.That(payload.ContainsKey("owner"), Is.True, "owner should be visible in __default__");
                Assert.That(payload.ContainsKey("dateBought"), Is.False, "dateBought is admin-only");
                Assert.That(payload.ContainsKey("comments"), Is.False, "comments is admin-only");
            }
            finally
            {
                try { subscriber?.Destroy(); } catch { /* ignore */ }
                try { admin.Destroy(); } catch { /* ignore */ }
            }
        }
    }
}
