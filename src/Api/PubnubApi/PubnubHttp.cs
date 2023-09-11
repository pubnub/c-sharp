using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading.Tasks;
using System.Globalization;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace PubnubApi
{
    public class PubnubHttp : IPubnubHttp
    {
        private readonly PNConfiguration pubnubConfig;
        private readonly IJsonPluggableLibrary jsonLib;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
        private static HttpClient httpClientSubscribe;
        private static HttpClient httpClientNonsubscribe;
#endif

#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
        public PubnubHttp(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, HttpClient refHttpClientSubscribe, HttpClient refHttpClientNonsubscribe)
#else
        public PubnubHttp(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log, EndPoint.TelemetryManager telemetryManager)
#endif
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
            httpClientSubscribe = refHttpClientSubscribe;
            httpClientNonsubscribe = refHttpClientNonsubscribe;
#endif
        }

        HttpWebRequest IPubnubHttp.SetProxy<T>(HttpWebRequest request)
        {
#if !NETSTANDARD10
            if (pubnubConfig.Proxy != null)
            {
                request.Proxy = pubnubConfig.Proxy;
            }
#endif
            return request;
        }

        HttpWebRequest IPubnubHttp.SetTimeout<T>(RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
#if NET35 || NET40 || NET45 || NET461 || NET48
            request.Timeout = GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000;
#endif
            return request;
        }

        HttpWebRequest IPubnubHttp.SetNoCache<T>(HttpWebRequest request)
        {
            request.Headers["Cache-Control"] = "no-cache";
            request.Headers["Pragma"] = "no-cache";

            return request;
        }

        HttpWebRequest IPubnubHttp.SetServicePointConnectionLimit<T>(RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
#if NET35 || NET40 || NET45 || NET461 || NET48
            if (pubnubRequestState.ResponseType == PNOperationType.PNHeartbeatOperation)
            {
                int estimateConnectionLimit = pubnubConfig.SubscribeTimeout/pubnubConfig.PresenceInterval;
                if (estimateConnectionLimit > request.ServicePoint.ConnectionLimit)
                {
                    request.ServicePoint.ConnectionLimit = estimateConnectionLimit;
                }
            }
#endif
            return request;
        }

        HttpWebRequest IPubnubHttp.SetServicePointSetTcpKeepAlive<T>(RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
#if NET35 || NET40 || NET45 || NET461 || NET48
            if (pubnubConfig.PresenceInterval > 0)
            {
                request.ServicePoint.SetTcpKeepAlive(true, pubnubConfig.PresenceInterval * 1000, 1000);
            }
#endif
            return request;
        }

         HttpWebRequest IPubnubHttp.SetTcpKeepAlive(HttpWebRequest request)
        {
#if NET35 || NET40 || NET45 || NET461 || NET48
            request.KeepAlive = true;
#endif
            return request;
        }

        async Task<string> IPubnubHttp.SendRequestAndGetJsonResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            if (pubnubConfig.UseClassicHttpWebRequest)
            {
                return await SendRequestAndGetJsonResponseClassicHttp(requestUri, pubnubRequestState, request).ConfigureAwait(false);
            }
            else
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                if (pubnubConfig.UseTaskFactoryAsyncInsteadOfHttpClient)
                {
                    return await SendRequestAndGetJsonResponseTaskFactory(pubnubRequestState, request).ConfigureAwait(false);
                }
                else
                {
                    return await SendRequestAndGetJsonResponseHttpClient(requestUri, pubnubRequestState, request).ConfigureAwait(false);
                }
#else
                return await SendRequestAndGetJsonResponseTaskFactory(pubnubRequestState, request).ConfigureAwait(false);
#endif
            }

        }

        async Task<byte[]> IPubnubHttp.SendRequestAndGetStreamResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            if (pubnubConfig.UseClassicHttpWebRequest)
            {
                return await SendRequestAndGetStreamResponseClassicHttp(pubnubRequestState, request).ConfigureAwait(false);
            }
            else
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                if (pubnubConfig.UseTaskFactoryAsyncInsteadOfHttpClient)
                {
                    return await SendRequestAndGetStreamResponseTaskFactory(pubnubRequestState, request).ConfigureAwait(false);
                }
                else
                {
                    return await SendRequestAndGetStreamResponseHttpClient(requestUri, pubnubRequestState).ConfigureAwait(false);
                }
