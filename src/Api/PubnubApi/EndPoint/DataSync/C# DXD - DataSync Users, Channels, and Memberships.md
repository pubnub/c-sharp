# API

# DataSync — User, Channel & Membership API

This document describes the proposed C\# SDK surface for the **User** REST API (`/v4/subkeys/{subscribeKey}/users`), the **Channel** REST API (`/v4/subkeys/{subscribeKey}/channels`), and the **Membership** REST API (`/v4/subkeys/{subscribeKey}/memberships`).

Users and Channels are typed specialisations of generic Entities — the server implicitly sets `entityClass = "user"` or `entityClass = "channel"` respectively, so the SDK does not expose `entityClass` as a parameter. Both share the `EntityResource` response model.

Memberships are a typed specialisation of generic Relationships — the server implicitly sets `relationshipClass = "membership"` and exposes domain-friendly fields `channelId` / `userId` instead of `entityAId` / `entityBId`. The response model reuses `RelationshipResource`.

# User

## User Appendix: Response Model

All single-user responses (`CreateUser`, `GetUser`, `UpdateUser`, `PatchUser`) return `PNResult<PNDataSyncUserResult>`.

```c#
public class PNDataSyncUserResult
{
    public string Id { get; internal set; }
    public int EntityClassVersion { get; internal set; }
    public string Status { get; internal set; }
    public Dictionary<string, object> Payload { get; internal set; }
    public string CreatedAt { get; internal set; }
    public string UpdatedAt { get; internal set; }
    public string ETag { get; internal set; }
    public string ExpiresAt { get; internal set; }
}
```

Delete returns `PNResult<PNDataSyncDeleteUserResult>`:

```c#
public class PNDataSyncDeleteUserResult
{
}
```

List responses (`GetUsers`) return `PNResult<PNDataSyncUsersListResult>`:

```c#
public class PNDataSyncUsersListResult
{
    public List<PNDataSyncUserResult> Data { get; internal set; } = new();
    public PaginationMeta Meta { get; internal set; }
    public PaginationLinks Links { get; internal set; }
}
```

`PaginationMeta` and `PaginationLinks` are the same classes defined in the Entity API.

# CreateUser

## Create User

### Request Parameters — `CreateUserParameters`

```c#
public class CreateUserParameters
{
    /// <summary>
    /// User identifier. Optional — if not provided, the server generates one.
    /// Must be 1–255 characters if provided.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Schema version of the entity class. Required. Must be >= 1.
    /// </summary>
    public int EntityClassVersion { get; set; }

    /// <summary>
    /// User status (e.g., "active", "inactive"). 1–100 characters.
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
POST /v4/subkeys/{subscribeKey}/users
Content-Type: application/vnd.pubnub.objects.user+json;version=1
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
{
  "data": {
    "id": "user-alice",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "displayName": "Alice Johnson",
      "email": "alice@example.com",
      "preferences": {
        "theme": "dark",
        "locale": "en-US"
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
    "id": "user-alice",
    "entityClass": "user",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "displayName": "Alice Johnson",
      "email": "alice@example.com",
      "preferences": {
        "theme": "dark",
        "locale": "en-US"
      }
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T12:00:00.000Z",
    "eTag": "AuXk1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.CreateUser(new CreateUserParameters
{
    Id = "user-alice",
    EntityClassVersion = 1,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "displayName", "Alice Johnson" },
        { "email", "alice@example.com" },
        { "preferences", new Dictionary<string, object>
            {
                { "theme", "dark" },
                { "locale", "en-US" }
            }
        }
    },
    IdempotencyKey = "f47ac10b-58cc-4372-a567-0e02b2c3d479"
});
var user = result.Result;
Console.WriteLine(user.Id);     // "user-alice"
Console.WriteLine(user.Status); // "active"
```

# GetUser

## Get User by ID

### Request Parameters — `GetUserParameters`

```c#
public class GetUserParameters
{
    /// <summary>
    /// User identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
GET /v4/subkeys/{subscribeKey}/users/{userId}
```

