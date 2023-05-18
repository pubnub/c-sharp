using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class PublishFileMessageOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private PNCallback<PNPublishFileMessageResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private string channelName;
        private string currentFileId;
        private string currentFileName;
        private object msg;
        private bool storeInHistory = true;
        private Dictionary<string, object> userMetadata;
        private int ttl = -1;

        public PublishFileMessageOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public PublishFileMessageOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public PublishFileMessageOperation Message(object message)
        {
            this.msg = message;
            return this;
        }

        public PublishFileMessageOperation ShouldStore(bool store)
        {
            this.storeInHistory = store;
            return this;
        }

        public PublishFileMessageOperation Meta(Dictionary<string, object> metadata)
        {
            this.userMetadata = metadata;
            return this;
        }

        public PublishFileMessageOperation Ttl(int ttl)
        {
            this.ttl = ttl;
            return this;
        }

        public PublishFileMessageOperation FileId(string id)
        {
            this.currentFileId = id;
            return this;
        }

        public PublishFileMessageOperation FileName(string name)
        {
            this.currentFileName = name;
            return this;
        }

        public PublishFileMessageOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNPublishFileMessageResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

            if (string.IsNullOrEmpty(this.currentFileId) || string.IsNullOrEmpty(this.currentFileName))
            {
                throw new ArgumentException("Missing File Id or Name");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                ProcessFileMessagePublish(this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                ProcessFileMessagePublish(this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNPublishFileMessageResult>> ExecuteAsync()
        {
            return await ProcessFileMessagePublish(this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                ProcessFileMessagePublish(this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                ProcessFileMessagePublish(this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void ProcessFileMessagePublish(Dictionary<string, object> externalQueryParam, PNCallback<PNPublishFileMessageResult> callback)
        {
            if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim()))
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message"));
                callback.OnResponse(null, status);
                return;
            }

            Dictionary<string, object> publishPayload = new Dictionary<string, object>();
            if (this.msg != null && !string.IsNullOrEmpty(this.msg.ToString()))
            {
                publishPayload.Add("message", this.msg);
            }
            publishPayload.Add("file", new Dictionary<string, string> {
                        { "id", currentFileId },
                        { "name", currentFileName } });

            RequestState<PNPublishFileMessageResult> requestState = new RequestState<PNPublishFileMessageResult>();
            requestState.ResponseType = PNOperationType.PNPublishFileMessageOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;


            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildPublishFileMessageRequest("GET", "", this.channelName, publishPayload, this.storeInHistory, this.ttl, this.userMetadata, null, externalQueryParam);

            string json = "";
            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                json = r.Result.Item1;
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse(requestState, json);

                if (result != null && result.Count >= 3)
                {
                    int publishStatus;
                    var _ = Int32.TryParse(result[0].ToString(), out publishStatus);
                    if (publishStatus == 1)
                    {
                        ProcessResponseCallbacks(result, requestState);
                    }
                    else
                    {
                        PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(400, result[1].ToString());
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishFileMessageResult>(PNOperationType.PNPublishFileMessageOperation, category, requestState, 400, new PNException(json));
                        if (requestState.PubnubCallback != null)
                        {
                            requestState.PubnubCallback.OnResponse(default(PNPublishFileMessageResult), status);
                        }
                    }
                }
                else
                {
                    ProcessResponseCallbacks(result, requestState);
                }
            }
        }

        private async Task<PNResult<PNPublishFileMessageResult>> ProcessFileMessagePublish(Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNPublishFileMessageResult> ret = new PNResult<PNPublishFileMessageResult>();
            if (string.IsNullOrEmpty(this.currentFileId) || string.IsNullOrEmpty(this.currentFileName))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File Id or Name", new ArgumentException("Missing File Id or Name")) };
                ret.Status = errStatus;
                return ret;
            }
            if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim()))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel", new ArgumentException("Missing Channel")) };
                ret.Status = errStatus;
                return ret;
            }

            Dictionary<string, object> publishPayload = new Dictionary<string, object>();
            if (this.msg != null && !string.IsNullOrEmpty(this.msg.ToString()))
            {
                publishPayload.Add("message", this.msg);
            }
            publishPayload.Add("file", new Dictionary<string, string> {
                        { "id", currentFileId },
                        { "name", currentFileName } });

            RequestState<PNPublishFileMessageResult> requestState = new RequestState<PNPublishFileMessageResult>();
            requestState.ResponseType = PNOperationType.PNPublishFileMessageOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildPublishFileMessageRequest("GET", "", this.channelName, publishPayload, this.storeInHistory, this.ttl, this.userMetadata, null, externalQueryParam);

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse(requestState, json);

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
                            PNPublishFileMessageResult responseResult = responseBuilder.JsonToObject<PNPublishFileMessageResult>(resultList, true);
                            if (responseResult != null)
                            {
                                ret.Result = responseResult;
                            }
                        }
                    }
                }
            }

            return ret;
        }

    }
}
