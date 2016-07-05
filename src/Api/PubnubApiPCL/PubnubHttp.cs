using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Collections;

namespace PubnubApi
{
    public class PubnubHttp : IPubnubHttp
    {
        private PNConfiguration _pnConfig = null;
        private IJsonPluggableLibrary _jsonLib = null;

        public PubnubHttp(PNConfiguration pnConfiguation, IJsonPluggableLibrary jsonPluggableLibrary)
        {
            this._pnConfig = pnConfiguation;
            this._jsonLib = jsonPluggableLibrary;
        }

        PubnubWebRequest IPubnubHttp.SetProxy<T>(PubnubWebRequest request)
        {
            //REVISIT
            //#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
            //            if (_pnConfig.EnableProxy && _pubnubProxy != null)
            //            {
            //                //                LoggingMethod.WriteToLog(string.Format("DateTime {0}, ProxyServer={1}; ProxyPort={2}; ProxyUserName={3}", DateTime.Now.ToString(), _pubnubProxy.ProxyServer, _pubnubProxy.ProxyPort, _pubnubProxy.ProxyUserName), LoggingMethod.LevelInfo);
            //                //                WebProxy webProxy = new WebProxy(_pubnubProxy.ProxyServer, _pubnubProxy.ProxyPort);
            //                //                webProxy.Credentials = new NetworkCredential(_pubnubProxy.ProxyUserName, _pubnubProxy.ProxyPassword);
            //                //                request.Proxy = webProxy;
            //            }
            //#endif
            //No proxy setting for WP7
            return request;
        }

        PubnubWebRequest IPubnubHttp.SetTimeout<T>(RequestState<T> pubnubRequestState, PubnubWebRequest request)
        {
            //REVISIT
            //#if (!SILVERLIGHT && !WINDOWS_PHONE && !NETFX_CORE)
            //            //request.Timeout = GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000;
            //#endif
            //No Timeout setting for WP7
            return request;
        }


        PubnubWebRequest IPubnubHttp.SetServicePointSetTcpKeepAlive(PubnubWebRequest request)
        {
#if ((!__MonoCS__) && (!SILVERLIGHT) && !WINDOWS_PHONE && !NETFX_CORE)
            //request.ServicePoint.SetTcpKeepAlive(true, base.LocalClientHeartbeatInterval * 1000, 1000);
#endif
            //do nothing for mono
            return request;
        }

