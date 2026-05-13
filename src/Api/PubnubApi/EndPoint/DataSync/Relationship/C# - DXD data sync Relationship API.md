# DataSync — Relationship API

This document describes the proposed C# SDK surface for the Relationship REST API
(`/v4/subkeys/{subscribeKey}/relationships`). Relationships are typed, schema-versioned
links between two entities. They carry a `relationshipClass` (which must be pre-registered
via the server-side Relationship Class API), references to two entities (`entityAId` and
`entityBId`), and a free-form `payload` for custom data.

> **Server-side only prerequisite:** A relationship class with a matching
> `name` + `version` must be created via the metadata API before relationships
> of that class can be created. Relationship class CRUD is a server-side
> operation and is not exposed in the client SDK.

---

## 1. Create Relationship

### Request Parameters — `CreateRelationshipParameters`

```c#
public class CreateRelationshipParameters
{
    /// <summary>
    /// Relationship identifier. Optional — if not provided, the server generates one.
    /// Must be 1–255 characters if provided.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// First entity ID (the "A" side of the relationship). Required. Immutable after creation.
    /// Must be 1–255 characters.
    /// </summary>
    public string EntityAId { get; set; }

    /// <summary>
    /// Second entity ID (the "B" side of the relationship). Required. Immutable after creation.
    /// Must be 1–255 characters.
    /// </summary>
    public string EntityBId { get; set; }

    /// <summary>
    /// Relationship class identifier (e.g., "ProductOwner", "FriendOf").
    /// Required. Immutable after creation.
    /// </summary>
    public string RelationshipClass { get; set; }

    /// <summary>
    /// Version of the relationship class. Required. Must be >= 1.
    /// </summary>
    public int RelationshipClassVersion { get; set; }

    /// <summary>
    /// Relationship status (e.g., "active", "inactive"). 1–100 characters.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// User-defined custom properties. Supports arbitrarily nested objects.
    /// </summary>
    public Dictionary<string, object> Payload { get; set; }

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
POST /v4/subkeys/{subscribeKey}/relationships
Content-Type: application/vnd.pubnub.objects.relationship+json;version=1
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
{
  "data": {
    "id": "rel-001",
    "entityAId": "user-123",
    "entityBId": "product-456",
    "relationshipClass": "ProductOwner",
    "relationshipClassVersion": 1,
    "status": "active",
    "payload": {
      "purchaseDate": "2026-01-15",
      "warrantyYears": 3
    }
  }
}
```

Example response (`201 Created`):

```json
{
  "status": 201,
  "data": {
    "id": "rel-001",
    "entityAId": "user-123",
    "entityBId": "product-456",
    "relationshipClass": "ProductOwner",
    "relationshipClassVersion": 1,
    "status": "active",
    "payload": {
      "purchaseDate": "2026-01-15",
      "warrantyYears": 3
    },
    "createdAt": "2026-04-20T10:00:00.000Z",
    "updatedAt": "2026-04-20T10:00:00.000Z",
    "eTag": "Aqrs1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.CreateRelationship(new CreateRelationshipParameters
{
    Id = "rel-001",
    EntityAId = "user-123",
    EntityBId = "product-456",
    RelationshipClass = "ProductOwner",
    RelationshipClassVersion = 1,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "purchaseDate", "2026-01-15" },
        { "warrantyYears", 3 }
    },
    IdempotencyKey = "f47ac10b-58cc-4372-a567-0e02b2c3d479"
});
var relationship = result.Data;
Console.WriteLine(relationship.Id);                // "rel-001"
Console.WriteLine(relationship.RelationshipClass); // "ProductOwner"
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.CreateRelationship()
    .Id("rel-001")
    .EntityAId("user-123")
    .EntityBId("product-456")
    .RelationshipClass("ProductOwner")
    .RelationshipClassVersion(1)
    .Status("active")
    .Payload(new Dictionary<string, object>
    {
        { "purchaseDate", "2026-01-15" },
        { "warrantyYears", 3 }
    })
    .IdempotencyKey("f47ac10b-58cc-4372-a567-0e02b2c3d479")
    .ExecuteAsync();
var relationship = result.Data;
```

---

## 2. Get Relationship by ID

### Request Parameters — `GetRelationshipParameters`

