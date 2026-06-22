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
    public class WhenDataSyncChannelEventIsReceived : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdChannelIds = new();

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
            createdChannelIds.Clear();
        }

        [TearDown]
        public async Task Cleanup()
        {
            if (pubnub != null)
            {
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

        private async Task<PNDataSyncChannelResult> CreateTestChannel(
            string id = null,
            string status = "active",
            Dictionary<string, object> payload = null)
        {
            id ??= UniqueId();
            var result = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
            {
                Id = id,
                EntityClassVersion = TestEntityClassVersion,
                Status = status,
                Payload = payload ?? new Dictionary<string, object> { { "name", $"channel-{id}" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateChannel failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdChannelIds.Add(result.Result.Id);
            return result.Result;
        }

        /// <summary>
        /// Subscribes to the given channel, runs the trigger action, and waits for a DataSync
        /// event matching the predicate. Events for a channel are published on a PubNub channel
        /// named after the channel id. Channels are modeled as DataSync entities, so create/update
        /// events populate <see cref="PNDataSyncEventResult.EntityData"/>.
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
        public async Task ThenCreatingChannelShouldDeliverCreateEvent()
        {
            var channelId = UniqueId();

            var dataSyncEvent = await CaptureEventAsync(
                channelId,
                e => string.Equals(e.Event, "create", StringComparison.OrdinalIgnoreCase)
                     && e.EntityData?.Id == channelId,
                async () =>
                {
                    var response = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
                    {
                        Id = channelId,
                        EntityClassVersion = TestEntityClassVersion,
                        Status = "active",
                        Payload = new Dictionary<string, object> { { "name", "General" } },
                        IdempotencyKey = Guid.NewGuid().ToString()
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"CreateChannel failed: {response.Status.ErrorData?.Information}");
                    createdChannelIds.Add(response.Result.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("create").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(channelId));
            Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
            Assert.That(dataSyncEvent.EntityData.Id, Is.EqualTo(channelId));
            Assert.That(dataSyncEvent.EntityData.Payload, Is.Not.Null);
        }

        [Test]
        public async Task ThenUpdatingChannelShouldDeliverUpdateEvent()
        {
            var created = await CreateTestChannel(status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.EntityData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
                    {
                        Id = created.Id,
                        EntityClassVersion = TestEntityClassVersion,
                        Status = "updated",
                        Payload = new Dictionary<string, object> { { "name", "General Updated" } }
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"UpdateChannel failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
            Assert.That(dataSyncEvent.EntityData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenPatchingChannelShouldDeliverUpdateEvent()
        {
            var created = await CreateTestChannel(status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "update", StringComparison.OrdinalIgnoreCase)
                     && e.EntityData?.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
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
                        $"PatchChannel failed: {response.Status.ErrorData?.Information}");
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("update").IgnoreCase);
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.EntityData, Is.Not.Null);
            Assert.That(dataSyncEvent.EntityData.Id, Is.EqualTo(created.Id));
        }

        [Test]
        public async Task ThenDeletingChannelShouldDeliverDeleteEvent()
        {
            var created = await CreateTestChannel(status: "active");

            var dataSyncEvent = await CaptureEventAsync(
                created.Id,
                e => string.Equals(e.Event, "delete", StringComparison.OrdinalIgnoreCase)
                     && e.Id == created.Id,
                async () =>
                {
                    var response = await pubnub.DataSync.DeleteChannel(new DeleteChannelParameters
                    {
                        Id = created.Id
                    });
                    Assert.That(response.Status.Error, Is.False,
                        $"DeleteChannel failed: {response.Status.ErrorData?.Information}");
                    createdChannelIds.Remove(created.Id);
                });

            Assert.That(dataSyncEvent.Event, Is.EqualTo("delete").IgnoreCase);
            Assert.That(dataSyncEvent.Source, Is.EqualTo("data-sync"));
            Assert.That(dataSyncEvent.Type, Is.EqualTo("entity").IgnoreCase);
            Assert.That(dataSyncEvent.Channel, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.Id, Is.EqualTo(created.Id));
            Assert.That(dataSyncEvent.DeletedAt, Is.Not.Null.And.Not.Empty);
        }
    }
}
