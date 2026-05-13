using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi.EndPoint;
using PubNubMessaging.Tests;

namespace PubnubApi.Tests.DataSync;

[TestFixture]
public class WhenCreateGenericEntity
{
    private static readonly string PlaygroundSubKey = "sub-c-bd4cd136-8eca-45c2-b5db-73d3b43d6552";
    
    [Test]
    public async Task PlaygroundCreate()
    {
        var testPubnub = new Pubnub(new PNConfiguration(new UserId("data_sync_test_guy"))
        {
            SubscribeKey = PlaygroundSubKey,
            Origin = "pn-cupid-objekts.core.az1.pdx1.aws.int.ps.pn"
        });
        var op = new CreateEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new CreateEntityParameters
            {
                Id = "entity-abc-10",
                EntityClass = "vehicle",
                EntityClassVersion = 1,
                Status = "active",
                Payload = new Dictionary<string, object>
                {
                    { "make", "Toyota" },
                    { "model", "Camry" },
                    { "year", 2025 },
                    { "owner", new Dictionary<string, object>
                        {
                            { "name", "Alice" },
                            { "license", "XYZ-1234" }
                        }
                    }
                },
                IdempotencyKey = "f47ac10b-58cc-4372-a567-099999999999"
            }
        );
        var response = await op.ExecuteAsync();
        ;
    }
    
    [Test]
    public async Task PlaygroundGet()
    {
        var testPubnub = new Pubnub(new PNConfiguration(new UserId("data_sync_test_guy"))
        {
            SubscribeKey = PlaygroundSubKey,
            Origin = "pn-cupid-objekts.core.az1.pdx1.aws.int.ps.pn"
        });
        var op = new GetEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new GetEntityParameters() { Id = "entity-abc-5" });
        var response = await op.ExecuteAsync();
        ;
    }
    
    [Test]
    public async Task PlaygroundGetList()
    {
        var testPubnub = new Pubnub(new PNConfiguration(new UserId("data_sync_test_guy"))
        {
            SubscribeKey = PlaygroundSubKey,
            Origin = "pn-cupid-objekts.core.az1.pdx1.aws.int.ps.pn"
        });
        var op = new GetEntitiesOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new GetEntitiesParameters()
            {
                EntityClass = "vehicle",
                EntityClassVersion = 1
            });
        var response = await op.ExecuteAsync();
        ;
    }
    
    [Test]
    public async Task PlaygroundUpdateEntity()
    {
        var testPubnub = new Pubnub(new PNConfiguration(new UserId("data_sync_test_guy"))
        {
            SubscribeKey = PlaygroundSubKey,
            Origin = "pn-cupid-objekts.core.az1.pdx1.aws.int.ps.pn"
        });
        var op = new UpdateEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new UpdateEntityParameters()
            {
                Id = "entity-abc",
                EntityClassVersion = 1,
                Payload = new Dictionary<string, object>
                {
                    { "make", "Ford" },
                    { "model", "Focus" },
                    { "year", 1995 },
                    { "owner", new Dictionary<string, object>
                        {
                            { "name", "John" },
                            { "license", "ABC-4321" }
                        }
                    }
                }
                
            });
        var response = await op.ExecuteAsync();
        ;
    }
    
    [Test]
    public async Task PlaygroundDeleteEntity()
    {
        var testPubnub = new Pubnub(new PNConfiguration(new UserId("data_sync_test_guy"))
        {
            SubscribeKey = PlaygroundSubKey,
            Origin = "pn-cupid-objekts.core.az1.pdx1.aws.int.ps.pn"
        });
        var op = new DeleteEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new DeleteEntityParameters()
            {
                Id = "entity-abc-3"
            });
        var response = await op.ExecuteAsync();
        ;
    }

    [Test]
    public async Task PlaygroundPatchEntity()
    {
        var testPubnub = new Pubnub(new PNConfiguration(new UserId("data_sync_test_guy"))
        {
            SubscribeKey = PlaygroundSubKey,
            Origin = "pn-cupid-objekts.core.az1.pdx1.aws.int.ps.pn"
        });
        var entityId = "entity-abc-10";

        // 1. replace — change a top-level field
        var r1 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/status", Value = "inactive" }
                },
                IdempotencyKey = "a1b2c3d4-0001-4000-a000-000000000001"
            }).ExecuteAsync();
        ;

        // 2. add — insert a new nested field with a complex value
        var r2 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation
                    {
                        Op = JsonPatchOperationType.Add,
                        Path = "/payload/maintenance",
                        Value = new Dictionary<string, object>
                        {
                            { "lastServiceDate", "2026-03-01" },
                            { "mileageAtService", 41500 }
                        }
                    }
                },
                IdempotencyKey = "a1b2c3d4-0002-4000-a000-000000000002"
            }).ExecuteAsync();
        ;

        // 3. remove — delete a field
        var r3 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Remove, Path = "/payload/owner/license" }
                },
                IdempotencyKey = "a1b2c3d4-0003-4000-a000-000000000003"
            }).ExecuteAsync();
        ;

        // 4. move — relocate a field from one path to another
        var r4 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Move, Path = "/payload/previousOwner", From = "/payload/owner" }
                },
                IdempotencyKey = "a1b2c3d4-0004-4000-a000-000000000004"
            }).ExecuteAsync();
        ;

        // 5. copy — duplicate a value to a new location
        var r5 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Copy, Path = "/payload/makeBackup", From = "/payload/make" }
                },
                IdempotencyKey = "a1b2c3d4-0005-4000-a000-000000000005"
            }).ExecuteAsync();
        ;

        // 6. test + replace — conditional guard; whole batch fails if test fails
        var r6 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Test, Path = "/status", Value = "inactive" },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/status", Value = "suspended" }
                },
                IdempotencyKey = "a1b2c3d4-0006-4000-a000-000000000006"
            }).ExecuteAsync();
        ;

        // 7. add with null value — explicitly set a field to null
        var r7 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Add, Path = "/payload/notes", Value = null }
                },
                IdempotencyKey = "a1b2c3d4-0007-4000-a000-000000000007"
            }).ExecuteAsync();
        ;

        // 8. add to array — append via RFC 6901 end-of-array "/-" pointer
        var r8 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Add, Path = "/payload/tags/-", Value = "premium" }
                },
                IdempotencyKey = "a1b2c3d4-0008-4000-a000-000000000008"
            }).ExecuteAsync();
        ;

        // 9. replace with numeric and boolean values
        var r9 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/year", Value = 2027 },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/mileage", Value = 55000.5 },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/payload/isElectric", Value = true }
                },
                IdempotencyKey = "a1b2c3d4-0009-4000-a000-000000000009"
            }).ExecuteAsync();
        ;

        // 10. multi-op batch with IfMatch — all six op types + optimistic concurrency
        var r10 = await new PatchEntityOperation(testPubnub.PNConfig, testPubnub.JsonPluggableLibrary, null, null, testPubnub,
            new PatchEntityParameters
            {
                Id = entityId,
                Operations = new List<JsonPatchOperation>
                {
                    new JsonPatchOperation { Op = JsonPatchOperationType.Test,    Path = "/status",               Value = "suspended" },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/status",               Value = "active" },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Add,     Path = "/payload/mileage",      Value = 42000 },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Remove,  Path = "/payload/notes" },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Copy,    Path = "/payload/modelCopy",    From = "/payload/model" },
                    new JsonPatchOperation { Op = JsonPatchOperationType.Move,    Path = "/payload/prevOwner",    From = "/payload/previousOwner" }
                },
                IfMatch = "REPLACE_WITH_CURRENT_ETAG",
                IdempotencyKey = "a1b2c3d4-0010-4000-a000-000000000010"
            }).ExecuteAsync();
        ;
    }
}