using System;
using System.Collections.Generic;
using System.IO;
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
    public class GetFileUrlOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private PNCallback<PNFileUrlResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private string channelName;
        private string currentFileId;
        private string currentFileName;

        public GetFileUrlOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public GetFileUrlOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public GetFileUrlOperation FileId(string fileId)
        {
            this.currentFileId = fileId;
            return this;
        }

        public GetFileUrlOperation FileName(string fileName)
        {
            this.currentFileName = fileName;
            return this;
        }

        public GetFileUrlOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNFileUrlResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }
            if (string.IsNullOrEmpty(this.currentFileId))
            {
                throw new ArgumentException("Missing File Id");
            }
            if (string.IsNullOrEmpty(this.currentFileName))
            {
                throw new ArgumentException("Missing File Name");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                ProcessGetFileUrl(this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                ProcessGetFileUrl(this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNFileUrlResult>> ExecuteAsync()
        {
            return await ProcessGetFileUrl(this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                ProcessGetFileUrl(this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                ProcessGetFileUrl(this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void ProcessGetFileUrl(Dictionary<string, object> externalQueryParam, PNCallback<PNFileUrlResult> callback)
        {
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildGetFileUrlOrDeleteReqest("GET", "", this.channelName, this.currentFileId, this.currentFileName, externalQueryParam, PNOperationType.PNFileUrlOperation);

            PNFileUrlResult result = new PNFileUrlResult();
            result.Url = request.ToString();

            PNStatus status = new PNStatus { Error = false, StatusCode = 200 };

            callback.OnResponse(result, status);
        }

        private async Task<PNResult<PNFileUrlResult>> ProcessGetFileUrl(Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNFileUrlResult> ret = new PNResult<PNFileUrlResult>();

            if (string.IsNullOrEmpty(this.currentFileId))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File Id", new ArgumentException("Missing File Id")) };
                ret.Status = errStatus;
                return ret;
            }

            if (string.IsNullOrEmpty(this.currentFileName))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid file name", new ArgumentException("Invalid file name")) };
                ret.Status = errStatus;
                return ret;
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildGetFileUrlOrDeleteReqest("GET", "", this.channelName, this.currentFileId, this.currentFileName, externalQueryParam, PNOperationType.PNFileUrlOperation);

            PNFileUrlResult result = new PNFileUrlResult();
            result.Url = request.ToString();

            PNStatus status = new PNStatus { Error = false, StatusCode = 200 };

            ret.Result = result;
            ret.Status = status;
            await Task.Factory.StartNew(() =>{ }).ConfigureAwait(false);//dummy stmt.

            return ret;
        }

    }
}
