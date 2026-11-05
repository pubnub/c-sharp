# AGENTS.md

Guidance for AI coding agents working on the PubNub C# SDK, and the single source of truth for guidance shared by all agents. Cursor and other agents that read `AGENTS.md` (such as Codex) load it directly, and `CLAUDE.md` imports it for Claude Code. Area-specific rules live in `.cursor/rules/` and reusable workflows in `.claude/skills/` (see [Where to find more](#where-to-find-more)).

## What this repository is

- **PubNub** is a hosted real-time messaging platform. Clients publish JSON messages to named channels, and other clients receive them through long-lived HTTP subscribe requests. Add-on services cover presence, message history, access control, user and channel metadata, files, mobile push and typed records (Data Sync).
- **This repository** builds the PubNub SDK for .NET. One shared source tree, `src/Api/PubnubApi`, is compiled into four assemblies:

  | Project | Assembly | Targets | Shipped as |
  | --- | --- | --- | --- |
  | `src/Api/PubnubApi` | `Pubnub.dll` | `net6.0` | NuGet package `Pubnub` |
  | `src/Api/PubnubApiPCL` | `PubnubPCL.dll` | `netstandard2.0`, `net6.0` | NuGet package `PubnubPCL`, the one the README tells customers to install |
  | `src/Api/PubnubApiUWP` | `PubnubUWP.dll` | `uap10.0` | NuGet package `PubnubUWP` (built with MSBuild on Windows) |
  | `src/Api/PubnubApiUnity` | `PubnubApiUnity.dll` | `netstandard2.0`, `net6.0` | **Not a NuGet package.** A single DLL with BouncyCastle merged in by ILRepack, imported into the separate PubNub Unity SDK project |

- **The API is asynchronous.** Every operation has `ExecuteAsync()`, returning `Task<PNResult<T>>` (result plus `PNStatus`), and a callback-based `Execute(PNCallback<T>)`. Subscribe runs in the background and delivers events to listeners.
- **It is one of many PubNub SDKs** (JavaScript, Swift, Kotlin, PHP, Python, Go and others) that call the same REST API. Data written by one SDK is read by the others, so wire formats, crypto headers, request signatures and error semantics must stay compatible. Before adding a feature that exists elsewhere, check how the other SDKs and the public docs behave.
- Public docs: <https://www.pubnub.com/docs/sdks/c-sharp>. LLM-oriented doc index: <https://www.pubnub.com/llms-full.txt>.

## PubNub concepts

| Term | Meaning |
| --- | --- |
| Keyset | Publish key, subscribe key and secret key from the PubNub Admin Portal. The secret key is for servers only. |
| User ID | Required client identity: `new PNConfiguration(new UserId("..."))`. Older APIs and the wire format call it UUID (`uuid` query parameter). |
| Channel, channel group | A named message stream; a server-side named set of channels that can be subscribed to as one. |
| Timetoken | 17-digit timestamp in 100-nanosecond units since the Unix epoch. Orders messages and serves as the subscribe and history cursor. |
| Publish, Signal, Fire | A regular (optionally stored) message; a small, never-stored message; a message delivered only to PubNub Functions, not to subscribers. |
| Presence | Who is subscribed to a channel (`HereNow`, `WhereNow`) and per-user state. The SDK sends heartbeats once a presence timeout is configured. |
| Message Persistence | Stored history: `FetchHistory()`, legacy `History()`, `MessageCounts()`, `DeleteMessages()`. |
| Access Manager (PAM) | A server holding the secret key issues tokens with `GrantToken()`; clients send them after `SetAuthToken()`. v2 `Grant()`/`Audit()` with auth keys is legacy. |
| App Context | Formerly "Objects": user (UUID) and channel metadata plus memberships and channel members, under `/v2/objects`. |
| Message Actions | Reactions and receipts attached to a published message's timetoken. |
| Files | Upload a file to storage, then publish a file message; list, download and delete by file ID. |
| Mobile Push | Register device tokens (APNs2, FCM) to receive push notifications for channels. |
| Data Sync | Newer typed-record service (entities, users, channels, memberships, relationships) under `/v1/datasync`, reached through `pubnub.DataSync`. |
| Crypto module | Optional message and file encryption. Its binary header format is shared by all SDKs. |

## Repository map