#else
                return await SendRequestAndGetStreamResponseTaskFactory(pubnubRequestState, request).ConfigureAwait(false);
#endif
            }

        }

        async Task<string> IPubnubHttp.SendRequestAndGetJsonResponseWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] postData, string contentType)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, postData bytearray len= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), postData.Length), pubnubConfig.LogVerbosity);
            if (pubnubConfig.UseClassicHttpWebRequest)
            {
                return await SendRequestAndGetJsonResponseClassicHttpWithPOST(pubnubRequestState, request, postData, contentType).ConfigureAwait(false);
            }
            else
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                if (pubnubConfig.UseTaskFactoryAsyncInsteadOfHttpClient)
                {
                    return await SendRequestAndGetJsonResponseTaskFactoryWithPOST(pubnubRequestState, request, postData, contentType).ConfigureAwait(false);
                }
                else
                {
                    return await SendRequestAndGetJsonResponseHttpClientWithPOST(requestUri, pubnubRequestState, postData, contentType).ConfigureAwait(false);
                }
#else
                return await SendRequestAndGetJsonResponseTaskFactoryWithPOST(pubnubRequestState, request, postData, contentType).ConfigureAwait(false);
#endif
            }
        }

        async Task<string> IPubnubHttp.SendRequestAndGetJsonResponseWithPATCH<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] patchData, string contentType)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, patchData = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), Encoding.UTF8.GetString(patchData, 0, patchData.Length)), pubnubConfig.LogVerbosity);
            if (pubnubConfig.UseClassicHttpWebRequest)
            {
                return await SendRequestAndGetJsonResponseClassicHttpWithPATCH(pubnubRequestState, request, patchData).ConfigureAwait(false);
            }
            else
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                if (pubnubConfig.UseTaskFactoryAsyncInsteadOfHttpClient)
                {
                    return await SendRequestAndGetJsonResponseTaskFactoryWithPATCH(pubnubRequestState, request, patchData, contentType).ConfigureAwait(false);
                }
                else
                {
                    return await SendRequestAndGetJsonResponseHttpClientWithPATCH(requestUri, pubnubRequestState, patchData, contentType).ConfigureAwait(false);
                }
#else
                return await SendRequestAndGetJsonResponseTaskFactoryWithPATCH(pubnubRequestState, request, patchData, contentType).ConfigureAwait(false);
#endif
            }
        }