```c#
public class GetRelationshipParameters
{
    /// <summary>
    /// Relationship identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
GET /v4/subkeys/{subscribeKey}/relationships/{relationshipId}
```

No request body.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "rel-001",
    "entityAId": "user-123",
    "entityBId": "product-456",
    "relationshipClass": "ProductOwner",
    "relationshipClassVersion": 1,
    "status": "active",
    "payload": {
      "purchaseDate": "2026-01-15",
      "warrantyYears": 3
    },
    "createdAt": "2026-04-20T10:00:00.000Z",
    "updatedAt": "2026-04-20T10:00:00.000Z",
    "eTag": "Aqrs1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetRelationship(new GetRelationshipParameters
{
    Id = "rel-001"
});
var relationship = result.Data;
Console.WriteLine(relationship.EntityAId);             // "user-123"
Console.WriteLine(relationship.Payload["warrantyYears"]); // 3
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.GetRelationship()
    .Id("rel-001")
    .ExecuteAsync();
var relationship = result.Data;
```

---

## 3. Get Relationships (List)

### Request Parameters — `GetRelationshipsParameters`

```c#
public class GetRelationshipsParameters
{
    /// <summary>
    /// Relationship class name to filter by. Required.
    /// </summary>
    public string RelationshipClass { get; set; }

    /// <summary>
    /// Filter relationships by the first entity (A-side) ID. Optional.
    /// At least one of EntityAId or EntityBId should be provided for meaningful results.
    /// </summary>
    public string EntityAId { get; set; }

    /// <summary>
    /// Filter relationships by the second entity (B-side) ID. Optional.
    /// At least one of EntityAId or EntityBId should be provided for meaningful results.
    /// </summary>
    public string EntityBId { get; set; }

    /// <summary>
    /// Pagination cursor returned from a previous request.
    /// </summary>
    public string Cursor { get; set; }

    /// <summary>
    /// Maximum number of items to return per page.
    /// Min 1, max 100, default 20.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Filter expression using AppContext Query Language (e.g., "status == 'active'").
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    /// Advanced filter expression supporting logical operators and nested conditions.
    /// </summary>
    public string FilterAdvanced { get; set; }

    /// <summary>
    /// Comma-separated list of fields to sort by. Prefix with + for ascending
    /// or - for descending (default). Example: "-createdAt,+id".
    /// </summary>
    public string Sort { get; set; }
}
```

### Request Object Structure (Wire Format)

```
GET /v4/subkeys/{subscribeKey}/relationships?relationship_class={relationshipClass}&entity_a_id={entityAId}&entity_b_id={entityBId}&cursor={cursor}&limit={limit}&filter={filter}&filter_advanced={filterAdvanced}&sort={sort}
```

No request body. All filtering/pagination is via query parameters.
`relationship_class` is the only required query parameter.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": [
    {
      "id": "rel-001",
      "entityAId": "user-123",
      "entityBId": "product-456",
      "relationshipClass": "ProductOwner",
      "relationshipClassVersion": 1,
      "status": "active",
      "payload": {
        "purchaseDate": "2026-01-15",
        "warrantyYears": 3
      },
      "createdAt": "2026-04-20T10:00:00.000Z",
      "updatedAt": "2026-04-20T10:00:00.000Z",
      "eTag": "Aqrs1...",
      "expiresAt": null
    },
    {
      "id": "rel-002",
      "entityAId": "user-123",
      "entityBId": "product-789",
      "relationshipClass": "ProductOwner",
      "relationshipClassVersion": 1,
      "status": "active",
      "payload": {
        "purchaseDate": "2026-03-10",
        "warrantyYears": 2
      },
      "createdAt": "2026-04-19T14:30:00.000Z",
      "updatedAt": "2026-04-20T09:15:00.000Z",
      "eTag": "Btuv2...",
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
    "self": "/subkeys/{subscribeKey}/relationships?relationship_class=ProductOwner&entity_a_id=user-123&limit=10&sort=-createdAt",
    "next": "/subkeys/{subscribeKey}/relationships?relationship_class=ProductOwner&entity_a_id=user-123&cursor=TjIw&limit=10&sort=-createdAt",
    "prev": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
{
    RelationshipClass = "ProductOwner",
    EntityAId = "user-123",
    Limit = 10,
    Filter = "status == 'active'",
    Sort = "-createdAt"
});

foreach (var rel in result.Data)
{
    Console.WriteLine($"{rel.Id}: {rel.EntityAId} -> {rel.EntityBId}");
}

// Pagination
if (result.Meta?.HasNext == true)
{
    var nextPage = await pubnub.DataSync.GetRelationships(new GetRelationshipsParameters
    {
        RelationshipClass = "ProductOwner",
        EntityAId = "user-123",
        Cursor = result.Meta.NextCursor
    });
}
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.GetRelationships()
    .RelationshipClass("ProductOwner")
    .EntityAId("user-123")
    .Limit(10)
    .Filter("status == 'active'")
    .Sort("-createdAt")
    .ExecuteAsync();
```

