using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections;
using System.Text;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace PubnubApi.EndPoint
{
    internal class SubscribeManager2 : IDisposable
    {
        private PNConfiguration config;
        private IJsonPluggableLibrary jsonLibrary;
        private IPubnubUnitTest unit;
        private IPubnubLog pubnubLog;
        private EndPoint.TelemetryManager pubnubTelemetryMgr;
        private IPubnubHttp pubnubHttp;

        private Timer SubscribeHeartbeatCheckTimer;
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
        private HttpClient httpSubscribe { get; set; }
        private HttpClient httpNonsubscribe { get; set; }
        private HttpClient httpNetworkStatus { get; set; }
        private PubnubHttpClientHandler pubnubHttpClientHandler { get; set; }
#else
        private HttpWebRequest httpSubscribe { get; set; }
#endif
        public SubscribeManager2(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            //PubnubInstance = instance;

#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
            if (httpSubscribe == null)
            {
                if (config.Proxy != null)
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    if (httpClientHandler.SupportsProxy)
                    {
                        httpClientHandler.Proxy = config.Proxy;
                        httpClientHandler.UseProxy = true;
                    }
                    pubnubHttpClientHandler = new PubnubHttpClientHandler("PubnubHttpClientHandler", httpClientHandler, config, jsonLibrary, unit, log);
                    httpSubscribe = new HttpClient(pubnubHttpClientHandler);
                }
                else
                {
                    httpSubscribe = new HttpClient();
                }
                httpSubscribe.DefaultRequestHeaders.Accept.Clear();
                httpSubscribe.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpSubscribe.Timeout = TimeSpan.FromSeconds(config.SubscribeTimeout);
            }
            if (httpNonsubscribe == null)
            {
                if (config.Proxy != null)
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    if (httpClientHandler.SupportsProxy)
                    {
                        httpClientHandler.Proxy = config.Proxy;
                        httpClientHandler.UseProxy = true;
                    }
                    pubnubHttpClientHandler = new PubnubHttpClientHandler("PubnubHttpClientHandler", httpClientHandler, config, jsonLibrary, unit, log);
                    httpNonsubscribe = new HttpClient(pubnubHttpClientHandler);
                }
                else
                {
                    httpNonsubscribe = new HttpClient();
                }
                httpNonsubscribe.DefaultRequestHeaders.Accept.Clear();
                httpNonsubscribe.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpNonsubscribe.Timeout = TimeSpan.FromSeconds(config.NonSubscribeRequestTimeout);
            }
            pubnubHttp = new PubnubHttp(config, jsonLibrary, log, pubnubTelemetryMgr, httpSubscribe, httpNonsubscribe);
#else
            pubnubHttp = new PubnubHttp(config, jsonLibrary, log, pubnubTelemetryMgr);
#endif
        }

