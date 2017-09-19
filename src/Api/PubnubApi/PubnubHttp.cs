using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading.Tasks;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace PubnubApi
{
    public class PubnubHttp : IPubnubHttp
    {
        private PNConfiguration pubnubConfig = null;
        private IJsonPluggableLibrary jsonLib = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.TelemetryManager pubnubTelemetryMgr = null;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        private static HttpClient httpClientSubscribe = null;
        private static HttpClient httpClientNonsubscribe = null;
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
            //if (httpClient == null)
            //{
            //    httpClient = new HttpClient();
            //    httpClient.DefaultRequestHeaders.Accept.Clear();
            //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //}
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
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
            //request.ServicePoint.SetTcpKeepAlive(true, base.LocalClientHeartbeatInterval * 1000, 1000);
#endif
            //do nothing for mono
            return request;
        }

        async Task<string> IPubnubHttp.SendRequestAndGetJsonResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            if (pubnubConfig.UseClassicHttpWebRequest)
            {
                return await SendRequestAndGetJsonResponseClassicHttp(requestUri, pubnubRequestState, request);
            }
            else
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
                return await SendRequestAndGetJsonResponseHttpClient(requestUri, pubnubRequestState, request);
#else
                return await SendRequestAndGetJsonResponseTaskFactory(requestUri, pubnubRequestState, request);
#endif
            }

        }

        async Task<string> IPubnubHttp.SendRequestAndGetJsonResponseWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, string postData)
        {
            if (pubnubConfig.UseClassicHttpWebRequest)
            {
                return await SendRequestAndGetJsonResponseClassicHttpWithPOST(requestUri, pubnubRequestState, request, postData);
            }
            else
            {
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
                return await SendRequestAndGetJsonResponseHttpClientWithPOST(requestUri, pubnubRequestState, request, postData);
#else
                return await SendRequestAndGetJsonResponseTaskFactoryWithPOST(requestUri, pubnubRequestState, request, postData);
#endif
            }
        }

        //private string ReadStreamFromResponse(HttpWebResponse response)
        //{
        //    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got PubnubWebResponse", DateTime.Now.ToString()));
        //    using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
        //    {
        //        //Need to return this response 
        //        string jsonString = streamReader.ReadToEnd();
        //        System.Diagnostics.Debug.WriteLine(jsonString);
        //        System.Diagnostics.Debug.WriteLine("");
        //        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON", DateTime.Now.ToString()));
        //        return jsonString;
        //    }
        //}
