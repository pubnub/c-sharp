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
    public class FireOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private object msg;
        private string channelName = "";
        private bool httpPost;
        private Dictionary<string, object> userMetadata;
        private readonly int ttl = -1;
        private PNCallback<PNPublishResult> savedCallback;
        private bool syncRequest;
        private Dictionary<string, object> queryParam;

        public FireOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        public FireOperation Message(object message)
        {
            this.msg = message;
            return this;
        }

        public FireOperation Channel(string channelName)
        {
            this.channelName = channelName;
            return this;
        }

        public FireOperation Meta(Dictionary<string, object> metadata)
        {
            this.userMetadata = metadata;
            return this;
        }

        public FireOperation UsePOST(bool post)
        {
            this.httpPost = post;
            return this;
        }

        public FireOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNPublishResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNPublishResult> callback)
        {
            if (string.IsNullOrEmpty(this.channelName) || string.IsNullOrEmpty(this.channelName.Trim()) || this.msg == null)
            {
                throw new ArgumentException("Missing Channel or Message");
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid publish key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                syncRequest = false;
                this.savedCallback = callback;
                Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                syncRequest = false;
                this.savedCallback = callback;
                Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNPublishResult>> ExecuteAsync()
        {
            syncRequest = false;
            return await Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam).ConfigureAwait(false);
        }

        public PNPublishResult Sync()
        {
            System.Threading.ManualResetEvent syncEvent = new System.Threading.ManualResetEvent(false);
            Task<PNPublishResult> task = Task<PNPublishResult>.Factory.StartNew(() =>
            {
                syncRequest = true;
                syncEvent = new System.Threading.ManualResetEvent(false);
                Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, new PNPublishResultExt((r,s)=> { SyncResult = r; syncEvent.Set(); }));
                syncEvent.WaitOne(config.NonSubscribeRequestTimeout * 1000);

                return SyncResult;
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
            return task.Result;
        }

        private static PNPublishResult SyncResult { get; set; }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                if (!syncRequest)
                {
                    Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, savedCallback);
                }
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                if (!syncRequest)
                {
                    Fire(this.channelName, this.msg, false, this.ttl, this.userMetadata, this.queryParam, savedCallback);
                }
            })
            { IsBackground = true }.Start();
#endif
        }

        private void Fire(string channel, object message, bool storeInHistory, int ttl, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam, PNCallback<PNPublishResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || message == null)
            {
                PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing Channel or Message", new ArgumentException("Missing Channel or Message")) };
                callback.OnResponse(null, status);
                return;
            }

            if (string.IsNullOrEmpty(config.PublishKey) || string.IsNullOrEmpty(config.PublishKey.Trim()) || config.PublishKey.Length <= 0)
            {
                PNStatus status = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid publish key", new ArgumentException("Invalid publish key")) };
                callback.OnResponse(null, status);
                return;
            }

            if (callback == null)
            {
                return;
            }

            Dictionary<string, string> urlParam = new Dictionary<string, string>();
            urlParam.Add("norep", "true");

            string requestMethodName = (this.httpPost) ? "POST" : "GET";
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildPublishRequest(requestMethodName, "", channel, message, storeInHistory, ttl, metaData, urlParam, externalQueryParam);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new [] { channel };
            requestState.ResponseType = PNOperationType.PNFireOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = "";

            if (this.httpPost)
            {
                requestState.UsePostMethod = true;
                Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
                messageEnvelope.Add("message", message);
                string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
                byte[] postData = Encoding.UTF8.GetBytes(postMessage);
                UrlProcessRequest(request, requestState, false, postData).ContinueWith(r =>
                {
                    json = r.Result.Item1;
                }, TaskContinuationOptions.ExecuteSynchronously).Wait();
            }
            else
            {
                UrlProcessRequest(request, requestState, false).ContinueWith(r =>
                {
                    json = r.Result.Item1;
                }, TaskContinuationOptions.ExecuteSynchronously).Wait();
            }

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPublishResult>(requestState, json);
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
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<PNPublishResult>(PNOperationType.PNFireOperation, category, requestState, 400, new PNException(json));
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
        }

        private async Task<PNResult<PNPublishResult>> Fire(string channel, object message, bool storeInHistory, int ttl, Dictionary<string, object> metaData, Dictionary<string, object> externalQueryParam)
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

            Dictionary<string, string> urlParam = new Dictionary<string, string>();
            urlParam.Add("norep", "true");

            string requestMethodName = (this.httpPost) ? "POST" : "GET";
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildPublishRequest(requestMethodName, "", channel, message, storeInHistory, ttl, metaData, urlParam, externalQueryParam);

            RequestState<PNPublishResult> requestState = new RequestState<PNPublishResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNFireOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<string, PNStatus> JsonAndStatusTuple;

            if (this.httpPost)
            {
                requestState.UsePostMethod = true;
                Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
                messageEnvelope.Add("message", message);
                string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
                byte[] postData = Encoding.UTF8.GetBytes(postMessage);
                JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, postData).ConfigureAwait(false);
            }
            else
            {
                JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            }

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
                    }
                }
            }

            return ret;
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

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

}