#pragma warning disable

        internal async Task<Tuple<string, PNStatus>> HandshakeRequest<T>(PNOperationType responseType, string[] channels, string[] channelGroups, long? timetoken, int? region, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
        {
            Tuple<string, PNStatus> resp = new Tuple<string, PNStatus> ("", null);

            try
            {
                string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);

                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, "");
                Uri request = urlBuilder.BuildMultiChannelSubscribeRequest("GET", "", channels, channelGroups, timetoken.GetValueOrDefault(), region.GetValueOrDefault(), channelsJsonState, initialSubscribeUrlParams, externalQueryParam);

                RequestState<T> pubnubRequestState = new RequestState<T>();
                pubnubRequestState.Channels = channels;
                pubnubRequestState.ChannelGroups = channelGroups;
                pubnubRequestState.ResponseType = responseType;
                //pubnubRequestState.Reconnect = reconnect;
                pubnubRequestState.Timetoken = timetoken.GetValueOrDefault();
                pubnubRequestState.Region = region.GetValueOrDefault();
                pubnubRequestState.TimeQueued = DateTime.Now;

                // Wait for message
                
                await UrlProcessRequest<T>(request, pubnubRequestState, false).ContinueWith(r =>
                {
                    resp = r.Result;
                }, TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager=> MultiChannelSubscribeInit \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), string.Join(",", channelGroups.OrderBy(x => x).ToArray()), ex), config.LogVerbosity);
            }
            return resp;
        }

        internal void HandshakeRequestCancellation()
        {
            if (httpSubscribe != null)
            {
                try
                {
                    #if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                    httpSubscribe.CancelPendingRequests();
                    #else
                    httpSubscribe.Abort();
                    #endif     
                    httpSubscribe = null;
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => HandshakeRequestCancellation. Done.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                }
                catch(Exception ex)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => HandshakeRequestCancellation Exception: {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), config.LogVerbosity);
                }
            }
            else
            {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => HandshakeRequestCancellation. No request to cancel.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
            }
        }
        internal async Task<Tuple<string, PNStatus>> ReceiveRequest<T>(PNOperationType responseType, string[] channels, string[] channelGroups, long? timetoken, int? region, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
        {
            Tuple<string, PNStatus> resp = new Tuple<string, PNStatus> ("", null);

            try
            {
                string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);

                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, "");
                Uri request = urlBuilder.BuildMultiChannelSubscribeRequest("GET", "", channels, channelGroups, timetoken.GetValueOrDefault(), region.GetValueOrDefault(), channelsJsonState, initialSubscribeUrlParams, externalQueryParam);

                RequestState<T> pubnubRequestState = new RequestState<T>();
                pubnubRequestState.Channels = channels;
                pubnubRequestState.ChannelGroups = channelGroups;
                pubnubRequestState.ResponseType = responseType;
                //pubnubRequestState.Reconnect = reconnect;
                pubnubRequestState.Timetoken = timetoken.GetValueOrDefault();
                pubnubRequestState.Region = region.GetValueOrDefault();
                pubnubRequestState.TimeQueued = DateTime.Now;

                // Wait for message
                
                await UrlProcessRequest<T>(request, pubnubRequestState, false).ContinueWith(r =>
                {
                    resp = r.Result;
                }, TaskContinuationOptions.ExecuteSynchronously).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager=> MultiChannelSubscribeInit \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), string.Join(",", channelGroups.OrderBy(x => x).ToArray()), ex), config.LogVerbosity);
            }
            return resp;
        }

        internal void ReceiveRequestCancellation()
        {
            if (httpSubscribe != null)
            {
                try
                {
                    #if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                    httpSubscribe.CancelPendingRequests();
                    #else
                    httpSubscribe.Abort();
                    #endif   
                    httpSubscribe = null;
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => ReceiveRequestCancellation. Done.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                }
                catch(Exception ex)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => ReceiveRequestCancellation Exception: {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), config.LogVerbosity);
                }
            }
            else
            {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => RequestCancellation. No request to cancel.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
            }
        }

        internal void ReceiveReconnectRequestCancellation()
        {
            if (httpSubscribe != null)
            {
                try
                {
                    #if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                    httpSubscribe.CancelPendingRequests();
                    #else
                    httpSubscribe.Abort();
                    #endif   
                    httpSubscribe = null;
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => ReceiveReconnectRequestCancellation. Done.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                }
                catch(Exception ex)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => ReceiveReconnectRequestCancellation Exception: {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex), config.LogVerbosity);
                }
            }
            else
            {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} SubscribeManager => ReceiveReconnectRequestCancellation. No request to cancel.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
            }
        }

        internal protected async Task<Tuple<string, PNStatus>> UrlProcessRequest<T>(Uri requestUri, RequestState<T> pubnubRequestState, bool terminateCurrentSubRequest)
        {
            return await UrlProcessRequest(requestUri, pubnubRequestState, terminateCurrentSubRequest, null).ConfigureAwait(false);
        }

#pragma warning disable
        internal protected async Task<Tuple<string, PNStatus>> UrlProcessRequest<T>(Uri requestUri, RequestState<T> pubnubRequestState, bool terminateCurrentSubRequest, byte[] postOrPatchData)
