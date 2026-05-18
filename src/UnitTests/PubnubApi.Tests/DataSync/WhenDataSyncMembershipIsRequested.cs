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
    public class WhenDataSyncMembershipIsRequested : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdMembershipIds = new();
        private readonly List<string> createdUserIds = new();
        private readonly List<string> createdChannelIds = new();

        private const int TestRelationshipClassVersion = 1;
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
            string channelId = null,
            string userId = null,
            string status = "active",
            Dictionary<string, object> payload = null)
        {
            if (channelId == null)
            {
                var channel = await CreateTestChannel();
                channelId = channel.Id;
            }

            if (userId == null)
            {
                var user = await CreateTestUser();
                userId = user.Id;
            }

            var result = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                ChannelId = channelId,
                UserId = userId,
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

        #region CreateMembership

        [Test]
        public async Task ThenCreateWithAllFieldsShouldReturnCreatedMembership()
        {
            var channel = await CreateTestChannel();
            var user = await CreateTestUser();

            var payload = new Dictionary<string, object>
            {
                { "role", "administrator" },
                { "permissions", new Dictionary<string, object>
                    {
                        { "read", true },
                        { "write", true }
                    }
                }
            };

            var response = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                ChannelId = channel.Id,
                UserId = user.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "active",
                Payload = payload,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var membership = response.Result;
            Assert.That(membership, Is.Not.Null);
            Assert.That(membership.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(membership.ChannelId, Is.EqualTo(channel.Id));
            Assert.That(membership.UserId, Is.EqualTo(user.Id));
            Assert.That(membership.RelationshipClassVersion, Is.EqualTo(TestRelationshipClassVersion));
            Assert.That(membership.Status, Is.EqualTo("active"));
            Assert.That(membership.Payload, Is.Not.Null);
            Assert.That(membership.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(membership.ETag, Is.Not.Null.And.Not.Empty);

            createdMembershipIds.Add(membership.Id);
        }

        [Test]
        public async Task ThenCreateWithExplicitIdShouldUseProvidedId()
        {
            var channel = await CreateTestChannel();
            var user = await CreateTestUser();
            var membershipId = UniqueId();

            var response = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                Id = membershipId,
                ChannelId = channel.Id,
                UserId = user.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "key", "value" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Id, Is.EqualTo(membershipId));

            createdMembershipIds.Add(response.Result.Id);
        }

        [Test]
        public async Task ThenCreateWithoutIdShouldReturnServerGeneratedId()
        {
            var channel = await CreateTestChannel();
            var user = await CreateTestUser();

            var response = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                ChannelId = channel.Id,
                UserId = user.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Payload = new Dictionary<string, object> { { "key", "value" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var membership = response.Result;
            Assert.That(membership, Is.Not.Null);
            Assert.That(membership.Id, Is.Not.Null.And.Not.Empty);

            createdMembershipIds.Add(membership.Id);
        }

        [Test]
        public async Task ThenCreateWithMinimalFieldsShouldSucceed()
        {
            var channel = await CreateTestChannel();
            var user = await CreateTestUser();

            var response = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                ChannelId = channel.Id,
                UserId = user.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.RelationshipClassVersion, Is.EqualTo(TestRelationshipClassVersion));

            createdMembershipIds.Add(response.Result.Id);
        }

        [Test]
        public async Task ThenCreateWithNestedPayloadShouldPreserveStructure()
        {
            var payload = new Dictionary<string, object>
            {
                { "role", "moderator" },
                {
                    "metadata", new Dictionary<string, object>
                    {
                        { "assignedBy", "admin" },
                        {
                            "permissions", new Dictionary<string, object>
                            {
                                { "canBan", true },
                                { "canMute", true }
                            }
                        }
                    }
                },
                { "tags", new List<object> { "trusted", "active", "moderator" } }
            };

            var membership = await CreateTestMembership(payload: payload);

            Assert.That(membership.Payload, Is.Not.Null);
            Assert.That(membership.Payload, Contains.Key("role"));
            Assert.That(membership.Payload, Contains.Key("metadata"));
            Assert.That(membership.Payload, Contains.Key("tags"));
        }

        #endregion

        #region GetMembership

        [Test]
        public async Task ThenGetAfterCreateShouldReturnSameData()
        {
            var created = await CreateTestMembership();

            var response = await pubnub.DataSync.GetMembership(new GetMembershipParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var membership = response.Result;
            Assert.That(membership, Is.Not.Null);
            Assert.That(membership.Id, Is.EqualTo(created.Id));
            Assert.That(membership.ChannelId, Is.EqualTo(created.ChannelId));
            Assert.That(membership.UserId, Is.EqualTo(created.UserId));
            Assert.That(membership.RelationshipClassVersion, Is.EqualTo(created.RelationshipClassVersion));
            Assert.That(membership.Status, Is.EqualTo(created.Status));
            Assert.That(membership.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetShouldReturnTimestampsAndETag()
        {
            var created = await CreateTestMembership();

            var response = await pubnub.DataSync.GetMembership(new GetMembershipParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var membership = response.Result;
            Assert.That(membership.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(membership.UpdatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(membership.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetNonExistentMembershipShouldReturnError()
        {
            var response = await pubnub.DataSync.GetMembership(new GetMembershipParameters
            {
                Id = $"non-existent-{Guid.NewGuid():N}"
            });

            Assert.That(response.Status.Error, Is.True);
        }

        #endregion

        #region GetMemberships (List)

        [Test]
        public async Task ThenListShouldReturnResults()
        {
            await CreateTestMembership();
            await CreateTestMembership();

            var response = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters());

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task ThenListFilteredByChannelIdShouldReturnMatchingResults()
        {
            var channel = await CreateTestChannel();
            await CreateTestMembership(channelId: channel.Id);
            await CreateTestMembership(channelId: channel.Id);

            var response = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
            {
                ChannelId = channel.Id
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(response.Result.Data.All(m => m.ChannelId == channel.Id), Is.True);
        }

        [Test]
        public async Task ThenListFilteredByUserIdShouldReturnMatchingResults()
        {
            var user = await CreateTestUser();
            await CreateTestMembership(userId: user.Id);
            await CreateTestMembership(userId: user.Id);

            var response = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
            {
                UserId = user.Id
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(response.Result.Data.All(m => m.UserId == user.Id), Is.True);
        }

        [Test]
        public async Task ThenListWithLimitShouldRespectLimit()
        {
            await CreateTestMembership();
            await CreateTestMembership();
            await CreateTestMembership();

            var response = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
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
                await CreateTestMembership();
            }

            var firstPage = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
            {
                Limit = 1
            });

            Assert.That(firstPage.Status.Error, Is.False);
            Assert.That(firstPage.Result.Data.Count, Is.EqualTo(1));

            if (firstPage.Result.Meta?.HasNext == true)
            {
                var secondPage = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
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
            await CreateTestMembership();

            var response = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
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
            await CreateTestMembership(status: "alpha");
            await Task.Delay(500);
            await CreateTestMembership(status: "beta");

            var response = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
            {
                Sort = "-createdAt"
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
        }

        #endregion

        #region UpdateMembership

        [Test]
        public async Task ThenUpdateShouldReplaceAllFields()
        {
            var created = await CreateTestMembership(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "role", "member" },
                    { "level", 1 }
                });

            var newPayload = new Dictionary<string, object>
            {
                { "role", "admin" },
                { "level", 5 },
                { "promoted", true }
            };

            var response = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
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
            var created = await CreateTestMembership();

            var response = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
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
            var created = await CreateTestMembership();

            await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "first-update",
                Payload = new Dictionary<string, object> { { "v", 1 } }
            });

            var response = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "second-update",
                Payload = new Dictionary<string, object> { { "v", 2 } },
                IfMatch = created.ETag
            });

            Assert.That(response.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenUpdateShouldBeReflectedByGet()
        {
            var created = await CreateTestMembership(
                status: "before",
                payload: new Dictionary<string, object> { { "original", true } });

            await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "after",
                Payload = new Dictionary<string, object> { { "replaced", true } }
            });

            var getResponse = await pubnub.DataSync.GetMembership(
                new GetMembershipParameters { Id = created.Id });

            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Status, Is.EqualTo("after"));
            Assert.That(getResponse.Result.Payload, Contains.Key("replaced"));
            Assert.That(getResponse.Result.Payload, Does.Not.ContainKey("original"));
        }

        #endregion

        #region PatchMembership

        [Test]
        public async Task ThenPatchReplaceShouldUpdateValue()
        {
            var created = await CreateTestMembership(status: "active");

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "banned"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Status, Is.EqualTo("banned"));
        }

        [Test]
        public async Task ThenPatchAddShouldCreateNewField()
        {
            var created = await CreateTestMembership();

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/priority",
                        Value = "high"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Payload, Contains.Key("priority"));
        }

        [Test]
        public async Task ThenPatchAddNestedObjectShouldCreateStructure()
        {
            var created = await CreateTestMembership();

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/audit",
                        Value = new Dictionary<string, object>
                        {
                            { "lastReviewedAt", "2026-04-01" },
                            { "reviewedBy", "admin" }
                        }
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("audit"));
        }

        [Test]
        public async Task ThenPatchRemoveShouldDeleteField()
        {
            var created = await CreateTestMembership(payload: new Dictionary<string, object>
            {
                { "fieldToRemove", "value" },
                { "fieldToKeep", "keep" }
            });

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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
            var created = await CreateTestMembership(payload: new Dictionary<string, object>
            {
                { "original", "test-value" }
            });

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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
            var created = await CreateTestMembership(payload: new Dictionary<string, object>
            {
                { "source", "move-me" }
            });

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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
            var created = await CreateTestMembership(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "role", "member" },
                    { "level", 1 },
                    { "joinedAt", "2025-01-01" }
                });

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/level",
                        Value = 10
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/verified",
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
            Assert.That(patched.Payload, Contains.Key("verified"));
        }

        [Test]
        public async Task ThenPatchTestAndReplaceShouldApplyConditionally()
        {
            var created = await CreateTestMembership(status: "active");

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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
            var created = await CreateTestMembership(status: "active");

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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
            var created = await CreateTestMembership(status: "active");

            await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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

            var response = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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

        #endregion

        #region DeleteMembership

        [Test]
        public async Task ThenDeleteExistingMembershipShouldSucceed()
        {
            var created = await CreateTestMembership();

            var response = await pubnub.DataSync.DeleteMembership(new DeleteMembershipParameters
            {
                Id = created.Id
            });

            Assert.That(response.Status.Error, Is.False);
            createdMembershipIds.Remove(created.Id);
        }

        [Test]
        public async Task ThenGetAfterDeleteShouldReturnError()
        {
            var created = await CreateTestMembership();

            var deleteResponse = await pubnub.DataSync.DeleteMembership(new DeleteMembershipParameters
            {
                Id = created.Id
            });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdMembershipIds.Remove(created.Id);

            var getResponse = await pubnub.DataSync.GetMembership(new GetMembershipParameters
            {
                Id = created.Id
            });
            Assert.That(getResponse.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenDeleteNonExistentMembershipShouldReturnError()
        {
            var response = await pubnub.DataSync.DeleteMembership(new DeleteMembershipParameters
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
            // Create a user and channel for the membership
            var channel = await CreateTestChannel();
            var user = await CreateTestUser();

            // CREATE
            var createResponse = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
            {
                ChannelId = channel.Id,
                UserId = user.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "new",
                Payload = new Dictionary<string, object>
                {
                    { "role", "member" },
                    { "joinedAt", "2025-01-01" }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });
            Assert.That(createResponse.Status.Error, Is.False);
            var created = createResponse.Result;
            Assert.That(created.Id, Is.Not.Null.And.Not.Empty);
            createdMembershipIds.Add(created.Id);

            // READ
            var getResponse = await pubnub.DataSync.GetMembership(
                new GetMembershipParameters { Id = created.Id });
            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Id, Is.EqualTo(created.Id));
            Assert.That(getResponse.Result.Status, Is.EqualTo("new"));
            Assert.That(getResponse.Result.ChannelId, Is.EqualTo(channel.Id));
            Assert.That(getResponse.Result.UserId, Is.EqualTo(user.Id));

            // UPDATE (full replace)
            var updateResponse = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "updated",
                Payload = new Dictionary<string, object>
                {
                    { "role", "admin" },
                    { "promotedAt", "2025-06-01" }
                }
            });
            Assert.That(updateResponse.Status.Error, Is.False);
            Assert.That(updateResponse.Result.Status, Is.EqualTo("updated"));

            // PATCH
            var patchResponse = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
            {
                Id = created.Id,
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
            var verifyResponse = await pubnub.DataSync.GetMembership(
                new GetMembershipParameters { Id = created.Id });
            Assert.That(verifyResponse.Status.Error, Is.False);
            Assert.That(verifyResponse.Result.Status, Is.EqualTo("patched"));
            Assert.That(verifyResponse.Result.Payload, Contains.Key("patchedField"));

            // LIST - verify membership in listing filtered by channel
            var listResponse = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
            {
                ChannelId = channel.Id
            });
            Assert.That(listResponse.Status.Error, Is.False);
            Assert.That(listResponse.Result.Data.Any(m => m.Id == created.Id), Is.True);

            // DELETE
            var deleteResponse = await pubnub.DataSync.DeleteMembership(
                new DeleteMembershipParameters { Id = created.Id });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdMembershipIds.Remove(created.Id);

            // Verify deletion
            var getAfterDelete = await pubnub.DataSync.GetMembership(
                new GetMembershipParameters { Id = created.Id });
            Assert.That(getAfterDelete.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenETagFlowShouldMaintainConcurrency()
        {
            var created = await CreateTestMembership(status: "v1");
            var etag1 = created.ETag;
            Assert.That(etag1, Is.Not.Null.And.Not.Empty);

            var updateResponse = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "v2",
                Payload = new Dictionary<string, object> { { "step", 2 } },
                IfMatch = etag1
            });
            Assert.That(updateResponse.Status.Error, Is.False);
            var etag2 = updateResponse.Result.ETag;
            Assert.That(etag2, Is.Not.Null.And.Not.Empty);
            Assert.That(etag2, Is.Not.EqualTo(etag1));

            var patchResponse = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
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
            var staleResponse = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "should-fail",
                IfMatch = etag1
            });
            Assert.That(staleResponse.Status.Error, Is.True);
        }

        #endregion
    }
}