        void IPubnubHttp.SendRequestAndGetResult<T>(Uri requestUri, RequestState<T> pubnubRequestState, PubnubWebRequest request)
        {
            //REVISIT
#if (SILVERLIGHT || WINDOWS_PHONE || NETFX_CORE)
            //For WP7, Ensure that the RequestURI length <= 1599
            //For SL, Ensure that the RequestURI length <= 1482 for Large Text Message. If RequestURI Length < 1343, Successful Publish occurs
            IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(UrlProcessResponseCallback<T>), pubnubRequestState);
            Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
#else
            if (!ClientNetworkStatus.MachineSuspendMode && !PubnubWebRequest.MachineSuspendMode)
            {
                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(UrlProcessResponseCallback<T>), pubnubRequestState);
                Timer webRequestTimer = new Timer(OnPubnubWebRequestTimeout<T>, pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, Timeout.Infinite);
                //ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(OnPubnubWebRequestTimeout<T>), pubnubRequestState, GetTimeoutInSecondsForResponseType(pubnubRequestState.ResponseType) * 1000, true);
            }
            else
            {
                //REVISIT
                #region FOR RECONNECT
                ReconnectState<T> netState = new ReconnectState<T>();
                netState.Channels = pubnubRequestState.Channels;
                netState.ChannelGroups = pubnubRequestState.ChannelGroups;
                netState.ResponseType = pubnubRequestState.ResponseType;
                netState.SubscribeRegularCallback = pubnubRequestState.SubscribeRegularCallback;
                netState.PresenceRegularCallback = pubnubRequestState.PresenceRegularCallback;
                netState.ErrorCallback = pubnubRequestState.ErrorCallback;
                netState.ConnectCallback = pubnubRequestState.ConnectCallback;
                netState.Timetoken = pubnubRequestState.Timetoken;
                netState.Reconnect = pubnubRequestState.Reconnect;

                //_reconnectFromSuspendMode = netState;
                return;
                #endregion
            }
#endif
            //REVISIT
            #region PRESENCE HEARTBEAT FOR PRESENCE AND SUBSCRIBE
            if (pubnubRequestState.ResponseType == ResponseType.Presence || pubnubRequestState.ResponseType == ResponseType.Subscribe)
            {
                //if (presenceHeartbeatTimer != null)
                //{
                //    presenceHeartbeatTimer.Dispose();
                //    presenceHeartbeatTimer = null;
                //}
                //if ((pubnubRequestState.Channels != null && pubnubRequestState.Channels.Length > 0 && pubnubRequestState.Channels.Where(s => s.Contains("-pnpres") == false).ToArray().Length > 0)
                //    || (pubnubRequestState.ChannelGroups != null && pubnubRequestState.ChannelGroups.Length > 0 && pubnubRequestState.ChannelGroups.Where(s => s.Contains("-pnpres") == false).ToArray().Length > 0))
                //{
                //    RequestState<T> presenceHeartbeatState = new RequestState<T>();
                //    presenceHeartbeatState.Channels = pubnubRequestState.Channels;
                //    presenceHeartbeatState.ChannelGroups = pubnubRequestState.ChannelGroups;
                //    presenceHeartbeatState.ResponseType = ResponseType.PresenceHeartbeat;
                //    presenceHeartbeatState.ErrorCallback = pubnubRequestState.ErrorCallback;
                //    presenceHeartbeatState.Request = null;
                //    presenceHeartbeatState.Response = null;

                //    if (base.PresenceHeartbeatInterval > 0)
                //    {
                //        presenceHeartbeatTimer = new Timer(OnPresenceHeartbeatIntervalTimeout<T>, presenceHeartbeatState, base.PresenceHeartbeatInterval * 1000, base.PresenceHeartbeatInterval * 1000);
                //    }
                //}
            }
            #endregion

        }

        protected void UrlProcessResponseCallback<T>(IAsyncResult asynchronousResult)
        {
            List<object> result = new List<object>();

            RequestState<T> asyncRequestState = asynchronousResult.AsyncState as RequestState<T>;

            string channel = "";
            string channelGroup = "";
            if (asyncRequestState != null)
            {
                if (asyncRequestState.Channels != null)
                {
                    channel = (asyncRequestState.Channels.Length > 0) ? string.Join(",", asyncRequestState.Channels) : ",";
                }
                if (asyncRequestState.ChannelGroups != null)
                {
                    channelGroup = string.Join(",", asyncRequestState.ChannelGroups);
                }
            }
            //if (asynchRequestState != null && asynchRequestState.c

            PubnubWebRequest asyncWebRequest = asyncRequestState.Request as PubnubWebRequest;
            try
            {
                if (asyncWebRequest != null)
                {
                    PubnubWebResponse asyncWebResponse = (PubnubWebResponse)asyncWebRequest.EndGetResponse(asynchronousResult);
                    {
                        asyncRequestState.Response = asyncWebResponse;

                        using (StreamReader streamReader = new StreamReader(asyncWebResponse.GetResponseStream()))
                        {
                            //REVISIT
                            #region CHECK REQUEST STATE FOR SUBSCRIBE AND PRESENCE
                            //if (asyncRequestState.ResponseType == ResponseType.Subscribe || asyncRequestState.ResponseType == ResponseType.Presence)
                            //{
                            //    if (!overrideTcpKeepAlive && (
                            //                (channelInternetStatus.ContainsKey(channel) && !channelInternetStatus[channel])
                            //                    || (channelGroupInternetStatus.ContainsKey(channelGroup) && !channelGroupInternetStatus[channelGroup])
                            //                    ))
                            //    {
                            //        if (asyncRequestState.Channels != null && asyncRequestState.Channels.Length > 0)
                            //        {
                            //            for (int index = 0; index < asyncRequestState.Channels.Length; index++)
                            //            {
                            //                string activeChannel = asyncRequestState.Channels[index].ToString();
                            //                string activeChannelGroup = "";

                            //                string status = "Internet connection available";

                            //                PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
                            //                callbackKey.Channel = activeChannel;
                            //                callbackKey.ResponseType = asyncRequestState.ResponseType;

                            //                if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                            //                {
                            //                    object callbackObject;
                            //                    bool channelAvailable = channelCallbacks.TryGetValue(callbackKey, out callbackObject);
                            //                    if (channelAvailable)
                            //                    {
                            //                        if (asyncRequestState.ResponseType == ResponseType.Presence)
                            //                        {
                            //                            PubnubPresenceChannelCallback currentPubnubCallback = callbackObject as PubnubPresenceChannelCallback;

                            //                            //TODO: PANDU - Revisit logic on connect callback
                            //                            if (currentPubnubCallback != null && channelCallbacks.ContainsKey(callbackKey))
                            //                            {
                            //                                CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                            //                                    activeChannel, activeChannelGroup, asyncRequestState.ErrorCallback,
                            //                                    status, PubnubErrorCode.YesInternet, null, null);
                            //                            }
                            //                        }
                            //                        else
                            //                        {
                            //                            PubnubSubscribeChannelCallback<T> currentPubnubCallback = callbackObject as PubnubSubscribeChannelCallback<T>;

                            //                            //TODO: PANDU - Revisit logic on connect callback
                            //                            if (currentPubnubCallback != null && channelCallbacks.ContainsKey(callbackKey))
                            //                            {
                            //                                CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                            //                                    activeChannel, activeChannelGroup, asyncRequestState.ErrorCallback,
                            //                                    status, PubnubErrorCode.YesInternet, null, null);
                            //                            }
                            //                        }
                            //                    }

                            //                }
                            //            }
                            //        }

                            //        if (asyncRequestState.ChannelGroups != null && asyncRequestState.ChannelGroups.Length > 0)
                            //        {
                            //            for (int index = 0; index < asyncRequestState.ChannelGroups.Length; index++)
                            //            {
                            //                string activeChannel = "";
                            //                string activeChannelGroup = asyncRequestState.ChannelGroups[index].ToString();

                            //                string status = "Internet connection available";

                            //                PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
                            //                callbackKey.ChannelGroup = activeChannel;
                            //                callbackKey.ResponseType = asyncRequestState.ResponseType;

                            //                if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
                            //                {
                            //                    object callbackObject;
                            //                    bool channelAvailable = channelGroupCallbacks.TryGetValue(callbackKey, out callbackObject);
                            //                    if (channelAvailable)
                            //                    {
                            //                        if (asyncRequestState.ResponseType == ResponseType.Presence)
                            //                        {
                            //                            PubnubPresenceChannelGroupCallback currentPubnubCallback = callbackObject as PubnubPresenceChannelGroupCallback;
                            //                            if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null)
                            //                            {
                            //                                CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                            //                                    activeChannel, activeChannelGroup, asyncRequestState.ErrorCallback,
                            //                                    status, PubnubErrorCode.YesInternet, null, null);
                            //                            }
                            //                        }
                            //                        else
                            //                        {
                            //                            PubnubSubscribeChannelGroupCallback<T> currentPubnubCallback = callbackObject as PubnubSubscribeChannelGroupCallback<T>;
                            //                            if (currentPubnubCallback != null && currentPubnubCallback.ConnectCallback != null)
                            //                            {
                            //                                CallErrorCallback(PubnubErrorSeverity.Info, PubnubMessageSource.Client,
                            //                                    activeChannel, activeChannelGroup, asyncRequestState.ErrorCallback,
                            //                                    status, PubnubErrorCode.YesInternet, null, null);
                            //                            }
                            //                        }
                            //                    }

                            //                }
                            //            }
                            //        }
                            //    }

                            //    channelInternetStatus.AddOrUpdate(channel, true, (key, oldValue) => true);
                            //    channelGroupInternetStatus.AddOrUpdate(channelGroup, true, (key, oldValue) => true);
                            //}
                            #endregion

                            //Deserialize the result
                            string jsonString = streamReader.ReadToEnd();
#if !NETFX_CORE
                            //streamReader.Close ();
#endif

                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON for channel={1} ({2}) ={3}", DateTime.Now.ToString(), channel, asyncRequestState.ResponseType.ToString(), jsonString), LoggingMethod.LevelInfo);

                            //REVISIT
                            //if (overrideTcpKeepAlive)
                            //{
                            //    TerminateLocalClientHeartbeatTimer(asyncWebRequest.RequestUri);
                            //}

                            //REVISIT
                            #region CHECK IF RESPONSE IS FOR PRESENCEHEARTBEAT
                            if (asyncRequestState.ResponseType == ResponseType.PresenceHeartbeat)
                            {
                                //if (base.JsonPluggableLibrary.IsDictionaryCompatible(jsonString))
                                //{
                                //    Dictionary<string, object> deserializeStatus = base.JsonPluggableLibrary.DeserializeToDictionaryOfObject(jsonString);
                                //    int statusCode = 0; //default. assuming all is ok 
                                //    if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message"))
                                //    {
                                //        Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
                                //        string statusMessage = deserializeStatus["message"].ToString();

                                //        if (statusCode != 200)
                                //        {
                                //            PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType(statusCode, statusMessage);
                                //            int pubnubStatusCode = (int)pubnubErrorType;
                                //            string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);

                                //            PubnubClientError error = new PubnubClientError(pubnubStatusCode, PubnubErrorSeverity.Critical, statusMessage, PubnubMessageSource.Server, asyncRequestState.Request, asyncRequestState.Response, errorDescription, channel, channelGroup);
                                //            GoToCallback(error, asyncRequestState.ErrorCallback);
                                //        }
                                //    }
                                //}
                            }
                            #endregion

                            else if (jsonString != "[]")
                            {
                                bool errorCallbackRaised = false;
                                if (_jsonLib.IsDictionaryCompatible(jsonString))
                                {
                                    Dictionary<string, object> deserializeStatus = _jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                    int statusCode = 0; //default. assuming all is ok 
                                    if (deserializeStatus.ContainsKey("status") && deserializeStatus.ContainsKey("message"))
                                    {
                                        Int32.TryParse(deserializeStatus["status"].ToString(), out statusCode);
                                        string statusMessage = deserializeStatus["message"].ToString();

                                        if (statusCode != 200)
                                        {
                                            PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType(statusCode, statusMessage);
                                            int pubnubStatusCode = (int)pubnubErrorType;
                                            string errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);

                                            PubnubClientError error = new PubnubClientError(pubnubStatusCode, PubnubErrorSeverity.Critical, statusMessage, PubnubMessageSource.Server, asyncRequestState.Request, asyncRequestState.Response, errorDescription, channel, channelGroup);
                                            errorCallbackRaised = true;
                                            //GoToCallback(error, asyncRequestState.ErrorCallback);
                                        }
                                    }
                                }
                                if (!errorCallbackRaised)
                                {
                                    result = WrapResultBasedOnResponseType<T>(asyncRequestState.ResponseType, jsonString, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.Reconnect, asyncRequestState.Timetoken, asyncRequestState.Request, asyncRequestState.ErrorCallback);
                                }
                            }
                        }
#if !NETFX_CORE
                        //asyncWebResponse.Close ();
#endif
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0}, Request aborted for channel={1}, channel group={2}", DateTime.Now.ToString(), channel, channelGroup), LoggingMethod.LevelInfo);
                }

                ProcessResponseCallbacks<T>(result, asyncRequestState);

                #region FOR SUBSCRIBE AND PRESENCE
                if ((asyncRequestState.ResponseType == ResponseType.Subscribe || asyncRequestState.ResponseType == ResponseType.Presence) && (result != null) && (result.Count > 0))
                {
                    //if (asyncRequestState.Channels != null)
                    //{
                    //    foreach (string currentChannel in asyncRequestState.Channels)
                    //    {
                    //        multiChannelSubscribe.AddOrUpdate(currentChannel, Convert.ToInt64(result[1].ToString()), (key, oldValue) => Convert.ToInt64(result[1].ToString()));
                    //    }
                    //}
                    //if (asyncRequestState.ChannelGroups != null && asyncRequestState.ChannelGroups.Length > 0)
                    //{
                    //    foreach (string currentChannelGroup in asyncRequestState.ChannelGroups)
                    //    {
                    //        multiChannelGroupSubscribe.AddOrUpdate(currentChannelGroup, Convert.ToInt64(result[1].ToString()), (key, oldValue) => Convert.ToInt64(result[1].ToString()));
                    //    }
                    //}
                }

                switch (asyncRequestState.ResponseType)
                {
                    case ResponseType.Subscribe:
                    case ResponseType.Presence:
                        //MultiplexInternalCallback<T>(asyncRequestState.ResponseType, result, asyncRequestState.SubscribeRegularCallback, asyncRequestState.PresenceRegularCallback, asyncRequestState.ConnectCallback, asyncRequestState.WildcardPresenceCallback, asyncRequestState.ErrorCallback);
                        break;
                    default:
                        break;
                }
                #endregion

            }
            catch (WebException webEx)
            {
                HttpStatusCode currentHttpStatusCode;
                if (webEx.Response != null && asyncRequestState != null)
                {
                    if (webEx.Response.GetType().ToString() == "System.Net.HttpWebResponse"
                             || webEx.Response.GetType().ToString() == "MS.Internal.Modern.ClientHttpWebResponse"
                             || webEx.Response.GetType().ToString() == "System.Net.Browser.ClientHttpWebResponse")
                    {
                        currentHttpStatusCode = ((HttpWebResponse)webEx.Response).StatusCode;
                    }
                    else
                    {
                        currentHttpStatusCode = ((PubnubWebResponse)webEx.Response).HttpStatusCode;
                    }
                    PubnubWebResponse exceptionResponse = new PubnubWebResponse(webEx.Response, currentHttpStatusCode);
                    if (exceptionResponse != null)
                    {
                        asyncRequestState.Response = exceptionResponse;

                        using (StreamReader streamReader = new StreamReader(asyncRequestState.Response.GetResponseStream()))
                        {
                            string jsonString = streamReader.ReadToEnd();

#if !NETFX_CORE
                            //streamReader.Close ();
#endif

                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, JSON for channel={1} ({2}) ={3}", DateTime.Now.ToString(), channel, asyncRequestState.ResponseType.ToString(), jsonString), LoggingMethod.LevelInfo);

                            //if (overrideTcpKeepAlive)
                            //{
                            //    TerminateLocalClientHeartbeatTimer(asyncWebRequest.RequestUri);
                            //}

                            if ((int)currentHttpStatusCode < 200 || (int)currentHttpStatusCode >= 300)
                            {
                                result = null;
                                string errorDescription = "";
                                int pubnubStatusCode = 0;

                                if ((int)currentHttpStatusCode == 500 || (int)currentHttpStatusCode == 502 || (int)currentHttpStatusCode == 503 || (int)currentHttpStatusCode == 504 || (int)currentHttpStatusCode == 414)
                                {
                                    //This status code is not giving json string.
                                    string statusMessage = currentHttpStatusCode.ToString();
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }
                                else if (_jsonLib.IsArrayCompatible(jsonString))
                                {
                                    List<object> deserializeStatus = _jsonLib.DeserializeToListOfObject(jsonString);
                                    string statusMessage = deserializeStatus[1].ToString();
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }
                                else if (_jsonLib.IsDictionaryCompatible(jsonString))
                                {
                                    Dictionary<string, object> deserializeStatus = _jsonLib.DeserializeToDictionaryOfObject(jsonString);
                                    string statusMessage = deserializeStatus.ContainsKey("message") ? deserializeStatus["message"].ToString() : (deserializeStatus.ContainsKey("error") ? deserializeStatus["error"].ToString() : jsonString);
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, statusMessage);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }
                                else
                                {
                                    PubnubErrorCode pubnubErrorType = PubnubErrorCodeHelper.GetErrorType((int)currentHttpStatusCode, jsonString);
                                    pubnubStatusCode = (int)pubnubErrorType;
                                    errorDescription = PubnubErrorCodeDescription.GetStatusCodeDescription(pubnubErrorType);
                                }

                                PubnubClientError error = new PubnubClientError(pubnubStatusCode, PubnubErrorSeverity.Critical, jsonString, PubnubMessageSource.Server, asyncRequestState.Request, asyncRequestState.Response, errorDescription, channel, channelGroup);
                                //GoToCallback(error, asyncRequestState.ErrorCallback);

                            }
                            else if (jsonString != "[]")
                            {
                                result = WrapResultBasedOnResponseType<T>(asyncRequestState.ResponseType, jsonString, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.Reconnect, asyncRequestState.Timetoken, asyncRequestState.Request, asyncRequestState.ErrorCallback);
                            }
                            else
                            {
                                result = null;
                            }
                        }
                    }
#if !NETFX_CORE
                    //exceptionResponse.Close ();
#endif

                    if (result != null && result.Count > 0)
                    {
                        ProcessResponseCallbacks<T>(result, asyncRequestState);
                    }

                    //if (result == null && currentHttpStatusCode == HttpStatusCode.NotFound
                    //    && (asyncRequestState.ResponseType == ResponseType.Presence || asyncRequestState.ResponseType == ResponseType.Subscribe)
                    //    && webEx.Response.GetType().ToString() == "System.Net.Browser.ClientHttpWebResponse")
                    //{
                    //    ProcessResponseCallbackExceptionHandler(webEx, asyncRequestState);
                    //}
                }
                else
                {
                    
                    //ProcessResponseCallbackWebExceptionHandler<T>(webEx, asyncRequestState, channel, channelGroup);
                }
            }
            catch (Exception ex)
            {
                
                //ProcessResponseCallbackExceptionHandler<T>(ex, asyncRequestState);
            }
        }

        /// <summary>
        /// Gets the result by wrapping the json response based on the request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="jsonString"></param>
        /// <param name="channels"></param>
        /// <param name="reconnect"></param>
        /// <param name="lastTimetoken"></param>
        /// <param name="errorCallback"></param>
        /// <returns></returns>
        protected List<object> WrapResultBasedOnResponseType<T>(ResponseType type, string jsonString, string[] channels, string[] channelGroups, bool reconnect, long lastTimetoken, PubnubWebRequest request, Action<PubnubClientError> errorCallback)
        {
            List<object> result = new List<object>();

            try
            {
                string multiChannel = (channels != null) ? string.Join(",", channels) : "";
                string multiChannelGroup = (channelGroups != null) ? string.Join(",", channelGroups) : "";

                if (!string.IsNullOrEmpty(jsonString))
                {
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        object deSerializedResult = _jsonLib.DeserializeToObject(jsonString);
                        List<object> result1 = ((IEnumerable)deSerializedResult).Cast<object>().ToList();

                        if (result1 != null && result1.Count > 0)
                        {
                            result = result1;
                        }

                        switch (type)
                        {
                            case ResponseType.Time:
                                break;
                            default:
                                break;
                        }
                        ;//switch stmt end
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return result;
        }

        protected void OnPubnubWebRequestTimeout<T>(object state, bool timeout)
        {
            if (timeout && state != null)
            {
                RequestState<T> currentState = state as RequestState<T>;
                if (currentState != null)
                {
                    PubnubWebRequest request = currentState.Request;
                    if (request != null)
                    {
                        string currentMultiChannel = (currentState.Channels == null) ? "" : string.Join(",", currentState.Channels);
                        string currentMultiChannelGroup = (currentState.ChannelGroups == null) ? "" : string.Join(",", currentState.ChannelGroups);
                        LoggingMethod.WriteToLog(string.Format("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached.Request abort for channel={1} ;channelgroup={2}", DateTime.Now.ToString(), currentMultiChannel, currentMultiChannelGroup), LoggingMethod.LevelInfo);
                        currentState.Timeout = true;
                        //TerminatePendingWebRequest(currentState);
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime: {0}, OnPubnubWebRequestTimeout: client request timeout reached. However state is unknown", DateTime.Now.ToString()), LoggingMethod.LevelError);
                }
            }
        }

        protected void OnPubnubWebRequestTimeout<T>(System.Object requestState)
        {
            RequestState<T> currentState = requestState as RequestState<T>;
            if (currentState != null && currentState.Response == null && currentState.Request != null)
            {
                currentState.Timeout = true;
                //TerminatePendingWebRequest(currentState);
                LoggingMethod.WriteToLog(string.Format("DateTime: {0}, **WP7 OnPubnubWebRequestTimeout**", DateTime.Now.ToString()), LoggingMethod.LevelError);
            }
        }


        protected int GetTimeoutInSecondsForResponseType(ResponseType type)
        {
            int timeout;
            if (type == ResponseType.Subscribe || type == ResponseType.Presence)
            {
                timeout = _pnConfig.SubscribeTimeout;
            }
            else
            {
                timeout = _pnConfig.NonSubscribeRequestTimeout;
            }
            return timeout;
        }

        protected void ProcessResponseCallbacks<T>(List<object> result, RequestState<T> asyncRequestState)
        {
            bool callbackAvailable = false;
            if (result != null && result.Count >= 1)
            {
                if (asyncRequestState.SubscribeRegularCallback != null || asyncRequestState.PresenceRegularCallback != null || asyncRequestState.NonSubscribeRegularCallback != null)
                {
                    callbackAvailable = true;
                }
                else
                {
                    if (asyncRequestState.ResponseType == ResponseType.Subscribe || asyncRequestState.ResponseType == ResponseType.Presence)
                    {
                        if (asyncRequestState.Channels != null && asyncRequestState.Channels.Length > 0)
                        {
                            List<string> chList = asyncRequestState.Channels.ToList();
                            foreach (string ch in chList)
                            {
                                PubnubChannelCallbackKey callbackKey = new PubnubChannelCallbackKey();
                                callbackKey.Channel = ch;
                                callbackKey.ResponseType = asyncRequestState.ResponseType;

                                //if (channelCallbacks.Count > 0 && channelCallbacks.ContainsKey(callbackKey))
                                //{
                                //    callbackAvailable = true;
                                //    break;
                                //}
                            }
                        }
                        if (!callbackAvailable && asyncRequestState.ChannelGroups != null && asyncRequestState.ChannelGroups.Length > 0)
                        {
                            List<string> cgList = asyncRequestState.ChannelGroups.ToList();
                            foreach (string cg in cgList)
                            {
                                PubnubChannelGroupCallbackKey callbackKey = new PubnubChannelGroupCallbackKey();
                                callbackKey.ChannelGroup = cg;
                                callbackKey.ResponseType = asyncRequestState.ResponseType;

                                //if (channelGroupCallbacks.Count > 0 && channelGroupCallbacks.ContainsKey(callbackKey))
                                //{
                                //    callbackAvailable = true;
                                //    break;
                                //}
                            }
                        }
                    }
                }
            }
            if (callbackAvailable)
            {
                //ResponseToConnectCallback<T>(result, asyncRequestState.ResponseType, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.ConnectCallback);
                ResponseToUserCallback<T>(result, asyncRequestState.ResponseType, asyncRequestState.Channels, asyncRequestState.ChannelGroups, asyncRequestState.NonSubscribeRegularCallback);
            }
        }

        private void ResponseToUserCallback<T>(List<object> result, ResponseType type, string[] channels, string[] channelGroups, Action<T> userCallback)
        {
            string[] messageChannels = null;
            string[] messageChannelGroups = null;
            string[] messageWildcardPresenceChannels = null;
            switch (type)
            {
                case ResponseType.Time:
                    if (result != null && result.Count > 0)
                    {
                        GoToCallback<T>(result, userCallback, true, type);
                    }
                    break;
                default:
                    break;
            }
        }

        private void JsonResponseToCallback<T>(List<object> result, Action<T> callback)
        {
            string callbackJson = "";

            if (typeof(T) == typeof(string))
            {
                callbackJson = _jsonLib.SerializeToJsonString(result);

                Action<string> castCallback = callback as Action<string>;
                castCallback(callbackJson);
            }
        }

        private void JsonResponseToCallback<T>(object result, Action<T> callback)
        {
            string callbackJson = "";

            if (typeof(T) == typeof(string))
            {
                callbackJson = _jsonLib.SerializeToJsonString(result);

                Action<string> castCallback = callback as Action<string>;
                castCallback(callbackJson);
            }
        }

        private void JsonResponseToCallback<T>(long result, Action<T> callback)
        {
            if (typeof(T) == typeof(long))
            {
                Action<long> castCallback = callback as Action<long>;
                castCallback(result);
            }
        }

        //		protected void GoToCallback<T> (object result, Action<T> Callback)
        //		{
        //			if (Callback != null) {
        //				if (typeof(T) == typeof(string)) {
        //					JsonResponseToCallback (result, Callback);
        //				} else {
        //					Callback ((T)(object)result);
        //				}
        //			}
        //		}

        protected void GoToCallback<T>(List<object> result, Action<T> Callback, bool internalObject, ResponseType type)
        {
            if (Callback != null)
            {
                if (typeof(T) == typeof(string))
                {
                    JsonResponseToCallback(result, Callback);
                }
                else if (typeof(T) == typeof(long) && type == ResponseType.Time)
                {
                    long timetoken;
                    Int64.TryParse(result[0].ToString(), out timetoken);
                    JsonResponseToCallback(timetoken, Callback);
                }
                else
                {
                    T ret = default(T);
                    if (!internalObject)
                    {
                        ret = _jsonLib.DeserializeToObject<T>(result);
                    }
                    else
                    {
                        NewtonsoftJsonDotNet jsonLib = new NewtonsoftJsonDotNet();
                        ret = jsonLib.DeserializeToObject<T>(result);
                    }

                    Callback(ret);
                }
            }
        }

        protected void GoToCallback(object result, Action<string> Callback)
        {
            if (Callback != null)
            {
                JsonResponseToCallback(result, Callback);
            }
        }

        protected void GoToCallback(object result, Action<object> Callback)
        {
            if (Callback != null)
            {
                Callback(result);
            }
        }

        protected void GoToCallback(PubnubClientError error, Action<PubnubClientError> Callback)
        {
            //if (Callback != null && error != null)
            //{
            //    if ((int)error.Severity <= (int)_errorLevel)
            //    { //Checks whether the error serverity falls in the range of error filter level
            //      //Do not send 107 = PubnubObjectDisposedException
            //      //Do not send 105 = WebRequestCancelled
            //      //Do not send 130 = PubnubClientMachineSleep
            //        if (error.StatusCode != 107
            //            && error.StatusCode != 105
            //            && error.StatusCode != 130
            //            && error.StatusCode != 4040) //Error Code that should not go out
            //        {
            //            Callback(error);
            //        }
            //    }
            //}
        }

    }
}
