using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class SignalOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private object msg;
        private string channelName = "";
        private PNCallback<PNPublishResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public SignalOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

            if (instance != null)
            {
                if (!ChannelRequest.ContainsKey(instance.InstanceId))
                {
                    ChannelRequest.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
                }
                if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
                {
                    ChannelInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
                }
                if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
                {
                    ChannelGroupInternetStatus.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, bool>());
                }
            }
        }

        public SignalOperation Message(object message)
        {
            msg = message;
            return this;
        }

        public SignalOperation Channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        public SignalOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNPublishResult> callback)
        {
            if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(channelName.Trim()) || this.msg == null)
            {
                throw new ArgumentException("Missing Channel or Message");
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid publish key");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                try
                {
                    Signal(this.channelName, this.msg, null, this.queryParam, callback);
                }
                catch(Exception ex)
                {
                    PNStatus unexpectedExceptionStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Unexpected exception", ex) };
                    callback.OnResponse(null, unexpectedExceptionStatus);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                try
                {
                    Signal(this.channelName, this.msg, null, this.queryParam, callback);
                }
                catch(Exception ex)
                {
                    PNStatus unexpectedExceptionStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Unexpected exception", ex) };
                    callback.OnResponse(null, unexpectedExceptionStatus);
                }
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNPublishResult>> ExecuteAsync()
        {
            return await Signal(this.channelName, this.msg, null, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                Signal(this.channelName, this.msg, null, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                Signal(this.channelName, this.msg, null, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void Signal(string channel, object message, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam, PNCallback<PNPublishResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
            {
                PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message"))};
                callback.OnResponse(null, status);
                return;
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
            {
                PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid publish key", new ArgumentException("Invalid publish key")) };
                callback.OnResponse(null, status);
                return;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid subscribe key", new ArgumentException("Invalid subscribe key")) };
                callback.OnResponse(null, status);
                return;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildSignalRequest("GET", "", channel, message, metaData, externalQueryParam);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNSignalOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);

                    if (result != null && result.Count >= 3)
                    {
                        int signalStatus;
                        var _ = Int32.TryParse(result[0].ToString(), out signalStatus);
                        if (signalStatus == 1)
                        {
                            ProcessResponseCallbacks(result, requestState);
                        }
                        else
                        {
                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(PNOperationType.PNSignalOperation, category, requestState, 400, new PNException(json));
                            if (requestState.PubnubCallback != null)
                            {
                                requestState.PubnubCallback.OnResponse(default(PNPublishResult), status);
                            }
                        }
                    }
                    else
                    {
                        ProcessResponseCallbacks(result, requestState);
                    }
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNPublishResult>> Signal(string channel, object message, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNPublishResult> ret = new PNResult<PNPublishResult>();

            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message")) };
                ret.Status = errStatus;
                return ret;
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid publish key", new ArgumentException("Invalid publish key")) };
                ret.Status = errStatus;
                return ret;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid subscribe key", new ArgumentException("Invalid subscribe key")) };
                ret.Status = errStatus;
                return ret;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildSignalRequest("GET", "", channel, message, metaData, externalQueryParam);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNSignalOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPublishResult>(requestState, json);
                if (result != null && result.Count >= 3)
                {
                    int publishStatus;
                    var _ = Int32.TryParse(result[0].ToString(), out publishStatus);
                    if (publishStatus == 1)
                    {
                        List<object> resultList = ProcessJsonResponse(requestState, json);
                        if (resultList != null && resultList.Count > 0)
                        {
                            ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                            PNPublishResult responseResult = responseBuilder.JsonToObject<PNPublishResult>(resultList, true);
                            if (responseResult != null)
                            {
                                ret.Result = responseResult;
                            }
                        }
                        else
                        {
                            PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(PNOperationType.PNSignalOperation, category, requestState, 400, new PNException(json));
                            ret.Status = status;
                            ret.Result = default(PNPublishResult);
                        }
                    }
                }
            }

            return ret;
        }
    }
}
