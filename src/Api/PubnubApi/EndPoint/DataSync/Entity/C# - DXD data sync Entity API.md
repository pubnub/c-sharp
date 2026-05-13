# DataSync — Entity API

This document describes the proposed C# SDK surface for the generic Entity REST API
(`/v4/subkeys/{subscribeKey}/entities`). Entities are schema-versioned, class-based objects
that carry a free-form `payload` instead of the fixed fields found on Users or Channels.

---

## 1. Create Entity

### Request Parameters — `CreateEntityParameters`

```c#
public class CreateEntityParameters
{
    /// <summary>
    /// Entity identifier. Optional — if not provided, the server generates one.
    /// Must be 1–255 characters if provided.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Entity class identifier (e.g., "vehicle", "order", "sensor").
    /// Required. Immutable after creation.
    /// </summary>
    public string EntityClass { get; set; }

    /// <summary>
    /// Schema version of the entity class. Required. Must be >= 1.
    /// </summary>
    public int EntityClassVersion { get; set; }

    /// <summary>
    /// Entity status (e.g., "active", "inactive"). 1–100 characters.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// User-defined custom properties. Supports arbitrarily nested objects.
    /// </summary>
    public Dictionary<string, object>? Payload { get; set; }

    /// <summary>
    /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
    /// Required for POST requests. If a request with the same idempotency key
    /// was already processed, the server returns the original response without
    /// creating a duplicate.
    /// </summary>
    public string IdempotencyKey { get; set; }
}
```

### Request Object Structure (Wire Format)

The SDK constructs the following HTTP request:

```
POST /v4/subkeys/{subscribeKey}/entities
Content-Type: application/vnd.pubnub.objects.entity+json;version=1
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
{
  "data": {
    "id": "entity-abc",
    "entityClass": "vehicle",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "make": "Toyota",
      "model": "Camry",
      "year": 2025,
      "owner": {
        "name": "Alice",
        "license": "XYZ-1234"
      }
    }
  }
}
```

Example response (`201 Created`):

```json
{
  "status": 201,
  "data": {
    "id": "entity-abc",
    "entityClass": "vehicle",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "make": "Toyota",
      "model": "Camry",
      "year": 2025,
      "owner": {
        "name": "Alice",
        "license": "XYZ-1234"
      }
    },
    "createdAt": "2026-03-20T12:00:00.000Z",
    "updatedAt": "2026-03-20T12:00:00.000Z",
    "eTag": "BfklQ...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.CreateEntity(new CreateEntityParameters
{
    Id = "entity-abc",
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
    IdempotencyKey = "f47ac10b-58cc-4372-a567-0e02b2c3d479"
});
var entity = result.Data;
Console.WriteLine(entity.Id);          // "entity-abc"
Console.WriteLine(entity.EntityClass); // "vehicle"
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.CreateEntity()
    .Id("entity-abc")
    .EntityClass("vehicle")
    .EntityClassVersion(1)
    .Status("active")
    .Payload(new Dictionary<string, object>
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
    })
    .IdempotencyKey("f47ac10b-58cc-4372-a567-0e02b2c3d479")
    .ExecuteAsync();
var entity = result.Data;
```

---

## 2. Get Entity by ID

### Request Parameters — `GetEntityParameters`

```c#
public class GetEntityParameters
{
    /// <summary>
    /// Entity identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
GET /v4/subkeys/{subscribeKey}/entities/{entityId}
```

No request body.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "entity-abc",
    "entityClass": "vehicle",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "make": "Toyota",
      "model": "Camry",
      "year": 2025,
      "owner": {
        "name": "Alice",
        "license": "XYZ-1234"
      }
    },
    "createdAt": "2026-03-20T12:00:00.000Z",
    "updatedAt": "2026-03-20T12:00:00.000Z",
    "eTag": "BfklQ...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetEntity(new GetEntityParameters
{
    Id = "entity-abc"
});
var entity = result.Data;
Console.WriteLine(entity.Payload["make"]); // "Toyota"
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.GetEntity()
    .Id("entity-abc")
    .ExecuteAsync();