```text
src/
  Pubnub.sln                       All projects: Api, UnitTests, Snippets
  Api/PubnubApi/                   The shared source tree; the other library projects link these files
    Pubnub.cs                      Client: one factory method per API call, listeners, Destroy()
    PNConfiguration.cs             Keys, user ID, timeouts, retries, crypto, logging
    PubnubCoreBase.cs              Base class of the operations: request state, response parsing, shared state
    EndPoint/                      One operation class per REST call, grouped by feature; DataSync/ has the Data Sync facade
    EventEngine/                   Subscribe and presence state machines (the default subscribe implementation)
    Transport/, TransportContract/ Middleware (URL building, auth, signing) and HttpClientService
    JsonDataParse/                 Result parsers, registered in DeserializeToInternalObjectUtility.cs
    Model/                         Consumer/ results, Derived/ callback wrappers (*Ext), Server/ wire models
    Security/Crypto/               CryptoModule and the cryptors
    Log/                           IPubnubLogger, PubnubLogModule, PubnubDefaultLogger
    Entity/                        Channel, ChannelGroup, Subscription, SubscriptionSet entities
    Properties/AssemblyInfo.cs     Assembly version, bumped by the release bot
  Api/PubnubApiPCL/, PubnubApiUWP/, PubnubApiUnity/   Projects that link ..\PubnubApi\**\*.cs file by file
  UnitTests/
    PubnubApi.Tests/               NUnit tests: mostly live, plus offline event-engine and crypto tests
    PubnubApiPCL.Tests/            Links the PubnubApi.Tests files against PubnubPCL; the only unit test project CI runs
    AcceptanceTests/               SpecFlow contract tests against PubNub's mock server (CI only)
    MockServerPubnubApi*/          Legacy local mock server for the disabled stub mode
  PubnubApi.Snippets/              Documentation snippets (`// snippet.<name>` ... `// snippet.end`), built in CI
  Examples/                        Old sample apps; not in Pubnub.sln and not built in CI
