---
name: add-endpoint
description: Adds a new REST API operation to the PubNub C# SDK, covering the operation class, Pubnub factory method, PNOperationType, result model, callback wrapper, parser registration, Retry support, project links for PCL/UWP/Unity, offline tests and docs snippet. Use when implementing a new PubNub API call, adding a missing operation, or porting an endpoint from another PubNub SDK.
---

# Add an endpoint (operation)

Copy this checklist and track progress:

```text
- [ ] 1. API contract understood
- [ ] 2. PNOperationType member
- [ ] 3. Result model and callback wrapper
- [ ] 4. Parser, registered in DeserializeToInternalObjectUtility
- [ ] 5. Operation class
- [ ] 6. Factory method on Pubnub, Retry case in PNStatus
- [ ] 7. New files linked into PCL, UWP and Unity projects
- [ ] 8. Offline tests, linked into PubnubApiPCL.Tests
- [ ] 9. Snippet
- [ ] 10. Checks pass (run-checks skill)
```

Paths are relative to `src/Api/PubnubApi/` unless they start with `src/`.

## 1. Understand the API contract

- Find the REST path, HTTP method, query parameters, body, response and error shapes. Sources: the user's spec, other PubNub SDKs, or <https://www.pubnub.com/docs>. Don't guess field names.
- Use `EndPoint/PubSub/RemoveMessageActionOperation.cs` as the reference for a fluent builder, and the `EndPoint/DataSync/*` operations for the `Execute(callback)` wrapper. For Data Sync calls, follow the existing `XxxParameters` + `XxxOperation` pairs and the `DataSync.cs` facade instead of a `Pubnub` factory method.

## 2. `Enum/PNOperationType.cs`

Append `PNDoThingOperation` as the **last** member. Never insert in the middle, so existing integer values stay stable.

## 3. Result model and callback wrapper

`Model/Consumer/Feature/PNDoThingResult.cs`:

```csharp
namespace PubnubApi
{
    public class PNDoThingResult
    {
        /// <summary>Number of things.</summary>
        public int Count { get; internal set; }
    }
}
```

`Model/Derived/Feature/PNDoThingResultExt.cs`:

```csharp
using System;

namespace PubnubApi
{
    public class PNDoThingResultExt : PNCallback<PNDoThingResult>
    {
        private readonly Action<PNDoThingResult, PNStatus> callbackAction;

        public PNDoThingResultExt(Action<PNDoThingResult, PNStatus> callback)
        {
            callbackAction = callback;
        }

        public override void OnResponse(PNDoThingResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
```

## 4. Parser and registration