#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
        async Task<string> SendRequestAndGetJsonResponseHttpClient<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            string jsonString = "";
            HttpResponseMessage response = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetJsonResponseHttpClient", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
                cts.CancelAfter(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    response = await httpClientSubscribe.GetAsync(requestUri, cts.Token).ConfigureAwait(false);
                }
                else if (string.Compare(FindHttpGetOrDeleteMethod(pubnubRequestState), "DELETE", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    response = await httpClientNonsubscribe.DeleteAsync(requestUri, cts.Token).ConfigureAwait(false);
                }
                else
                {
                    response = await httpClientNonsubscribe.GetAsync(requestUri, cts.Token).ConfigureAwait(false);
                }
                if (response.IsSuccessStatusCode || response.Content != null)
                {
                    var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    stopWatch.Stop();
                    if (pubnubTelemetryMgr != null)
                    {
                        await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                    }
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                        pubnubRequestState.GotJsonResponse = true;
                    }
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }
                else
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, No HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }

            }
            catch (HttpRequestException httpReqEx)
            {
                if (httpReqEx.InnerException is WebException)
                {
                    WebException currentWebException = httpReqEx.InnerException as WebException;
                    if (currentWebException != null)
                    {
                        if (currentWebException.Response != null)
                        {
                            pubnubRequestState.Response = currentWebException.Response as HttpWebResponse;
                            using (StreamReader streamReader = new StreamReader(currentWebException.Response.GetResponseStream()))
                            {
                                //Need to return this response 
                                jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON from HttpClient WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                return jsonString;
                            }
                        }
                    }
                    
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClient InnerException WebException status {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ((WebException)httpReqEx.InnerException).Status.ToString()), pubnubConfig.LogVerbosity);
                    throw httpReqEx.InnerException;
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClient HttpRequestException {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), httpReqEx.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClient Exception {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            finally
            {
                if (response != null && response.Content != null)
                {
                    response.Content.Dispose();
                    pubnubRequestState.Response = null;
                    pubnubRequestState.Request = null;
                }
            }
            return jsonString;
        }

        async Task<byte[]> SendRequestAndGetStreamResponseHttpClient<T>(Uri requestUri, RequestState<T> pubnubRequestState)
        {
            byte[] streamBytes = null;
            HttpResponseMessage response = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetStreamResponseHttpClient", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
                cts.CancelAfter(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                response = await httpClientNonsubscribe.GetAsync(requestUri, cts.Token).ConfigureAwait(false);
                if (response.IsSuccessStatusCode || response.Content != null)
                {
                    var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    stopWatch.Stop();
                    if (pubnubTelemetryMgr != null)
                    {
                        await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                    }
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        streamBytes = ms.ToArray();
                    }
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }
                else
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, No HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }

            }
            catch (HttpRequestException httpReqEx)
            {
                if (httpReqEx.InnerException is WebException)
                {
                    WebException currentWebException = httpReqEx.InnerException as WebException;
                    if (currentWebException != null)
                    {
                        if (currentWebException.Response != null)
                        {
                            pubnubRequestState.Response = currentWebException.Response as HttpWebResponse;
                            var errorStream = currentWebException.Response.GetResponseStream();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                errorStream.CopyTo(ms);
                                streamBytes = ms.ToArray();
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved Stream Bytes from HttpClient WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            }
                        }
                    }

                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetStreamResponseHttpClient InnerException WebException status {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ((WebException)httpReqEx.InnerException).Status.ToString()), pubnubConfig.LogVerbosity);
                    throw httpReqEx.InnerException;
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetStreamResponseHttpClient HttpRequestException {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), httpReqEx.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetStreamResponseHttpClient Exception {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            finally
            {
                if (response != null && response.Content != null)
                {
                    response.Content.Dispose();
                    pubnubRequestState.Response = null;
                    pubnubRequestState.Request = null;
                }
            }
            return streamBytes;
        }

        async Task<string> SendRequestAndGetJsonResponseHttpClientWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, byte[] postData, string contentType)
        {
            string jsonString = "";
            HttpResponseMessage response = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, SendRequestAndGetJsonResponseHttpClientPOST Before httpClient.GetAsync", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                cts.CancelAfter(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                ByteArrayContent postDataContent = new ByteArrayContent(postData);
                postDataContent.Headers.Remove("Content-Type");
                if (string.IsNullOrEmpty(contentType))
                {
                    postDataContent.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                }
                else
                {
                    postDataContent.Headers.TryAddWithoutValidation("Content-Type", contentType);
                }
                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    response = await httpClientSubscribe.PostAsync(requestUri, postDataContent, cts.Token).ConfigureAwait(false);
                }
                else
                {
                    response = await httpClientNonsubscribe.PostAsync(requestUri, postDataContent, cts.Token).ConfigureAwait(false);
                }

                if (response.IsSuccessStatusCode || response.Content != null)
                {
                    stopWatch.Stop();
                    if (pubnubTelemetryMgr != null)
                    {
                        await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                    }
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got POST HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                    if ((int)response.StatusCode == 204 && pubnubRequestState.ResponseType == PNOperationType.PNFileUploadOperation)
                    {
                        return "{}";
                    }
                    else
                    {
                        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        using (StreamReader streamReader = new StreamReader(stream))
                        {
                            jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                            pubnubRequestState.GotJsonResponse = true;
                        }
                    }
                }
                else
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, No POST HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }

            }
            catch (HttpRequestException httpReqEx)
            {
                if (httpReqEx.InnerException is WebException)
                {
                    WebException currentWebException = httpReqEx.InnerException as WebException;
                    if (currentWebException != null)
                    {
                        if (currentWebException.Response != null)
                        {
                            pubnubRequestState.Response = currentWebException.Response as HttpWebResponse;
                            using (StreamReader streamReader = new StreamReader(currentWebException.Response.GetResponseStream()))
                            {
                                //Need to return this response 
                                jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON from HttpClient POST WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                return jsonString;
                            }
                        }
                    }

                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST InnerException WebException status {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ((WebException)httpReqEx.InnerException).Status.ToString()), pubnubConfig.LogVerbosity);
                    throw httpReqEx.InnerException;
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST HttpRequestException {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), httpReqEx.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST Exception {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            finally
            {
                if (response != null && response.Content != null)
                {
                    response.Content.Dispose();
                    pubnubRequestState.Response = null;
                    pubnubRequestState.Request = null;
                }
            }
            return jsonString;
        }

        async Task<string> SendRequestAndGetJsonResponseHttpClientWithPATCH<T>(Uri requestUri, RequestState<T> pubnubRequestState, byte[] patchData, string contentType)
        {
            string jsonString = "";
            HttpResponseMessage response = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, SendRequestAndGetJsonResponseHttpClientWithPATCH Before httpClient.SendAsync", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                cts.CancelAfter(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                HttpMethod httpMethod = new HttpMethod("PATCH");
                ByteArrayContent patchDataContent = new ByteArrayContent(patchData);
                patchDataContent.Headers.Remove("Content-Type");
                if (string.IsNullOrEmpty(contentType))
                {
                    patchDataContent.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                }
                else
                {
                    patchDataContent.Headers.TryAddWithoutValidation("Content-Type", contentType);
                }

                HttpRequestMessage requestMsg = new HttpRequestMessage(httpMethod, requestUri)
                {
                    Content = patchDataContent
                };
                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    response = await httpClientSubscribe.SendAsync(requestMsg, cts.Token).ConfigureAwait(false);
                }
                else
                {
                    response = await httpClientNonsubscribe.SendAsync(requestMsg, cts.Token).ConfigureAwait(false);
                }

                if (response.IsSuccessStatusCode || response.Content != null)
                {
                    var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    stopWatch.Stop();
                    if (pubnubTelemetryMgr != null)
                    {
                        await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                    }
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                        pubnubRequestState.GotJsonResponse = true;
                    }
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got POST HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }
                else
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, No POST HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }

            }
            catch (HttpRequestException httpReqEx)
            {
                if (httpReqEx.InnerException is WebException)
                {
                    WebException currentWebException = httpReqEx.InnerException as WebException;
                    if (currentWebException != null)
                    {
                        if (currentWebException.Response != null)
                        {
                            pubnubRequestState.Response = currentWebException.Response as HttpWebResponse;
                            using (StreamReader streamReader = new StreamReader(currentWebException.Response.GetResponseStream()))
                            {
                                //Need to return this response 
                                jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON from HttpClient POST WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                return jsonString;
                            }
                        }
                    }

                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST InnerException WebException status {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ((WebException)httpReqEx.InnerException).Status.ToString()), pubnubConfig.LogVerbosity);
                    throw httpReqEx.InnerException;
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST HttpRequestException {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), httpReqEx.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST Exception {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            finally
            {
                if (response != null && response.Content != null)
                {
                    response.Content.Dispose();
                    pubnubRequestState.Response = null;
                    pubnubRequestState.Request = null;
                }
            }
            return jsonString;
        }
#endif

        async Task<string> SendRequestAndGetJsonResponseTaskFactory<T>(RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            HttpWebResponse response = null;
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetJsonResponseTaskFactory", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            try
            {
                request.Method = FindHttpGetOrDeleteMethod(pubnubRequestState);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                var _ = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                response = await Task.Factory.FromAsync<HttpWebResponse>(request.BeginGetResponse, asyncPubnubResult => (HttpWebResponse)request.EndGetResponse(asyncPubnubResult), pubnubRequestState).ConfigureAwait(false);
                stopWatch.Stop();
                if (pubnubConfig.EnableTelemetry && pubnubTelemetryMgr != null)
                {
                    await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                }
                if (response != null) 
                { 
                    pubnubRequestState.Response = response;
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got PubnubWebResponse for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), request.RequestUri.ToString()));
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        //Need to return this response 
    #if NET35 || NET40
                        string jsonString = streamReader.ReadToEnd();
    #else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
    #endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        pubnubRequestState.GotJsonResponse = true;
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, SendRequestAndGetJsonResponseTaskFactory => Retrieved JSON", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

                        if (pubnubRequestState.Response != null)
                        {
    #if NET35 || NET40 || NET45 || NET461 || NET48
                            pubnubRequestState.Response.Close();
    #endif
                            pubnubRequestState.Response = null;
                            pubnubRequestState.Request = null;
                        }

                        return jsonString;
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    pubnubRequestState.Response = ex.Response as HttpWebResponse;
                    using (StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    throw;
                }
                return "";
            }
            catch
            {
                throw;
            }
        }

        async Task<byte[]> SendRequestAndGetStreamResponseTaskFactory<T>(RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            HttpWebResponse response = null;
            byte[] streamBytes;
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetStreamResponseTaskFactory", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            try
            {
                request.Method = FindHttpGetOrDeleteMethod(pubnubRequestState);
                var _ = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                response = await Task.Factory.FromAsync<HttpWebResponse>(request.BeginGetResponse, asyncPubnubResult => (HttpWebResponse)request.EndGetResponse(asyncPubnubResult), pubnubRequestState).ConfigureAwait(false);
                stopWatch.Stop();
                if (pubnubConfig.EnableTelemetry && pubnubTelemetryMgr != null)
                {
                    await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                }
                pubnubRequestState.Response = response;
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got PubnubWebResponse for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), request.RequestUri.ToString()));
                int statusCode = (int)pubnubRequestState.Response.StatusCode;
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, status code = {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), statusCode));
                using (Stream stream = response.GetResponseStream())
                {
                    long totalSize = 0;
                    long receivedSize = 0;
                    //Allocate 1K buffer
                    byte[] buffer = new byte[1024];
                    using(MemoryStream ms = new MemoryStream())
                    {
#if NET35 || NET40
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
#else
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
#endif
                        receivedSize += bytesRead;
                        while (bytesRead > 0)
                        {
                            ms.Write(buffer, 0, bytesRead);
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            receivedSize += bytesRead;
                        }
                        streamBytes = ms.ToArray();
                    }
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, totalsize = {1}; received = {2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), totalSize, receivedSize));
                    //Need to return this response 
                    pubnubRequestState.GotJsonResponse = true;
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved Stream", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

                    if (pubnubRequestState.Response != null)
                    {
#if NET35 || NET40 || NET45 || NET461 || NET48
                        pubnubRequestState.Response.Close();
#endif
                        pubnubRequestState.Response = null;
                        pubnubRequestState.Request = null;
                    }

                    return streamBytes;
                }
            }
            catch (WebException ex)
            {
                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    throw;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Exception in SendRequestAndGetStreamResponseTaskFactory {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex));
                throw;
            }
        }

        async Task<string> SendRequestAndGetJsonResponseTaskFactoryWithPOST<T>(RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] postData, string contentType)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before Task.Factory.FromAsync With POST", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            try
            {
                request.Method = "POST";
                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                request.ContentType = contentType;

                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState).ConfigureAwait(false))
                {
#if NET35 || NET40
                    requestStream.Write(postData, 0, postData.Length);
                    requestStream.Flush();
#else
                    await requestStream.WriteAsync(postData, 0, postData.Length).ConfigureAwait(false);
                    await requestStream.FlushAsync().ConfigureAwait(false);
#endif

                }

                WebResponse response = await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, pubnubRequestState).ConfigureAwait(false);
                stopWatch.Stop();
                if (pubnubTelemetryMgr != null)
                {
                    await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                }
                pubnubRequestState.Response = response as HttpWebResponse;
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got PubnubWebResponse With POST for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), request.RequestUri.ToString()));
                int statusCode = (int)pubnubRequestState.Response.StatusCode;
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, statusCode {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), statusCode));
                if (statusCode == 204 && pubnubRequestState.ResponseType == PNOperationType.PNFileUploadOperation)
                {
                    return "{}";
                }
                else
                {
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                            string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON With POST", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        pubnubRequestState.GotJsonResponse = true;

                        if (pubnubRequestState.Response != null)
                        {
#if NET35 || NET40 || NET45 || NET461 || NET48
                            pubnubRequestState.Response.Close();
#endif
                            pubnubRequestState.Response = null;
                            pubnubRequestState.Request = null;
                        }

                        return jsonString;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    pubnubRequestState.Response = ex.Response as HttpWebResponse;
                    using (StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON  With POST from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    throw;
                }
                return "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Exception in SendRequestAndGetJsonResponseTaskFactoryWithPOST {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex));
                throw;
            }
        }

        async Task<string> SendRequestAndGetJsonResponseTaskFactoryWithPATCH<T>(RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] patchData, string contentType)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before Task.Factory.FromAsync With PATCH", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            try
            {
                request.Method = "PATCH";
                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                request.ContentType = "application/json";

                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState).ConfigureAwait(false))
                {
#if NET35 || NET40
                    requestStream.Write(patchData, 0, patchData.Length);
                    requestStream.Flush();
#else
                    await requestStream.WriteAsync(patchData, 0, patchData.Length).ConfigureAwait(false);
                    await requestStream.FlushAsync().ConfigureAwait(false);
#endif

                }

                WebResponse response = await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, pubnubRequestState).ConfigureAwait(false);
                stopWatch.Stop();
                if (pubnubTelemetryMgr != null)
                {
                    await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                }
                pubnubRequestState.Response = response as HttpWebResponse;
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Got PubnubWebResponse With PATCH for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), request.RequestUri.ToString()));
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    //Need to return this response 
#if NET35 || NET40
                    string jsonString = streamReader.ReadToEnd();
