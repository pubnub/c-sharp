using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections;
using System.Text;
using PubnubApi.EventEngine.Subscribe.Common;
using Newtonsoft.Json;
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

        public async Task<Tuple<HandshakeResponse, PNStatus>> HandshakeRequest(PNOperationType responseType, string[] channels, string[] channelGroups, long? timetoken, int? region, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
        {
            string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, "");
            Uri request = urlBuilder.BuildMultiChannelSubscribeRequest("GET", "", channels, channelGroups, timetoken.GetValueOrDefault(), region.GetValueOrDefault(), channelsJsonState, initialSubscribeUrlParams, externalQueryParam);

            RequestState<HandshakeResponse> pubnubRequestState = new RequestState<HandshakeResponse>();
            pubnubRequestState.Channels = channels;
            pubnubRequestState.ChannelGroups = channelGroups;
            pubnubRequestState.ResponseType = responseType;
            pubnubRequestState.Timetoken = timetoken.GetValueOrDefault();
            pubnubRequestState.Region = region.GetValueOrDefault();
            pubnubRequestState.TimeQueued = DateTime.Now;

            Tuple<string, PNStatus> responseTuple = await UrlProcessRequest(request, pubnubRequestState, false).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(responseTuple.Item1) && responseTuple.Item2 == null)
            {
                PNStatus status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, channels, channelGroups);
                HandshakeResponse handshakeResponse = JsonConvert.DeserializeObject<HandshakeResponse>(responseTuple.Item1);
                return new Tuple<HandshakeResponse, PNStatus>(handshakeResponse, status);
            }   

            return new Tuple<HandshakeResponse, PNStatus>(null, responseTuple.Item2);
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
        internal async Task<Tuple<ReceivingResponse<object>, PNStatus>> ReceiveRequest<T>(PNOperationType responseType, string[] channels, string[] channelGroups, long? timetoken, int? region, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
        {
            Tuple<ReceivingResponse<object>, PNStatus> resp = new Tuple<ReceivingResponse<object>, PNStatus>(null, null);

            try
            {
                string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);

                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, "");
                Uri request = urlBuilder.BuildMultiChannelSubscribeRequest("GET", "", channels, channelGroups, timetoken.GetValueOrDefault(), region.GetValueOrDefault(), channelsJsonState, initialSubscribeUrlParams, externalQueryParam);

                RequestState<ReceivingResponse<object>> pubnubRequestState = new RequestState<ReceivingResponse<object>>();
                pubnubRequestState.Channels = channels;
                pubnubRequestState.ChannelGroups = channelGroups;
                pubnubRequestState.ResponseType = responseType;
                //pubnubRequestState.Reconnect = reconnect;
                pubnubRequestState.Timetoken = timetoken.GetValueOrDefault();
                pubnubRequestState.Region = region.GetValueOrDefault();
                pubnubRequestState.TimeQueued = DateTime.Now;

                // Wait for message
                var responseTuple = await UrlProcessRequest(request, pubnubRequestState, false).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(responseTuple.Item1) && responseTuple.Item2 == null)
                {
                    PNStatus status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNConnectedCategory, channels, channelGroups);
                    ReceivingResponse<object> receiveResponse = JsonConvert.DeserializeObject<ReceivingResponse<object>>(responseTuple.Item1);
                    return new Tuple<ReceivingResponse<object>, PNStatus>(receiveResponse, status);
                }
                else if (responseTuple.Item2 != null)
                {
                    return new Tuple<ReceivingResponse<object>, PNStatus>(null, responseTuple.Item2);
                }
                return new Tuple<ReceivingResponse<object>, PNStatus>(null, new PNStatus(new Exception("ReceiveRequest failed."), PNOperationType.PNSubscribeOperation, PNStatusCategory.PNUnknownCategory, channels, channelGroups));
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
                if (pubnubRequestState != null)
                {
                    channel = (pubnubRequestState.Channels != null && pubnubRequestState.Channels.Length > 0) ? string.Join(",", pubnubRequestState.Channels.OrderBy(x => x).ToArray()) : ",";

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
#endif

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
                if (pubnubLog != null && config != null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, JSON= {jsonString} for request={requestUri}", config.LogVerbosity);
                }
                PNStatus errStatus = GetStatusIfError<T>(pubnubRequestState, jsonString);
                return new Tuple<string, PNStatus>((errStatus == null) ? jsonString : "", errStatus);
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
            else if (!NewtonsoftJsonDotNet.JsonFastCheck(jsonString))
            {
                status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, PNStatusCategory.PNNetworkIssuesCategory, asyncRequestState, (int)HttpStatusCode.NotFound, new PNException(jsonString));
            }

            return status;
        }

        internal bool Disconnect()
        {
            return true;
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
