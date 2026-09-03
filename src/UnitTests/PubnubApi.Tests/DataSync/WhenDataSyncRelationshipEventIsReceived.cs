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
    public class WhenDataSyncRelationshipEventIsReceived : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdRelationshipIds = new();
        private readonly List<string> createdEntityIds = new();
        
        private const int EventWaitTimeoutMs = 30 * 1000;
        private const int SubscribeSettleMs = 3000;

        [SetUp]
        public async Task Init()
        {
            var config = new PNConfiguration(new UserId($"ds-test-{Guid.NewGuid():N}".Substring(0, 30)))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
            };
            pubnub = createPubNubInstance(config);
            config.Origin = PubnubCommon.DataSyncOrigin;
            await GenerateDataSyncTestToken(pubnub);
            createdRelationshipIds.Clear();
            createdEntityIds.Clear();
        }

        [TearDown]
        public async Task Cleanup()
        {
            if (pubnub != null)
            {
                foreach (var id in createdRelationshipIds)
                {
                    try
                    {
                        await pubnub.DataSync.DeleteRelationship(new DeleteRelationshipParameters { Id = id });
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }

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

        private async Task<PNDataSyncEntityResult> CreateTestEntity(string id = null)
        {
            id ??= UniqueId();
            var result = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = id,
                EntityClass = DataSyncCommon.IntegrationTestEntityClass,
                EntityClassVersion = DataSyncCommon.EntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "name", $"entity-{id}" } },
                
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateEntity failed: {result.Status.ErrorData?.Information}");
            createdEntityIds.Add(result.Result.Id);
            return result.Result;
        }

        private async Task<PNDataSyncRelationshipResult> CreateTestRelationship(
            string id = null,
            string status = "active",
            Dictionary<string, object> payload = null)
        {
            var entityA = await CreateTestEntity();
            var entityB = await CreateTestEntity();

            var result = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                Id = id,
                EntityAId = entityA.Id,
                EntityBId = entityB.Id,
                RelationshipClass = DataSyncCommon.IntegrationTestRelationshipClass,
                RelationshipClassVersion = DataSyncCommon.RelationshipClassVersion,
                Status = status,
                Payload = payload ?? new Dictionary<string, object>
                {
                    { "role", "owner" },
                    { "since", "2025-01-01" }
                },
                
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateRelationship failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdRelationshipIds.Add(result.Result.Id);
            return result.Result;
        }

        /// <summary>
        /// Subscribes to the given channel, runs the trigger action, and waits for a DataSync
        /// event matching the predicate. Events for a relationship are published on a channel
        /// named after the relationship id.
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

        [Test]
        public async Task ThenCreatingRelationshipShouldDeliverCreateEvent()
        {
            var entityA = await CreateTestEntity();
            var entityB = await CreateTestEntity();
            var relationshipId = UniqueId();

            var dataSyncEvent = await CaptureEventAsync(
                entityA.Id,
                e => string.Equals(e.Event, "create", StringComparison.OrdinalIgnoreCase)
                     && e.RelationshipData?.Id == relationshipId,
                async () =>
                {
                    var response = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
                    {
                        Id = relationshipId,
                        EntityAId = entityA.Id,
                        EntityBId = entityB.Id,
                        RelationshipClass = DataSyncCommon.IntegrationTestRelationshipClass,
                        RelationshipClassVersion = DataSyncCommon.RelationshipClassVersion,
                        Status = "active",
                        Payload = new Dictionary<string, object>
                        {
                            { "role", "owner" },
                            { "since", "2025-01-01" }
                        },
                        
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"CreateRelationship failed: {response.Status.ErrorData?.Information}");
                    createdRelationshipIds.Add(response.Result.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("create").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(entityA.Id));
            Assert.That(dataSyncEvent.RelationshipData, Is.Not.Null);
            Assert.That(dataSyncEvent.RelationshipData.Id, Is.EqualTo(relationshipId));
            Assert.That(dataSyncEvent.RelationshipData.EntityAId, Is.EqualTo(entityA.Id));
            Assert.That(dataSyncEvent.RelationshipData.EntityBId, Is.EqualTo(entityB.Id));
            Assert.That(dataSyncEvent.RelationshipData.Payload, Is.Not.Null);
        }

        [Test]
        public async Task ThenUpdatingRelationshipShouldDeliverUpdateEvent()
        {
            var created = await CreateTestRelationship(id: UniqueId(), status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.EntityAId,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.RelationshipData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.SetRelationship(new SetRelationshipParameters
                    {
                        Id = created.Id,
                        RelationshipClassVersion = DataSyncCommon.RelationshipClassVersion,
                        Status = "updated",
                        Payload = new Dictionary<string, object>
                        {
                            { "role", "admin" },
                            { "promoted", true }
                        }
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"UpdateRelationship failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.EntityAId));
            Assert.That(dataSyncEvent.RelationshipData, Is.Not.Null);
            Assert.That(dataSyncEvent.RelationshipData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenPatchingRelationshipShouldDeliverUpdateEvent()
        {
            var created = await CreateTestRelationship(id: UniqueId(), status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.EntityAId,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.RelationshipData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
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
                        $"PatchRelationship failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.EntityAId));
            Assert.That(dataSyncEvent.RelationshipData, Is.Not.Null);
            Assert.That(dataSyncEvent.RelationshipData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenDeletingRelationshipShouldDeliverDeleteEvent()
        {
            var created = await CreateTestRelationship(id: UniqueId(), status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.EntityAId,
                e => string.Equals(e.Event, "delete", StringComparison.OrdinalIgnoreCase)
                     && e.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.DeleteRelationship(new DeleteRelationshipParameters
                    {
                        Id = created.Id
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"DeleteRelationship failed: {response.Status.ErrorData?.Information}");
                    createdRelationshipIds.Remove(created.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("delete").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.EntityAId));
            Assert.That(dataSyncEvent.Id, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.DeletedAt, Is.Not.Null.And.Not.Empty);
        }
    }
}
