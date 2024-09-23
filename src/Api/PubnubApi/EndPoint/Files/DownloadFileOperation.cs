using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;
using static System.Net.WebRequestMethods;
using System.Threading.Channels;

namespace PubnubApi.EndPoint
{
    public class DownloadFileOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;

        private PNCallback<PNDownloadFileResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private string channelName;
        private string currentFileId;
        private string currentFileName;
        private string currentFileCipherKey;

        public DownloadFileOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;

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

        public DownloadFileOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public DownloadFileOperation FileId(string fileId)
        {
            this.currentFileId = fileId;
            return this;
        }

        public DownloadFileOperation FileName(string fileName)
        {
            this.currentFileName = fileName;
            return this;
        }

        public DownloadFileOperation CipherKey(string cipherKeyForFile)
        {
            this.currentFileCipherKey = cipherKeyForFile;
            return this;
        }

        public DownloadFileOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNDownloadFileResult> callback)
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
                ProcessFileDownloadRequest(this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                ProcessFileDownloadRequest(this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNDownloadFileResult>> ExecuteAsync()
        {
            return await ProcessFileDownloadRequest(this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                ProcessFileDownloadRequest(this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                ProcessFileDownloadRequest(this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void ProcessFileDownloadRequest(Dictionary<string, object> externalQueryParam, PNCallback<PNDownloadFileResult> callback)
        {
            var requestParameter = CreateRequestParameter();
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildGetFileUrlOrDeleteReqest(Constants.GET, "", this.channelName, this.currentFileId, this.currentFileName, externalQueryParam, PNOperationType.PNDownloadFileOperation);

            RequestState<PNDownloadFileResult> requestState = new RequestState<PNDownloadFileResult>();
            requestState.ResponseType = PNOperationType.PNDownloadFileOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            byte[] item1Bytes = null;
            UrlProcessRequestForStream(request, requestState, false,"").ContinueWith(r =>
            {
                item1Bytes = r.Result.Item1;
                if (item1Bytes != null)
                {
                    byte[] outputBytes = null;
                    if (string.IsNullOrEmpty(this.currentFileCipherKey) && string.IsNullOrEmpty(config.CipherKey) && config.CryptoModule == null)
                    {
                        outputBytes = item1Bytes;
                    }
                    else
                    {
                        CryptoModule currentCryptoModule = !string.IsNullOrEmpty(this.currentFileCipherKey) ? new CryptoModule(new LegacyCryptor(this.currentFileCipherKey, true, pubnubLog), null) : (config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, true, pubnubLog), null));
                        try
                        {
                            outputBytes = currentCryptoModule.Decrypt(item1Bytes);
                            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Stream length (after Decrypt)= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), item1Bytes.Length), config.LogVerbosity);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("{0}\nMessage might be not encrypted, returning as is...", ex.ToString());
                            outputBytes = item1Bytes;
                        }
                    }
                    PNDownloadFileResult result = new PNDownloadFileResult();
                    result.FileBytes = outputBytes;
                    result.FileName = currentFileName;
                    callback.OnResponse(result, r.Result.Item2);
                }
                else
                {
                    if (r.Result.Item2 != null)
                    {
                        callback.OnResponse(null, r.Result.Item2);
                    }
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();


        }

        private async Task<PNResult<PNDownloadFileResult>> ProcessFileDownloadRequest(Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNDownloadFileResult> ret = new PNResult<PNDownloadFileResult>();

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

            var requestParameter = CreateRequestParameter();
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildGetFileUrlOrDeleteReqest(Constants.GET, "", this.channelName, this.currentFileId, this.currentFileName, externalQueryParam, PNOperationType.PNDownloadFileOperation);

            RequestState<PNDownloadFileResult> requestState = new RequestState<PNDownloadFileResult>();
            requestState.ResponseType = PNOperationType.PNDownloadFileOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            Tuple<byte[], PNStatus> JsonAndStatusTuple = await UrlProcessRequestForStream(request, requestState, false,"").ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            byte[] item1Bytes = JsonAndStatusTuple.Item1;
            if (item1Bytes != null)
            {
                byte[] outputBytes = null;
                if (string.IsNullOrEmpty(this.currentFileCipherKey) && string.IsNullOrEmpty(config.CipherKey) && config.CryptoModule == null)
                {
                    outputBytes = item1Bytes;
                }
                else
                {
                    CryptoModule currentCryptoModule = !string.IsNullOrEmpty(this.currentFileCipherKey) ? new CryptoModule(new LegacyCryptor(this.currentFileCipherKey, true, pubnubLog), null) : (config.CryptoModule ??= new CryptoModule(new LegacyCryptor(config.CipherKey, true, pubnubLog), null));
                    try
                    {
                        outputBytes = currentCryptoModule.Decrypt(item1Bytes);
                        LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Stream length (after Decrypt)= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), item1Bytes.Length), config.LogVerbosity);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("{0}\nMessage might be not encrypted, returning as is...", ex.ToString());
                        outputBytes = item1Bytes;
                        ret.Status = new PNStatus { Error = true, ErrorData = new PNErrorData("Decryption error", ex) };
                    }
                }

                PNDownloadFileResult result = new PNDownloadFileResult();
                result.FileBytes = outputBytes;
                result.FileName = currentFileName;
                ret.Result = result;
            }

            return ret;
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

            string queryString = UriUtil.BuildQueryString(requestQueryStringParams);

            var requestParameter = new RequestParameter() {
                RequestType = Constants.GET,
                PathSegment = pathSegments,
                Query = requestQueryStringParams
            };

            return requestParameter;
        }
    }
}