#if !NET35 && !NET40 && !NET45 && !NET461 && !NETSTANDARD10
        async Task<string> SendRequestAndGetJsonResponseHttpClient<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            string jsonString = "";
            HttpResponseMessage response = null;
            try
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseHttpClient", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
                //HttpClient httpClient = new HttpClient();
                //if (httpClient.Timeout != TimeSpan.FromSeconds(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType)))
                //{
                //    httpClient.Timeout = TimeSpan.FromSeconds(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType));
                //}
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    response = await httpClientSubscribe.GetAsync(requestUri);
                }
                else if (pubnubRequestState.ResponseType == PNOperationType.PNDeleteMessageOperation)
                {
                    response = await httpClientNonsubscribe.DeleteAsync(requestUri);
                }
                else
                {
                    response = await httpClientNonsubscribe.GetAsync(requestUri);
                }
                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();
                stopWatch.Stop();
                if (pubnubTelemetryMgr != null)
                {
                    pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                }
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    jsonString = await streamReader.ReadToEndAsync();
                    pubnubRequestState.GotJsonResponse = true;
                }

                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got HttpResponseMessage for {1}", DateTime.Now.ToString(), requestUri));
            }
            catch (Exception ex)
            {
                throw ex;
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
            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, SendRequestAndGetJsonResponseHttpClientPOST Before httpClient.GetAsync", DateTime.Now.ToString()));
                //HttpClient httpClient = new HttpClient();
                //httpClient.Timeout = TimeSpan.FromSeconds(GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType));
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                StringContent jsonPostString = new StringContent(postData, Encoding.UTF8);
                if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                {
                    response = await httpClientSubscribe.PostAsync(requestUri, jsonPostString);
                }
                else
                {
                    response = await httpClientNonsubscribe.PostAsync(requestUri, jsonPostString);
                }
                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();
                stopWatch.Stop();
                if (pubnubTelemetryMgr != null)
                {
                    pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                }
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    jsonString = await streamReader.ReadToEndAsync();
                    pubnubRequestState.GotJsonResponse = true;
                }

                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got POST HttpResponseMessage for {1}", DateTime.Now.ToString(), requestUri));
            }
            catch (Exception ex)
            {
                throw ex;
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

        async Task<string> SendRequestAndGetJsonResponseTaskFactory<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            HttpWebResponse response = null;
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseTaskFactory", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
            try
            {
                request.Method = (pubnubRequestState != null && pubnubRequestState.ResponseType == PNOperationType.PNDeleteMessageOperation) ? "DELETE" : "GET";
                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                response = await Task.Factory.FromAsync<HttpWebResponse>(request.BeginGetResponse, asyncPubnubResult => (HttpWebResponse)request.EndGetResponse(asyncPubnubResult), pubnubRequestState);
                stopWatch.Stop();
                if (pubnubConfig.EnableTelemetry && pubnubTelemetryMgr != null)
                {
                    pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                }
                pubnubRequestState.Response = response;
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got PubnubWebResponse for {1}", DateTime.Now.ToString(), request.RequestUri.ToString()));
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    //Need to return this response 
#if NET35 || NET40
                    string jsonString = streamReader.ReadToEnd();
#else
                    string jsonString = await streamReader.ReadToEndAsync();
#endif
                    System.Diagnostics.Debug.WriteLine(jsonString);
                    pubnubRequestState.GotJsonResponse = true; 
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON", DateTime.Now.ToString()));

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
                        string jsonString = await streamReader.ReadToEndAsync();
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON from WebException response", DateTime.Now.ToString()));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
                {
                    throw ex;
                }
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return task.ContinueWith(t => ReadStreamFromResponse(t.Result));

            /*
            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before BeginGetResponse", DateTime.Now.ToString()));
            var taskComplete = new TaskCompletionSource<string>();

            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(
                (asynchronousResult) => {
                    RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                    PubnubWebRequest asyncWebRequest = asyncRequestState.Request as PubnubWebRequest;
                    if (asyncWebRequest != null)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before EndGetResponse", DateTime.Now.ToString()));
                        PubnubWebResponse asyncWebResponse = (PubnubWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, After EndGetResponse", DateTime.Now.ToString()));
                        using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Inside StreamReader", DateTime.Now.ToString()));
                            //Need to return this response 
                            string jsonString = streamReader.ReadToEnd();
                            System.Diagnostics.Debug.WriteLine(jsonString);
                            System.Diagnostics.Debug.WriteLine("");
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON", DateTime.Now.ToString()));
                            taskComplete.TrySetResult(jsonString);
                        }
                    }
                }
                ), pubnubRequestState);

            Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);

            return taskComplete.Task;
            */
        }

        async Task<string> SendRequestAndGetJsonResponseTaskFactoryWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, string postData)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before Task.Factory.FromAsync With POST", DateTime.Now.ToString()));
            try
            {
                request.Method = "POST";
                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();

                request.ContentType = "application/json";

                byte[] data = Encoding.UTF8.GetBytes(postData);
                //request.ContentLength = data.Length;
                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState))
                {
#if NET35 || NET40
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Flush();
#else
                    await requestStream.WriteAsync(data, 0, data.Length);
                    await requestStream.FlushAsync();
#endif

                }

                WebResponse response = await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, pubnubRequestState);
                stopWatch.Stop();
                if (pubnubTelemetryMgr != null)
                {
                    pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                }
                pubnubRequestState.Response = response as HttpWebResponse;
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Got PubnubWebResponse With POST for {1}", DateTime.Now.ToString(), request.RequestUri.ToString()));
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    //Need to return this response 
#if NET35 || NET40
                    string jsonString = streamReader.ReadToEnd();