#pragma warning restore
        {
            return await UrlProcessRequest(requestUri, pubnubRequestState, terminateCurrentSubRequest, postOrPatchData,"").ConfigureAwait(false);
        }

        internal protected async Task<Tuple<string, PNStatus>> UrlProcessRequest<T>(Uri requestUri, RequestState<T> pubnubRequestState, bool terminateCurrentSubRequest, byte[] postOrPatchData, string contentType)
        {
            string channel = "";
            PNConfiguration currentConfig;
            IPubnubLog currentLog;

            try
            {
                if (terminateCurrentSubRequest)
                {
                    //TerminateCurrentSubscriberRequest();
                }

                //if (PubnubInstance == null)
                //{
                //    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "DateTime {0}, PubnubInstance is null. Exiting UrlProcessRequest", DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                //    return new Tuple<string, PNStatus>("", null);
                //}

                if (pubnubRequestState != null)
                {
                    channel = (pubnubRequestState.Channels != null && pubnubRequestState.Channels.Length > 0) ? string.Join(",", pubnubRequestState.Channels.OrderBy(x => x).ToArray()) : ",";

                    //if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId) && !channel.Equals(",", StringComparison.OrdinalIgnoreCase) && !ChannelRequest[PubnubInstance.InstanceId].ContainsKey(channel) && (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation || pubnubRequestState.ResponseType == PNOperationType.Presence))
                    //{
                    //    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    //    {
                    //        LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, UrlProcessRequest ChannelRequest PubnubInstance.InstanceId Channel NOT matching", DateTime.Now.ToString(CultureInfo.InvariantCulture)), currentConfig.LogVerbosity);
                    //    }
                    //    return new Tuple<string, PNStatus>("", null);
                    //}
                }

#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                //do nothing
#else
                // Create Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
                request = pubnubHttp.SetServicePointConnectionLimit(pubnubRequestState, request);
                request = pubnubHttp.SetNoCache<T>(request);
                request = pubnubHttp.SetProxy<T>(request);
                request = pubnubHttp.SetTimeout<T>(pubnubRequestState, request);
                request = pubnubHttp.SetServicePointSetTcpKeepAlive(pubnubRequestState, request);
                request = pubnubHttp.SetTcpKeepAlive(request);
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = "application/json";
                }
                request.ContentType = contentType;

                pubnubRequestState.Request = request;
                httpSubscribe = request;

                //if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId) && (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation || pubnubRequestState.ResponseType == PNOperationType.Presence))
                //{
                //    ChannelRequest[PubnubInstance.InstanceId].AddOrUpdate(channel, pubnubRequestState.Request, (key, oldState) => pubnubRequestState.Request);
                //}
#endif

                //if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                //{
                //    LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Request={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), requestUri.ToString()), currentConfig.LogVerbosity);
                //}

                //if (pubnubRequestState != null && pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation)
                //{
                //    SubscribeRequestTracker.AddOrUpdate(PubnubInstance.InstanceId, DateTime.Now, (key, oldState) => DateTime.Now);
                //}

                string jsonString = "";
#if !NET35 && !NET40 && !NET45 && !NET461 && !NET48 && !NETSTANDARD10
                if (pubnubRequestState != null && pubnubRequestState.UsePostMethod)
                {
                    jsonString = await pubnubHttp.SendRequestAndGetJsonResponseWithPOST(requestUri, pubnubRequestState, null, postOrPatchData, contentType).ConfigureAwait(false);
                }
                else if (pubnubRequestState != null && pubnubRequestState.UsePatchMethod)
                {
                    jsonString = await pubnubHttp.SendRequestAndGetJsonResponseWithPATCH(requestUri, pubnubRequestState, null, postOrPatchData, contentType).ConfigureAwait(false);
                }
                else
                {
                    jsonString = await pubnubHttp.SendRequestAndGetJsonResponse(requestUri, pubnubRequestState, null).ConfigureAwait(false);
                }
#else
                if (pubnubRequestState != null && pubnubRequestState.UsePostMethod)
                {
                    jsonString = await pubnubHttp.SendRequestAndGetJsonResponseWithPOST(requestUri, pubnubRequestState, request, postOrPatchData, contentType).ConfigureAwait(false);
                }
                else if (pubnubRequestState != null && pubnubRequestState.UsePatchMethod)
                {
                    jsonString = await pubnubHttp.SendRequestAndGetJsonResponseWithPATCH(requestUri, pubnubRequestState, request, postOrPatchData, contentType).ConfigureAwait(false);
                }
                else
                {
                    jsonString = await pubnubHttp.SendRequestAndGetJsonResponse(requestUri, pubnubRequestState, request).ConfigureAwait(false);
                }
#endif

                //if (SubscribeDisconnected.ContainsKey(PubnubInstance.InstanceId) && SubscribeDisconnected[PubnubInstance.InstanceId])
                //{
                //    if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                //    {
                //        LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0},Received JSON but SubscribeDisconnected = {1} for request={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonString, requestUri), currentConfig.LogVerbosity);
                //    }
                //    throw new OperationCanceledException("Disconnected");
                //}

                //if (pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                //{
                //    LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, JSON= {1} for request={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), jsonString, requestUri), currentConfig.LogVerbosity);
                //}
                PNStatus errStatus = GetStatusIfError<T>(pubnubRequestState, jsonString);
                if (errStatus == null && pubnubRequestState != null)
                {
                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(pubnubRequestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, pubnubRequestState, (int)HttpStatusCode.OK, null);
                    return new Tuple<string, PNStatus>(jsonString, status);
                }
                else
                {
                    jsonString = "";
                    return new Tuple<string, PNStatus>(jsonString, errStatus);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                string exceptionMessage = "";
                Exception innerEx = null;
                WebException webEx = null;
                PNStatus status = null;

                if (ex.InnerException != null)
                {
                    if (ex is WebException)
                    {
                        webEx = ex as WebException;
                        exceptionMessage = webEx.ToString();
                    }
                    else
                    {
                        innerEx = ex.InnerException;
                        exceptionMessage = innerEx.ToString();
                    }
                }
                else
                {
                    innerEx = ex;
                    exceptionMessage = innerEx.ToString();
                }

                if (exceptionMessage.IndexOf("The request was aborted: The request was canceled", StringComparison.CurrentCultureIgnoreCase) == -1
                && exceptionMessage.IndexOf("Machine suspend mode enabled. No request will be processed.", StringComparison.CurrentCultureIgnoreCase) == -1
                && (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation && exceptionMessage.IndexOf("The operation has timed out", StringComparison.CurrentCultureIgnoreCase) == -1)
                && exceptionMessage.IndexOf("A task was canceled", StringComparison.CurrentCultureIgnoreCase) == -1
                && errorMessage.IndexOf("The operation was canceled", StringComparison.CurrentCultureIgnoreCase) == -1)
                {
                    PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(webEx == null ? innerEx : webEx);
                    status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(pubnubRequestState.ResponseType, category, pubnubRequestState, (int)HttpStatusCode.NotFound, new PNException(ex));
                        //if (pubnubRequestState != null && pubnubRequestState.PubnubCallback != null)
                        //{
                        //    pubnubRequestState.PubnubCallback.OnResponse(default(T), status);
                        //}
                        //else
                        //{
                        //    Announce(status);
                        //}

                    //if (PubnubInstance != null && pubnubConfig.TryGetValue(PubnubInstance.InstanceId, out currentConfig) && pubnubLog.TryGetValue(PubnubInstance.InstanceId, out currentLog))
                    //{
                    //    LoggingMethod.WriteToLog(currentLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} PubnubBaseCore UrlProcessRequest Exception={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), webEx != null ? webEx.ToString() : exceptionMessage), currentConfig.LogVerbosity);
                    //}
                }

                return new Tuple<string, PNStatus>("", status);
            }
        }

        protected string BuildJsonUserState(string channel, string channelGroup, bool local)
        {
            Dictionary<string, object> channelUserStateDictionary = null;
            Dictionary<string, object> channelGroupUserStateDictionary = null;

            if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(channelGroup))
            {
                throw new ArgumentException("BuildJsonUserState takes either channel or channelGroup at one time. Send one at a time by passing empty value for other.");
            }

            //if (local)
            //{
            //    if (!string.IsNullOrEmpty(channel) && ChannelLocalUserState[PubnubInstance.InstanceId].ContainsKey(channel))
            //    {
            //        ChannelLocalUserState[PubnubInstance.InstanceId].TryGetValue(channel, out channelUserStateDictionary);
            //    }
            //    if (!string.IsNullOrEmpty(channelGroup) && ChannelGroupLocalUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
            //    {
            //        ChannelGroupLocalUserState[PubnubInstance.InstanceId].TryGetValue(channelGroup, out channelGroupUserStateDictionary);
            //    }
            //}
            //else
            //{
            //    if (!string.IsNullOrEmpty(channel) && ChannelUserState.ContainsKey(PubnubInstance.InstanceId) && ChannelUserState[PubnubInstance.InstanceId].ContainsKey(channel))
            //    {
            //        ChannelUserState[PubnubInstance.InstanceId].TryGetValue(channel, out channelUserStateDictionary);
            //    }
            //    if (!string.IsNullOrEmpty(channelGroup)&& ChannelGroupUserState.ContainsKey(PubnubInstance.InstanceId)  && ChannelGroupUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
            //    {
            //        ChannelGroupUserState[PubnubInstance.InstanceId].TryGetValue(channelGroup, out channelGroupUserStateDictionary);
            //    }
            //}

            StringBuilder jsonStateBuilder = new StringBuilder();

            if (channelUserStateDictionary != null)
            {
                string[] channelUserStateKeys = channelUserStateDictionary.Keys.ToArray<string>();

                for (int keyIndex = 0; keyIndex < channelUserStateKeys.Length; keyIndex++)
                {
                    string channelUserStateKey = channelUserStateKeys[keyIndex];
                    object channelUserStateValue = channelUserStateDictionary[channelUserStateKey];
                    if (channelUserStateValue == null)
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", "null"));
                    }
                    else if (channelUserStateValue.GetType().ToString() == "System.Boolean")
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, channelUserStateValue.ToString().ToLowerInvariant());
                    }
                    else
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelUserStateKey, (channelUserStateValue.GetType().ToString() == "System.String") ? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", channelUserStateValue) : channelUserStateValue);
                    }
                    if (keyIndex < channelUserStateKeys.Length - 1)
                    {
                        jsonStateBuilder.Append(',');
                    }
                }
            }
            if (channelGroupUserStateDictionary != null)
            {
                string[] channelGroupUserStateKeys = channelGroupUserStateDictionary.Keys.ToArray<string>();

                for (int keyIndex = 0; keyIndex < channelGroupUserStateKeys.Length; keyIndex++)
                {
                    string channelGroupUserStateKey = channelGroupUserStateKeys[keyIndex];
                    object channelGroupUserStateValue = channelGroupUserStateDictionary[channelGroupUserStateKey];
                    if (channelGroupUserStateValue == null)
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, string.Format(CultureInfo.InvariantCulture, "\"{0}\"", "null"));
                    }
                    else if (channelGroupUserStateValue.GetType().ToString() == "System.Boolean")
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, channelGroupUserStateValue.ToString().ToLowerInvariant());
                    }
                    else
                    {
                        jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\":{1}", channelGroupUserStateKey, (channelGroupUserStateValue.GetType().ToString() == "System.String") ? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", channelGroupUserStateValue) : channelGroupUserStateValue);
                    }
                    if (keyIndex < channelGroupUserStateKeys.Length - 1)
                    {
                        jsonStateBuilder.Append(',');
                    }
                }
            }

            return jsonStateBuilder.ToString();
        }

        protected string BuildJsonUserState(string[] channels, string[] channelGroups, bool local)
        {
            string retJsonUserState = "";

            StringBuilder jsonStateBuilder = new StringBuilder();

            if (channels != null && channels.Length > 0)
            {
                for (int index = 0; index < channels.Length; index++)
                {
                    string currentJsonState = BuildJsonUserState(channels[index], "", local);
                    if (!string.IsNullOrEmpty(currentJsonState))
                    {
                        currentJsonState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", channels[index], currentJsonState);
                        if (jsonStateBuilder.Length > 0)
                        {
                            jsonStateBuilder.Append(',');
                        }
                        jsonStateBuilder.Append(currentJsonState);
                    }
                }
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                for (int index = 0; index < channelGroups.Length; index++)
                {
                    string currentJsonState = BuildJsonUserState("", channelGroups[index], local);
                    if (!string.IsNullOrEmpty(currentJsonState))
                    {
                        currentJsonState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", channelGroups[index], currentJsonState);
                        if (jsonStateBuilder.Length > 0)
                        {
                            jsonStateBuilder.Append(',');
                        }
                        jsonStateBuilder.Append(currentJsonState);
                    }
                }
            }

            if (jsonStateBuilder.Length > 0)
            {
                retJsonUserState = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", jsonStateBuilder);
            }

            return retJsonUserState;
        }

        private PNStatus GetStatusIfError<T>(RequestState<T> asyncRequestState, string jsonString)
        {
            PNStatus status = null;
            if (string.IsNullOrEmpty(jsonString)) { return status;  }

            PNConfiguration currentConfig;
            PNOperationType type = PNOperationType.None;
            if (asyncRequestState != null)
            {
                type = asyncRequestState.ResponseType;
            }
            if (jsonLibrary.IsDictionaryCompatible(jsonString, type))
            {
                Dictionary<string, object> deserializeStatus = jsonLibrary.DeserializeToDictionaryOfObject(jsonString);
                int statusCode = 0; //default. assuming all is ok 
                if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("error") && string.Equals(deserializeStatus["error"].ToString(), "true", StringComparison.OrdinalIgnoreCase))
                {
                        status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNUnknownCategory, asyncRequestState, (int)HttpStatusCode.NotFound, new PNException(jsonString));
                }
                else if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("error") && deserializeStatus.ContainsKey("status") && Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode) && statusCode > 0)
                {
                    string errorMessageJson = deserializeStatus["error"].ToString();
                    Dictionary<string, object> errorDic = jsonLibrary.DeserializeToDictionaryOfObject(errorMessageJson);
                    if (errorDic != null && errorDic.Count > 0 && errorDic.ContainsKey("message")
                        && statusCode != 200)
                    {
                        string statusMessage = errorDic["message"].ToString();
                        PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
                        status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
                    }
                }
                else if (deserializeStatus.Count >= 1 && deserializeStatus.ContainsKey("status") && string.Equals(deserializeStatus["status"].ToString(), "error", StringComparison.OrdinalIgnoreCase) && deserializeStatus.ContainsKey("error"))
                {
                    string errorMessageJson = deserializeStatus["error"].ToString();
                    Dictionary<string, object> errorDic = jsonLibrary.DeserializeToDictionaryOfObject(errorMessageJson);
                    if (errorDic != null && errorDic.Count > 0 && errorDic.ContainsKey("code") && errorDic.ContainsKey("message"))
                    {
                        statusCode = PNStatusCodeHelper.GetHttpStatusCode(errorDic["code"].ToString());
                        string statusMessage = errorDic["message"].ToString();
                        if (statusCode != 200)
                        {
                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
                            status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
                        }
                    }
                }
                else if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message"))
                {
                    var _ = Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
                    string statusMessage = deserializeStatus["message"].ToString();

                    if (statusCode != 200)
                    {
                        PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, statusMessage);
                        status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, category, asyncRequestState, statusCode, new PNException(jsonString));
                    }
                }

            }
            else if (jsonString.ToLowerInvariant().TrimStart().IndexOf("<head", StringComparison.CurrentCultureIgnoreCase) == 0
                || jsonString.ToLowerInvariant().TrimStart().IndexOf("<html", StringComparison.CurrentCultureIgnoreCase) == 0
                || jsonString.ToLowerInvariant().TrimStart().IndexOf("<!doctype", StringComparison.CurrentCultureIgnoreCase) == 0)//Html is not expected. Only json format messages are expected.
            {
                status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNNetworkIssuesCategory, asyncRequestState, (int)HttpStatusCode.NotFound, new PNException(jsonString));
            }
            else if (jsonString.ToLowerInvariant().TrimStart().IndexOf("<?xml", StringComparison.CurrentCultureIgnoreCase) == 0
                  || jsonString.ToLowerInvariant().TrimStart().IndexOf("<Error", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNNetworkIssuesCategory, asyncRequestState, (int)HttpStatusCode.NotFound, new PNException(jsonString));
            }

            return status;
        }

        internal List<object> WrapResultBasedOnResponseType<T>(PNOperationType type, string jsonString, string[] channels, string[] channelGroups)
        {
            List<object> result = new List<object>();
            try
            {
                string multiChannel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
                string multiChannelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

                if (!string.IsNullOrEmpty(jsonString))
                {
                    object deserializedResult = jsonLibrary.DeserializeToObject(jsonString);
                    List<object> result1 = ((IEnumerable)deserializedResult).Cast<object>().ToList();

                    if (result1 != null && result1.Count > 0)
                    {
                        result = result1;
                    }

                    switch (type)
                    {
                        case PNOperationType.PNSubscribeOperation:
                        case PNOperationType.Presence:
                            if (result.Count == 3 && result[0] is object[] && (result[0] as object[]).Length == 0 && result[2].ToString() == "")
                            {
                                result.RemoveAt(2);
                            }
                            if (result.Count == 4 && result[0] is object[] && (result[0] as object[]).Length == 0 && result[2].ToString() == "" && result[3].ToString() == "")
                            {
                                result.RemoveRange(2, 2);
                            }
                            result.Add(multiChannelGroup);
                            result.Add(multiChannel);

                            break;
                        case PNOperationType.PNHeartbeatOperation:
                            //Dictionary<string, object> heartbeatadictionary = jsonLibrary.DeserializeToDictionaryOfObject(jsonString);
                            //result = new List<object>();
                            //result.Add(heartbeatadictionary);
                            //result.Add(multiChannel);
                            break;
                        default:
                            break;
                    }
                    //switch stmt end
                }
            }
            catch { /* ignore */ }

            return result;
        }


        internal bool Disconnect()
        {
            //if (SubscribeDisconnected[PubnubInstance.InstanceId])
            //{
            //    return false;
            //}
            //LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, SubscribeManager Manual Disconnect", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
            //SubscribeDisconnected[PubnubInstance.InstanceId] = true;
            //TerminateCurrentSubscriberRequest();
            //PubnubCoreBase2.TerminatePresenceHeartbeatTimer();
            //TerminateReconnectTimer();

            return true;
        }

        private void RegisterPresenceHeartbeatTimer<T>(string[] channels, string[] channelGroups)
        {
            //if (PresenceHeartbeatTimer != null)
            //{
            //    try
            //    {
            //        PresenceHeartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //        PresenceHeartbeatTimer.Dispose();
            //        PresenceHeartbeatTimer = null;
            //    }
            //    catch {  /* ignore */ }
            //}
            //if ((channels != null && channels.Length > 0 && channels.Where(s => s != null && s.Contains("-pnpres") == false).Any())
            //    || (channelGroups != null && channelGroups.Length > 0 && channelGroups.Where(s => s != null && s.Contains("-pnpres") == false).Any()))
            //{
            //    RequestState<T> presenceHeartbeatState = new RequestState<T>();
            //    presenceHeartbeatState.Channels = channels;
            //    presenceHeartbeatState.ChannelGroups = channelGroups;
            //    presenceHeartbeatState.ResponseType = PNOperationType.PNHeartbeatOperation;
            //    presenceHeartbeatState.Request = null;
            //    presenceHeartbeatState.Response = null;

            //    if (config.ContainsKey(PubnubInstance.InstanceId) && config[PubnubInstance.InstanceId].PresenceInterval > 0)
            //    {
            //        PresenceHeartbeatTimer = new Timer(OnPresenceHeartbeatIntervalTimeout<T>, presenceHeartbeatState, config[PubnubInstance.InstanceId].PresenceInterval * 1000, config[PubnubInstance.InstanceId].PresenceInterval * 1000);
            //    }
            //}
        }