`JsonDataParse/PNDoThingJsonDataParse.cs`. For a JSON object response, `listObject` holds its top-level properties. Look them up by name, never by position:

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PubnubApi
{
    internal static class PNDoThingJsonDataParse
    {
        internal static PNDoThingResult GetObject(IJsonPluggableLibrary jsonPlug, List<object> listObject)
        {
            var result = new PNDoThingResult();
            foreach (object rawObject in listObject)
            {
                Dictionary<string, object> property = jsonPlug.ConvertToDictionaryObject(rawObject);
                if (property == null || !property.TryGetValue("data", out object data) || data == null)
                {
                    continue;
                }

                Dictionary<string, object> dataDictionary = jsonPlug.ConvertToDictionaryObject(data);
                if (dataDictionary != null && dataDictionary.TryGetValue("count", out object count) && count != null)
                {
                    result.Count = Convert.ToInt32(count, CultureInfo.InvariantCulture);
                }
            }
            return result;
        }
    }
}
```

Register it in `JsonDataParse/DeserializeToInternalObjectUtility.cs`. An unregistered type silently produces a `null` result:

```csharp
else if (typeof(T) == typeof(PNDoThingResult))
{
    PNDoThingResult result = PNDoThingJsonDataParse.GetObject(jsonPlug, listObject);
    ret = (T)Convert.ChangeType(result, typeof(PNDoThingResult), CultureInfo.InvariantCulture);
}
```

## 5. Operation class: `EndPoint/Feature/DoThingOperation.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class DoThingOperation : PubnubCoreBase
    {
        private const PNOperationType OperationType = PNOperationType.PNDoThingOperation;

        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;

        private string channelName;
        private int limit = -1;
        private Dictionary<string, object> queryParam;
        private PNCallback<PNDoThingResult> savedCallback;

        public DoThingOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance)
            : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            PubnubInstance = instance;
        }

        /// <summary>Channel to do the thing on.</summary>
        public DoThingOperation Channel(string channel)
        {
            channelName = channel;
            return this;
        }

        /// <summary>Maximum number of things to return.</summary>
        public DoThingOperation Limit(int count)
        {
            limit = count;
            return this;
        }

        /// <summary>Additional query parameters.</summary>
        public DoThingOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        /// <summary>Executes the request and delivers the result to <paramref name="callback"/>.</summary>
        public void Execute(PNCallback<PNDoThingResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

            savedCallback = callback;
            ExecuteAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    callback.OnResponse(default, new PNStatus
                    {
                        Error = true,
                        ErrorData = new PNErrorData(t.Exception?.Message, t.Exception)
                    });
                    return;
                }
                callback.OnResponse(t.Result.Result, t.Result.Status);
            });
        }

        /// <summary>Executes the request.</summary>
        public async Task<PNResult<PNDoThingResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await DoThing().ConfigureAwait(false);
        }

        internal void Retry()
        {
            if (savedCallback != null)
            {
                Execute(savedCallback);
            }
        }

        private async Task<PNResult<PNDoThingResult>> DoThing()
        {
            var returnValue = new PNResult<PNDoThingResult>();
            if (string.IsNullOrEmpty(config.SubscribeKey?.Trim()))
            {
                returnValue.Status = ErrorStatus("Invalid subscribe key");
                return returnValue;
            }
            if (string.IsNullOrEmpty(channelName?.Trim()))
            {
                returnValue.Status = ErrorStatus("Missing Channel");
                return returnValue;
            }

            var requestState = new RequestState<PNDoThingResult>
            {
                Channels = new[] { channelName },
                ResponseType = OperationType,
                Reconnect = false,
                EndPointOperation = this
            };

            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter: CreateRequestParameter(), operationType: OperationType);
            var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest)
                .ConfigureAwait(false);

            if (transportResponse.Error != null)
            {
                int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
                PNStatusCategory category =
                    PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
                returnValue.Status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(OperationType,
                    category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                return returnValue;
            }

            string responseString = Encoding.UTF8.GetString(transportResponse.Content);
            PNStatus errorStatus = GetStatusIfError(requestState, responseString);
            if (errorStatus != null)
            {
                returnValue.Status = errorStatus;
                return returnValue;
            }

            requestState.GotJsonResponse = true;
            returnValue.Status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(OperationType,
                PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
            List<object> resultList = ProcessJsonResponse(requestState, responseString);
            returnValue.Result =
                new ResponseBuilder(config, jsonLibrary).JsonToObject<PNDoThingResult>(resultList, true);
            logger?.Trace($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
            return returnValue;
        }

        private static PNStatus ErrorStatus(string message)
        {
            return new PNStatus
            {
                Error = true,
                ErrorData = new PNErrorData(message, new ArgumentException(message))
            };
        }

        private RequestParameter CreateRequestParameter()
        {
            // Path segments are encoded by the middleware; query values must be encoded here.
            var pathSegments = new List<string> { "v1", "feature", config.SubscribeKey, "channel", channelName };
            var query = new Dictionary<string, string>();
            if (limit >= 0)
            {
                query.Add("limit", limit.ToString(CultureInfo.InvariantCulture));
            }
            if (queryParam != null)
            {
                foreach (KeyValuePair<string, object> kvp in queryParam)
                {
                    if (!query.ContainsKey(kvp.Key))
                    {
                        query.Add(kvp.Key,
                            UriUtil.EncodeUriComponent(kvp.Value.ToString(), OperationType, false, false, false));
                    }
                }
            }

            return new RequestParameter
            {
                RequestType = Constants.GET,
                PathSegment = pathSegments,
                Query = query
            };
        }
    }
}
```

- **Bodies (POST, PATCH, PUT):** set `RequestType`, put the serialized body in `BodyContentString` (`jsonLibrary.SerializeToJsonString(...)`), and add a `Content-Type` header to `Headers`.
- **No `auth` parameter:** if the operation must not send one, add it to `excludedAuthOperationTypes` in `Transport/Middleware.cs`.
- **Every `await` ends with `.ConfigureAwait(false)`.**

## 6. Factory method and Retry

In `Pubnub.cs`, next to the methods of the same feature:

```csharp
public DoThingOperation DoThing()
{
    return new DoThingOperation(pubnubConfig.ContainsKey(InstanceId) ? pubnubConfig[InstanceId] : null,
        JsonPluggableLibrary, pubnubUnitTest, tokenManager, this);
}
```

In the `Retry()` switch in `Model/Consumer/PNStatus.cs`:

```csharp
case PNOperationType.PNDoThingOperation:
    if (savedEndpointOperation is DoThingOperation doThingOperation)
    {
        doThingOperation.Retry();
    }
    break;
