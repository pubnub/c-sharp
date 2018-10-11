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
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
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
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        private static HttpClient httpClientSubscribe;
        private static HttpClient httpClientNonsubscribe;
#endif

#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        public PubnubHttp(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, HttpClient refHttpClientSubscribe, HttpClient refHttpClientNonsubscribe)
#else
        public PubnubHttp(PNConfiguration config, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubLog log, EndPoint.TelemetryManager telemetryManager)
#endif
        {
            pubnubConfig = config;
            jsonLib = jsonPluggableLibrary;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
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
#if NET35 || NET40 || NET45 || NET461
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


        HttpWebRequest IPubnubHttp.SetServicePointSetTcpKeepAlive(HttpWebRequest request)
        {
            //do nothing
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
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
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

        async Task<string> IPubnubHttp.SendRequestAndGetJsonResponseWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, string postData)
        {
            if (pubnubConfig.UseClassicHttpWebRequest)
            {
                return await SendRequestAndGetJsonResponseClassicHttpWithPOST(requestUri, pubnubRequestState, request, postData).ConfigureAwait(false);
            }
            else
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
                if (pubnubConfig.UseTaskFactoryAsyncInsteadOfHttpClient)
                {
                    return await SendRequestAndGetJsonResponseTaskFactoryWithPOST(pubnubRequestState, request, postData).ConfigureAwait(false);
                }
                else
                {
                    return await SendRequestAndGetJsonResponseHttpClientWithPOST(requestUri, pubnubRequestState, request, postData).ConfigureAwait(false);
                }
#else
                return await SendRequestAndGetJsonResponseTaskFactoryWithPOST(pubnubRequestState, request, postData).ConfigureAwait(false);
#endif
            }
        }