---

## 4. Update Relationship (Full Replacement)

### Request Parameters — `UpdateRelationshipParameters`

```c#
public class UpdateRelationshipParameters
{
    /// <summary>
    /// Relationship identifier. Required.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Version of the relationship class. Required. Must be >= 1.
    /// Note: entityAId, entityBId, and relationshipClass are immutable
    /// and cannot be changed after creation.
    /// </summary>
    public int RelationshipClassVersion { get; set; }

    /// <summary>
    /// Relationship status (e.g., "active", "inactive"). 1–100 characters.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// User-defined custom properties. Supports arbitrarily nested objects.
    /// Replaces the entire payload — omitted fields are removed.
    /// </summary>
    public Dictionary<string, object> Payload { get; set; }

    /// <summary>
    /// ETag for optimistic concurrency control. If provided, the server rejects
    /// the update when the current resource version does not match (HTTP 412).
    /// </summary>
    public string IfMatch { get; set; }
}
```

### Request Object Structure (Wire Format)

```
PUT /v4/subkeys/{subscribeKey}/relationships/{relationshipId}
Content-Type: application/vnd.pubnub.objects.relationship+json;version=1
If-Match: <optional eTag>
```

Request body:

```json
{
  "data": {
    "relationshipClassVersion": 2,
    "status": "active",
    "payload": {
      "purchaseDate": "2026-01-15",
      "warrantyYears": 5,
      "extendedWarranty": true
    }
  }
}
```

Note: `entityAId`, `entityBId`, and `relationshipClass` are **not** included in the update body because they are immutable.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "rel-001",
    "entityAId": "user-123",
    "entityBId": "product-456",
    "relationshipClass": "ProductOwner",
    "relationshipClassVersion": 2,
    "status": "active",
    "payload": {
      "purchaseDate": "2026-01-15",
      "warrantyYears": 5,
      "extendedWarranty": true
    },
    "createdAt": "2026-04-20T10:00:00.000Z",
    "updatedAt": "2026-04-20T14:30:00.000Z",
    "eTag": "Cwxy3...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.UpdateRelationship(new UpdateRelationshipParameters
{
    Id = "rel-001",
    RelationshipClassVersion = 2,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "purchaseDate", "2026-01-15" },
        { "warrantyYears", 5 },
        { "extendedWarranty", true }
    },
    IfMatch = "Aqrs1..."
});
var relationship = result.Data;
Console.WriteLine(relationship.ETag); // "Cwxy3..."
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.UpdateRelationship()
    .Id("rel-001")
    .RelationshipClassVersion(2)
    .Status("active")
    .Payload(new Dictionary<string, object>
    {
        { "purchaseDate", "2026-01-15" },
        { "warrantyYears", 5 },
        { "extendedWarranty", true }
    })
    .IfMatch("Aqrs1...")
    .ExecuteAsync();
var relationship = result.Data;
```

---

## 5. Patch Relationship (JSON Patch — RFC 6902)

### Request Parameters — `PatchRelationshipParameters`

```c#
public class PatchRelationshipParameters
{
    /// <summary>
    /// Relationship identifier. Required.
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
    public string IfMatch { get; set; }

