using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace PubnubApi
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient httpClient;
        private PubnubLogModule logger;

        public HttpClientService(IWebProxy proxy)
        {
            httpClient = new HttpClient()
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
            if (proxy == null) return;
            httpClient = new HttpClient(new HttpClientHandler()
            {
                Proxy = proxy,
                UseProxy = true
            });
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        public void SetLogger(PubnubLogModule logger)
        {
            this.logger = logger;
        }

        public async Task<TransportResponse> GetRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
            CancellationTokenSource ctsWithTimeout = null;
            try
            {
                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Get, requestUri: transportRequest.RequestUrl);
                ApplyHeaders(requestMessage.Headers, transportRequest.Headers);
                logger?.Debug(
                    $"HttpClient Service: Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl}" +
                    (requestMessage.Headers.Any()
                        ? $"\n  Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}"
                        : ""));
                if (transportRequest.Timeout.HasValue)
                {
                    ctsWithTimeout =
                        CancellationTokenSource.CreateLinkedTokenSource(transportRequest.CancellationTokenSource.Token);
                    ctsWithTimeout.CancelAfter(transportRequest.Timeout.Value);
                }

                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken:ctsWithTimeout?.Token??transportRequest.CancellationTokenSource.Token).ConfigureAwait(false);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug(
                    $"HttpClient Service: Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url \n{transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                logger?.Warn(
                    $"HttpClient Service: Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }
            finally
            {
                ctsWithTimeout?.Dispose();
                transportRequest.CancellationTokenSource?.Dispose();
            }

            return transportResponse;
        }

        public async Task<TransportResponse> PostRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
            CancellationTokenSource ctsWithTimeout = null;
            try
            {
                HttpContent postData = null;
                if (!string.IsNullOrEmpty(transportRequest.BodyContentString))
                {
                    var contentType = "application/json";
                    if (transportRequest.Headers.TryGetValue("Content-Type", out var ct))
                    {
                        contentType = ct;
                    }
                    postData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8);
                    postData.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
                }
                else if (transportRequest.BodyContentBytes != null)
                {
                    postData = new ByteArrayContent(transportRequest.BodyContentBytes);
                    ApplyHeaders(postData.Headers, transportRequest.Headers);
                }

                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Post, requestUri: transportRequest.RequestUrl)
                        { Content = postData };
                ApplyHeaders(requestMessage.Headers, transportRequest.Headers, excludeKey: "Content-Type");
                logger?.Debug(
                    $"HttpClient Service:Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl}" +
                    (requestMessage.Headers.Any()
                        ? $"\n  Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}"
                        : ""));
                if (transportRequest.Timeout.HasValue)
                {
                    ctsWithTimeout =
                        CancellationTokenSource.CreateLinkedTokenSource(transportRequest.CancellationTokenSource.Token);
                    ctsWithTimeout.CancelAfter(transportRequest.Timeout.Value);
                }
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: ctsWithTimeout?.Token??transportRequest.CancellationTokenSource.Token).ConfigureAwait(false);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug(
                    $"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                logger?.Warn(
                    $"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }
            finally
            {
                ctsWithTimeout?.Dispose();
                transportRequest.CancellationTokenSource?.Dispose();
            }

            return transportResponse;
        }

        public async Task<TransportResponse> PutRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
            CancellationTokenSource ctsWithTimeout = null;
            try
            {
                HttpContent putData = null;

                if (!string.IsNullOrEmpty(transportRequest.BodyContentString))
                {
                    var contentType = "application/json";
                    if (transportRequest.Headers.TryGetValue("Content-Type", out var ct))
                    {
                        contentType = ct;
                    }
                    putData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8);
                    putData.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
                }
                else if (transportRequest.BodyContentBytes != null)
                {
                    putData = new ByteArrayContent(transportRequest.FormData);
                    ApplyHeaders(putData.Headers, transportRequest.Headers);
                }

                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Put, requestUri: transportRequest.RequestUrl)
                        { Content = putData };
                ApplyHeaders(requestMessage.Headers, transportRequest.Headers, excludeKey: "Content-Type");

                logger?.Debug(
                    $"HttpClient Service:Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl}" +
                    (requestMessage.Headers.Any()
                        ? $"\n  Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}"
                        : ""));
                if (transportRequest.Timeout.HasValue)
                {
                    ctsWithTimeout =
                        CancellationTokenSource.CreateLinkedTokenSource(transportRequest.CancellationTokenSource.Token);
                    ctsWithTimeout.CancelAfter(transportRequest.Timeout.Value);
                }
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: ctsWithTimeout?.Token??transportRequest.CancellationTokenSource.Token).ConfigureAwait(false);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug(
                    $"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                logger?.Warn(
                    $"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }
            finally
            {
                ctsWithTimeout?.Dispose();
                transportRequest.CancellationTokenSource?.Dispose();
            }

            return transportResponse;
        }

        public async Task<TransportResponse> DeleteRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
            CancellationTokenSource ctsWithTimeout = null;
            try
            {
                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Delete, requestUri: transportRequest.RequestUrl);
                ApplyHeaders(requestMessage.Headers, transportRequest.Headers);

                logger?.Debug(
                    $"HttpClient Service:Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl}" +
                    (requestMessage.Headers.Any()
                        ? $"\n  Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}"
                        : ""));
                if (transportRequest.Timeout.HasValue)
                {
                    ctsWithTimeout =
                        CancellationTokenSource.CreateLinkedTokenSource(transportRequest.CancellationTokenSource.Token);
                    ctsWithTimeout.CancelAfter(transportRequest.Timeout.Value);
                }
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: ctsWithTimeout?.Token??transportRequest.CancellationTokenSource.Token).ConfigureAwait(false);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug(
                    $"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                logger?.Warn(
                    $"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }
            finally
            {
                ctsWithTimeout?.Dispose();
                transportRequest.CancellationTokenSource?.Dispose();
            }

            return transportResponse;
        }

        public async Task<TransportResponse> PatchRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
            CancellationTokenSource ctsWithTimeout = null;
            try
            {
                HttpContent patchData = null;

                if (!string.IsNullOrEmpty(transportRequest.BodyContentString))
                {
                    var contentType = "application/json";
                    if (transportRequest.Headers.TryGetValue("Content-Type", out var ct))
                    {
                        contentType = ct;
                    }
                    patchData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8);
                    patchData.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(contentType);
                }
                else if (transportRequest.BodyContentBytes != null)
                {
                    patchData = new ByteArrayContent(transportRequest.FormData);
                    ApplyHeaders(patchData.Headers, transportRequest.Headers);
                }

                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(new HttpMethod("PATCH"), requestUri: transportRequest.RequestUrl)
                        { Content = patchData };
                ApplyHeaders(requestMessage.Headers, transportRequest.Headers, excludeKey: "Content-Type");

                logger?.Debug(
                    $"HttpClient Service:Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl}" +
                    (requestMessage.Headers.Any()
                        ? $"\n  Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}"
                        : ""));
                if (transportRequest.Timeout.HasValue)
                {
                    ctsWithTimeout =
                        CancellationTokenSource.CreateLinkedTokenSource(transportRequest.CancellationTokenSource.Token);
                    ctsWithTimeout.CancelAfter(transportRequest.Timeout.Value);
                }
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: ctsWithTimeout?.Token??transportRequest.CancellationTokenSource.Token).ConfigureAwait(false);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug(
                    $"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                logger?.Warn(
                    $"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }
            finally
            {
                ctsWithTimeout?.Dispose();
                transportRequest.CancellationTokenSource?.Dispose();
            }

            return transportResponse;
        }
        
        //This is because server returns eTag in the "someetag" format instead of "\"someetag\"" which is technically wrong,
        //meaning that HttpHeaders.Add will throw a System.FormattingException, necessitating the usage of TryAddWithoutValidation() in these cases
        private static readonly HashSet<string> HeadersWithRelaxedValidation = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "If-Match",
            "If-None-Match"
        };
        private static void ApplyHeaders(HttpHeaders target, Dictionary<string, string> source, string excludeKey = null)
        {
            foreach (var kvp in source)
            {
                if (excludeKey != null && string.Equals(kvp.Key, excludeKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (HeadersWithRelaxedValidation.Contains(kvp.Key))
                {
                    target.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
                else
                {
                    target.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private TransportResponse GetTransportResponseForTaskCancelation(TransportRequest transportRequest,
            TaskCanceledException taskCanceledException, CancellationTokenSource ctsWithTimeout)
        {
            TransportResponse transportResponse;
            
            logger?.Warn($"HttpClient Service: TaskCanceledException for url {transportRequest.RequestUrl}");
            transportResponse = new TransportResponse()
            {
                RequestUrl = transportRequest.RequestUrl,
                Error = taskCanceledException,
            };
            if (ctsWithTimeout is { Token.IsCancellationRequested: true } &&
                !transportRequest.CancellationTokenSource.IsCancellationRequested)
            {
                logger?.Debug("HttpClient Service: Task canceled due to timeout");
                transportResponse.IsTimeOut = true;
            }
            else
            {
                logger?.Debug("HttpClient Service: Task canceled due to cancellation request");
                transportResponse.IsCancelled = true;
            }
            return transportResponse;
        }
    }
}