#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        async Task<string> SendRequestAndGetJsonResponseHttpClient<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            string jsonString = "";
            HttpResponseMessage response = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseHttpClient", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
                cts.CancelAfter(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    response = await httpClientSubscribe.GetAsync(requestUri, cts.Token).ConfigureAwait(false);
                }
                else if (pubnubRequestState.ResponseType == PNOperationType.PNDeleteMessageOperation)
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
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }
                else
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, No HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
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
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON from HttpClient WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                return jsonString;
                            }
                        }
                    }
                    
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, SendRequestAndGetJsonResponseHttpClient InnerException WebException status {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ((WebException)httpReqEx.InnerException).Status.ToString()), pubnubConfig.LogVerbosity);
                    throw httpReqEx.InnerException;
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, SendRequestAndGetJsonResponseHttpClient HttpRequestException {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), httpReqEx.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, SendRequestAndGetJsonResponseHttpClient Exception {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
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

        async Task<string> SendRequestAndGetJsonResponseHttpClientWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, string postData)
        {
            string jsonString = "";
            HttpResponseMessage response = null;
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, SendRequestAndGetJsonResponseHttpClientPOST Before httpClient.GetAsync", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                cts.CancelAfter(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                StringContent jsonPostString = new StringContent(postData, Encoding.UTF8);
                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    response = await httpClientSubscribe.PostAsync(requestUri, jsonPostString, cts.Token).ConfigureAwait(false);
                }
                else
                {
                    response = await httpClientNonsubscribe.PostAsync(requestUri, jsonPostString, cts.Token).ConfigureAwait(false);
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
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got POST HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
                }
                else
                {
                    stopWatch.Stop();
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, No POST HttpResponseMessage for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri));
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
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON from HttpClient POST WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                return jsonString;
                            }
                        }
                    }

                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST InnerException WebException status {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ((WebException)httpReqEx.InnerException).Status.ToString()), pubnubConfig.LogVerbosity);
                    throw httpReqEx.InnerException;
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST HttpRequestException {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), httpReqEx.Message), pubnubConfig.LogVerbosity);
                throw;
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, SendRequestAndGetJsonResponseHttpClientPOST Exception {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.Message), pubnubConfig.LogVerbosity);
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
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseTaskFactory", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            try
            {
                request.Method = (pubnubRequestState != null && pubnubRequestState.ResponseType == PNOperationType.PNDeleteMessageOperation) ? "DELETE" : "GET";
                new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                response = await Task.Factory.FromAsync<HttpWebResponse>(request.BeginGetResponse, asyncPubnubResult => (HttpWebResponse)request.EndGetResponse(asyncPubnubResult), pubnubRequestState).ConfigureAwait(false);
                stopWatch.Stop();
                if (pubnubConfig.EnableTelemetry && pubnubTelemetryMgr != null)
                {
                    await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                }
                pubnubRequestState.Response = response;
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got PubnubWebResponse for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), request.RequestUri.ToString()));
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
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

                    if (pubnubRequestState.Response != null)
                    {
#if NET35 || NET40 || NET45 || NET461
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
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
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

        async Task<string> SendRequestAndGetJsonResponseTaskFactoryWithPOST<T>(RequestState<T> pubnubRequestState, HttpWebRequest request, string postData)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before Task.Factory.FromAsync With POST", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            try
            {
                request.Method = "POST";
                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                request.ContentType = "application/json";

                byte[] data = Encoding.UTF8.GetBytes(postData);
                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState).ConfigureAwait(false))
                {
#if NET35 || NET40
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Flush();
#else
                    await requestStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                    await requestStream.FlushAsync().ConfigureAwait(false);
#endif

                }

                WebResponse response = await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, pubnubRequestState).ConfigureAwait(false);
                stopWatch.Stop();
                if (pubnubTelemetryMgr != null)
                {
                    await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                }
                pubnubRequestState.Response = response as HttpWebResponse;
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got PubnubWebResponse With POST for {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), request.RequestUri.ToString()));
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
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON With POST", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                    pubnubRequestState.GotJsonResponse = true;

                    if (pubnubRequestState.Response != null)
                    {
#if NET35 || NET40 || NET45 || NET461
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
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON  With POST from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
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
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseClassicHttp", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<string>();
            try
            {
                request.Method = (pubnubRequestState != null && pubnubRequestState.ResponseType == PNOperationType.PNDeleteMessageOperation) ? "DELETE" : "GET";
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before BeginGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                request.BeginGetResponse(new AsyncCallback(
                    async (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before EndGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, After EndGetResponse", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Inside StreamReader", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                taskComplete.TrySetResult(jsonString);
                            }
                            if (asyncRequestState.Response != null)
                            {
#if NET35 || NET40 || NET45 || NET461
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
                        await Task.Factory.StartNew(() => { });
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }
                
                if (ex.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
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

        async Task<string> SendRequestAndGetJsonResponseClassicHttpWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, string postData)
        {
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseClassicHttpWithPOST", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<string>();
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";

                byte[] data = Encoding.UTF8.GetBytes(postData);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
#if !NET35 && !NET40 && !NET45 && !NET461
                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState).ConfigureAwait(false))
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Flush();
                }
#else
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Flush();
                }
#endif

                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(
                    async (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before EndGetResponse With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                await pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, After EndGetResponse With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Inside StreamReader With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON With POST ", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                                taskComplete.TrySetResult(jsonString);
                            }
                            if (asyncRequestState.Response != null)
                            {
#if NET35 || NET40 || NET45 || NET461
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
                        await Task.Factory.StartNew(() => { });
                        string jsonString = streamReader.ReadToEnd();
#else
                        string jsonString = await streamReader.ReadToEndAsync().ConfigureAwait(false);
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON  With POST from WebException response", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
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
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached.Request abort for channel={1} ;channelgroup={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), currentMultiChannel, currentMultiChannelGroup), pubnubConfig.LogVerbosity);
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
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached. However state is unknown", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);
                }
            }
        }

        protected void OnPubnubWebRequestTimeout<T>(System.Object requestState)
        {
            RequestState<T> currentState = requestState as RequestState<T>;
            if (currentState != null && currentState.Response == null && currentState.Request != null)
            {
                currentState.Timeout = true;
                try
                {
                    currentState.Request.Abort();
                }
                catch {  /* ignore */ }

                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, **WP7 OnPubnubWebRequestTimeout**", DateTime.Now.ToString(CultureInfo.InvariantCulture)), pubnubConfig.LogVerbosity);

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
            else
            {
                timeout = pubnubConfig.NonSubscribeRequestTimeout;
            }
            return timeout;
        }


    }
}