var entity = result.Data;
```

---

## 3. Get Entities (List)

### Request Parameters — `GetEntitiesParameters`

```c#
public class GetEntitiesParameters
{
    /// <summary>
    /// Entity class name to filter by. Required.
    /// </summary>
    public string EntityClass { get; set; }

    /// <summary>
    /// Schema version of the entity class. Optional — if not provided the server
    /// returns entities matching the latest version.
    /// </summary>
    public int? EntityClassVersion { get; set; }

    /// <summary>
    /// Pagination cursor returned from a previous request.
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// Maximum number of items to return per page.
    /// Min 1, max 100, default 20.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Filter expression using AppContext Query Language (e.g., "status == 'active'").
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// Advanced filter expression supporting logical operators and nested conditions.
    /// </summary>
    public string? FilterAdvanced { get; set; }

    /// <summary>
    /// Comma-separated list of fields to sort by. Prefix with + for ascending
    /// or - for descending (default). Example: "-createdAt,+id".
    /// </summary>
    public string? Sort { get; set; }
}
```

### Request Object Structure (Wire Format)

```
GET /v4/subkeys/{subscribeKey}/entities?entity_class={entityClass}&entity_class_version={entityClassVersion}&cursor={cursor}&limit={limit}&filter={filter}&filter_advanced={filterAdvanced}&sort={sort}
```

No request body. All filtering/pagination is via query parameters.
`entity_class` is the only required query parameter.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": [
    {
      "id": "entity-abc",
      "entityClass": "vehicle",
      "entityClassVersion": 1,
      "status": "active",
      "payload": {
        "make": "Toyota",
        "model": "Camry",
        "year": 2025
      },
      "createdAt": "2026-03-20T12:00:00.000Z",
      "updatedAt": "2026-03-20T12:00:00.000Z",
      "eTag": "BfklQ...",
      "expiresAt": null
    },
    {
      "id": "entity-def",
      "entityClass": "vehicle",
      "entityClassVersion": 1,
      "status": "inactive",
      "payload": {
        "make": "Honda",
        "model": "Civic",
        "year": 2024
      },
      "createdAt": "2026-03-19T08:00:00.000Z",
      "updatedAt": "2026-03-20T09:30:00.000Z",
      "eTag": "Xmrp2...",
      "expiresAt": null
    }
  ],
  "meta": {
    "next_cursor": "TjIw",
    "prev_cursor": null,
    "has_next": true,
    "has_prev": false,
    "limit": 10
  },
  "links": {
    "self": "/subkeys/{subscribeKey}/entities?entity_class=vehicle&limit=10&sort=-createdAt",
    "next": "/subkeys/{subscribeKey}/entities?entity_class=vehicle&cursor=TjIw&limit=10&sort=-createdAt",
    "prev": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
{
    EntityClass = "vehicle",
    EntityClassVersion = 1,
    Limit = 10,
    Filter = "status == 'active'",
    Sort = "-createdAt"
});

foreach (var entity in result.Data)
{
    Console.WriteLine($"{entity.Id}: {entity.Payload["make"]}");
}

// Pagination
if (result.Meta?.HasNext == true)
{
    var nextPage = await pubnub.DataSync.GetEntities(new GetEntitiesParameters
    {
        EntityClass = "vehicle",
        Cursor = result.Meta.NextCursor
    });
}
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.GetEntities()
    .EntityClass("vehicle")
    .EntityClassVersion(1)
    .Limit(10)
    .Filter("status == 'active'")
    .Sort("-createdAt")
    .ExecuteAsync();
```

---

## 4. Update Entity (Full Replacement)

### Request Parameters — `UpdateEntityParameters`

```c#
public class UpdateEntityParameters
{
    /// <summary>
    /// Entity identifier. Required.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Schema version of the entity class. Required. Must be >= 1.
    /// Note: entityClass is immutable and cannot be changed after creation.
    /// </summary>
    public int EntityClassVersion { get; set; }

    /// <summary>
    /// Entity status (e.g., "active", "inactive"). 1–100 characters.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// User-defined custom properties. Supports arbitrarily nested objects.
    /// Replaces the entire payload — omitted fields are removed.
    /// </summary>
    public Dictionary<string, object>? Payload { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency control. If provided, the server rejects
    /// the update when the current resource version does not match (HTTP 412).
    /// </summary>
    public string? IfMatch { get; set; }
}
```