#else
                    string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                    System.Diagnostics.Debug.WriteLine(jsonString);
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON With PATCH", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                    pubnubRequestState.GotJsonResponse = true;

                    if (pubnubRequestState.Response != null)
                    {
#if NET35 || NET40 || NET45 || NET461 || NET48
                        pubnubRequestState.Response.Close();
#endif
                        pubnubRequestState.Response = null;
                        pubnubRequestState.Request = null;
                    }

                    return jsonString;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    pubnubRequestState.Response = ex.Response as HttpWebResponse;
                    using (StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON  With PATCH from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    throw;
                }
                return "";
            }
            catch
            {
                throw;
            }
        }

        async Task<string> SendRequestAndGetJsonResponseClassicHttp<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetJsonResponseClassicHttp", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<string>();
            try
            {
                request.Method = FindHttpGetOrDeleteMethod<T>(pubnubRequestState);
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before BeginGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                request.BeginGetResponse(new AsyncCallback(
                    async (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before EndGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, After EndGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Inside StreamReader", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, SendRequestAndGetJsonResponseClassicHttp => Retrieved JSON", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                taskComplete.TrySetResult(jsonString);
                            }
                            if (asyncRequestState.Response != null)
                            {
#if NET35 || NET40 || NET45 || NET461 || NET48
                                pubnubRequestState.Response.Close();
#endif
                                asyncRequestState.Response = null;
                                asyncRequestState.Request = null;
                            }
                        }
                    }
                    ), pubnubRequestState);

                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                return taskComplete.Task.Result;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    pubnubRequestState.Response = ex.Response as HttpWebResponse;
                    using (StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                        await Task.Factory.StartNew(() => { }).ConfigureAwait(false);
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }
                
                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    taskComplete.TrySetException(ex);
                }
                return "";
            }
            catch (Exception ex)
            {
                taskComplete.TrySetException(ex);
                return "";
            }
        }

        async Task<byte[]> SendRequestAndGetStreamResponseClassicHttp<T>(RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetStreamResponseClassicHttp", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<byte[]>();
            try
            {
                request.Method = FindHttpGetOrDeleteMethod<T>(pubnubRequestState);
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before BeginGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                request.BeginGetResponse(new AsyncCallback(
                    async (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before EndGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, After EndGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Inside StreamReader", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, SendRequestAndGetStreamResponseClassicHttp => Retrieved JSON", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                taskComplete.TrySetResult(null);
                            }
                            if (asyncRequestState.Response != null)
                            {
#if NET35 || NET40 || NET45 || NET461 || NET48
                                pubnubRequestState.Response.Close();
#endif
                                asyncRequestState.Response = null;
                                asyncRequestState.Request = null;
                            }
                        }
                    }
                    ), pubnubRequestState);

                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                return taskComplete.Task.Result;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    pubnubRequestState.Response = ex.Response as HttpWebResponse;
                    using (StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                        await Task.Factory.StartNew(() => { }).ConfigureAwait(false);
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return null;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    taskComplete.TrySetException(ex);
                }
                return null;
            }
            catch (Exception ex)
            {
                taskComplete.TrySetException(ex);
                return null;
            }
        }

        async Task<string> SendRequestAndGetJsonResponseClassicHttpWithPOST<T>(RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] postData, string contentType)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetJsonResponseClassicHttpWithPOST", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<string>();
            try
            {
                request.Method = "POST";
                request.ContentType = contentType;

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48
                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState).ConfigureAwait(false))
                {
                    requestStream.Write(postData, 0, postData.Length);
                    requestStream.Flush();
                }