```

## 7. Link the new files

Add one line per new file to **each** of `src/Api/PubnubApiPCL/PubnubApiPCL.csproj`, `src/Api/PubnubApiUWP/PubnubApiUWP.csproj` and `src/Api/PubnubApiUnity/PubnubApiUnity.csproj`:

```xml
<Compile Include="..\PubnubApi\EndPoint\Feature\DoThingOperation.cs" Link="EndPoint\Feature\DoThingOperation.cs" />
```

## 8. Offline tests

Create `src/UnitTests/PubnubApi.Tests/WhenDoThingIsRequested.cs`. Test projects use C# 10, and the namespace is `PubNubMessaging.Tests`. Injecting an `HttpMessageHandler` runs the whole pipeline (request building, middleware, parsing) without network:

```csharp
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PubnubApi;

namespace PubNubMessaging.Tests
{
    [TestFixture]
    public class WhenDoThingIsRequested
    {
        private class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode statusCode;
            private readonly string body;

            public StubHandler(HttpStatusCode statusCode, string body)
            {
                this.statusCode = statusCode;
                this.body = body;
            }

            public HttpRequestMessage LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(body),
                    RequestMessage = request
                });
            }
        }

        private static Pubnub CreatePubnub(StubHandler handler)
        {
            var config = new PNConfiguration(new UserId("test-user"))
            {
                SubscribeKey = "sub-key",
                PublishKey = "pub-key"
            };
            return new Pubnub(config, httpTransportService: new HttpClientService(handler, false));
        }

        [Test]
        public async Task ThenRequestIsBuiltAndResultIsParsed()
        {
            var handler = new StubHandler(HttpStatusCode.OK, "{\"status\":200,\"data\":{\"count\":3}}");
            Pubnub pubnub = CreatePubnub(handler);

            PNResult<PNDoThingResult> response = await pubnub.DoThing().Channel("my channel").Limit(5).ExecuteAsync();

            Assert.IsFalse(response.Status.Error, response.Status.ErrorData?.Information);
            Assert.AreEqual("/v1/feature/sub-key/channel/my%20channel", handler.LastRequest.RequestUri.AbsolutePath);
            StringAssert.Contains("limit=5", handler.LastRequest.RequestUri.Query);
            Assert.AreEqual(3, response.Result.Count);
            pubnub.Destroy();
        }

        [Test]
        public async Task ThenMissingChannelReturnsErrorStatus()
        {
            Pubnub pubnub = CreatePubnub(new StubHandler(HttpStatusCode.OK, "{}"));

            PNResult<PNDoThingResult> response = await pubnub.DoThing().ExecuteAsync();

            Assert.IsTrue(response.Status.Error);
            pubnub.Destroy();
        }
    }
}
```

- Also cover an error response (for example 400 with `{"status":400,"error":{"message":"..."}}`) and the `Execute(callback)` path.
- Link the test file into `src/UnitTests/PubnubApiPCL.Tests/PubnubApiPCL.Tests.csproj`: `<Compile Include="..\PubnubApi.Tests\WhenDoThingIsRequested.cs" Link="WhenDoThingIsRequested.cs" />`.
- Add a live test only if the user wants one. It must read keys from `PubnubCommon`.

## 9. Snippet

Add a `// snippet.<name>` ... `// snippet.end` block to the feature's file in `src/PubnubApi.Snippets/`, using placeholder keys. It must compile.

## 10. Verify

Follow the `run-checks` skill: build the library, PCL and Unity, run the offline tests, and run `Test-LinkedSources.ps1` and `Test-ConfigureAwait.ps1`. Then propose a one-line changelog entry for the PR description. Don't edit versions, `PackageReleaseNotes` or `CHANGELOG`; the release bot writes them.
