---
name: port-cross-sdk-feature
description: Ports a feature that already exists in another PubNub SDK (JavaScript, Kotlin, Swift, PHP, Python, Go...) or in a cross-SDK design document into the PubNub C# SDK, keeping wire formats and semantics identical across .NET, Unity and UWP builds. Use when the user points to another SDK's implementation, a PubNub spec or design doc, or shared cross-SDK test vectors.
---

# Port a cross-SDK feature

PubNub SDKs must interoperate: data one SDK writes, the others read. Parity with the reference implementation matters more than idiomatic novelty.

## Workflow

1. **Collect the sources of truth:** the design doc or spec, the reference SDK's implementation and tests, and any shared test vectors. Ask the user for paths if they aren't given. Note anything the spec leaves open.
2. **Extract the contract** as a short list:
   - the wire format: paths, parameters, headers, bodies, byte layouts
   - defaults and validation rules
   - error cases
   - which behaviors are intentionally SDK-specific
3. **Map it onto the C# structure.** Decide which existing component the feature extends:
   - `Security/Crypto/` and `CryptoModule`
   - an operation in `EndPoint/` (see the `add-endpoint` skill)
   - `EventEngine/`
   - `PNConfiguration`

   Then decide how it gets registered: `Pubnub.cs` factory methods, `CryptoModule.CreateFromCipherKey()`, or the `DataSync` facade.
4. **Adapt to .NET and this SDK, not the reference language.**
   - Return `PNStatus` error results from `ExecuteAsync()`, and use `PNException` with the existing generic messages.
   - Every `await` ends with `.ConfigureAwait(false)`.
   - Use only APIs available in `netstandard2.0`. When a primitive is missing there (as `AesGcm` is), use the existing BouncyCastle dependency rather than adding a new one.
   - Respect Unity's constraints: no `dynamic`, no runtime code generation, no new dependencies unless they are merged by ILRepack.
   - Link every new file into the PCL, UWP and Unity projects.
5. **Check backward compatibility in both directions:** existing C# users upgrading, and older SDKs reading data this SDK produces. Staged rollouts (decrypt first, encrypt later) must leave defaults unchanged.
6. **Write tests** (NUnit, C# 10):
   - Known-answer tests from the shared vectors, as a static array fed to `[TestCaseSource]` (see `src/UnitTests/PubnubApi.Tests/AesGcmCryptorVectorTests.cs`).
     - Write non-ASCII characters as `\u....` or `\U0001F30D` escapes, because shared files are often mangled in transit (emoji turned into `:shortcodes:`).
     - Assert derived values such as hashes and hex so that a wrong literal fails loudly.
   - Behavioral tests mirroring the reference SDK's test cases.
   - Negative tests: tampering, malformed input, wrong keys, superseded formats.
   - Link the new test files into `PubnubApiPCL.Tests.csproj`.
7. **Verify** with the `run-checks` skill. Then summarize the differences from the reference SDK and why each one exists.