.github/workflows/                 CI on Windows runners, release automation
```

| Feature | Client methods | Code (under `src/Api/PubnubApi/`) |
| --- | --- | --- |
| Publish, signals | `Publish()`, `Signal()`, `Fire()` | `EndPoint/PubSub/` |
| Subscribe | `Subscribe<T>()`, `Unsubscribe<T>()`, `UnsubscribeAll<T>()`, `AddListener()`, `Channel(name)`, `SubscriptionSet(...)` | `EndPoint/PubSub/`, `EventEngine/Subscribe/`, `Entity/` |
| Presence | `HereNow()`, `WhereNow()`, `GetPresenceState()`, `SetPresenceState()` | `EndPoint/Presence/`, `EventEngine/Presence/` |
| Message Persistence | `FetchHistory()`, `History()`, `MessageCounts()`, `DeleteMessages()` | `EndPoint/StoragePlayback/` |
| Channel groups | `AddChannelsToChannelGroup()`, `ListChannelsForChannelGroup()`... | `EndPoint/ChannelGroup/` |
| Mobile Push | `AddPushNotificationsOnChannels()`, `AuditPushChannelProvisions()`... | `EndPoint/Push/` |
| Access Manager | `GrantToken()`, `RevokeToken()`, `ParseToken()`, `SetAuthToken()`, legacy `Grant()`, `Audit()` | `EndPoint/Access/`, `EndPoint/TokenManager.cs`, `CBOR.cs` |
| App Context | `SetUuidMetadata()`, `GetChannelMetadata()`, `SetMemberships()`, `ManageChannelMembers()`... | `EndPoint/Objects/` |
| Message Actions | `AddMessageAction()`, `GetMessageActions()`, `RemoveMessageAction()` | `EndPoint/PubSub/*MessageAction*` |
| Files | `SendFile()`, `ListFiles()`, `DownloadFile()`, `GetFileUrl()`, `DeleteFile()`, `PublishFileMessage()` | `EndPoint/Files/` |
| Data Sync | `pubnub.DataSync.CreateEntity(...)`, `.GetUsers(...)`... | `EndPoint/DataSync/` |
| Time | `Time()` | `EndPoint/TimeOperation.cs` |
| Encryption | `PNConfiguration.CryptoModule`, `pubnub.Encrypt()`/`Decrypt()` | `Security/Crypto/` |

## How a request works

```csharp
PNResult<PNPublishResult> response = await pubnub.Publish()
    .Channel("my_channel")
    .Message(new { text = "hi" })
    .ExecuteAsync()
    .ConfigureAwait(false);
if (response.Status.Error) { /* response.Status.ErrorData.Information */ }
```

1. The factory method returns a new operation object (`PublishOperation`). Its fluent setters return the same object.
2. `ExecuteAsync()` validates input and builds a `RequestParameter` (path segments, query, body, headers) in `CreateRequestParameter()`.
3. `transportMiddleware.PreapareTransportRequest()` (sic) does several things:
   - adds `uuid`, `pnsdk` and `auth` (a token from `SetAuthToken()` wins over `AuthKey`)
   - signs the request with HMAC-SHA256 when `SecretKey` is set
   - URL-encodes the path segments and applies the timeout

   Query values must already be encoded with `UriUtil.EncodeUriComponent()`.
4. `transportMiddleware.Send()` calls the `IHttpClientService`. The default is `HttpClientService`, built on `HttpClient` with HTTP/2 on `net6.0`; a replacement can be passed to the `Pubnub` constructor. Transport failures come back in `TransportResponse.Error` and are never thrown.
5. `GetStatusIfError()` detects error payloads. `ProcessJsonResponse()` and `ResponseBuilder.JsonToObject<T>()` then hand the response to the parser registered for `T` in `JsonDataParse/DeserializeToInternalObjectUtility.cs`.
6. The result is a `PNResult<T>` holding `Result` and `Status` (`PNStatus`: `Error`, `ErrorData`, `StatusCode`, `Category`, `Operation`). `Execute(callback)` delivers the same pair to `PNCallback<T>.OnResponse`.

**Logging:** set `config.LogLevel = PubnubLogLevel.Debug` and add a logger with `pubnub.SetLogger(IPubnubLogger)`.

**JSON:** Newtonsoft.Json (`NewtonsoftJsonDotNet`). It can be replaced with `SetJsonPluggableLibrary()`, but the internals depend on Newtonsoft container types.

## Commands

Run from the repository root. Building needs the .NET 8 SDK or newer, because the library uses C# 12 syntax. `src/Api/PubnubApi/global.json` (`"7.0.x"`) is not a valid version pattern, so `dotnet` ignores it.

| Purpose | Command |
| --- | --- |
| Build the main library | `dotnet build src/Api/PubnubApi/PubnubApi.csproj` |
| Build PubnubPCL (`netstandard2.0` and `net6.0`) | `dotnet build src/Api/PubnubApiPCL/PubnubApiPCL.csproj` |
| Build the Unity DLL (runs ILRepack) | `dotnet build src/Api/PubnubApiUnity/PubnubApiUnity.csproj -c Release` |
| Build the snippets, as CI does | `dotnet build src/PubnubApi.Snippets/PubnubApi.Snippets.csproj` |
| One test fixture or test | `dotnet test src/UnitTests/PubnubApi.Tests/PubnubApi.Tests.csproj --filter "FullyQualifiedName~EncryptionTests"` |
| Unit tests as CI runs them (live keys) | `dotnet test src/UnitTests/PubnubApiPCL.Tests/PubnubApiPCL.Tests.csproj` |

Offline tests (crypto vectors, event-engine transitions and retry configuration; about 200 tests, no keys needed):

```bash
dotnet test src/UnitTests/PubnubApi.Tests/PubnubApi.Tests.csproj --filter "FullyQualifiedName~AesGcmCryptorVectorTests|FullyQualifiedName~StateTransition|FullyQualifiedName~RetryConfigurationTest"
```

- **UWP** builds only on Windows with MSBuild and the UWP SDK (10.0.16299 or later), as `.github/workflows/release/build-packages.ps1` does: `dotnet restore /p:BuildWithNetFrameworkHostedCompiler=true`, then `msbuild PubnubApiUWP.csproj`.
- **Live tests** read keys from environment variables:
  - `PN_PUB_KEY`, `PN_SUB_KEY`, `PN_SEC_KEY`
  - `PN_PAM_PUB_KEY`, `PN_PAM_SUB_KEY`, `PN_PAM_SEC_KEY`, which fall back to the `demo-36` keyset
  - `PN_DS_*` (Data Sync) and `PN_CAT_*` (grant categories)

  They publish to real keysets, so don't run live tests unless asked. Without keys they fail with network or access errors; that is not a regression from your change.
- **The `run-checks` skill** has the full verification workflow, including two scripts: `Test-LinkedSources.ps1` checks the hand-maintained project file lists, and `Test-ConfigureAwait.ps1` checks the await rule.

## Working rules

- **Every `await` in library code ends with `.ConfigureAwait(false)`.**
  - This includes `await Task.Delay(...)` and awaits on the SDK's own async methods.
  - The SDK runs inside applications that have a synchronization context: Unity's main thread, UWP and WinForms UI threads, classic ASP.NET. Resuming on that context can deadlock callers that block on SDK tasks.
  - Check with `Test-ConfigureAwait.ps1`; the whole library currently passes (`-All`).
- **Don't add blocking or fire-and-forget async code.** No `.Result`, `.Wait()` or `GetAwaiter().GetResult()` on SDK tasks, and no new `async void` methods; an exception thrown from one crashes the host process.
- **Link every new, renamed or deleted source file:**
  - For a file under `src/Api/PubnubApi`, add `<Compile Include="..\PubnubApi\Dir\File.cs" Link="Dir\File.cs" />` to `PubnubApiPCL.csproj`, `PubnubApiUWP.csproj` and `PubnubApiUnity.csproj`.
  - For a test file under `PubnubApi.Tests`, add the equivalent link to `PubnubApiPCL.Tests.csproj`, since CI runs only that project.
  - Then run `Test-LinkedSources.ps1`. CI never builds the Unity project, so a missing Unity link only shows up in the Unity SDK.
- **Library code must compile for `netstandard2.0`** (PCL and Unity) as well as `net6.0` and `uap10.0`. Don't use APIs missing from .NET Standard 2.0, such as `Convert.FromHexString`, `HttpVersionPolicy` or `System.Text.Json`, without an `#if NET6_0_OR_GREATER || NET60` guard and a fallback. Build PCL and Unity before finishing.
- **Respect Unity's constraints.** The Unity SDK runs this DLL under IL2CPP (ahead-of-time compilation) on many platforms.
  - Avoid `dynamic`, `Reflection.Emit` and runtime code generation.
  - Don't add third-party dependencies. A new one must either ship with Unity, as Newtonsoft.Json does, or be merged in `src/Api/PubnubApiUnity/ILRepack.targets`.
