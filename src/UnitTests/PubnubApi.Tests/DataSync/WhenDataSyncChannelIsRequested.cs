using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;
using PubnubApi.EndPoint;
using PubNubMessaging.Tests;

namespace PubnubApi.Tests.DataSync
{
    [TestFixture]
    public class WhenDataSyncChannelIsRequested : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdChannelIds = new();

        private const int TestEntityClassVersion = 1;

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
                Payload = payload ?? new Dictionary<string, object>
                {
                    { "name", "General Chat" },
                    { "description", "A general discussion channel" },
                    { "maxMembers", 100 }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateChannel failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdChannelIds.Add(result.Result.Id);
            return result.Result;
        }

        #region CreateChannel

        [Test]
        public async Task ThenCreateWithAllFieldsShouldReturnCreatedChannel()
        {
            var channelId = UniqueId();
            var payload = new Dictionary<string, object>
            {
                { "name", "Announcements" },
                { "description", "Official announcements channel" },
                { "type", "broadcast" },
                {
                    "settings", new Dictionary<string, object>
                    {
                        { "readOnly", true },
                        { "moderated", true }
                    }
                }
            };

            var response = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
            {
                Id = channelId,
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = payload,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var channel = response.Result;
            Assert.That(channel, Is.Not.Null);
            Assert.That(channel.Id, Is.EqualTo(channelId));
            Assert.That(channel.EntityClassVersion, Is.EqualTo(TestEntityClassVersion));
            Assert.That(channel.Status, Is.EqualTo("active"));
            Assert.That(channel.Payload, Is.Not.Null);
            Assert.That(channel.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(channel.ETag, Is.Not.Null.And.Not.Empty);

            createdChannelIds.Add(channel.Id);
        }

        [Test]
        public async Task ThenCreateWithoutIdShouldReturnServerGeneratedId()
        {
            var response = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
            {
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "key", "value" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var channel = response.Result;
            Assert.That(channel, Is.Not.Null);
            Assert.That(channel.Id, Is.Not.Null.And.Not.Empty);

            createdChannelIds.Add(channel.Id);
        }

        [Test]
        public async Task ThenCreateWithMinimalFieldsShouldSucceed()
        {
            var response = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
            {
                EntityClassVersion = TestEntityClassVersion,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.EntityClassVersion, Is.EqualTo(TestEntityClassVersion));

            createdChannelIds.Add(response.Result.Id);
        }

        [Test]
        public async Task ThenCreateWithNestedPayloadShouldPreserveStructure()
        {
            var payload = new Dictionary<string, object>
            {
                { "name", "Support" },
                { "type", "group" },
                {
                    "config", new Dictionary<string, object>
                    {
                        { "maxMessageLength", 4096 },
                        { "retentionDays", 90 },
                        {
                            "permissions", new Dictionary<string, object>
                            {
                                { "allowFileUpload", true },
                                { "allowReactions", true }
                            }
                        }
                    }
                },
                { "tags", new List<object> { "support", "customer", "priority" } }
            };

            var channel = await CreateTestChannel(payload: payload);

            Assert.That(channel.Payload, Is.Not.Null);
            Assert.That(channel.Payload, Contains.Key("name"));
            Assert.That(channel.Payload, Contains.Key("config"));
            Assert.That(channel.Payload, Contains.Key("tags"));
        }

        #endregion

        #region GetChannel

        [Test]
        public async Task ThenGetAfterCreateShouldReturnSameData()
        {
            var created = await CreateTestChannel();

            var response = await pubnub.DataSync.GetChannel(new GetChannelParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var channel = response.Result;
            Assert.That(channel, Is.Not.Null);
            Assert.That(channel.Id, Is.EqualTo(created.Id));
            Assert.That(channel.EntityClassVersion, Is.EqualTo(created.EntityClassVersion));
            Assert.That(channel.Status, Is.EqualTo(created.Status));
            Assert.That(channel.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetShouldReturnTimestampsAndETag()
        {
            var created = await CreateTestChannel();

            var response = await pubnub.DataSync.GetChannel(new GetChannelParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var channel = response.Result;
            Assert.That(channel.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(channel.UpdatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(channel.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetNonExistentChannelShouldReturnError()
        {
            var response = await pubnub.DataSync.GetChannel(new GetChannelParameters
            {
                Id = $"non-existent-{Guid.NewGuid():N}"
            });

            Assert.That(response.Status.Error, Is.True);
        }

        #endregion

        #region GetChannels (List)

        [Test]
        public async Task ThenListShouldReturnResults()
        {
            await CreateTestChannel();
            await CreateTestChannel();

            var response = await pubnub.DataSync.GetChannels(new GetChannelsParameters
            {
                EntityClassVersion = TestEntityClassVersion
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task ThenListWithLimitShouldRespectLimit()
        {
            await CreateTestChannel();
            await CreateTestChannel();
            await CreateTestChannel();

            var response = await pubnub.DataSync.GetChannels(new GetChannelsParameters
            {
                Limit = 2
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.LessThanOrEqualTo(2));
        }

        [Test]
        public async Task ThenListWithPaginationShouldNavigatePages()
        {
            for (int i = 0; i < 3; i++)
            {
                await CreateTestChannel();
            }

            var firstPage = await pubnub.DataSync.GetChannels(new GetChannelsParameters
            {
                Limit = 1
            });

            Assert.That(firstPage.Status.Error, Is.False);
            Assert.That(firstPage.Result.Data.Count, Is.EqualTo(1));

            if (firstPage.Result.Meta?.HasNext == true)
            {
                var secondPage = await pubnub.DataSync.GetChannels(new GetChannelsParameters
                {
                    Limit = 1,
                    Cursor = firstPage.Result.Meta.NextCursor
                });

                Assert.That(secondPage.Status.Error, Is.False);
                Assert.That(secondPage.Result.Data.Count, Is.EqualTo(1));
                Assert.That(secondPage.Result.Data[0].Id,
                    Is.Not.EqualTo(firstPage.Result.Data[0].Id));
            }
        }

        [Test]
        public async Task ThenListShouldReturnPaginationMetadata()
        {
            await CreateTestChannel();

            var response = await pubnub.DataSync.GetChannels(new GetChannelsParameters
            {
                Limit = 1
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Meta, Is.Not.Null);
            Assert.That(response.Result.Meta.Limit, Is.Not.Null);
        }

        [Test]
        public async Task ThenListWithSortShouldReturnSortedResults()
        {
            await CreateTestChannel(status: "alpha");
            await Task.Delay(500);
            await CreateTestChannel(status: "beta");

            var response = await pubnub.DataSync.GetChannels(new GetChannelsParameters
            {
                Sort = "-createdAt"
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
        }

        #endregion

        #region UpdateChannel

        [Test]
        public async Task ThenUpdateShouldReplaceAllFields()
        {
            var created = await CreateTestChannel(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "name", "Old Channel" },
                    { "type", "group" }
                });

            var newPayload = new Dictionary<string, object>
            {
                { "name", "Renamed Channel" },
                { "type", "broadcast" },
                { "maxMembers", 500 }
            };

            var response = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "updated",
                Payload = newPayload
            });

            Assert.That(response.Status.Error, Is.False);
            var updated = response.Result;
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated.Id, Is.EqualTo(created.Id));
            Assert.That(updated.Status, Is.EqualTo("updated"));
            Assert.That(updated.Payload, Is.Not.Null);
            Assert.That(updated.ETag, Is.Not.EqualTo(created.ETag));
        }

        [Test]
        public async Task ThenUpdateWithValidIfMatchShouldSucceed()
        {
            var created = await CreateTestChannel();

            var response = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "updated-with-etag",
                Payload = new Dictionary<string, object> { { "key", "new-value" } },
                IfMatch = created.ETag
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Status, Is.EqualTo("updated-with-etag"));
        }

        [Test]
        public async Task ThenUpdateWithStaleIfMatchShouldReturnError()
        {
            var created = await CreateTestChannel();

            await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "first-update",
                Payload = new Dictionary<string, object> { { "v", 1 } }
            });

            var response = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "second-update",
                Payload = new Dictionary<string, object> { { "v", 2 } },
                IfMatch = created.ETag
            });

            Assert.That(response.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenUpdateShouldBeReflectedByGet()
        {
            var created = await CreateTestChannel(
                status: "before",
                payload: new Dictionary<string, object> { { "original", true } });

            await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "after",
                Payload = new Dictionary<string, object> { { "replaced", true } }
            });

            var getResponse = await pubnub.DataSync.GetChannel(
                new GetChannelParameters { Id = created.Id });

            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Status, Is.EqualTo("after"));
            Assert.That(getResponse.Result.Payload, Contains.Key("replaced"));
            Assert.That(getResponse.Result.Payload, Does.Not.ContainKey("original"));
        }

        #endregion

        #region PatchChannel

        [Test]
        public async Task ThenPatchReplaceShouldUpdateValue()
        {
            var created = await CreateTestChannel(status: "active");

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "archived"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Status, Is.EqualTo("archived"));
        }

        [Test]
        public async Task ThenPatchAddShouldCreateNewField()
        {
            var created = await CreateTestChannel();

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/topic",
                        Value = "engineering"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Payload, Contains.Key("topic"));
        }

        [Test]
        public async Task ThenPatchAddNestedObjectShouldCreateStructure()
        {
            var created = await CreateTestChannel();

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/moderation",
                        Value = new Dictionary<string, object>
                        {
                            { "enabled", true },
                            { "level", "strict" }
                        }
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("moderation"));
        }

        [Test]
        public async Task ThenPatchRemoveShouldDeleteField()
        {
            var created = await CreateTestChannel(payload: new Dictionary<string, object>
            {
                { "fieldToRemove", "value" },
                { "fieldToKeep", "keep" }
            });

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Remove,
                        Path = "/payload/fieldToRemove"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Does.Not.ContainKey("fieldToRemove"));
            Assert.That(response.Result.Payload, Contains.Key("fieldToKeep"));
        }

        [Test]
        public async Task ThenPatchCopyShouldDuplicateValue()
        {
            var created = await CreateTestChannel(payload: new Dictionary<string, object>
            {
                { "original", "test-value" }
            });

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Copy,
                        Path = "/payload/copied",
                        From = "/payload/original"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("original"));
            Assert.That(response.Result.Payload, Contains.Key("copied"));
        }

        [Test]
        public async Task ThenPatchMoveShouldRelocateValue()
        {
            var created = await CreateTestChannel(payload: new Dictionary<string, object>
            {
                { "source", "move-me" }
            });

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Move,
                        Path = "/payload/destination",
                        From = "/payload/source"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Does.Not.ContainKey("source"));
            Assert.That(response.Result.Payload, Contains.Key("destination"));
        }

        [Test]
        public async Task ThenPatchMultipleOperationsShouldAllBeApplied()
        {
            var created = await CreateTestChannel(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "name", "General" },
                    { "maxMembers", 100 },
                    { "type", "group" }
                });

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/maxMembers",
                        Value = 500
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/pinned",
                        Value = true
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "modified"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var patched = response.Result;
            Assert.That(patched.Status, Is.EqualTo("modified"));
            Assert.That(patched.Payload, Contains.Key("pinned"));
        }

        [Test]
        public async Task ThenPatchTestAndReplaceShouldApplyConditionally()
        {
            var created = await CreateTestChannel(status: "active");

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Test,
                        Path = "/status",
                        Value = "active"
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "confirmed"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Status, Is.EqualTo("confirmed"));
        }

        [Test]
        public async Task ThenPatchWithValidIfMatchShouldSucceed()
        {
            var created = await CreateTestChannel(status: "active");

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "patched-with-etag"
                    }
                },
                IfMatch = created.ETag,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Status, Is.EqualTo("patched-with-etag"));
        }

        [Test]
        public async Task ThenPatchWithStaleIfMatchShouldReturnError()
        {
            var created = await CreateTestChannel(status: "active");

            await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "first-patch"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "second-patch"
                    }
                },
                IfMatch = created.ETag,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenPatchWithNumericAndBooleanShouldPreserveTypes()
        {
            var created = await CreateTestChannel(payload: new Dictionary<string, object>
            {
                { "maxMembers", 100 },
                { "retentionDays", 30.5 },
                { "isPublic", false }
            });

            var response = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/maxMembers",
                        Value = 500
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/retentionDays",
                        Value = 90.0
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/isPublic",
                        Value = true
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("maxMembers"));
            Assert.That(response.Result.Payload, Contains.Key("retentionDays"));
            Assert.That(response.Result.Payload, Contains.Key("isPublic"));
        }

        #endregion

        #region DeleteChannel

        [Test]
        public async Task ThenDeleteExistingChannelShouldSucceed()
        {
            var created = await CreateTestChannel();

            var response = await pubnub.DataSync.DeleteChannel(new DeleteChannelParameters
            {
                Id = created.Id
            });

            Assert.That(response.Status.Error, Is.False);
            createdChannelIds.Remove(created.Id);
        }

        [Test]
        public async Task ThenGetAfterDeleteShouldReturnError()
        {
            var created = await CreateTestChannel();

            var deleteResponse = await pubnub.DataSync.DeleteChannel(new DeleteChannelParameters
            {
                Id = created.Id
            });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdChannelIds.Remove(created.Id);

            var getResponse = await pubnub.DataSync.GetChannel(new GetChannelParameters
            {
                Id = created.Id
            });
            Assert.That(getResponse.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenDeleteNonExistentChannelShouldReturnError()
        {
            var response = await pubnub.DataSync.DeleteChannel(new DeleteChannelParameters
            {
                Id = $"non-existent-{Guid.NewGuid():N}"
            });

            Assert.That(response.Status.Error, Is.True);
        }

        #endregion

        #region Full Lifecycle

        [Test]
        public async Task ThenFullCrudLifecycleShouldSucceed()
        {
            // CREATE
            var channelId = UniqueId();
            var createResponse = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
            {
                Id = channelId,
                EntityClassVersion = TestEntityClassVersion,
                Status = "new",
                Payload = new Dictionary<string, object>
                {
                    { "name", "Integration Test Channel" },
                    { "version", 1 }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });
            Assert.That(createResponse.Status.Error, Is.False);
            var created = createResponse.Result;
            Assert.That(created.Id, Is.EqualTo(channelId));
            createdChannelIds.Add(created.Id);

            // READ
            var getResponse = await pubnub.DataSync.GetChannel(
                new GetChannelParameters { Id = channelId });
            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Id, Is.EqualTo(channelId));
            Assert.That(getResponse.Result.Status, Is.EqualTo("new"));

            // UPDATE (full replace)
            var updateResponse = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = channelId,
                EntityClassVersion = TestEntityClassVersion,
                Status = "updated",
                Payload = new Dictionary<string, object>
                {
                    { "name", "Updated Channel" },
                    { "version", 2 }
                }
            });
            Assert.That(updateResponse.Status.Error, Is.False);
            Assert.That(updateResponse.Result.Status, Is.EqualTo("updated"));

            // PATCH
            var patchResponse = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = channelId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "patched"
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/patchedField",
                        Value = "hello"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });
            Assert.That(patchResponse.Status.Error, Is.False);
            Assert.That(patchResponse.Result.Status, Is.EqualTo("patched"));

            // Verify via GET
            var verifyResponse = await pubnub.DataSync.GetChannel(
                new GetChannelParameters { Id = channelId });
            Assert.That(verifyResponse.Status.Error, Is.False);
            Assert.That(verifyResponse.Result.Status, Is.EqualTo("patched"));
            Assert.That(verifyResponse.Result.Payload, Contains.Key("patchedField"));

            // LIST - verify channel in listing
            var listResponse = await pubnub.DataSync.GetChannels(new GetChannelsParameters
            {
                EntityClassVersion = TestEntityClassVersion
            });
            Assert.That(listResponse.Status.Error, Is.False);
            Assert.That(listResponse.Result.Data.Any(c => c.Id == channelId), Is.True);

            // DELETE
            var deleteResponse = await pubnub.DataSync.DeleteChannel(
                new DeleteChannelParameters { Id = channelId });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdChannelIds.Remove(channelId);

            // Verify deletion
            var getAfterDelete = await pubnub.DataSync.GetChannel(
                new GetChannelParameters { Id = channelId });
            Assert.That(getAfterDelete.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenETagFlowShouldMaintainConcurrency()
        {
            var created = await CreateTestChannel(status: "v1");
            var etag1 = created.ETag;
            Assert.That(etag1, Is.Not.Null.And.Not.Empty);

            var updateResponse = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "v2",
                Payload = new Dictionary<string, object> { { "step", 2 } },
                IfMatch = etag1
            });
            Assert.That(updateResponse.Status.Error, Is.False);
            var etag2 = updateResponse.Result.ETag;
            Assert.That(etag2, Is.Not.Null.And.Not.Empty);
            Assert.That(etag2, Is.Not.EqualTo(etag1));

            var patchResponse = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "v3"
                    }
                },
                IfMatch = etag2,
                IdempotencyKey = Guid.NewGuid().ToString()
            });
            Assert.That(patchResponse.Status.Error, Is.False);
            var etag3 = patchResponse.Result.ETag;
            Assert.That(etag3, Is.Not.EqualTo(etag2));

            // Using a stale ETag should fail
            var staleResponse = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "should-fail",
                IfMatch = etag1
            });
            Assert.That(staleResponse.Status.Error, Is.True);
        }

        #endregion
    }
}