#else
                    string jsonString = await streamReader.ReadToEndAsync();
#endif
                    System.Diagnostics.Debug.WriteLine(jsonString);
                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON With POST", DateTime.Now.ToString()));
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
                        string jsonString = await streamReader.ReadToEndAsync();
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON  With POST from WebException response", DateTime.Now.ToString()));
                        return jsonString;
                    }
                }

                if (ex.Message.IndexOf("The request was aborted: The request was canceled") == -1
                                && ex.Message.IndexOf("Machine suspend mode enabled. No request will be processed.") == -1)
                {
                    throw ex;
                }
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        async Task<string> SendRequestAndGetJsonResponseClassicHttp<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request)
        {
            //HttpWebResponse response = null;
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseClassicHttp", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<string>();
            try
            {
                request.Method = (pubnubRequestState != null && pubnubRequestState.ResponseType == PNOperationType.PNDeleteMessageOperation) ? "DELETE" : "GET";
                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before BeginGetResponse", DateTime.Now.ToString()));
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(
                    (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before EndGetResponse", DateTime.Now.ToString()));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, After EndGetResponse", DateTime.Now.ToString()));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Inside StreamReader", DateTime.Now.ToString()));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON", DateTime.Now.ToString()));
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
                        string jsonString = await streamReader.ReadToEndAsync();
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON from WebException response", DateTime.Now.ToString()));
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
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, Inside SendRequestAndGetJsonResponseClassicHttpWithPOST", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
            var taskComplete = new TaskCompletionSource<string>();
            try
            {
                request.Method = "POST";
                request.ContentType = "application/json";

                byte[] data = Encoding.UTF8.GetBytes(postData);
                //request.ContentLength = data.Length;
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
#if !NET35 && !NET40 && !NET45 && !NET461
                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, pubnubRequestState))
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
                    (asynchronousResult) => {
                        RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;
                        HttpWebRequest asyncWebRequest = asyncRequestState.Request as HttpWebRequest;
                        if (asyncWebRequest != null)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Before EndGetResponse With POST ", DateTime.Now.ToString()));
                            HttpWebResponse asyncWebResponse = (HttpWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                            stopWatch.Stop();
                            if (pubnubTelemetryMgr != null)
                            {
                                pubnubTelemetryMgr.StoreLatency(stopWatch.ElapsedMilliseconds, pubnubRequestState.ResponseType);
                            }
                            asyncRequestState.Response = asyncWebResponse;
                            System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, After EndGetResponse With POST ", DateTime.Now.ToString()));
                            using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Inside StreamReader With POST ", DateTime.Now.ToString()));
                                //Need to return this response 
                                string jsonString = streamReader.ReadToEnd();
                                asyncRequestState.GotJsonResponse = true;

                                System.Diagnostics.Debug.WriteLine(jsonString);
                                System.Diagnostics.Debug.WriteLine("");
                                System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON With POST ", DateTime.Now.ToString()));
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
                        string jsonString = await streamReader.ReadToEndAsync();
#endif
                        System.Diagnostics.Debug.WriteLine(jsonString);
                        System.Diagnostics.Debug.WriteLine("");
                        System.Diagnostics.Debug.WriteLine(string.Format("DateTime {0}, Retrieved JSON  With POST from WebException response", DateTime.Now.ToString()));
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
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached.Request abort for channel={1} ;channelgroup={2}", DateTime.Now.ToString(), currentMultiChannel, currentMultiChannelGroup), pubnubConfig.LogVerbosity);
                        currentState.Timeout = true;
                        try
                        {
                            request.Abort();
                        }
                        catch { }
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached. However state is unknown", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
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
                catch { }
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime: {0}, **WP7 OnPubnubWebRequestTimeout**", DateTime.Now.ToString()), pubnubConfig.LogVerbosity);
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
