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
    public class WhenDataSyncMembershipEventIsReceived : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdMembershipIds = new();
        private readonly List<string> createdUserIds = new();
        private readonly List<string> createdChannelIds = new();

        private const int TestRelationshipClassVersion = 1;
        private const int TestEntityClassVersion = 1;
        private const int EventWaitTimeoutMs = 30 * 1000;
        private const int SubscribeSettleMs = 3000;

        [SetUp]
        public void Init()
        {
            var config = new PNConfiguration(new UserId($"ds-test-{Guid.NewGuid():N}".Substring(0, 30)))
            {
                SubscribeKey = PubnubCommon.DataSyncSubscribeKey,
            };
            pubnub = createPubNubInstance(config);
            config.Origin = PubnubCommon.DataSyncOrigin;
            createdMembershipIds.Clear();
            createdUserIds.Clear();
            createdChannelIds.Clear();
        }

        [TearDown]
        public async Task Cleanup()
        {
            if (pubnub != null)
            {
                foreach (var id in createdMembershipIds)
                {
                    try
                    {
                        await pubnub.DataSync.DeleteMembership(new DeleteMembershipParameters { Id = id });
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }

                foreach (var id in createdUserIds)
                {
                    try
                    {
                        await pubnub.DataSync.DeleteUser(new DeleteUserParameters { Id = id });
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }

                foreach (var id in createdChannelIds)
                {
                    try
                    {
                        await pubnub.DataSync.DeleteChannel(new DeleteChannelParameters { Id = id });
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

        private async Task<PNDataSyncUserResult> CreateTestUser(string id = null)
        {
            id ??= UniqueId();
            var result = await pubnub.DataSync.CreateUser(new CreateUserParameters
            {
                Id = id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "name", $"user-{id}" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateUser failed: {result.Status.ErrorData?.Information}");
            createdUserIds.Add(result.Result.Id);
            return result.Result;
        }

        private async Task<PNDataSyncChannelResult> CreateTestChannel(string id = null)
        {
            id ??= UniqueId();
            var result = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
            {
                Id = id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "name", $"channel-{id}" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateChannel failed: {result.Status.ErrorData?.Information}");
            createdChannelIds.Add(result.Result.Id);
            return result.Result;
        }

        private async Task<PNDataSyncMembershipResult> CreateTestMembership(
            string id = null,
            string status = "active",
            Dictionary<string, object> payload = null)
        {
            var channel = await CreateTestChannel();
            var user = await CreateTestUser();

            var result = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                Id = id,
                ChannelId = channel.Id,
                UserId = user.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = status,
                Payload = payload ?? new Dictionary<string, object>
                {
                    { "role", "member" },
                    { "joinedAt", "2025-01-01" }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateMembership failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdMembershipIds.Add(result.Result.Id);
            return result.Result;
        }

        /// <summary>
        /// Subscribes to the given channel, runs the trigger action, and waits for a DataSync
        /// event matching the predicate. Events for a membership are published on a channel named
        /// after the membership id. Memberships are modeled as DataSync relationships, so
        /// create/update events populate <see cref="PNDataSyncEventResult.RelationshipData"/>.
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
        public async Task ThenCreatingMembershipShouldDeliverCreateEvent()
        {
            var channel = await CreateTestChannel();
            var user = await CreateTestUser();
            var membershipId = UniqueId();

            var dataSyncEvent = await CaptureEventAsync(
                membershipId,
                e => string.Equals(e.Event, "create", StringComparison.OrdinalIgnoreCase)
                     && e.RelationshipData?.Id == membershipId,
                async () =>
                {
                    var response = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
                    {
                        Id = membershipId,
                        ChannelId = channel.Id,
                        UserId = user.Id,
                        RelationshipClassVersion = TestRelationshipClassVersion,
                        Status = "active",
                        Payload = new Dictionary<string, object>
                        {
                            { "role", "member" },
                            { "joinedAt", "2025-01-01" }
                        },
                        IdempotencyKey = Guid.NewGuid().ToString()
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"CreateMembership failed: {response.Status.ErrorData?.Information}");
                    createdMembershipIds.Add(response.Result.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("create").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(membershipId));
            Assert.That(dataSyncEvent.RelationshipData, Is.Not.Null);
            Assert.That(dataSyncEvent.RelationshipData.Id, Is.EqualTo(membershipId));
            // ChannelId maps to entityAId, UserId maps to entityBId on the wire.
            Assert.That(dataSyncEvent.RelationshipData.EntityAId, Is.EqualTo(channel.Id));
            Assert.That(dataSyncEvent.RelationshipData.EntityBId, Is.EqualTo(user.Id));
            Assert.That(dataSyncEvent.RelationshipData.Payload, Is.Not.Null);
        }

        [Test]
        public async Task ThenUpdatingMembershipShouldDeliverUpdateEvent()
        {
            var created = await CreateTestMembership(id: UniqueId(), status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.RelationshipData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
                    {
                        Id = created.Id,
                        RelationshipClassVersion = TestRelationshipClassVersion,
                        Status = "updated",
                        Payload = new Dictionary<string, object>
                        {
                            { "role", "admin" },
                            { "promoted", true }
                        }
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"UpdateMembership failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.RelationshipData, Is.Not.Null);
            Assert.That(dataSyncEvent.RelationshipData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenPatchingMembershipShouldDeliverUpdateEvent()
        {
            var created = await CreateTestMembership(id: UniqueId(), status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.RelationshipData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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
                        IdempotencyKey = Guid.NewGuid().ToString()
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"PatchMembership failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.RelationshipData, Is.Not.Null);
            Assert.That(dataSyncEvent.RelationshipData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenDeletingMembershipShouldDeliverDeleteEvent()
        {
            var created = await CreateTestMembership(id: UniqueId(), status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "delete", StringComparison.OrdinalIgnoreCase)
                     && e.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.DeleteMembership(new DeleteMembershipParameters
                    {
                        Id = created.Id
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"DeleteMembership failed: {response.Status.ErrorData?.Information}");
                    createdMembershipIds.Remove(created.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("delete").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("relationship").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.Id, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.DeletedAt, Is.Not.Null.And.Not.Empty);
        }
    }
}