### Request Object Structure (Wire Format)

```
PUT /v4/subkeys/{subscribeKey}/entities/{entityId}
Content-Type: application/vnd.pubnub.objects.entity+json;version=1
If-Match: <optional eTag>
```

Request body:

```json
{
  "data": {
    "entityClassVersion": 2,
    "status": "active",
    "payload": {
      "make": "Toyota",
      "model": "Camry",
      "year": 2026,
      "color": "blue",
      "owner": {
        "name": "Alice",
        "license": "XYZ-1234"
      }
    }
  }
}
```

Note: `entityClass` is **not** included in the update body because it is immutable.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "entity-abc",
    "entityClass": "vehicle",
    "entityClassVersion": 2,
    "status": "active",
    "payload": {
      "make": "Toyota",
      "model": "Camry",
      "year": 2026,
      "color": "blue",
      "owner": {
        "name": "Alice",
        "license": "XYZ-1234"
      }
    },
    "createdAt": "2026-03-20T12:00:00.000Z",
    "updatedAt": "2026-03-20T14:30:00.000Z",
    "eTag": "Cxyz1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.UpdateEntity(new UpdateEntityParameters
{
    Id = "entity-abc",
    EntityClassVersion = 2,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "make", "Toyota" },
        { "model", "Camry" },
        { "year", 2026 },
        { "color", "blue" },
        { "owner", new Dictionary<string, object>
            {
                { "name", "Alice" },
                { "license", "XYZ-1234" }
            }
        }
    },
    IfMatch = "BfklQ..."
});
var entity = result.Data;
Console.WriteLine(entity.ETag); // "Cxyz1..."
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.UpdateEntity()
    .Id("entity-abc")
    .EntityClassVersion(2)
    .Status("active")
    .Payload(new Dictionary<string, object>
    {
        { "make", "Toyota" },
        { "model", "Camry" },
        { "year", 2026 },
        { "color", "blue" },
        { "owner", new Dictionary<string, object>
            {
                { "name", "Alice" },
                { "license", "XYZ-1234" }
            }
        }
    })
    .IfMatch("BfklQ...")
    .ExecuteAsync();
var entity = result.Data;
```

---

## 5. Patch Entity (JSON Patch — RFC 6902)

### Request Parameters — `PatchEntityParameters`

```c#
public class PatchEntityParameters
{
    /// <summary>
    /// Entity identifier. Required.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// List of JSON Patch operations (RFC 6902) to apply.
    /// </summary>
    public List<JsonPatchOperation> Operations { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency control. If provided, the server rejects
    /// the patch when the current resource version does not match (HTTP 412).
    /// </summary>
    public string? IfMatch { get; set; }

    /// <summary>
    /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
    /// Required for PATCH requests.
    /// </summary>
    public string IdempotencyKey { get; set; }
}

public class JsonPatchOperation
{
    /// <summary>
    /// The operation to perform: "add", "remove", "replace", "move", "copy", or "test".
    /// </summary>
    public string Op { get; set; }

