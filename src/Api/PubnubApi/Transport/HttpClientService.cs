using System;
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
        private readonly PubnubLogModule logger;

        public HttpClientService(IWebProxy proxy, PNConfiguration configuration )
        {
            ServicePointManager.DefaultConnectionLimit = 50;
            logger = configuration.Logger;
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

        public async Task<TransportResponse> GetRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
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
                logger?.Debug($"Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl} \n Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: transportRequest.CancellationTokenSource.Token);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug($"HttpClient Service:Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException taskCanceledException)
            {
                logger?.Error($"HttpClient Service: Request is cancelled for url {transportRequest.RequestUrl}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = taskCanceledException
                };
            }
            catch (Exception e)
            {
                logger?.Error($"HttpClient Service: Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }

            return transportResponse;
        }

        public async Task<TransportResponse> PostRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
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
                logger?.Debug($"Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl} \n Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: transportRequest.CancellationTokenSource.Token);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug($"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException)
            {
                logger?.Error($"Request is cancelled for url {transportRequest.RequestUrl}");
                transportResponse = null;
            }
            catch (Exception e)
            {
                logger?.Error($"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }

            return transportResponse;
        }

        public async Task<TransportResponse> PutRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
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
                logger?.Debug($"Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl} \n Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: transportRequest.CancellationTokenSource.Token);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug($"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException)
            {
                logger?.Error($"Request is cancelled for url {transportRequest.RequestUrl}");
                transportResponse = null;
            }
            catch (Exception e)
            {
                logger?.Error($"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }

            return transportResponse;
        }

        public async Task<TransportResponse> DeleteRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
            try
            {
                if (transportRequest.Timeout.HasValue) httpClient.Timeout = (TimeSpan)transportRequest.Timeout;
                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Delete, requestUri: transportRequest.RequestUrl);
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }
                logger?.Debug($"Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl} \n Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: transportRequest.CancellationTokenSource.Token);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug($"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException)
            {
                logger?.Error($"Request is cancelled for url {transportRequest.RequestUrl}");
                transportResponse = null;
            }
            catch (Exception e)
            {
                logger?.Error($"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }

            return transportResponse;
        }

        public async Task<TransportResponse> PatchRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
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
                logger?.Debug($"Sending http request {transportRequest.RequestType} to {transportRequest.RequestUrl} \n Header {string.Join(", ", requestMessage.Headers.Select(kv => $"{kv.Key}: {kv.Value}"))}");
                var httpResult = await httpClient.SendAsync(request: requestMessage,
                    cancellationToken: transportRequest.CancellationTokenSource.Token);
                var responseContent = await httpResult.Content.ReadAsByteArrayAsync();
                transportResponse = new TransportResponse()
                {
                    StatusCode = (int)httpResult.StatusCode,
                    Content = responseContent,
                    Headers = httpResult.Headers.ToDictionary(h => h.Key, h => h.Value),
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath
                };
                logger?.Debug($"Received http response from server with status code {httpResult.StatusCode}, content-length: {transportResponse.Content.Length} bytes, for url {transportRequest.RequestUrl}");
            }
            catch (TaskCanceledException)
            {
                logger?.Error($"Request is cancelled for url {transportRequest.RequestUrl}");
                transportResponse = null;
            }
            catch (Exception e)
            {
                logger?.Error($"Exception for http call url {transportRequest.RequestUrl}, exception message: {e.Message}, stacktrace: {e.StackTrace}");
                transportResponse = new TransportResponse()
                {
                    RequestUrl = transportRequest.RequestUrl,
                    Error = e
                };
            }
            return transportResponse;
        }
    }
}