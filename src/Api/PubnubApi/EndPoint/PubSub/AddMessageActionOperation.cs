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
    public class AddMessageActionOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string msgActionChannelName = "";
        private long messageTimetoken;
        private PNMessageAction addMessageAction;
        private PNCallback<PNAddMessageActionResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public AddMessageActionOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

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


        public AddMessageActionOperation Channel(string channelName)
        {
            msgActionChannelName = channelName;
            return this;
        }

        /// <summary>
        /// The publish timetoken of a parent message
        /// </summary>
        /// <param name="timetoken"></param>
        /// <returns></returns>
        public AddMessageActionOperation MessageTimetoken(long timetoken)
        {
            messageTimetoken = timetoken;
            return this;
        }

        public AddMessageActionOperation Action(PNMessageAction messageAction)
        {
            addMessageAction = messageAction;
            return this;
        }

        public AddMessageActionOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNAddMessageActionResult> callback)
        {
            if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0)
            {
                throw new MissingMemberException("subscribe key is required");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                Publish(this.msgActionChannelName, this.messageTimetoken, this.addMessageAction, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                Publish(this.msgActionChannelName, this.messageTimetoken, this.addMessageAction, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNAddMessageActionResult>> ExecuteAsync()
        {
            if (config == null || string.IsNullOrEmpty(config.SubscribeKey) || config.SubscribeKey.Trim().Length <= 0)
            {
                throw new MissingMemberException("subscribe key is required");
            }

            return await Publish(this.msgActionChannelName, this.messageTimetoken, this.addMessageAction, this.queryParam).ConfigureAwait(false);
        }


        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                Publish(this.msgActionChannelName, this.messageTimetoken, this.addMessageAction, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                Publish(this.msgActionChannelName, this.messageTimetoken, this.addMessageAction, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void Publish(string channel, long messageTimetoken, PNMessageAction messageAction, Dictionary<string, object> externalQueryParam, PNCallback<PNAddMessageActionResult> callback)
        {
            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || messageAction == null)
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Missing Channel or MessageAction", new ArgumentException("Missing Channel or MessageAction"));
                callback.OnResponse(null, status);
                return;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Invalid subscribe key", new MissingMemberException("Invalid subscribe key"));
                callback.OnResponse(null, status);
                return;
            }

            if (callback == null)
            {
                return;
            }

            string requestMethodName = "POST";
            string postMessage = jsonLibrary.SerializeToJsonString(messageAction);
            byte[] postData = Encoding.UTF8.GetBytes(postMessage);
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildAddMessageActionRequest(requestMethodName, postMessage, channel, messageTimetoken, externalQueryParam);

            RequestState<PNAddMessageActionResult> requestState = new RequestState<PNAddMessageActionResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNAddMessageActionOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;
            requestState.UsePostMethod = true;

            UrlProcessRequest(request, requestState, false, postData).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);

                    ProcessResponseCallbacks(result, requestState);
                }
                CleanUp();
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNAddMessageActionResult>> Publish(string channel, long messageTimetoken, PNMessageAction messageAction, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNAddMessageActionResult> ret = new PNResult<PNAddMessageActionResult>();

            if (string.IsNullOrEmpty(channel) || string.IsNullOrEmpty(channel.Trim()) || messageAction == null)
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Missing Channel or MessageAction", new ArgumentException("Missing Channel or MessageAction"));
                ret.Status = status;
                return ret;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus status = new PNStatus();
                status.Error = true;
                status.ErrorData = new PNErrorData("Invalid subscribe key", new MissingMemberException("Invalid subscribe key"));
                ret.Status = status;
                return ret;
            }

            string requestMethodName = "POST";
            string postMessage = jsonLibrary.SerializeToJsonString(messageAction);
            byte[] postData = Encoding.UTF8.GetBytes(postMessage);
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildAddMessageActionRequest(requestMethodName, postMessage, channel, messageTimetoken, externalQueryParam);

            RequestState<PNAddMessageActionResult> requestState = new RequestState<PNAddMessageActionResult>();
            requestState.Channels = new[] { channel };
            requestState.ResponseType = PNOperationType.PNAddMessageActionOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;
            requestState.UsePostMethod = true;

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, postData).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNAddMessageActionResult responseResult = responseBuilder.JsonToObject<PNAddMessageActionResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }

        private void CleanUp()
        {
            this.savedCallback = null;
        }
    }

}