    /// <summary>
    /// JSON Pointer (RFC 6901) to the target location.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The value to apply (required for "add", "replace", "test").
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Source path for "move" and "copy" operations.
    /// </summary>
    public string? From { get; set; }
}
```

### Request Object Structure (Wire Format)

```
PATCH /v4/subkeys/{subscribeKey}/entities/{entityId}
Content-Type: application/json-patch+json
If-Match: <optional eTag>
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
[
  { "op": "replace", "path": "/status", "value": "inactive" },
  { "op": "add",     "path": "/payload/mileage", "value": 42000 },
  { "op": "remove",  "path": "/payload/owner/license" }
]
```

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "entity-abc",
    "entityClass": "vehicle",
    "entityClassVersion": 1,
    "status": "inactive",
    "payload": {
      "make": "Toyota",
      "model": "Camry",
      "year": 2025,
      "mileage": 42000,
      "owner": {
        "name": "Alice"
      }
    },
    "createdAt": "2026-03-20T12:00:00.000Z",
    "updatedAt": "2026-03-20T15:45:00.000Z",
    "eTag": "Dpqr3...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.PatchEntity(new PatchEntityParameters
{
    Id = "entity-abc",
    Operations = new List<JsonPatchOperation>
    {
        new JsonPatchOperation { Op = "replace", Path = "/status", Value = "inactive" },
        new JsonPatchOperation { Op = "add", Path = "/payload/mileage", Value = 42000 },
        new JsonPatchOperation { Op = "remove", Path = "/payload/owner/license" }
    },
    IfMatch = "BfklQ...",
    IdempotencyKey = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
});
var entity = result.Data;
Console.WriteLine(entity.Status);              // "inactive"
Console.WriteLine(entity.Payload["mileage"]);  // 42000
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.PatchEntity()
    .Id("entity-abc")
    .Replace("/status", "inactive")
    .Add("/payload/mileage", 42000)
    .Remove("/payload/owner/license")
    .IfMatch("BfklQ...")
    .IdempotencyKey("a1b2c3d4-e5f6-7890-abcd-ef1234567890")
    .ExecuteAsync();
var entity = result.Data;
```

---

## 6. Delete Entity

### Request Parameters — `DeleteEntityParameters`

```c#
public class DeleteEntityParameters
{
    /// <summary>
    /// Entity identifier. Required.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency control. If provided, the server rejects
    /// the delete when the current resource version does not match (HTTP 412).
    /// </summary>
    public string? IfMatch { get; set; }
}
```

### Request Object Structure (Wire Format)

```
DELETE /v4/subkeys/{subscribeKey}/entities/{entityId}
If-Match: <optional eTag>
```

No request body.

Example response (`200 OK`):

The response body is empty on success.

### User-facing API call example

```c#
await pubnub.DataSync.DeleteEntity(new DeleteEntityParameters
{
    Id = "entity-abc",
    IfMatch = "Dpqr3..."
});
```

Alternative API using the existing builder pattern:

```c#
await pubnub.DataSync.DeleteEntity()
    .Id("entity-abc")
    .IfMatch("Dpqr3...")
    .ExecuteAsync();
```

---

## Appendix: Response Model

All single-entity responses (`CreateEntity`, `GetEntity`, `UpdateEntity`, `PatchEntity`) return
an envelope with a `data` property containing an `EntityResource`:

```c#
public class EntityResource
{
    public string Id { get; set; }
    public string EntityClass { get; set; }
    public int EntityClassVersion { get; set; }
    public string? Status { get; set; }
    public Dictionary<string, object>? Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ETag { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
```

All responses (single and list) may include `links` and `meta`. List responses (`GetEntities`)
use `meta` for cursor-based pagination and `links` for HATEOAS navigation:

```c#
public class EntitiesListResult
{
    public List<EntityResource> Data { get; set; }
    public PaginationMeta? Meta { get; set; }
    public PaginationLinks? Links { get; set; }
}

public class PaginationMeta
{
    public string? NextCursor { get; set; }
    public string? PrevCursor { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrev { get; set; }
    public int? Limit { get; set; }
}

public class PaginationLinks
{
    public string Self { get; set; }
    public string? Next { get; set; }
    public string? Prev { get; set; }
}
```

## Appendix: Error Responses

All entity endpoints may return the following error codes:

| HTTP Status | Meaning | When |
|---|---|---|
| 400 | Bad Request | Malformed JSON, missing required fields, validation failure |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Valid auth but insufficient permissions |
| 404 | Not Found | Entity ID does not exist (single-resource endpoints only) |
| 406 | Not Acceptable | `Accept` header not supported |
| 412 | Precondition Failed | `If-Match` ETag does not match current version (PUT, PATCH, DELETE) |
| 415 | Unsupported Media Type | Wrong `Content-Type` header (POST, PUT, PATCH) |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Unexpected server failure |
| 503 | Service Unavailable | Server temporarily down |

Error response body format:

```json
{
  "errors": [
    {
      "errorCode": "SYN_1004",
      "message": "Invalid request",
      "path": "/entityClass"
    }
  ]
}
```
