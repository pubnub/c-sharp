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
    internal class GenerateFileUploadUrlOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private Dictionary<string, object> queryParam;

        private string channelName;
        private string sendFileName;

        public GenerateFileUploadUrlOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public GenerateFileUploadUrlOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public GenerateFileUploadUrlOperation FileName(string fileName)
        {
            this.sendFileName = fileName;
            return this;
        }

        public GenerateFileUploadUrlOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNGenerateFileUploadUrlResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

            if (string.IsNullOrEmpty(this.sendFileName))
            {
                throw new ArgumentException("Missing File Name");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                GenerateFileUploadUrl(this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                GenerateFileUploadUrl(this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNGenerateFileUploadUrlResult>> ExecuteAsync()
        {
            return await GenerateFileUploadUrl(this.queryParam).ConfigureAwait(false);
        }

        private void GenerateFileUploadUrl(Dictionary<string, object> externalQueryParam, PNCallback<PNGenerateFileUploadUrlResult> callback)
        {
            RequestState<PNGenerateFileUploadUrlResult> requestState = new RequestState<PNGenerateFileUploadUrlResult>();
            requestState.ResponseType = PNOperationType.PNGenerateFileUploadUrlOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = true;

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(sendFileName))
            {
                messageEnvelope.Add("name", sendFileName);
            }
            string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] postData = Encoding.UTF8.GetBytes(postMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGenerateFileUploadUrlRequest("POST", postMessage, this.channelName, externalQueryParam);

            UrlProcessRequest(request, requestState, false, postData).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNGenerateFileUploadUrlResult>> GenerateFileUploadUrl(Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNGenerateFileUploadUrlResult> ret = new PNResult<PNGenerateFileUploadUrlResult>();
            if (string.IsNullOrEmpty(sendFileName))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid file name", new ArgumentException("Invalid file name")) };
                ret.Status = errStatus;
                return ret;
            }

            RequestState<PNGenerateFileUploadUrlResult> requestState = new RequestState<PNGenerateFileUploadUrlResult>();
            requestState.ResponseType = PNOperationType.PNGenerateFileUploadUrlOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = true;

            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(sendFileName))
            {
                messageEnvelope.Add("name", sendFileName);
            }
            string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] postData = Encoding.UTF8.GetBytes(postMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildGenerateFileUploadUrlRequest("POST", postMessage, this.channelName, externalQueryParam);

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, postData).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNGenerateFileUploadUrlResult responseResult = responseBuilder.JsonToObject<PNGenerateFileUploadUrlResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }

    }
}
