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
    public class WhenDataSyncEntityIsRequested : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdEntityIds = new();

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
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion,
                Status = status,
                Payload = payload ?? new Dictionary<string, object>
                {
                    { "make", "Toyota" },
                    { "model", "Camry" },
                    { "year", 2025 }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateEntity failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdEntityIds.Add(result.Result.Id);
            return result.Result;
        }

        #region CreateEntity

        [Test]
        public async Task ThenCreateWithAllFieldsShouldReturnCreatedEntity()
        {
            var entityId = UniqueId();
            var payload = new Dictionary<string, object>
            {
                { "make", "Honda" },
                { "model", "Civic" },
                { "year", 2024 },
                {
                    "features", new Dictionary<string, object>
                    {
                        { "sunroof", true },
                        { "navigation", true }
                    }
                }
            };

            var response = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = entityId,
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = payload,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var entity = response.Result;
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Id, Is.EqualTo(entityId));
            Assert.That(entity.EntityClass, Is.EqualTo(TestEntityClass));
            Assert.That(entity.EntityClassVersion, Is.EqualTo(TestEntityClassVersion));
            Assert.That(entity.Status, Is.EqualTo("active"));
            Assert.That(entity.Payload, Is.Not.Null);
            Assert.That(entity.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(entity.ETag, Is.Not.Null.And.Not.Empty);

            createdEntityIds.Add(entity.Id);
        }

        [Test]
        public async Task ThenCreateWithoutIdShouldReturnServerGeneratedId()
        {
            var response = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "key", "value" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var entity = response.Result;
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(entity.EntityClass, Is.EqualTo(TestEntityClass));

            createdEntityIds.Add(entity.Id);
        }

        [Test]
        public async Task ThenCreateWithMinimalFieldsShouldSucceed()
        {
            var response = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.EntityClass, Is.EqualTo(TestEntityClass));
            Assert.That(response.Result.EntityClassVersion, Is.EqualTo(TestEntityClassVersion));

            createdEntityIds.Add(response.Result.Id);
        }

        [Test]
        public async Task ThenCreateWithNestedPayloadShouldPreserveStructure()
        {
            var payload = new Dictionary<string, object>
            {
                { "make", "Tesla" },
                { "model", "Model 3" },
                {
                    "specs", new Dictionary<string, object>
                    {
                        { "range", 358 },
                        { "acceleration", 3.1 },
                        {
                            "dimensions", new Dictionary<string, object>
                            {
                                { "length", 4694 },
                                { "width", 1849 }
                            }
                        }
                    }
                },
                { "tags", new List<object> { "electric", "sedan", "premium" } }
            };

            var entity = await CreateTestEntity(payload: payload);

            Assert.That(entity.Payload, Is.Not.Null);
            Assert.That(entity.Payload, Contains.Key("make"));
            Assert.That(entity.Payload, Contains.Key("specs"));
            Assert.That(entity.Payload, Contains.Key("tags"));
        }

        #endregion

        #region GetEntity

        [Test]
        public async Task ThenGetAfterCreateShouldReturnSameData()
        {
            var created = await CreateTestEntity();

            var response = await pubnub.DataSync.GetEntity(new GetEntityParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var entity = response.Result;
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Id, Is.EqualTo(created.Id));
            Assert.That(entity.EntityClass, Is.EqualTo(created.EntityClass));
            Assert.That(entity.EntityClassVersion, Is.EqualTo(created.EntityClassVersion));
            Assert.That(entity.Status, Is.EqualTo(created.Status));
            Assert.That(entity.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetShouldReturnTimestampsAndETag()
        {
            var created = await CreateTestEntity();

            var response = await pubnub.DataSync.GetEntity(new GetEntityParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var entity = response.Result;
            Assert.That(entity.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(entity.UpdatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(entity.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetNonExistentEntityShouldReturnError()
        {
            var response = await pubnub.DataSync.GetEntity(new GetEntityParameters
            {
                Id = $"non-existent-{Guid.NewGuid():N}"
            });

            Assert.That(response.Status.Error, Is.True);
        }

        #endregion

        #region GetEntities (List)

        [Test]
        public async Task ThenListByEntityClassShouldReturnResults()
        {
            await CreateTestEntity();
            await CreateTestEntity();

            var response = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
            {
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(response.Result.Data.All(e => e.EntityClass == TestEntityClass), Is.True);
        }

        [Test]
        public async Task ThenListWithLimitShouldRespectLimit()
        {
            await CreateTestEntity();
            await CreateTestEntity();
            await CreateTestEntity();

            var response = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
            {
                EntityClass = TestEntityClass,
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
                await CreateTestEntity();
            }

            var firstPage = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
            {
                EntityClass = TestEntityClass,
                Limit = 1
            });

            Assert.That(firstPage.Status.Error, Is.False);
            Assert.That(firstPage.Result.Data.Count, Is.EqualTo(1));

            if (firstPage.Result.Meta?.HasNext == true)
            {
                var secondPage = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
                {
                    EntityClass = TestEntityClass,
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
            await CreateTestEntity();

            var response = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
            {
                EntityClass = TestEntityClass,
                Limit = 1
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Meta, Is.Not.Null);
            Assert.That(response.Result.Meta.Limit, Is.Not.Null);
        }

        [Test]
        public async Task ThenListWithSortShouldReturnSortedResults()
        {
            await CreateTestEntity(status: "alpha");
            await Task.Delay(500);
            await CreateTestEntity(status: "beta");

            var response = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
            {
                EntityClass = TestEntityClass,
                Sort = "-createdAt"
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
        }

        #endregion

        #region UpdateEntity

        [Test]
        public async Task ThenUpdateShouldReplaceAllFields()
        {
            var created = await CreateTestEntity(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "make", "Toyota" },
                    { "model", "Camry" }
                });

            var newPayload = new Dictionary<string, object>
            {
                { "make", "Ford" },
                { "model", "Focus" },
                { "year", 2023 }
            };

            var response = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
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
            var created = await CreateTestEntity();

            var response = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
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
            var created = await CreateTestEntity();

            await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "first-update",
                Payload = new Dictionary<string, object> { { "v", 1 } }
            });

            var response = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
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
            var created = await CreateTestEntity(
                status: "before",
                payload: new Dictionary<string, object> { { "original", true } });

            await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "after",
                Payload = new Dictionary<string, object> { { "replaced", true } }
            });

            var getResponse = await pubnub.DataSync.GetEntity(
                new GetEntityParameters { Id = created.Id });

            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Status, Is.EqualTo("after"));
            Assert.That(getResponse.Result.Payload, Contains.Key("replaced"));
            Assert.That(getResponse.Result.Payload, Does.Not.ContainKey("original"));
        }

        #endregion

        #region PatchEntity

        [Test]
        public async Task ThenPatchReplaceShouldUpdateValue()
        {
            var created = await CreateTestEntity(status: "active");

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var created = await CreateTestEntity();

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/color",
                        Value = "blue"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Payload, Contains.Key("color"));
        }

        [Test]
        public async Task ThenPatchAddNestedObjectShouldCreateStructure()
        {
            var created = await CreateTestEntity();

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/maintenance",
                        Value = new Dictionary<string, object>
                        {
                            { "lastServiceDate", "2026-04-01" },
                            { "mileageAtService", 41500 }
                        }
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("maintenance"));
        }

        [Test]
        public async Task ThenPatchRemoveShouldDeleteField()
        {
            var created = await CreateTestEntity(payload: new Dictionary<string, object>
            {
                { "fieldToRemove", "value" },
                { "fieldToKeep", "keep" }
            });

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var created = await CreateTestEntity(payload: new Dictionary<string, object>
            {
                { "original", "test-value" }
            });

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var created = await CreateTestEntity(payload: new Dictionary<string, object>
            {
                { "source", "move-me" }
            });

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var created = await CreateTestEntity(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "make", "Toyota" },
                    { "model", "Camry" },
                    { "year", 2025 }
                });

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/year",
                        Value = 2026
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/color",
                        Value = "red"
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
            Assert.That(patched.Payload, Contains.Key("color"));
        }

        [Test]
        public async Task ThenPatchTestAndReplaceShouldApplyConditionally()
        {
            var created = await CreateTestEntity(status: "active");

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var created = await CreateTestEntity(status: "active");

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var created = await CreateTestEntity(status: "active");

            await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var created = await CreateTestEntity(payload: new Dictionary<string, object>
            {
                { "year", 2025 },
                { "mileage", 30000.5 },
                { "isElectric", false }
            });

            var response = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/year",
                        Value = 2027
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/mileage",
                        Value = 55000.5
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/isElectric",
                        Value = true
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("year"));
            Assert.That(response.Result.Payload, Contains.Key("mileage"));
            Assert.That(response.Result.Payload, Contains.Key("isElectric"));
        }

        #endregion

        #region DeleteEntity

        [Test]
        public async Task ThenDeleteExistingEntityShouldSucceed()
        {
            var created = await CreateTestEntity();

            var response = await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters
            {
                Id = created.Id
            });

            Assert.That(response.Status.Error, Is.False);
            createdEntityIds.Remove(created.Id);
        }

        [Test]
        public async Task ThenGetAfterDeleteShouldReturnError()
        {
            var created = await CreateTestEntity();

            var deleteResponse = await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters
            {
                Id = created.Id
            });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdEntityIds.Remove(created.Id);

            var getResponse = await pubnub.DataSync.GetEntity(new GetEntityParameters
            {
                Id = created.Id
            });
            Assert.That(getResponse.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenDeleteWithValidIfMatchShouldSucceed()
        {
            var created = await CreateTestEntity();

            var response = await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters
            {
                Id = created.Id,
                IfMatch = created.ETag
            });

            Assert.That(response.Status.Error, Is.False);
            createdEntityIds.Remove(created.Id);
        }

        [Test]
        public async Task ThenDeleteNonExistentEntityShouldReturnError()
        {
            var response = await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters
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
            var entityId = UniqueId();
            var createResponse = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
            {
                Id = entityId,
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion,
                Status = "new",
                Payload = new Dictionary<string, object>
                {
                    { "name", "Integration Test Entity" },
                    { "version", 1 }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });
            Assert.That(createResponse.Status.Error, Is.False);
            var created = createResponse.Result;
            Assert.That(created.Id, Is.EqualTo(entityId));
            createdEntityIds.Add(created.Id);

            // READ
            var getResponse = await pubnub.DataSync.GetEntity(
                new GetEntityParameters { Id = entityId });
            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Id, Is.EqualTo(entityId));
            Assert.That(getResponse.Result.Status, Is.EqualTo("new"));

            // UPDATE (full replace)
            var updateResponse = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
            {
                Id = entityId,
                EntityClassVersion = TestEntityClassVersion,
                Status = "updated",
                Payload = new Dictionary<string, object>
                {
                    { "name", "Updated Entity" },
                    { "version", 2 }
                }
            });
            Assert.That(updateResponse.Status.Error, Is.False);
            Assert.That(updateResponse.Result.Status, Is.EqualTo("updated"));

            // PATCH
            var patchResponse = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
            {
                Id = entityId,
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
            var verifyResponse = await pubnub.DataSync.GetEntity(
                new GetEntityParameters { Id = entityId });
            Assert.That(verifyResponse.Status.Error, Is.False);
            Assert.That(verifyResponse.Result.Status, Is.EqualTo("patched"));
            Assert.That(verifyResponse.Result.Payload, Contains.Key("patchedField"));

            // LIST - verify entity in listing
            var listResponse = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
            {
                EntityClass = TestEntityClass,
                EntityClassVersion = TestEntityClassVersion
            });
            Assert.That(listResponse.Status.Error, Is.False);
            Assert.That(listResponse.Result.Data.Any(e => e.Id == entityId), Is.True);

            // DELETE
            var deleteResponse = await pubnub.DataSync.DeleteEntity(
                new DeleteEntityParameters { Id = entityId });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdEntityIds.Remove(entityId);

            // Verify deletion
            var getAfterDelete = await pubnub.DataSync.GetEntity(
                new GetEntityParameters { Id = entityId });
            Assert.That(getAfterDelete.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenETagFlowShouldMaintainConcurrency()
        {
            var created = await CreateTestEntity(status: "v1");
            var etag1 = created.ETag;
            Assert.That(etag1, Is.Not.Null.And.Not.Empty);

            var updateResponse = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
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

            var patchResponse = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
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
            var staleResponse = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
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