    /// <summary>
    /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
    /// Required for PATCH requests.
    /// </summary>
    public string IdempotencyKey { get; set; }
}
```

`JsonPatchOperation` is the same class defined in the Entity API (see `PatchEntityParameters.cs`).

### Request Object Structure (Wire Format)

```
PATCH /v4/subkeys/{subscribeKey}/relationships/{relationshipId}
Content-Type: application/json-patch+json
If-Match: <optional eTag>
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
[
  { "op": "replace", "path": "/status", "value": "inactive" },
  { "op": "add",     "path": "/payload/reason", "value": "product returned" },
  { "op": "remove",  "path": "/payload/extendedWarranty" }
]
```

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "rel-001",
    "entityAId": "user-123",
    "entityBId": "product-456",
    "relationshipClass": "ProductOwner",
    "relationshipClassVersion": 2,
    "status": "inactive",
    "payload": {
      "purchaseDate": "2026-01-15",
      "warrantyYears": 5,
      "reason": "product returned"
    },
    "createdAt": "2026-04-20T10:00:00.000Z",
    "updatedAt": "2026-04-20T16:45:00.000Z",
    "eTag": "Dyz04...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.PatchRelationship(new PatchRelationshipParameters
{
    Id = "rel-001",
    Operations = new List<JsonPatchOperation>
    {
        new JsonPatchOperation { Op = JsonPatchOperationType.Replace, Path = "/status", Value = "inactive" },
        new JsonPatchOperation { Op = JsonPatchOperationType.Add, Path = "/payload/reason", Value = "product returned" },
        new JsonPatchOperation { Op = JsonPatchOperationType.Remove, Path = "/payload/extendedWarranty" }
    },
    IfMatch = "Cwxy3...",
    IdempotencyKey = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
});
var relationship = result.Data;
Console.WriteLine(relationship.Status);              // "inactive"
Console.WriteLine(relationship.Payload["reason"]);   // "product returned"
```

Alternative API using the existing builder pattern:

```c#
var result = await pubnub.DataSync.PatchRelationship()
    .Id("rel-001")
    .Replace("/status", "inactive")
    .Add("/payload/reason", "product returned")
    .Remove("/payload/extendedWarranty")
    .IfMatch("Cwxy3...")
    .IdempotencyKey("a1b2c3d4-e5f6-7890-abcd-ef1234567890")
    .ExecuteAsync();
var relationship = result.Data;
```

---

## 6. Delete Relationship

### Request Parameters — `DeleteRelationshipParameters`

```c#
public class DeleteRelationshipParameters
{
    /// <summary>
    /// Relationship identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
DELETE /v4/subkeys/{subscribeKey}/relationships/{relationshipId}
```

No request body.

Example response (`200 OK`):

The response body is empty on success.

### User-facing API call example

```c#
await pubnub.DataSync.DeleteRelationship(new DeleteRelationshipParameters
{
    Id = "rel-001"
});
```

Alternative API using the existing builder pattern:

```c#
await pubnub.DataSync.DeleteRelationship()
    .Id("rel-001")
    .ExecuteAsync();
```

---

## Appendix: Response Model

All single-relationship responses (`CreateRelationship`, `GetRelationship`, `UpdateRelationship`,
`PatchRelationship`) return an envelope with a `data` property containing a `RelationshipResource`:

```c#
public class RelationshipResource
{
    public string Id { get; set; }
    public string EntityAId { get; set; }
    public string EntityBId { get; set; }
    public string RelationshipClass { get; set; }
    public int RelationshipClassVersion { get; set; }
    public string Status { get; set; }
    public Dictionary<string, object> Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ETag { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
```

All responses (single and list) may include `links` and `meta`. List responses (`GetRelationships`)
use `meta` for cursor-based pagination and `links` for HATEOAS navigation:

```c#
public class RelationshipsListResult
{
    public List<RelationshipResource> Data { get; set; }
    public PaginationMeta Meta { get; set; }
    public PaginationLinks Links { get; set; }
}
```

`PaginationMeta` and `PaginationLinks` are the same classes defined in the Entity API.

## Appendix: Error Responses

All relationship endpoints may return the following error codes:

| HTTP Status | Meaning | When |
|---|---|---|
| 400 | Bad Request | Malformed JSON, missing required fields, validation failure |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Valid auth but insufficient permissions |
| 404 | Not Found | Relationship ID does not exist (single-resource endpoints only) |
| 406 | Not Acceptable | `Accept` header not supported |
| 412 | Precondition Failed | `If-Match` ETag does not match current version (PUT, PATCH) |
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
      "path": "/entityAId"
    }
  ]
}
```