- **Keep backward compatibility.** This is a published library, and breaking changes need a major version.
  - Mark replaced APIs `[Obsolete]` rather than removing them.
  - Keep historical misspellings such as `PreapareTransportRequest`, `PNStatus.AdditonalData` and `PNHereNowResultEx`.
  - New `SubscribeCallback` events must be `virtual` with an empty body, like `DataSyncEvent`; a new abstract member breaks customer subclasses.
  - Before changing a signature, search for overrides and call sites, including in tests and snippets.
- **Test every behavior change**, preferring offline tests. Test projects use NUnit 3 with `LangVersion 10`, so test code can't use C# 11 or 12 syntax.
- **Match the surrounding style.** There is no `.editorconfig`.
  - Follow the file you edit for tabs or spaces, and for block or file-scoped namespaces.
  - Private fields are mostly camelCase without an underscore.
  - Nullable reference types are not enabled.
  - Add XML doc comments to new public members; the Unity build generates documentation and warns (CS1591) about undocumented ones.
- **Leave versions and changelogs to the release bot:** `AssemblyInfo.cs`, `PackageVersion` and `PackageReleaseNotes` in the library `.csproj` files, `.pubnub.yml` and `CHANGELOG`. Suggest the changelog line for the PR description instead.
- **Protect secrets.** Never print or commit keys; tests read them from environment variables.
- **Don't modify** `bin/`, `obj/`, `src/packages/`, `.github/.release/`, the strong-name key `pubnub.snk`, or untracked local scratch folders.
- **Keep parity with the other SDKs:** identical wire formats, and shared cross-SDK test vectors when they exist (the `port-cross-sdk-feature` skill).

## Known quirks

These are existing behaviors. Don't copy them into new code, and only fix them when asked, since some fixes change public behavior.