#else
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postData, 0, postData.Length);
                    requestStream.Flush();
                }
#endif

                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(
                    async (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before EndGetResponse With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, After EndGetResponse With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Inside StreamReader With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                taskComplete.TrySetResult(jsonString);
                            }
                            if (asyncRequestState.Response != null)
                            {
#if NET35 || NET40 || NET45 || NET461 || NET48
                                pubnubRequestState.Response.Close();
#endif
                                asyncRequestState.Response = null;
                                asyncRequestState.Request = null;
                            }

                        }
                    }
                    ), pubnubRequestState);

                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                return taskComplete.Task.Result;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    pubnubRequestState.Response = ex.Response as HttpWebResponse;
                    using (StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                        await Task.Factory.StartNew(() => { }).ConfigureAwait(false);
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON  With POST from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    taskComplete.TrySetException(ex);
                }
                return "";
            }
            catch (Exception ex)
            {
                taskComplete.TrySetException(ex);
                return "";
            }
        }

        async Task<string> SendRequestAndGetJsonResponseClassicHttpWithPATCH<T>(RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] patchData)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, Inside SendRequestAndGetJsonResponseClassicHttpWithPATCH", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<string>();
            try
            {
                request.Method = "PATCH";
                request.ContentType = "application/json";

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48
                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState).ConfigureAwait(false))
                {
                    requestStream.Write(patchData, 0, patchData.Length);
                    requestStream.Flush();
                }