#pragma warning disable
        void OnPresenceHeartbeatIntervalTimeout<T>(System.Object presenceHeartbeatState)
#pragma warning restore
        {
            ////Make presence heartbeat call
            //RequestState<T> currentState = presenceHeartbeatState as RequestState<T>;
            //if (currentState != null)
            //{
            //    string[] subscriberChannels = (currentState.Channels != null) ? currentState.Channels.Where(s => s.Contains("-pnpres") == false).ToArray() : null;
            //    string[] subscriberChannelGroups = (currentState.ChannelGroups != null) ? currentState.ChannelGroups.Where(s => s.Contains("-pnpres") == false).ToArray() : null;

            //    bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, currentState.ResponseType, currentState.PubnubCallback, currentState.Channels, currentState.ChannelGroups);
            //    if (networkConnection)
            //    {
            //        if ((subscriberChannels != null && subscriberChannels.Length > 0) || (subscriberChannelGroups != null && subscriberChannelGroups.Length > 0))
            //        {
            //            string channelsJsonState = BuildJsonUserState(subscriberChannels, subscriberChannelGroups, false);
            //            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
                        
            //            Uri request = urlBuilder.BuildPresenceHeartbeatRequest("GET", "", subscriberChannels, subscriberChannelGroups, channelsJsonState);

            //            RequestState<PNHeartbeatResult> requestState = new RequestState<PNHeartbeatResult>();
            //            requestState.Channels = currentState.Channels;
            //            requestState.ChannelGroups = currentState.ChannelGroups;
            //            requestState.ResponseType = PNOperationType.PNHeartbeatOperation;
            //            requestState.PubnubCallback = null;
            //            requestState.Reconnect = false;
            //            requestState.Response = null;
            //            requestState.TimeQueued = DateTime.Now;

            //            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            //            {
            //                string json = r.Result.Item1;
            //                if (!string.IsNullOrEmpty(json))
            //                {
            //                    List<object> result = ProcessJsonResponse(requestState, json);
            //                    //ProcessResponseCallbacks(result, requestState);
            //                }
            //            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
            //        }
            //    }
            //    else
            //    {
            //        if (PubnubInstance != null && !networkConnection)
            //        {
            //            PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNSubscribeOperation, PNStatusCategory.PNNetworkIssuesCategory, null, (int)System.Net.HttpStatusCode.NotFound, new PNException("Internet connection problem during presence heartbeat."));
            //            if (subscriberChannels != null && subscriberChannels.Length > 0)
            //            {
            //                status.AffectedChannels.AddRange(subscriberChannels.ToList());
            //            }
            //            if (subscriberChannelGroups != null && subscriberChannelGroups.Length > 0)
            //            {
            //                status.AffectedChannelGroups.AddRange(subscriberChannelGroups.ToList());
            //            }
            //            Announce(status);
            //        }

            //    }
            //}

        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                if (SubscribeHeartbeatCheckTimer != null)
                {
                    SubscribeHeartbeatCheckTimer.Dispose();
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            DisposeInternal(true);
        }
        #endregion

    }
}
