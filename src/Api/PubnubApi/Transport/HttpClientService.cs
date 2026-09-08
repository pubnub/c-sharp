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

                ApplyHeaders(requestMessage.Headers, transportRequest.Headers);
                ConfigureHttpVersion(requestMessage);
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
                ConfigureHttpVersion(requestMessage);
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
                ConfigureHttpVersion(requestMessage);
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
                
				ApplyHeaders(requestMessage.Headers, transportRequest.Headers);
                ConfigureHttpVersion(requestMessage);
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
                ConfigureHttpVersion(requestMessage);
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