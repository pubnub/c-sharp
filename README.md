<img width="1920" height="600" alt="csharp-header" src="https://github.com/user-attachments/assets/55b2b0de-7d0a-4b08-9ca4-d5a4e746b9f0" />

# PubNub C# SDK

[![NuGet](https://img.shields.io/nuget/v/PubnubPCL.svg)](https://www.nuget.org/packages/PubnubPCL)

PubNub provides global infrastructure for real-time, interactive applications.

Publish and receive messages in C#. Use this SDK for .NET applications compatible with the current PubNub C# packages.

[Documentation](https://www.pubnub.com/docs/sdks/c-sharp) · [API reference](https://www.pubnub.com/docs/sdks/c-sharp/api-reference/publish-and-subscribe) · [Changelog](https://www.pubnub.com/docs/sdks/c-sharp/changelog) · [Support](https://support.pubnub.com/)

## Requirements

| Requirement | Supported version or setup |
| --- | --- |
| .NET | [.NET 6.0 or .NET Standard 2.0 for `PubnubPCL`](https://www.nuget.org/packages/PubnubPCL) |
| Target framework | [Frameworks supported by the current PubNub NuGet packages](https://www.nuget.org/packages/PubnubPCL) |
| NuGet | Install the PubNub package compatible with the application's target framework. |

## Installation

For the console Quickstart, run from the project directory:

```sh
dotnet add package PubnubPCL
```

For other installation methods, see [environment setup](https://www.pubnub.com/docs/sdks/c-sharp).

## Quickstart

This example runs in a .NET 6+ console application. It subscribes to `hello_world`, publishes one message, and prints the received text.

### Get your keys

1. Open the [PubNub Admin Portal](https://admin.pubnub.com/).
2. Create an app and a keyset for development, or select an existing development keyset.
3. Copy its publish key and subscribe key.

If Access Manager is enabled, obtain a token from your trusted backend and call `pubnub.SetAuthToken(token)`. Keep the secret key on the backend.

### Send and receive a message

Replace `Program.cs` with:

Replace the key placeholders. Use a User ID that identifies the user or device in your app.

```csharp
using System;
using System.Threading.Tasks;
using PubnubApi;

class Program
{
    private static readonly TaskCompletionSource<bool> Received =
        new TaskCompletionSource<bool>();

    static async Task Main()
    {
        PNConfiguration config =
            new PNConfiguration(new UserId("hello-world-user"))
            {
                SubscribeKey = "YOUR_SUBSCRIBE_KEY",
                PublishKey = "YOUR_PUBLISH_KEY"
            };

        Pubnub pubnub = new Pubnub(config);

        SubscribeCallbackExt listener =
            new SubscribeCallbackExt(
                (pn, message) =>
                {
                    Console.WriteLine(message.Message);
                    Received.TrySetResult(true);
                },
                (pn, presence) => { },
                (pn, status) =>
                {
                    if (status.Category ==
                        PNStatusCategory.PNConnectedCategory)
                    {
                        _ = pubnub.Publish()
                            .Channel("hello_world")
                            .Message("Hello world")
                            .ExecuteAsync();
                    }
                }
            );

        pubnub.AddListener(listener);
        pubnub.Subscribe<string>()
            .Channels(new[] { "hello_world" })
            .Execute();

        await Received.Task;

        pubnub.UnsubscribeAll<string>();
        pubnub.RemoveListener(listener);
        pubnub.Destroy();
    }
}
```

### Run the example

```sh
dotnet run
```

The terminal should show:

```text
Hello world
```

The example calls `pubnub.UnsubscribeAll<string>()`, `pubnub.RemoveListener(listener)`, and `pubnub.Destroy()` after receiving the message.

For a complete application, see the [getting started guide](https://www.pubnub.com/docs/sdks/c-sharp).

## Next steps

| Task | Documentation |
| --- | --- |
| Configure the client | [Configuration](https://www.pubnub.com/docs/sdks/c-sharp/api-reference/configuration) |
| Work with subscriptions and messages | [Publish & Subscribe](https://www.pubnub.com/docs/sdks/c-sharp/api-reference/publish-and-subscribe) |
| Check channel occupancy | [Presence](https://www.pubnub.com/docs/sdks/c-sharp/api-reference/presence) |
| Read message history | [Message Persistence](https://www.pubnub.com/docs/sdks/c-sharp/api-reference/storage-and-playback) |

## Build with an AI coding assistant

The [PubNub MCP server](https://www.pubnub.com/docs/ai/pubnub-mcp-server) gives an AI coding assistant access to PubNub SDK documentation and PubNub APIs. Connect the assistant to the hosted server at [`https://mcp.pubnub.com`](https://mcp.pubnub.com), or run `npx @pubnub/mcp@latest` locally.

The [server repository](https://github.com/pubnub/pubnub-mcp-server) has setup steps for VS Code, Cursor, Claude Code, Claude Desktop, Codex, Gemini CLI, and OpenCode.

## Before production

Use [Access Manager](https://www.pubnub.com/docs/sdks/c-sharp/api-reference/access-manager) to grant each client the permissions it needs. Keep the secret key on your backend. Never include it in a distributed client.

Reuse the PubNub client for the application/service lifetime. Call `pubnub.UnsubscribeAll<string>()` when its subscriptions should stop, remove listeners with `pubnub.RemoveListener(listener)`, and call `pubnub.Destroy()` when the client is no longer needed.

Live delivery through PubNub SDKs is at-most-once. A subscriber can miss messages while disconnected or if its buffer overflows. For longer-gap recovery, see [Message Persistence](https://www.pubnub.com/docs/sdks/c-sharp/api-reference/storage-and-playback).

## Troubleshooting

| Symptom | Check |
| --- | --- |
| The project cannot resolve `PubnubApi` | Install the PubNub NuGet package compatible with the target framework and add `using PubnubApi;`. See the [C# SDK guide](https://www.pubnub.com/docs/sdks/c-sharp). |
| A publish succeeds but no message appears | Check the message handler, subscription readiness, keyset, and channel name. |
| The application targets a framework incompatible with the installed PubNub package | Verify the target-framework compatibility of the selected package; `PubnubPCL` targets .NET 6.0 and .NET Standard 2.0. See [NuGet](https://www.nuget.org/packages/PubnubPCL). |
| Extra clients or duplicate messages during development | For ASP.NET or dependency-injection applications, register and reuse a PubNub client at application/service scope rather than constructing one on every request or development reload. |

For setup help, see [troubleshooting](https://www.pubnub.com/docs/sdks/c-sharp). Check [network status](https://status.pubnub.com/) for service incidents.

## Releases

Read the [changelog](https://www.pubnub.com/docs/sdks/c-sharp/changelog) before upgrading.

For a major version change, review the [C# SDK changelog](https://www.pubnub.com/docs/sdks/c-sharp/changelog) for breaking changes before upgrading.

## Support and contributions

For setup or account help, contact [PubNub Support](https://support.pubnub.com/).

For a reproducible SDK bug, use [GitHub Issues](https://github.com/pubnub/c-sharp/issues). Include the SDK version, runtime, and a small reproduction with credentials removed.

Restore dependencies, build the affected targets, run the repository test suite, and include tests for behavioral changes before opening a pull request.

## License

See the [PubNub Software Development Kit License](https://github.com/pubnub/c-sharp/blob/master/LICENSE).
