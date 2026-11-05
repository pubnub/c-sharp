---
name: run-checks
description: Runs the PubNub C# SDK verification steps (project-link and ConfigureAwait(false) checks, builds of the PubnubApi, PCL and Unity variants, offline NUnit tests and targeted tests) and reports what was and wasn't run. Use after changing C# code or project files, before committing or opening a PR, or when asked to build or test.
---

# Run checks

Run everything from the repository root. You need the .NET 8 SDK or newer. The scripts are PowerShell: run them with `powershell -ExecutionPolicy Bypass -File <script>` on Windows, or `pwsh -File <script>` on macOS and Linux.

## 1. Project links and ConfigureAwait (seconds)

```powershell
powershell -ExecutionPolicy Bypass -File .claude/skills/run-checks/scripts/Test-LinkedSources.ps1
powershell -ExecutionPolicy Bypass -File .claude/skills/run-checks/scripts/Test-ConfigureAwait.ps1
```

- **`Test-LinkedSources.ps1`** fails when a file under `src/Api/PubnubApi` isn't linked into the PCL, UWP and Unity projects, or when a link points to a missing file.
  - It warns about test files not linked into `PubnubApiPCL.Tests`. Two are known: `AdditionalSubscribeTests.cs` and `WhenHistoryIsRequested.cs`. A file you added must never be on that list.
  - `-Strict` turns the warnings into failures.
- **`Test-ConfigureAwait.ps1`** scans the library files changed against `origin/master`, plus untracked files, and fails on any `await` without `.ConfigureAwait(false)`. `-All` scans the whole library, which currently passes.

## 2. Build

Each variant compiles the shared sources differently, so build all three:

```bash
dotnet build src/Api/PubnubApi/PubnubApi.csproj
dotnet build src/Api/PubnubApiPCL/PubnubApiPCL.csproj
dotnet build src/Api/PubnubApiUnity/PubnubApiUnity.csproj -c Release
```

- **PCL** catches APIs missing from `netstandard2.0`.
- **Unity** also runs ILRepack, and CI never builds it. Its Release build prints many CS1591 warnings (missing XML comments); they are expected. A cold build can take a few minutes.
- **UWP** builds only on Windows with the UWP SDK: `dotnet restore src/Api/PubnubApiUWP/PubnubApiUWP.csproj /p:BuildWithNetFrameworkHostedCompiler=true`, then `msbuild src/Api/PubnubApiUWP/PubnubApiUWP.csproj /p:Configuration=Release`. If that isn't available, skip it and say so.
- **Snippets:** after public API changes, also run `dotnet build src/PubnubApi.Snippets/PubnubApi.Snippets.csproj`.

## 3. Offline tests (always)

```bash
dotnet test src/UnitTests/PubnubApi.Tests/PubnubApi.Tests.csproj --filter "FullyQualifiedName~AesGcmCryptorVectorTests|FullyQualifiedName~StateTransition|FullyQualifiedName~RetryConfigurationTest"
dotnet test src/UnitTests/PubnubApi.Tests/PubnubApi.Tests.csproj --filter "FullyQualifiedName~<YourNewFixture>"
```

The first run also builds `MockServerPubnubApi`. Everything must pass.

## 4. Live tests (only when the user asks)

`dotnet test src/UnitTests/PubnubApiPCL.Tests/PubnubApiPCL.Tests.csproj` runs what CI runs. It needs the `PN_*` environment variables and publishes to real keysets. Without keys, failures are network or access errors, not regressions.

## Report

Summarize:
- which builds ran and their results
- which tests ran, with counts
- the link and ConfigureAwait check results
- anything you didn't run (UWP, live tests), and why
