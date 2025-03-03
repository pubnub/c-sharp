using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
	public class GetFileUrlOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;

        private PNCallback<PNFileUrlResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private string channelName;
        private string currentFileId;
        private string currentFileName;

        public GetFileUrlOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            PubnubInstance = instance;
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
            logger.Trace($"{GetType().Name} Execute invoked");
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
            this.savedCallback = callback;
            logger.Debug($"{GetType().Name} parameter validated.");
            ProcessGetFileUrl(this.queryParam, savedCallback);
        }

        public async Task<PNResult<PNFileUrlResult>> ExecuteAsync()
        {
            logger.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await ProcessGetFileUrl(this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
            ProcessGetFileUrl(this.queryParam, savedCallback);
        }

        private void ProcessGetFileUrl(Dictionary<string, object> externalQueryParam, PNCallback<PNFileUrlResult> callback)
        {
            var requestParameter = CreateRequestParameter();
            PNFileUrlResult result = new PNFileUrlResult();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNFileUrlOperation);
            result.Url = transportRequest.RequestUrl;
            PNStatus status = new PNStatus { Error = false, StatusCode = 200 };
            logger.Info($"{GetType().Name} request finished with status code {status.StatusCode}");
            callback.OnResponse(result, status);
        }

        private Task<PNResult<PNFileUrlResult>> ProcessGetFileUrl(Dictionary<string, object> externalQueryParam)
        {
            var requestParameter = CreateRequestParameter();
            PNResult<PNFileUrlResult> returnValue = new PNResult<PNFileUrlResult>();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNFileUrlOperation);

            if (string.IsNullOrEmpty(this.currentFileId))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File Id", new ArgumentException("Missing File Id")) };
                returnValue.Status = errStatus;
                return Task.FromResult(returnValue);
            }

            if (string.IsNullOrEmpty(this.currentFileName))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid file name", new ArgumentException("Invalid file name")) };
                returnValue.Status = errStatus;
                return Task.FromResult(returnValue);
            }

            PNFileUrlResult result = new PNFileUrlResult();
            result.Url = transportRequest.RequestUrl;

            PNStatus status = new PNStatus { Error = false, StatusCode = 200 };

            returnValue.Result = result;
            returnValue.Status = status;
            logger.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
            return Task.FromResult(returnValue);
        }

        private RequestParameter CreateRequestParameter()
        {
			List<string> pathSegments = new List<string>
			{
				"v1",
				"files",
				config.SubscribeKey,
				"channels",
				channelName,
				"files",
				currentFileId,
				currentFileName
			};

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (queryParam != null && queryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in queryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNDownloadFileOperation, false, false, false));
                    }
                }
            }

            var requestParameter = new RequestParameter() {
                RequestType = Constants.GET,
                PathSegment = pathSegments,
                Query = requestQueryStringParams
            };

            return requestParameter;
        }
    }
}
