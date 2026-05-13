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
    public class WhenDataSyncRelationshipIsRequested : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdRelationshipIds = new();
        private readonly List<string> createdEntityIds = new();

        private const string TestRelationshipClass = "integration-test-ownership";
        private const int TestRelationshipClassVersion = 1;
        private const string TestEntityClass = "integration-test-vehicle";
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
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "name", $"entity-{id}" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateEntity failed: {result.Status.ErrorData?.Information}");
            createdEntityIds.Add(result.Result.Id);
            return result.Result;
        }

        private async Task<PNDataSyncRelationshipResult> CreateTestRelationship(
            string entityAId = null,
            string entityBId = null,
            string status = "active",
            Dictionary<string, object> payload = null)
        {
            if (entityAId == null)
            {
                var entityA = await CreateTestEntity();
                entityAId = entityA.Id;
            }

            if (entityBId == null)
            {
                var entityB = await CreateTestEntity();
                entityBId = entityB.Id;
            }

            var result = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                EntityAId = entityAId,
                EntityBId = entityBId,
                RelationshipClass = TestRelationshipClass,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = status,
                Payload = payload ?? new Dictionary<string, object>
                {
                    { "role", "owner" },
                    { "since", "2025-01-01" }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateRelationship failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdRelationshipIds.Add(result.Result.Id);
            return result.Result;
        }

        #region CreateRelationship

        [Test]
        public async Task ThenCreateWithAllFieldsShouldReturnCreatedRelationship()
        {
            var entityA = await CreateTestEntity();
            var entityB = await CreateTestEntity();

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

            var response = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                EntityAId = entityA.Id,
                EntityBId = entityB.Id,
                RelationshipClass = TestRelationshipClass,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "active",
                Payload = payload,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var relationship = response.Result;
            Assert.That(relationship, Is.Not.Null);
            Assert.That(relationship.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(relationship.EntityAId, Is.EqualTo(entityA.Id));
            Assert.That(relationship.EntityBId, Is.EqualTo(entityB.Id));
            Assert.That(relationship.RelationshipClass, Is.EqualTo(TestRelationshipClass));
            Assert.That(relationship.RelationshipClassVersion, Is.EqualTo(TestRelationshipClassVersion));
            Assert.That(relationship.Status, Is.EqualTo("active"));
            Assert.That(relationship.Payload, Is.Not.Null);
            Assert.That(relationship.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(relationship.ETag, Is.Not.Null.And.Not.Empty);

            createdRelationshipIds.Add(relationship.Id);
        }

        [Test]
        public async Task ThenCreateWithExplicitIdShouldUseProvidedId()
        {
            var entityA = await CreateTestEntity();
            var entityB = await CreateTestEntity();
            var relationshipId = UniqueId();

            var response = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                Id = relationshipId,
                EntityAId = entityA.Id,
                EntityBId = entityB.Id,
                RelationshipClass = TestRelationshipClass,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "key", "value" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Id, Is.EqualTo(relationshipId));

            createdRelationshipIds.Add(response.Result.Id);
        }

        [Test]
        public async Task ThenCreateWithoutIdShouldReturnServerGeneratedId()
        {
            var entityA = await CreateTestEntity();
            var entityB = await CreateTestEntity();

            var response = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                EntityAId = entityA.Id,
                EntityBId = entityB.Id,
                RelationshipClass = TestRelationshipClass,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Payload = new Dictionary<string, object> { { "key", "value" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var relationship = response.Result;
            Assert.That(relationship, Is.Not.Null);
            Assert.That(relationship.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(relationship.RelationshipClass, Is.EqualTo(TestRelationshipClass));

            createdRelationshipIds.Add(relationship.Id);
        }

        [Test]
        public async Task ThenCreateWithMinimalFieldsShouldSucceed()
        {
            var entityA = await CreateTestEntity();
            var entityB = await CreateTestEntity();

            var response = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                EntityAId = entityA.Id,
                EntityBId = entityB.Id,
                RelationshipClass = TestRelationshipClass,
                RelationshipClassVersion = TestRelationshipClassVersion,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.RelationshipClass, Is.EqualTo(TestRelationshipClass));
            Assert.That(response.Result.RelationshipClassVersion, Is.EqualTo(TestRelationshipClassVersion));

            createdRelationshipIds.Add(response.Result.Id);
        }

        [Test]
        public async Task ThenCreateWithNestedPayloadShouldPreserveStructure()
        {
            var payload = new Dictionary<string, object>
            {
                { "role", "manager" },
                {
                    "metadata", new Dictionary<string, object>
                    {
                        { "assignedBy", "admin" },
                        {
                            "permissions", new Dictionary<string, object>
                            {
                                { "canEdit", true },
                                { "canDelete", false }
                            }
                        }
                    }
                },
                { "tags", new List<object> { "primary", "active", "verified" } }
            };

            var relationship = await CreateTestRelationship(payload: payload);

            Assert.That(relationship.Payload, Is.Not.Null);
            Assert.That(relationship.Payload, Contains.Key("role"));
            Assert.That(relationship.Payload, Contains.Key("metadata"));
            Assert.That(relationship.Payload, Contains.Key("tags"));
        }

        #endregion

        #region GetRelationship

        [Test]
        public async Task ThenGetAfterCreateShouldReturnSameData()
        {
            var created = await CreateTestRelationship();

            var response = await pubnub.DataSync.GetRelationship(new GetRelationshipParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var relationship = response.Result;
            Assert.That(relationship, Is.Not.Null);
            Assert.That(relationship.Id, Is.EqualTo(created.Id));
            Assert.That(relationship.EntityAId, Is.EqualTo(created.EntityAId));
            Assert.That(relationship.EntityBId, Is.EqualTo(created.EntityBId));
            Assert.That(relationship.RelationshipClass, Is.EqualTo(created.RelationshipClass));
            Assert.That(relationship.RelationshipClassVersion, Is.EqualTo(created.RelationshipClassVersion));
            Assert.That(relationship.Status, Is.EqualTo(created.Status));
            Assert.That(relationship.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetShouldReturnTimestampsAndETag()
        {
            var created = await CreateTestRelationship();

            var response = await pubnub.DataSync.GetRelationship(new GetRelationshipParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var relationship = response.Result;
            Assert.That(relationship.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(relationship.UpdatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(relationship.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetNonExistentRelationshipShouldReturnError()
        {
            var response = await pubnub.DataSync.GetRelationship(new GetRelationshipParameters
            {
                Id = $"non-existent-{Guid.NewGuid():N}"
            });

            Assert.That(response.Status.Error, Is.True);
        }

        #endregion

        #region GetRelationships (List)

        [Test]
        public async Task ThenListByRelationshipClassShouldReturnResults()
        {
            await CreateTestRelationship();
            await CreateTestRelationship();

            var response = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(response.Result.Data.All(r => r.RelationshipClass == TestRelationshipClass), Is.True);
        }

        [Test]
        public async Task ThenListFilteredByEntityAIdShouldReturnMatchingResults()
        {
            var entityA = await CreateTestEntity();
            await CreateTestRelationship(entityAId: entityA.Id);
            await CreateTestRelationship(entityAId: entityA.Id);

            var response = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass,
                EntityAId = entityA.Id
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(response.Result.Data.All(r => r.EntityAId == entityA.Id), Is.True);
        }

        [Test]
        public async Task ThenListFilteredByEntityBIdShouldReturnMatchingResults()
        {
            var entityB = await CreateTestEntity();
            await CreateTestRelationship(entityBId: entityB.Id);
            await CreateTestRelationship(entityBId: entityB.Id);

            var response = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass,
                EntityBId = entityB.Id
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(response.Result.Data.All(r => r.EntityBId == entityB.Id), Is.True);
        }

        [Test]
        public async Task ThenListWithLimitShouldRespectLimit()
        {
            await CreateTestRelationship();
            await CreateTestRelationship();
            await CreateTestRelationship();

            var response = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass,
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
                await CreateTestRelationship();
            }

            var firstPage = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass,
                Limit = 1
            });

            Assert.That(firstPage.Status.Error, Is.False);
            Assert.That(firstPage.Result.Data.Count, Is.EqualTo(1));

            if (firstPage.Result.Meta?.HasNext == true)
            {
                var secondPage = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
                {
                    RelationshipClass = TestRelationshipClass,
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
            await CreateTestRelationship();

            var response = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass,
                Limit = 1
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Meta, Is.Not.Null);
            Assert.That(response.Result.Meta.Limit, Is.Not.Null);
        }

        [Test]
        public async Task ThenListWithSortShouldReturnSortedResults()
        {
            await CreateTestRelationship(status: "alpha");
            await Task.Delay(500);
            await CreateTestRelationship(status: "beta");

            var response = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass,
                Sort = "-createdAt"
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
        }

        #endregion

        #region UpdateRelationship

        [Test]
        public async Task ThenUpdateShouldReplaceAllFields()
        {
            var created = await CreateTestRelationship(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "role", "viewer" },
                    { "level", 1 }
                });

            var newPayload = new Dictionary<string, object>
            {
                { "role", "admin" },
                { "level", 5 },
                { "promoted", true }
            };

            var response = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
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
            var created = await CreateTestRelationship();

            var response = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
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
            var created = await CreateTestRelationship();

            await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "first-update",
                Payload = new Dictionary<string, object> { { "v", 1 } }
            });

            var response = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
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
            var created = await CreateTestRelationship(
                status: "before",
                payload: new Dictionary<string, object> { { "original", true } });

            await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
            {
                Id = created.Id,
                RelationshipClassVersion = TestRelationshipClassVersion,
                Status = "after",
                Payload = new Dictionary<string, object> { { "replaced", true } }
            });

            var getResponse = await pubnub.DataSync.GetRelationship(
                new GetRelationshipParameters { Id = created.Id });

            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Status, Is.EqualTo("after"));
            Assert.That(getResponse.Result.Payload, Contains.Key("replaced"));
            Assert.That(getResponse.Result.Payload, Does.Not.ContainKey("original"));
        }

        #endregion

        #region PatchRelationship

        [Test]
        public async Task ThenPatchReplaceShouldUpdateValue()
        {
            var created = await CreateTestRelationship(status: "active");

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/status",
                        Value = "inactive"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Status, Is.EqualTo("inactive"));
        }

        [Test]
        public async Task ThenPatchAddShouldCreateNewField()
        {
            var created = await CreateTestRelationship();

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship();

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship(payload: new Dictionary<string, object>
            {
                { "fieldToRemove", "value" },
                { "fieldToKeep", "keep" }
            });

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship(payload: new Dictionary<string, object>
            {
                { "original", "test-value" }
            });

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship(payload: new Dictionary<string, object>
            {
                { "source", "move-me" }
            });

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "role", "viewer" },
                    { "level", 1 },
                    { "since", "2025-01-01" }
                });

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship(status: "active");

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship(status: "active");

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var created = await CreateTestRelationship(status: "active");

            await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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

            var response = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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

        #region DeleteRelationship

        [Test]
        public async Task ThenDeleteExistingRelationshipShouldSucceed()
        {
            var created = await CreateTestRelationship();

            var response = await pubnub.DataSync.DeleteRelationship(new DeleteRelationshipParameters
            {
                Id = created.Id
            });

            Assert.That(response.Status.Error, Is.False);
            createdRelationshipIds.Remove(created.Id);
        }

        [Test]
        public async Task ThenGetAfterDeleteShouldReturnError()
        {
            var created = await CreateTestRelationship();

            var deleteResponse = await pubnub.DataSync.DeleteRelationship(new DeleteRelationshipParameters
            {
                Id = created.Id
            });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdRelationshipIds.Remove(created.Id);

            var getResponse = await pubnub.DataSync.GetRelationship(new GetRelationshipParameters
            {
                Id = created.Id
            });
            Assert.That(getResponse.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenDeleteNonExistentRelationshipShouldReturnError()
        {
            var response = await pubnub.DataSync.DeleteRelationship(new DeleteRelationshipParameters
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
            // Create entities for the relationship
            var entityA = await CreateTestEntity();
            var entityB = await CreateTestEntity();

            // CREATE
            var createResponse = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
            {
                EntityAId = entityA.Id,
                EntityBId = entityB.Id,
                RelationshipClass = TestRelationshipClass,
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
            createdRelationshipIds.Add(created.Id);

            // READ
            var getResponse = await pubnub.DataSync.GetRelationship(
                new GetRelationshipParameters { Id = created.Id });
            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Id, Is.EqualTo(created.Id));
            Assert.That(getResponse.Result.Status, Is.EqualTo("new"));
            Assert.That(getResponse.Result.EntityAId, Is.EqualTo(entityA.Id));
            Assert.That(getResponse.Result.EntityBId, Is.EqualTo(entityB.Id));

            // UPDATE (full replace)
            var updateResponse = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
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
            var patchResponse = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var verifyResponse = await pubnub.DataSync.GetRelationship(
                new GetRelationshipParameters { Id = created.Id });
            Assert.That(verifyResponse.Status.Error, Is.False);
            Assert.That(verifyResponse.Result.Status, Is.EqualTo("patched"));
            Assert.That(verifyResponse.Result.Payload, Contains.Key("patchedField"));

            // LIST - verify relationship in listing
            var listResponse = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
            {
                RelationshipClass = TestRelationshipClass,
                EntityAId = entityA.Id
            });
            Assert.That(listResponse.Status.Error, Is.False);
            Assert.That(listResponse.Result.Data.Any(r => r.Id == created.Id), Is.True);

            // DELETE
            var deleteResponse = await pubnub.DataSync.DeleteRelationship(
                new DeleteRelationshipParameters { Id = created.Id });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdRelationshipIds.Remove(created.Id);

            // Verify deletion
            var getAfterDelete = await pubnub.DataSync.GetRelationship(
                new GetRelationshipParameters { Id = created.Id });
            Assert.That(getAfterDelete.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenETagFlowShouldMaintainConcurrency()
        {
            var created = await CreateTestRelationship(status: "v1");
            var etag1 = created.ETag;
            Assert.That(etag1, Is.Not.Null.And.Not.Empty);

            var updateResponse = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
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

            var patchResponse = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
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
            var staleResponse = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
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