No request body.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "user-alice",
    "entityClass": "user",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "displayName": "Alice Johnson",
      "email": "alice@example.com",
      "preferences": {
        "theme": "dark",
        "locale": "en-US"
      }
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T12:00:00.000Z",
    "eTag": "AuXk1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetUser(new GetUserParameters
{
    Id = "user-alice"
});
var user = result.Result;
Console.WriteLine(user.Payload["displayName"]); // "Alice Johnson"
```

# GetUsers

##  Get Users (List)

### Request Parameters — `GetUsersParameters`

```c#
public class GetUsersParameters
{
    /// <summary>
    /// Schema version of the entity class. Optional — if not provided the server
    /// returns users matching the latest version.
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
GET /v4/subkeys/{subscribeKey}/users?entity_class_version={entityClassVersion}&cursor={cursor}&limit={limit}&filter={filter}&filter_advanced={filterAdvanced}&sort={sort}
```

No request body. All filtering/pagination is via query parameters. Unlike the generic entity list, no `entity_class` parameter is required — it is implicitly `"user"`.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": [
    {
      "id": "user-alice",
      "entityClass": "user",
      "entityClassVersion": 1,
      "status": "active",
      "payload": {
        "displayName": "Alice Johnson",
        "email": "alice@example.com"
      },
      "createdAt": "2026-05-10T12:00:00.000Z",
      "updatedAt": "2026-05-10T12:00:00.000Z",
      "eTag": "AuXk1...",
      "expiresAt": null
    },
    {
      "id": "user-bob",
      "entityClass": "user",
      "entityClassVersion": 1,
      "status": "active",
      "payload": {
        "displayName": "Bob Smith",
        "email": "bob@example.com"
      },
      "createdAt": "2026-05-09T08:00:00.000Z",
      "updatedAt": "2026-05-10T09:30:00.000Z",
      "eTag": "BnYm2...",
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
    "self": "/subkeys/{subscribeKey}/users?limit=10&sort=-createdAt",
    "next": "/subkeys/{subscribeKey}/users?cursor=TjIw&limit=10&sort=-createdAt",
    "prev": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetUsers(new GetUsersParameters
{
    EntityClassVersion = 1,
    Limit = 10,
    Filter = "status == 'active'",
    Sort = "-createdAt"
});

foreach (var user in result.Result)
{
    Console.WriteLine($"{user.Id}: {user.Payload["displayName"]}");
}

// Pagination
if (result.Meta?.HasNext == true)
{
    var nextPage = await pubnub.DataSync.GetUsers(new GetUsersParameters
    {
        Cursor = result.Meta.NextCursor
    });
}
```

# UpdateUser

## Update User (Full Replacement)

### Request Parameters — `UpdateUserParameters`

```c#
public class UpdateUserParameters
{
    /// <summary>
    /// User identifier. Required.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Schema version of the entity class. Required. Must be >= 1.
    /// </summary>
    public int EntityClassVersion { get; set; }

    /// <summary>
    /// User status (e.g., "active", "inactive"). 1–100 characters.
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
PUT /v4/subkeys/{subscribeKey}/users/{userId}
Content-Type: application/vnd.pubnub.objects.user+json;version=1
If-Match: <optional eTag>
```

Request body:

```json
{
  "data": {
    "entityClassVersion": 2,
    "status": "active",
    "payload": {
      "displayName": "Alice J.",
      "email": "alice.j@example.com",
      "preferences": {
        "theme": "light",
        "locale": "en-GB"
      },
      "bio": "Software engineer"
    }
  }
}
```

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "user-alice",
    "entityClass": "user",
    "entityClassVersion": 2,
    "status": "active",
    "payload": {
      "displayName": "Alice J.",
      "email": "alice.j@example.com",
      "preferences": {
        "theme": "light",
        "locale": "en-GB"
      },
      "bio": "Software engineer"
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T14:30:00.000Z",
    "eTag": "CpQr3...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.UpdateUser(new UpdateUserParameters
{
    Id = "user-alice",
    EntityClassVersion = 2,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "displayName", "Alice J." },
        { "email", "alice.j@example.com" },
        { "preferences", new Dictionary<string, object>
            {
                { "theme", "light" },
                { "locale", "en-GB" }
            }
        },
        { "bio", "Software engineer" }
    },
    IfMatch = "AuXk1..."
});
var user = result.Result;
Console.WriteLine(user.ETag); // "CpQr3..."
```

# PatchUser

## Patch User (JSON Patch — RFC 6902\)

### Request Parameters — `PatchUserParameters`

```c#
public class PatchUserParameters
{
    /// <summary>
    /// User identifier. Required.
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
```

`JsonPatchOperation` is the same class defined in the Entity API (see `PatchEntityParameters.cs`).

### Request Object Structure (Wire Format)

```
PATCH /v4/subkeys/{subscribeKey}/users/{userId}
Content-Type: application/json-patch+json
If-Match: <optional eTag>
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
[
  { "op": "replace", "path": "/status", "value": "inactive" },
  { "op": "add",     "path": "/payload/bio", "value": "Senior engineer" },
  { "op": "remove",  "path": "/payload/preferences/locale" }
]
```

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "user-alice",
    "entityClass": "user",
    "entityClassVersion": 2,
    "status": "inactive",
    "payload": {
      "displayName": "Alice J.",
      "email": "alice.j@example.com",
      "preferences": {
        "theme": "light"
      },
      "bio": "Senior engineer"
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T15:45:00.000Z",
    "eTag": "DsWx4...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.PatchUser(new PatchUserParameters
{
    Id = "user-alice",
    Operations = new List<JsonPatchOperation>
    {
        new JsonPatchOperation { Op = "replace", Path = "/status", Value = "inactive" },
        new JsonPatchOperation { Op = "add", Path = "/payload/bio", Value = "Senior engineer" },
        new JsonPatchOperation { Op = "remove", Path = "/payload/preferences/locale" }
    },
    IfMatch = "CpQr3...",
    IdempotencyKey = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
});
var user = result.Result;
Console.WriteLine(user.Status);          // "inactive"
Console.WriteLine(user.Payload["bio"]);  // "Senior engineer"
```

# DeleteUser

## Delete User

### Request Parameters — `DeleteUserParameters`

```c#
public class DeleteUserParameters
{
    /// <summary>
    /// User identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
DELETE /v4/subkeys/{subscribeKey}/users/{userId}
```

No request body. No `If-Match` header support on this endpoint.

Example response (`200 OK`):

The response body is empty on success.

### User-facing API call example

```c#
await pubnub.DataSync.DeleteUser(new DeleteUserParameters
{
    Id = "user-alice"
});
```

# Channel

## Channel API Response Model

Channels are structurally identical to Users — both are typed specialisations of generic Entities. The server implicitly sets `entityClass = "channel"`, so the SDK does not expose `entityClass` as a parameter. 

All single-channel responses (`CreateChannel`, `GetChannel`, `UpdateChannel`, `PatchChannel`) return `PNResult<PNDataSyncChannelResult>`:

```c#
public class PNDataSyncChannelResult
{
    public string Id { get; internal set; }
    public int EntityClassVersion { get; internal set; }
    public string Status { get; internal set; }
    public Dictionary<string, object> Payload { get; internal set; }
    public string CreatedAt { get; internal set; }
    public string UpdatedAt { get; internal set; }
    public string ETag { get; internal set; }
    public string ExpiresAt { get; internal set; }
}
```

Delete returns `PNResult<PNDataSyncDeleteChannelResult>`:

```c#
public class PNDataSyncDeleteChannelResult
{
}
```

List responses (`GetChannels`) return `PNResult<PNDataSyncChannelsListResult>`:

```c#
public class PNDataSyncChannelsListResult
{
    public List<PNDataSyncChannelResult> Data { get; internal set; } = new();
    public PaginationMeta Meta { get; internal set; }
    public PaginationLinks Links { get; internal set; }
}
```

`PaginationMeta` and `PaginationLinks` are the same classes defined in the Entity API.

# CreateChannel

## Create Channel

### Request Parameters — `CreateChannelParameters`

```c#
public class CreateChannelParameters
{
    /// <summary>
    /// Channel identifier. Optional — if not provided, the server generates one.
    /// Must be 1–255 characters if provided.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Schema version of the entity class. Required. Must be >= 1.
    /// </summary>
    public int EntityClassVersion { get; set; }

    /// <summary>
    /// Channel status (e.g., "active", "archived"). 1–100 characters.
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
POST /v4/subkeys/{subscribeKey}/channels
Content-Type: application/vnd.pubnub.objects.channel+json;version=1
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
{
  "data": {
    "id": "channel-general",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "name": "General Chat",
      "description": "A channel for general discussions",
      "type": "group",
      "maxMembers": 500
    }
  }
}
```

Example response (`201 Created`):

```json
{
  "status": 201,
  "data": {
    "id": "channel-general",
    "entityClass": "channel",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "name": "General Chat",
      "description": "A channel for general discussions",
      "type": "group",
      "maxMembers": 500
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T12:00:00.000Z",
    "eTag": "KaLm1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.CreateChannel(new CreateChannelParameters
{
    Id = "channel-general",
    EntityClassVersion = 1,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "name", "General Chat" },
        { "description", "A channel for general discussions" },
        { "type", "group" },
        { "maxMembers", 500 }
    },
    IdempotencyKey = "d4e5f6a7-b8c9-0123-def0-123456789abc"
});
var channel = result.Result;
Console.WriteLine(channel.Id);     // "channel-general"
Console.WriteLine(channel.Status); // "active"
```

# GetChannel

## Get Channel by ID

### Request Parameters — `GetChannelParameters`

```c#
public class GetChannelParameters
{
    /// <summary>
    /// Channel identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
GET /v4/subkeys/{subscribeKey}/channels/{channelId}
```

No request body.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "channel-general",
    "entityClass": "channel",
    "entityClassVersion": 1,
    "status": "active",
    "payload": {
      "name": "General Chat",
      "description": "A channel for general discussions",
      "type": "group",
      "maxMembers": 500
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T12:00:00.000Z",
    "eTag": "KaLm1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetChannel(new GetChannelParameters
{
    Id = "channel-general"
});
var channel = result.Result;
Console.WriteLine(channel.Payload["name"]); // "General Chat"
```

# GetChannels

## Get Channels (List)

### Request Parameters — `GetChannelsParameters`

```c#
public class GetChannelsParameters
{
    /// <summary>
    /// Schema version of the entity class. Optional — if not provided the server
    /// returns channels matching the latest version.
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
GET /v4/subkeys/{subscribeKey}/channels?entity_class_version={entityClassVersion}&cursor={cursor}&limit={limit}&filter={filter}&filter_advanced={filterAdvanced}&sort={sort}
```

No request body. All filtering/pagination is via query parameters. Unlike the generic entity list, no `entity_class` parameter is required — it is implicitly `"channel"`.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": [
    {
      "id": "channel-general",
      "entityClass": "channel",
      "entityClassVersion": 1,
      "status": "active",
      "payload": {
        "name": "General Chat",
        "type": "group"
      },
      "createdAt": "2026-05-10T12:00:00.000Z",
      "updatedAt": "2026-05-10T12:00:00.000Z",
      "eTag": "KaLm1...",
      "expiresAt": null
    },
    {
      "id": "channel-announcements",
      "entityClass": "channel",
      "entityClassVersion": 1,
      "status": "active",
      "payload": {
        "name": "Announcements",
        "type": "broadcast"
      },
      "createdAt": "2026-05-09T08:00:00.000Z",
      "updatedAt": "2026-05-10T09:30:00.000Z",
      "eTag": "LbMn2...",
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
    "self": "/subkeys/{subscribeKey}/channels?limit=10&sort=-createdAt",
    "next": "/subkeys/{subscribeKey}/channels?cursor=TjIw&limit=10&sort=-createdAt",
    "prev": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetChannels(new GetChannelsParameters
{
    EntityClassVersion = 1,
    Limit = 10,
    Filter = "status == 'active'",
    Sort = "-createdAt"
});

foreach (var channel in result.Result)
{
    Console.WriteLine($"{channel.Id}: {channel.Payload["name"]}");
}

// Pagination
if (result.Meta?.HasNext == true)
{
    var nextPage = await pubnub.DataSync.GetChannels(new GetChannelsParameters
    {
        Cursor = result.Meta.NextCursor
    });
}
```

# UpdateChannel

## Update Channel (Full Replacement)

### Request Parameters — `UpdateChannelParameters`

```c#
public class UpdateChannelParameters
{
    /// <summary>
    /// Channel identifier. Required.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Schema version of the entity class. Required. Must be >= 1.
    /// </summary>
    public int EntityClassVersion { get; set; }

    /// <summary>
    /// Channel status (e.g., "active", "archived"). 1–100 characters.
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
PUT /v4/subkeys/{subscribeKey}/channels/{channelId}
Content-Type: application/vnd.pubnub.objects.channel+json;version=1
If-Match: <optional eTag>
```

Request body:

```json
{
  "data": {
    "entityClassVersion": 2,
    "status": "active",
    "payload": {
      "name": "General Chat (v2)",
      "description": "Updated general discussion channel",
      "type": "group",
      "maxMembers": 1000,
      "pinned": "Welcome to the updated channel!"
    }
  }
}
```

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "channel-general",
    "entityClass": "channel",
    "entityClassVersion": 2,
    "status": "active",
    "payload": {
      "name": "General Chat (v2)",
      "description": "Updated general discussion channel",
      "type": "group",
      "maxMembers": 1000,
      "pinned": "Welcome to the updated channel!"
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T14:30:00.000Z",
    "eTag": "McNo3...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.UpdateChannel(new UpdateChannelParameters
{
    Id = "channel-general",
    EntityClassVersion = 2,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "name", "General Chat (v2)" },
        { "description", "Updated general discussion channel" },
        { "type", "group" },
        { "maxMembers", 1000 },
        { "pinned", "Welcome to the updated channel!" }
    },
    IfMatch = "KaLm1..."
});
var channel = result.Result;
Console.WriteLine(channel.ETag); // "McNo3..."
```

# PatchChannel

## Patch Channel (JSON Patch — RFC 6902\)

### Request Parameters — `PatchChannelParameters`

```c#
public class PatchChannelParameters
{
    /// <summary>
    /// Channel identifier. Required.
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
```

`JsonPatchOperation` is the same class defined in the Entity API (see `PatchEntityParameters.cs`).

### Request Object Structure (Wire Format)

```
PATCH /v4/subkeys/{subscribeKey}/channels/{channelId}
Content-Type: application/json-patch+json
If-Match: <optional eTag>
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
[
  { "op": "replace", "path": "/status", "value": "archived" },
  { "op": "add",     "path": "/payload/archivedBy", "value": "admin-1" },
  { "op": "remove",  "path": "/payload/pinned" }
]
```

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "channel-general",
    "entityClass": "channel",
    "entityClassVersion": 2,
    "status": "archived",
    "payload": {
      "name": "General Chat (v2)",
      "description": "Updated general discussion channel",
      "type": "group",
      "maxMembers": 1000,
      "archivedBy": "admin-1"
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T16:00:00.000Z",
    "eTag": "NdOp4...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.PatchChannel(new PatchChannelParameters
{
    Id = "channel-general",
    Operations = new List<JsonPatchOperation>
    {
        new JsonPatchOperation { Op = "replace", Path = "/status", Value = "archived" },
        new JsonPatchOperation { Op = "add", Path = "/payload/archivedBy", Value = "admin-1" },
        new JsonPatchOperation { Op = "remove", Path = "/payload/pinned" }
    },
    IfMatch = "McNo3...",
    IdempotencyKey = "e5f6a7b8-c9d0-1234-ef01-23456789abcd"
});
var channel = result.Result;
Console.WriteLine(channel.Status);                  // "archived"
Console.WriteLine(channel.Payload["archivedBy"]);   // "admin-1"
```

# DeleteChannel

## Delete Channel

### Request Parameters — `DeleteChannelParameters`

```c#
public class DeleteChannelParameters
{
    /// <summary>
    /// Channel identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
DELETE /v4/subkeys/{subscribeKey}/channels/{channelId}
```

No request body. No `If-Match` header support on this endpoint.

Example response (`200 OK`):

The response body is empty on success.

### User-facing API call example

```c#
await pubnub.DataSync.DeleteChannel(new DeleteChannelParameters
{
    Id = "channel-general"
});
```

# Membership

## Membership API Response Model

**Domain mapping**: Memberships are a specialised form of Relationships. On the wire, channelId maps to entityAId and userId maps to entityBId. relationshipClass is implicitly "membership" and is not sent by the client.

All single-membership responses (`CreateMembership`, `GetMembership`, `UpdateMembership`, `PatchMembership`) return `PNResult<PNDataSyncMembershipResult>`.

```c#
public class PNDataSyncMembershipResult
{
    public string Id { get; internal set; }
    public string ChannelId { get; internal set; }
    public string UserId { get; internal set; }
    public int RelationshipClassVersion { get; internal set; }
    public string Status { get; internal set; }
    public Dictionary<string, object> Payload { get; internal set; }
    public string CreatedAt { get; internal set; }
    public string UpdatedAt { get; internal set; }
    public string ETag { get; internal set; }
    public string ExpiresAt { get; internal set; }
}
```

Delete returns `PNResult<PNDataSyncDeleteMembershipResult>`:

```c#
public class PNDataSyncDeleteMembershipResult
{
}
```

List responses (`GetMemberships`) return `PNResult<PNDataSyncMembershipsListResult>`:

```c#
public class PNDataSyncMembershipsListResult
{
    public List<PNDataSyncMembershipResult> Data { get; internal set; } = new();
    public PaginationMeta Meta { get; internal set; }
    public PaginationLinks Links { get; internal set; }
}
```

`PaginationMeta` and `PaginationLinks` are the same classes defined in the Entity API.

# CreateMembership

## Create Membership

### Request Parameters — `CreateMembershipParameters`

```c#
public class CreateMembershipParameters
{
    /// <summary>
    /// Membership identifier. Optional — if not provided, the server generates one.
    /// Must be 1–255 characters if provided.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Channel ID. Required. Immutable after creation.
    /// Maps to entityAId on the wire.
    /// </summary>
    public string ChannelId { get; set; }

    /// <summary>
    /// User ID. Required. Immutable after creation.
    /// Maps to entityBId on the wire.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Version of the membership relationship class. Required. Must be >= 1.
    /// </summary>
    public int RelationshipClassVersion { get; set; }

    /// <summary>
    /// Membership status (e.g., "active", "banned"). 1–100 characters.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// User-defined custom properties. Supports arbitrarily nested objects.
    /// </summary>
    public Dictionary<string, object>? Payload { get; set; }

    /// <summary>
    /// Idempotency key (UUIDv4) to ensure the request is processed exactly once.
    /// Required for POST requests.
    /// </summary>
    public string IdempotencyKey { get; set; }
}
```

### Request Object Structure (Wire Format)

The SDK constructs the following HTTP request:

```
POST /v4/subkeys/{subscribeKey}/memberships
Content-Type: application/vnd.pubnub.objects.membership+json;version=1
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
{
  "data": {
    "id": "mem-001",
    "channelId": "channel-general",
    "userId": "user-alice",
    "relationshipClassVersion": 1,
    "status": "active",
    "payload": {
      "role": "moderator",
      "joinedVia": "invite"
    }
  }
}
```

Example response (`201 Created`):

```json
{
  "status": 201,
  "data": {
    "id": "mem-001",
    "entityAId": "channel-general",
    "entityBId": "user-alice",
    "relationshipClass": "membership",
    "relationshipClassVersion": 1,
    "status": "active",
    "payload": {
      "role": "moderator",
      "joinedVia": "invite"
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T12:00:00.000Z",
    "eTag": "EmNp1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.CreateMembership(new CreateMembershipParameters
{
    Id = "mem-001",
    ChannelId = "channel-general",
    UserId = "user-alice",
    RelationshipClassVersion = 1,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "role", "moderator" },
        { "joinedVia", "invite" }
    },
    IdempotencyKey = "b2c3d4e5-f6a7-8901-bcde-f12345678901"
});
var membership = result.Result;
Console.WriteLine(membership.Id);                // "mem-001"
Console.WriteLine(membership.ChannelId);         // "channel-general"
Console.WriteLine(membership.UserId);         // "user-alice"
Console.WriteLine(membership.RelationshipClass); // "membership"
```

# GetMembership

## Get Membership by ID

### Request Parameters — `GetMembershipParameters`

```c#
public class GetMembershipParameters
{
    /// <summary>
    /// Membership identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
GET /v4/subkeys/{subscribeKey}/memberships/{membershipId}
```

No request body.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "mem-001",
    "entityAId": "channel-general",
    "entityBId": "user-alice",
    "relationshipClass": "membership",
    "relationshipClassVersion": 1,
    "status": "active",
    "payload": {
      "role": "moderator",
      "joinedVia": "invite"
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T12:00:00.000Z",
    "eTag": "EmNp1...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetMembership(new GetMembershipParameters
{
    Id = "mem-001"
});
var membership = result.Result;
Console.WriteLine(membership.ChannelId);          // "channel-general"
Console.WriteLine(membership.Payload["role"]);    // "moderator"
```

# GetMemberships

## Get Memberships (List)

### Request Parameters — `GetMembershipsParameters`

```c#
public class GetMembershipsParameters
{
    /// <summary>
    /// Filter memberships by user ID. Optional.
    /// At least one of UserId or ChannelId should be provided for meaningful results.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Filter memberships by channel ID. Optional.
    /// At least one of UserId or ChannelId should be provided for meaningful results.
    /// </summary>
    public string? ChannelId { get; set; }