#else
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(patchData, 0, patchData.Length);
                    requestStream.Flush();
                }
#endif

                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(
                    async (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Before EndGetResponse With PATCH ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType).ConfigureAwait(false);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, After EndGetResponse With PATCH ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Inside StreamReader With PATCH ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON With PATCH ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                taskComplete.TrySetResult(jsonString);
                            }
                            if (asyncRequestState.Response != null)
                            {
#if NET35 || NET40 || NET45 || NET461 || NET48
                                pubnubRequestState.Response.Close();
#endif
                                asyncRequestState.Response = null;
                                asyncRequestState.Request = null;
                            }

                        }
                    }
                    ), pubnubRequestState);

                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                return taskComplete.Task.Result;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    pubnubRequestState.Response = ex.Response as HttpWebResponse;
                    using (StreamReader streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        //Need to return this response 
#if NET35 || NET40
                        await Task.Factory.StartNew(() => { }).ConfigureAwait(false);
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Retrieved JSON  With PATCH from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled", StringComparison.OrdinalIgnoreCase) == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    taskComplete.TrySetException(ex);
                }
                return "";
            }
            catch (Exception ex)
            {
                taskComplete.TrySetException(ex);
                return "";
            }
        }

        protected void OnPubnubWebRequestTimeout<T>(object state, bool timeout)
        {
            if (timeout && state != null)
            {
                RequestState<T> currentState = state as RequestState<T>;
                if (currentState != null)
                {
                    HttpWebRequest request = currentState.Request;
                    if (request != null)
                    {
                        string currentMultiChannel = (currentState.Channels == null) ? "" : string.Join(",", currentState.Channels.OrderBy(x => x).ToArray());
                        string currentMultiChannelGroup = (currentState.ChannelGroups == null) ? "" : string.Join(",", currentState.ChannelGroups.OrderBy(x => x).ToArray());
                        LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached.Request abort for channel={1} ;channelgroup={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), currentMultiChannel, currentMultiChannelGroup), pubnubConfig.LogVerbosity);
                        currentState.Timeout = true;
                        try
                        {
                            request.Abort();
                        }
                        catch {  /* ignore */ }
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached. However state is unknown", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
                }
            }
        }

        protected void OnPubnubWebRequestTimeout<T>(System.Object requestState)
        {
            RequestState<T> currentState = requestState as RequestState<T>;
            if (currentState != null && currentState.Response == null && currentState.Request != null)
            {
                currentState.Timeout = true;
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime: {0}, **WP7 OnPubnubWebRequestTimeout** Initiated at {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), currentState.TimeQueued.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);

                try
                {
                    currentState.Request.Abort();
                }
                catch {  /* ignore */ }

                if (currentState.ResponseType != PNOperationType.PNSubscribeOperation 
                    && currentState.ResponseType != PNOperationType.Presence
                    && currentState.ResponseType != PNOperationType.PNHeartbeatOperation
                    && currentState.ResponseType != PNOperationType.Leave)
                {
                    PNStatusCategory errorCategory = PNStatusCategory.PNTimeoutCategory;
                    PNStatus status = new StatusBuilder(pubnubConfig, jsonLib).CreateStatusResponse<T>(currentState.ResponseType, errorCategory, currentState, (int)HttpStatusCode.NotFound, new PNException("Request timeout"));

                    if (currentState.Channels != null && currentState.Channels.Length > 0)
                    {
                        status.AffectedChannels.AddRange(currentState.Channels);
                    }

                    if (currentState.ChannelGroups != null && currentState.ChannelGroups.Length > 0)
                    {
                        status.AffectedChannels.AddRange(currentState.ChannelGroups);
                    }

                    if (currentState.PubnubCallback != null)
                    {
                        currentState.PubnubCallback.OnResponse(default(T), status);
                    }
                }
            }
        }

        protected int GetTimeoutInSecondsForResponseType(PNOperationType type)
        {
            int timeout;
            if (type == PNOperationType.PNSubscribeOperation || type == PNOperationType.Presence)
            {
                timeout = pubnubConfig.SubscribeTimeout;
            }
            else if (type == PNOperationType.PNGenerateFileUploadUrlOperation)
            {
                timeout = pubnubConfig.NonSubscribeRequestTimeout*3;
            }
            else if (type == PNOperationType.PNFileUploadOperation || type == PNOperationType.PNDownloadFileOperation)
            {
                timeout = pubnubConfig.NonSubscribeRequestTimeout * 25;
            }
            else
            {
                timeout = pubnubConfig.NonSubscribeRequestTimeout;
            }
            return timeout;
        }

        private static string FindHttpGetOrDeleteMethod<T>(RequestState<T> pubnubRequestState)
        {
            return (pubnubRequestState != null && (pubnubRequestState.ResponseType == PNOperationType.PNDeleteMessageOperation
                                                || pubnubRequestState.ResponseType == PNOperationType.PNDeleteUuidMetadataOperation
                                                || pubnubRequestState.ResponseType == PNOperationType.PNDeleteChannelMetadataOperation
                                                || pubnubRequestState.ResponseType == PNOperationType.PNRemoveMessageActionOperation
                                                || pubnubRequestState.ResponseType == PNOperationType.PNAccessManagerRevokeToken
                                                || pubnubRequestState.ResponseType == PNOperationType.PNDeleteFileOperation)) ? "DELETE" : "GET";

        }
    }
}
