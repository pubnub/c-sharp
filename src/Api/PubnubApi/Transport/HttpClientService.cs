﻿using System;
using System.Net.Http;
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
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }
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
                    postData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8, "application/json");
                }
                else if (transportRequest.BodyContentBytes != null)
                {
                    postData = new ByteArrayContent(transportRequest.BodyContentBytes);
                    foreach (var transportRequestHeader in transportRequest.Headers)
                    {
                        postData.Headers.Add(transportRequestHeader.Key, transportRequestHeader.Value);
                    }
                }

                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Post, requestUri: transportRequest.RequestUrl)
                        { Content = postData };
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
                    putData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8, "application/json");
                }
                else if (transportRequest.BodyContentBytes != null)
                {
                    putData = new ByteArrayContent(transportRequest.FormData);
                    foreach (var transportRequestHeader in transportRequest.Headers)
                    {
                        putData.Headers.Add(transportRequestHeader.Key, transportRequestHeader.Value);
                    }
                }

                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Put, requestUri: transportRequest.RequestUrl)
                        { Content = putData };
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

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
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

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
                    patchData = new StringContent(transportRequest.BodyContentString, Encoding.UTF8,
                        "application/json");
                }
                else if (transportRequest.BodyContentBytes != null)
                {
                    patchData = new ByteArrayContent(transportRequest.FormData);
                    foreach (var transportRequestHeader in transportRequest.Headers)
                    {
                        patchData.Headers.Add(transportRequestHeader.Key, transportRequestHeader.Value);
                    }
                }

                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(new HttpMethod("PATCH"), requestUri: transportRequest.RequestUrl)
                        { Content = patchData };
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, $"\"{kvp.Value}\"");
                    }
                }

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