    /// <summary>
    /// Version of the relationship class. Optional — if not provided the server
    /// returns memberships matching the latest version.
    /// </summary>
    public int? RelationshipClassVersion { get; set; }

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
GET /v4/subkeys/{subscribeKey}/memberships?user_id={userId}&channel_id={channelId}&relationship_class_version={relationshipClassVersion}&cursor={cursor}&limit={limit}&filter={filter}&filter_advanced={filterAdvanced}&sort={sort}
```

No request body. All filtering/pagination is via query parameters. Unlike the generic relationship list, no `relationship_class` parameter is required — it is implicitly `"membership"`. The domain-friendly query parameters `user_id` and `channel_id` replace `entity_a_id` / `entity_b_id`.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": [
    {
      "id": "mem-001",
      "entityAId": "channel-general",
      "entityBId": "user-alice",
      "relationshipClass": "membership",
      "relationshipClassVersion": 1,
      "status": "active",
      "payload": {
        "role": "moderator",
        "joinedVia": "invite"
      },
      "createdAt": "2026-05-10T12:00:00.000Z",
      "updatedAt": "2026-05-10T12:00:00.000Z",
      "eTag": "EmNp1...",
      "expiresAt": null
    },
    {
      "id": "mem-002",
      "entityAId": "channel-random",
      "entityBId": "user-alice",
      "relationshipClass": "membership",
      "relationshipClassVersion": 1,
      "status": "active",
      "payload": {
        "role": "member",
        "joinedVia": "self"
      },
      "createdAt": "2026-05-09T14:30:00.000Z",
      "updatedAt": "2026-05-10T09:15:00.000Z",
      "eTag": "FoQr2...",
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
    "self": "/subkeys/{subscribeKey}/memberships?user_id=user-alice&limit=10&sort=-createdAt",
    "next": "/subkeys/{subscribeKey}/memberships?user_id=user-alice&cursor=TjIw&limit=10&sort=-createdAt",
    "prev": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
{
    UserId = "user-alice",
    Limit = 10,
    Filter = "status == 'active'",
    Sort = "-createdAt"
});

foreach (var mem in result.Result)
{
    Console.WriteLine($"{mem.Id}: {mem.EntityAId} <-> {mem.EntityBId}");
}

// Pagination
if (result.Meta?.HasNext == true)
{
    var nextPage = await pubnub.DataSync.GetMemberships(new GetMembershipsParameters
    {
        UserId = "user-alice",
        Cursor = result.Meta.NextCursor
    });
}
```

# UpdateMembership

## Update Membership (Full Replacement)

### Request Parameters — `UpdateMembershipParameters`

```c#
public class UpdateMembershipParameters
{
    /// <summary>
    /// Membership identifier. Required.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Version of the membership relationship class. Required. Must be >= 1.
    /// Note: channelId, userId, and relationshipClass are immutable
    /// and cannot be changed after creation.
    /// </summary>
    public int RelationshipClassVersion { get; set; }

    /// <summary>
    /// Membership status (e.g., "active", "banned"). 1–100 characters.
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
PUT /v4/subkeys/{subscribeKey}/memberships/{membershipId}
Content-Type: application/vnd.pubnub.objects.membership+json;version=1
If-Match: <optional eTag>
```

Request body:

```json
{
  "data": {
    "relationshipClassVersion": 2,
    "status": "active",
    "payload": {
      "role": "admin",
      "joinedVia": "invite",
      "permissions": ["read", "write", "manage"]
    }
  }
}
```

Note: `channelId`, `userId`, and `relationshipClass` are **not** included in the update body because they are immutable.

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "mem-001",
    "entityAId": "channel-general",
    "entityBId": "user-alice",
    "relationshipClass": "membership",
    "relationshipClassVersion": 2,
    "status": "active",
    "payload": {
      "role": "admin",
      "joinedVia": "invite",
      "permissions": ["read", "write", "manage"]
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T14:30:00.000Z",
    "eTag": "GtUv3...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.UpdateMembership(new UpdateMembershipParameters
{
    Id = "mem-001",
    RelationshipClassVersion = 2,
    Status = "active",
    Payload = new Dictionary<string, object>
    {
        { "role", "admin" },
        { "joinedVia", "invite" },
        { "permissions", new List<string> { "read", "write", "manage" } }
    },
    IfMatch = "EmNp1..."
});
var membership = result.Result;
Console.WriteLine(membership.ETag); // "GtUv3..."
```

# PatchMembership

## Patch Membership (JSON Patch — RFC 6902\)

### Request Parameters — `PatchMembershipParameters`

```c#
public class PatchMembershipParameters
{
    /// <summary>
    /// Membership identifier. Required.
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
```

`JsonPatchOperation` is the same class defined in the Entity API (see `PatchEntityParameters.cs`).

### Request Object Structure (Wire Format)

```
PATCH /v4/subkeys/{subscribeKey}/memberships/{membershipId}
Content-Type: application/json-patch+json
If-Match: <optional eTag>
Idempotency-Key: <UUIDv4, required>
```

Request body:

```json
[
  { "op": "replace", "path": "/status", "value": "banned" },
  { "op": "add",     "path": "/payload/bannedReason", "value": "spam" },
  { "op": "remove",  "path": "/payload/permissions" }
]
```

Example response (`200 OK`):

```json
{
  "status": 200,
  "data": {
    "id": "mem-001",
    "entityAId": "channel-general",
    "entityBId": "user-alice",
    "relationshipClass": "membership",
    "relationshipClassVersion": 2,
    "status": "banned",
    "payload": {
      "role": "admin",
      "joinedVia": "invite",
      "bannedReason": "spam"
    },
    "createdAt": "2026-05-10T12:00:00.000Z",
    "updatedAt": "2026-05-10T16:45:00.000Z",
    "eTag": "HwXy4...",
    "expiresAt": null
  }
}
```

### User-facing API call example

```c#
var result = await pubnub.DataSync.PatchMembership(new PatchMembershipParameters
{
    Id = "mem-001",
    Operations = new List<JsonPatchOperation>
    {
        new JsonPatchOperation { Op = "replace", Path = "/status", Value = "banned" },
        new JsonPatchOperation { Op = "add", Path = "/payload/bannedReason", Value = "spam" },
        new JsonPatchOperation { Op = "remove", Path = "/payload/permissions" }
    },
    IfMatch = "GtUv3...",
    IdempotencyKey = "c3d4e5f6-a7b8-9012-cdef-123456789012"
});
var membership = result.Result;
Console.WriteLine(membership.Status);                  // "banned"
Console.WriteLine(membership.Payload["bannedReason"]); // "spam"
```

# DeleteMembership

## Delete Membership

### Request Parameters — `DeleteMembershipParameters`

```c#
public class DeleteMembershipParameters
{
    /// <summary>
    /// Membership identifier. Required.
    /// </summary>
    public string Id { get; set; }
}
```

### Request Object Structure (Wire Format)

```
DELETE /v4/subkeys/{subscribeKey}/memberships/{membershipId}
```

No request body. No `If-Match` header support on this endpoint.

Example response (`200 OK`):

The response body is empty on success.

### User-facing API call example

```c#
await pubnub.DataSync.DeleteMembership(new DeleteMembershipParameters
{
    Id = "mem-001"
});
```