- **Two subscribe implementations.** `PNConfiguration.EnableEventEngine` (true by default) selects the event engine (`SubscribeEndpoint`, `EventEngine/`); false selects the legacy `SubscribeManager`/`SubscribeOperation`. Entities (`Subscription`, `SubscriptionSet`) only work with the event engine. Subscribe changes usually need both paths.
- **Presence heartbeats are off by default.** The constructor sets the timeout field directly, so `PresenceInterval` stays 0, and the presence event engine only starts after `PresenceTimeout` or `SetPresenceTimeoutWithCustomInterval()` is set.
- **`ReconnectionPolicy = PNReconnectionPolicy.NONE` doesn't disable retries.** It leaves the default exponential `RetryConfiguration` in place; only `RetryConfiguration = null` disables retries.
- **The encryption format depends on which code path runs first.** With only a cipher key set, the crypto module is created lazily with `config.CryptoModule ??= ...` in many places. The event-engine subscribe path creates an AES-CBC-first module and the others a legacy-first one, so whichever runs first decides the format for the client's lifetime.
- **Validation is inconsistent.** Some checks throw (`ArgumentException`, `MissingMemberException`), especially in `Execute(callback)`; others return a `PNStatus` with `Error = true`. New code follows the Data Sync operations: throw only for a missing callback, and return error statuses from `ExecuteAsync()`.
- **The legacy callback path is selective.** It only calls `OnResponse` for operation types listed in `PubnubCoreBase.ResponseToUserCallback`. Data Sync sidesteps this by implementing `Execute()` as a wrapper around `ExecuteAsync()`.
- **Parsers are fragile.** App Context parsers index top-level JSON properties by position (`listObject[1]`), and a result type that isn't registered in `DeserializeToInternalObjectUtility` silently produces `null`.
- **Instances share static state.** `PubnubCoreBase` keeps static state, and some of it (the JSON library, the unit-test hook, the heartbeat timer, the dedupe manager) isn't keyed by instance, so several `Pubnub` instances in one process affect each other.
- **Cleanup is incomplete.** `Destroy()` cleans up legacy subscribe state but doesn't dispose the `HttpClient`, and `Pubnub` isn't `IDisposable`.
- **Data Sync operation type names are swapped.** `PNDataSyncUpdate*` is the PUT `Set*Operation`, and `PNDataSyncPatch*` is the PATCH `Update*Operation`.
- **The default logger prints nothing in Release builds.** It writes with `System.Diagnostics.Debug.WriteLine`, which is compiled out, so users need `SetLogger()`. `LogLevel` defaults to `None`.
- **Two test files never run in CI.** `AdditionalSubscribeTests.cs` and `WhenHistoryIsRequested.cs` aren't linked into `PubnubApiPCL.Tests`.

## Where to find more

Area rules in `.cursor/rules/` are markdown with YAML frontmatter. Cursor attaches them automatically by glob. Other agents should read the matching rule before editing:

| When editing | Rule |
| --- | --- |
| Any C# file | `.cursor/rules/csharp-conventions.mdc` |
| Library code under `src/Api/` | `.cursor/rules/async-and-threading.mdc` |
| `.csproj` files, `Pubnub.sln`, `global.json` | `.cursor/rules/projects-and-builds.mdc` |
| `src/Api/PubnubApiUnity/` | `.cursor/rules/unity-dll.mdc` |
| `EndPoint/`, `Model/`, `JsonDataParse/`, `Transport/`, `Pubnub.cs`, `PNOperationType.cs` | `.cursor/rules/endpoints.mdc` |
| `EventEngine/`, subscribe and presence operations, `Entity/`, `Callbacks/` | `.cursor/rules/subscribe-and-presence.mdc` |
| `Security/` and crypto tests | `.cursor/rules/crypto.mdc` |
| Data Sync code and tests | `.cursor/rules/data-sync.mdc` |
| `src/UnitTests/` | `.cursor/rules/tests.mdc` |
| `src/PubnubApi.Snippets/` | `.cursor/rules/snippets.mdc` |
| `.pubnub.yml`, `CHANGELOG`, `AssemblyInfo.cs`, `.github/workflows/` | `.cursor/rules/release-files.mdc` |

Skills in `.claude/skills/`, loaded by both Claude Code and Cursor:

- `add-endpoint`: adds a REST operation with its factory method, operation type, result model, parser, project links, tests and snippet.
- `run-checks`: builds the variants, runs offline tests, and runs the project-link and `ConfigureAwait(false)` checks.
- `port-cross-sdk-feature`: ports a feature from another PubNub SDK or a cross-SDK spec while keeping parity.

The PubNub MCP server (`https://mcp.pubnub.com`) is configured for this project in `.cursor/mcp.json` (Cursor) and `.mcp.json` (Claude Code). Each user enables it and signs in with their own PubNub account.
- Use it to look up PubNub documentation, REST API details and how other SDKs behave.
- It acts with the signed-in user's account permissions. Don't use its account-changing tools (creating or changing apps, keysets or Functions, publishing messages, deleting data) unless the user explicitly asks.
- Setup help: <https://www.pubnub.com/docs/ai/pubnub-mcp-server>.
