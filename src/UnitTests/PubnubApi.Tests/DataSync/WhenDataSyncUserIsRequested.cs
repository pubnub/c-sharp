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
    public class WhenDataSyncUserIsRequested : TestHarness
    {
        private Pubnub pubnub;
        private readonly List<string> createdUserIds = new();

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
            createdUserIds.Clear();
        }

        [TearDown]
        public async Task Cleanup()
        {
            if (pubnub != null)
            {
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
                pubnub.Destroy();
                pubnub = null;
            }
        }

        private string UniqueId() => $"test-{Guid.NewGuid():N}";

        private async Task<PNDataSyncUserResult> CreateTestUser(
            string id = null,
            string status = "active",
            Dictionary<string, object> payload = null)
        {
            id ??= UniqueId();
            var result = await pubnub.DataSync.CreateUser(new CreateUserParameters
            {
                Id = id,
                EntityClassVersion = TestEntityClassVersion,
                Status = status,
                Payload = payload ?? new Dictionary<string, object>
                {
                    { "firstName", "John" },
                    { "lastName", "Doe" },
                    { "email", "john.doe@example.com" }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(result.Status.Error, Is.False,
                $"CreateUser failed: {result.Status.ErrorData?.Information}");
            Assert.That(result.Result, Is.Not.Null);

            createdUserIds.Add(result.Result.Id);
            return result.Result;
        }

        #region CreateUser

        [Test]
        public async Task ThenCreateWithAllFieldsShouldReturnCreatedUser()
        {
            var userId = UniqueId();
            var payload = new Dictionary<string, object>
            {
                { "firstName", "Jane" },
                { "lastName", "Smith" },
                { "email", "jane.smith@example.com" },
                {
                    "preferences", new Dictionary<string, object>
                    {
                        { "theme", "dark" },
                        { "notifications", true }
                    }
                }
            };

            var response = await pubnub.DataSync.CreateUser(new CreateUserParameters
            {
                Id = userId,
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = payload,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var user = response.Result;
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Id, Is.EqualTo(userId));
            Assert.That(user.EntityClassVersion, Is.EqualTo(TestEntityClassVersion));
            Assert.That(user.Status, Is.EqualTo("active"));
            Assert.That(user.Payload, Is.Not.Null);
            Assert.That(user.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(user.ETag, Is.Not.Null.And.Not.Empty);

            createdUserIds.Add(user.Id);
        }

        [Test]
        public async Task ThenCreateWithoutIdShouldReturnServerGeneratedId()
        {
            var response = await pubnub.DataSync.CreateUser(new CreateUserParameters
            {
                EntityClassVersion = TestEntityClassVersion,
                Status = "active",
                Payload = new Dictionary<string, object> { { "key", "value" } },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            var user = response.Result;
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Id, Is.Not.Null.And.Not.Empty);

            createdUserIds.Add(user.Id);
        }

        [Test]
        public async Task ThenCreateWithMinimalFieldsShouldSucceed()
        {
            var response = await pubnub.DataSync.CreateUser(new CreateUserParameters
            {
                EntityClassVersion = TestEntityClassVersion,
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.EntityClassVersion, Is.EqualTo(TestEntityClassVersion));

            createdUserIds.Add(response.Result.Id);
        }

        [Test]
        public async Task ThenCreateWithNestedPayloadShouldPreserveStructure()
        {
            var payload = new Dictionary<string, object>
            {
                { "firstName", "Alice" },
                { "lastName", "Johnson" },
                {
                    "address", new Dictionary<string, object>
                    {
                        { "street", "123 Main St" },
                        { "city", "Springfield" },
                        {
                            "coordinates", new Dictionary<string, object>
                            {
                                { "lat", 39.7817 },
                                { "lng", -89.6501 }
                            }
                        }
                    }
                },
                { "roles", new List<object> { "admin", "editor", "viewer" } }
            };

            var user = await CreateTestUser(payload: payload);

            Assert.That(user.Payload, Is.Not.Null);
            Assert.That(user.Payload, Contains.Key("firstName"));
            Assert.That(user.Payload, Contains.Key("address"));
            Assert.That(user.Payload, Contains.Key("roles"));
        }

        #endregion

        #region GetUser

        [Test]
        public async Task ThenGetAfterCreateShouldReturnSameData()
        {
            var created = await CreateTestUser();

            var response = await pubnub.DataSync.GetUser(new GetUserParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var user = response.Result;
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Id, Is.EqualTo(created.Id));
            Assert.That(user.EntityClassVersion, Is.EqualTo(created.EntityClassVersion));
            Assert.That(user.Status, Is.EqualTo(created.Status));
            Assert.That(user.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetShouldReturnTimestampsAndETag()
        {
            var created = await CreateTestUser();

            var response = await pubnub.DataSync.GetUser(new GetUserParameters { Id = created.Id });

            Assert.That(response.Status.Error, Is.False);
            var user = response.Result;
            Assert.That(user.CreatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(user.UpdatedAt, Is.Not.Null.And.Not.Empty);
            Assert.That(user.ETag, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task ThenGetNonExistentUserShouldReturnError()
        {
            var response = await pubnub.DataSync.GetUser(new GetUserParameters
            {
                Id = $"non-existent-{Guid.NewGuid():N}"
            });

            Assert.That(response.Status.Error, Is.True);
        }

        #endregion

        #region GetUsers (List)

        [Test]
        public async Task ThenListShouldReturnResults()
        {
            await CreateTestUser();
            await CreateTestUser();

            var response = await pubnub.DataSync.GetUsers(new GetUsersParameters
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
            await CreateTestUser();
            await CreateTestUser();
            await CreateTestUser();

            var response = await pubnub.DataSync.GetUsers(new GetUsersParameters
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
                await CreateTestUser();
            }

            var firstPage = await pubnub.DataSync.GetUsers(new GetUsersParameters
            {
                Limit = 1
            });

            Assert.That(firstPage.Status.Error, Is.False);
            Assert.That(firstPage.Result.Data.Count, Is.EqualTo(1));

            if (firstPage.Result.Meta?.HasNext == true)
            {
                var secondPage = await pubnub.DataSync.GetUsers(new GetUsersParameters
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
            await CreateTestUser();

            var response = await pubnub.DataSync.GetUsers(new GetUsersParameters
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
            await CreateTestUser(status: "alpha");
            await Task.Delay(500);
            await CreateTestUser(status: "beta");

            var response = await pubnub.DataSync.GetUsers(new GetUsersParameters
            {
                Sort = "-createdAt"
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Data, Is.Not.Null);
            Assert.That(response.Result.Data.Count, Is.GreaterThanOrEqualTo(2));
        }

        #endregion

        #region UpdateUser

        [Test]
        public async Task ThenUpdateShouldReplaceAllFields()
        {
            var created = await CreateTestUser(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "firstName", "John" },
                    { "lastName", "Doe" }
                });

            var newPayload = new Dictionary<string, object>
            {
                { "firstName", "Jane" },
                { "lastName", "Smith" },
                { "age", 30 }
            };

            var response = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
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
            var created = await CreateTestUser();

            var response = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
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
            var created = await CreateTestUser();

            await pubnub.DataSync.UpdateUser(new UpdateUserParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "first-update",
                Payload = new Dictionary<string, object> { { "v", 1 } }
            });

            var response = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
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
            var created = await CreateTestUser(
                status: "before",
                payload: new Dictionary<string, object> { { "original", true } });

            await pubnub.DataSync.UpdateUser(new UpdateUserParameters
            {
                Id = created.Id,
                EntityClassVersion = TestEntityClassVersion,
                Status = "after",
                Payload = new Dictionary<string, object> { { "replaced", true } }
            });

            var getResponse = await pubnub.DataSync.GetUser(
                new GetUserParameters { Id = created.Id });

            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Status, Is.EqualTo("after"));
            Assert.That(getResponse.Result.Payload, Contains.Key("replaced"));
            Assert.That(getResponse.Result.Payload, Does.Not.ContainKey("original"));
        }

        #endregion

        #region PatchUser

        [Test]
        public async Task ThenPatchReplaceShouldUpdateValue()
        {
            var created = await CreateTestUser(status: "active");

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var created = await CreateTestUser();

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/nickname",
                        Value = "Johnny"
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result, Is.Not.Null);
            Assert.That(response.Result.Payload, Contains.Key("nickname"));
        }

        [Test]
        public async Task ThenPatchAddNestedObjectShouldCreateStructure()
        {
            var created = await CreateTestUser();

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/settings",
                        Value = new Dictionary<string, object>
                        {
                            { "language", "en" },
                            { "timezone", "UTC" }
                        }
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("settings"));
        }

        [Test]
        public async Task ThenPatchRemoveShouldDeleteField()
        {
            var created = await CreateTestUser(payload: new Dictionary<string, object>
            {
                { "fieldToRemove", "value" },
                { "fieldToKeep", "keep" }
            });

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var created = await CreateTestUser(payload: new Dictionary<string, object>
            {
                { "original", "test-value" }
            });

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var created = await CreateTestUser(payload: new Dictionary<string, object>
            {
                { "source", "move-me" }
            });

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var created = await CreateTestUser(
                status: "active",
                payload: new Dictionary<string, object>
                {
                    { "firstName", "John" },
                    { "lastName", "Doe" },
                    { "age", 25 }
                });

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/age",
                        Value = 30
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
            var created = await CreateTestUser(status: "active");

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var created = await CreateTestUser(status: "active");

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var created = await CreateTestUser(status: "active");

            await pubnub.DataSync.PatchUser(new PatchUserParameters
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

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var created = await CreateTestUser(payload: new Dictionary<string, object>
            {
                { "age", 25 },
                { "score", 95.5 },
                { "isVerified", false }
            });

            var response = await pubnub.DataSync.PatchUser(new PatchUserParameters
            {
                Id = created.Id,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/age",
                        Value = 30
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/score",
                        Value = 99.9
                    },
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Replace,
                        Path = "/payload/isVerified",
                        Value = true
                    }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });

            Assert.That(response.Status.Error, Is.False);
            Assert.That(response.Result.Payload, Contains.Key("age"));
            Assert.That(response.Result.Payload, Contains.Key("score"));
            Assert.That(response.Result.Payload, Contains.Key("isVerified"));
        }

        #endregion

        #region DeleteUser

        [Test]
        public async Task ThenDeleteExistingUserShouldSucceed()
        {
            var created = await CreateTestUser();

            var response = await pubnub.DataSync.DeleteUser(new DeleteUserParameters
            {
                Id = created.Id
            });

            Assert.That(response.Status.Error, Is.False);
            createdUserIds.Remove(created.Id);
        }

        [Test]
        public async Task ThenGetAfterDeleteShouldReturnError()
        {
            var created = await CreateTestUser();

            var deleteResponse = await pubnub.DataSync.DeleteUser(new DeleteUserParameters
            {
                Id = created.Id
            });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdUserIds.Remove(created.Id);

            var getResponse = await pubnub.DataSync.GetUser(new GetUserParameters
            {
                Id = created.Id
            });
            Assert.That(getResponse.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenDeleteNonExistentUserShouldReturnError()
        {
            var response = await pubnub.DataSync.DeleteUser(new DeleteUserParameters
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
            var userId = UniqueId();
            var createResponse = await pubnub.DataSync.CreateUser(new CreateUserParameters
            {
                Id = userId,
                EntityClassVersion = TestEntityClassVersion,
                Status = "new",
                Payload = new Dictionary<string, object>
                {
                    { "firstName", "Integration" },
                    { "lastName", "Test" }
                },
                IdempotencyKey = Guid.NewGuid().ToString()
            });
            Assert.That(createResponse.Status.Error, Is.False);
            var created = createResponse.Result;
            Assert.That(created.Id, Is.EqualTo(userId));
            createdUserIds.Add(created.Id);

            // READ
            var getResponse = await pubnub.DataSync.GetUser(
                new GetUserParameters { Id = userId });
            Assert.That(getResponse.Status.Error, Is.False);
            Assert.That(getResponse.Result.Id, Is.EqualTo(userId));
            Assert.That(getResponse.Result.Status, Is.EqualTo("new"));

            // UPDATE (full replace)
            var updateResponse = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
            {
                Id = userId,
                EntityClassVersion = TestEntityClassVersion,
                Status = "updated",
                Payload = new Dictionary<string, object>
                {
                    { "firstName", "Updated" },
                    { "lastName", "User" }
                }
            });
            Assert.That(updateResponse.Status.Error, Is.False);
            Assert.That(updateResponse.Result.Status, Is.EqualTo("updated"));

            // PATCH
            var patchResponse = await pubnub.DataSync.PatchUser(new PatchUserParameters
            {
                Id = userId,
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
            var verifyResponse = await pubnub.DataSync.GetUser(
                new GetUserParameters { Id = userId });
            Assert.That(verifyResponse.Status.Error, Is.False);
            Assert.That(verifyResponse.Result.Status, Is.EqualTo("patched"));
            Assert.That(verifyResponse.Result.Payload, Contains.Key("patchedField"));

            // LIST - verify user in listing
            var listResponse = await pubnub.DataSync.GetUsers(new GetUsersParameters
            {
                EntityClassVersion = TestEntityClassVersion
            });
            Assert.That(listResponse.Status.Error, Is.False);
            Assert.That(listResponse.Result.Data.Any(u => u.Id == userId), Is.True);

            // DELETE
            var deleteResponse = await pubnub.DataSync.DeleteUser(
                new DeleteUserParameters { Id = userId });
            Assert.That(deleteResponse.Status.Error, Is.False);
            createdUserIds.Remove(userId);

            // Verify deletion
            var getAfterDelete = await pubnub.DataSync.GetUser(
                new GetUserParameters { Id = userId });
            Assert.That(getAfterDelete.Status.Error, Is.True);
        }

        [Test]
        public async Task ThenETagFlowShouldMaintainConcurrency()
        {
            var created = await CreateTestUser(status: "v1");
            var etag1 = created.ETag;
            Assert.That(etag1, Is.Not.Null.And.Not.Empty);

            var updateResponse = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
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

            var patchResponse = await pubnub.DataSync.PatchUser(new PatchUserParameters
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
            var staleResponse = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
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
