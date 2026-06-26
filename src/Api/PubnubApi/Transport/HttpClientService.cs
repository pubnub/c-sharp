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
        private readonly bool enableHttp2;
        private TransportLogger transportLogger;

        public HttpClientService(IWebProxy proxy) : this(proxy, true)
        {
        }

        public HttpClientService(IWebProxy proxy, bool enableHttp2)
        {
            this.enableHttp2 = enableHttp2;
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

        /// <summary>
        /// Creates a transport backed by a caller-supplied <see cref="HttpMessageHandler"/>.
        /// Intended for testing and advanced transport scenarios (e.g. observing outgoing
        /// requests or stubbing responses). The supplied handler is fully responsible for
        /// TLS/certificate validation and proxy behavior; the SDK does not weaken or override
        /// them here. Do not pass a handler that disables certificate validation in production.
        /// </summary>
        /// <param name="handler">The message handler used to send requests.</param>
        /// <param name="enableHttp2">Whether outbound requests should request HTTP/2 with HTTP/1.1 fallback.</param>
        public HttpClientService(HttpMessageHandler handler, bool enableHttp2)
        {
            this.enableHttp2 = enableHttp2;
            httpClient = new HttpClient(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        // HTTP/2 is requested by default; RequestVersionOrLower guarantees HTTP/1.1 fallback
        // against non-HTTP/2 origins. Set per HttpRequestMessage because the SDK uses explicit
        // request messages over a shared HttpClient.
        private void ConfigureHttpVersion(HttpRequestMessage requestMessage)
        {
            if (!enableHttp2)
            {
                return;
            }
            requestMessage.Version = new Version(2, 0);
#if NET6_0_OR_GREATER || NET60
            requestMessage.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
#endif
        }

        public void SetLogger(PubnubLogModule logger)
        {
            this.transportLogger = new TransportLogger(logger);
        }

        public async Task<TransportResponse> GetRequest(TransportRequest transportRequest)
        {
            TransportResponse transportResponse;
            CancellationTokenSource ctsWithTimeout = null;
            try
            {
                HttpRequestMessage requestMessage =
                    new HttpRequestMessage(method: HttpMethod.Get, requestUri: transportRequest.RequestUrl);
                ConfigureHttpVersion(requestMessage);
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }
                transportLogger?.Request(transportRequest);
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
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath,
                    NegotiatedProtocolVersion = httpResult.Version
                };
                transportLogger?.Response(transportRequest, transportResponse, httpResult.Version);
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                transportLogger?.Exception(transportRequest, e);
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
                ConfigureHttpVersion(requestMessage);
                // Set Http Request header, When the header is not a payload content header.
                if (transportRequest.Headers.Keys.Count > 0 && transportRequest.BodyContentBytes == null)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }
                transportLogger?.Request(transportRequest);
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
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath,
                    NegotiatedProtocolVersion = httpResult.Version
                };
                transportLogger?.Response(transportRequest, transportResponse, httpResult.Version);
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                transportLogger?.Exception(transportRequest, e);
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
                ConfigureHttpVersion(requestMessage);
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

                transportLogger?.Request(transportRequest);
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
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath,
                    NegotiatedProtocolVersion = httpResult.Version
                };
                transportLogger?.Response(transportRequest, transportResponse, httpResult.Version);
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                transportLogger?.Exception(transportRequest, e);
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
                ConfigureHttpVersion(requestMessage);
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

                transportLogger?.Request(transportRequest);
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
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath,
                    NegotiatedProtocolVersion = httpResult.Version
                };
                transportLogger?.Response(transportRequest, transportResponse, httpResult.Version);
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                transportLogger?.Exception(transportRequest, e);
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
                ConfigureHttpVersion(requestMessage);
                if (transportRequest.Headers.Keys.Count > 0)
                {
                    foreach (var kvp in transportRequest.Headers)
                    {
                        requestMessage.Headers.Add(kvp.Key, $"\"{kvp.Value}\"");
                    }
                }

                transportLogger?.Request(transportRequest);
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
                    RequestUrl = httpResult.RequestMessage?.RequestUri?.AbsolutePath,
                    NegotiatedProtocolVersion = httpResult.Version
                };
                transportLogger?.Response(transportRequest, transportResponse, httpResult.Version);
            }
            catch (TaskCanceledException taskCanceledException)
            {
                transportResponse = GetTransportResponseForTaskCancelation(transportRequest, taskCanceledException, ctsWithTimeout);
            }
            catch (Exception e)
            {
                transportLogger?.Exception(transportRequest, e);
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
            
            transportLogger?.TaskCanceled(transportRequest);
            transportResponse = new TransportResponse()
            {
                RequestUrl = transportRequest.RequestUrl,
                Error = taskCanceledException,
            };
            if (ctsWithTimeout is { Token.IsCancellationRequested: true } &&
                !transportRequest.CancellationTokenSource.IsCancellationRequested)
            {
                transportLogger?.CanceledByTimeout();
                transportResponse.IsTimeOut = true;
            }
            else
            {
                transportLogger?.CanceledByRequest();
                transportResponse.IsCancelled = true;
            }
            return transportResponse;
        }
    }
}