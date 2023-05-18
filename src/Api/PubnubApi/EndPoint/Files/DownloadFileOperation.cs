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

namespace PubnubApi.EndPoint
{
    public class DownloadFileOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private PNCallback<PNDownloadFileResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private string channelName;
        private string currentFileId;
        private string currentFileName;
        private string currentFileCipherKey;

        public DownloadFileOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildGetFileUrlOrDeleteReqest("GET", "", this.channelName, this.currentFileId, this.currentFileName, externalQueryParam, PNOperationType.PNDownloadFileOperation);

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
                    string currentCipherKey = !string.IsNullOrEmpty(this.currentFileCipherKey) ? this.currentFileCipherKey : config.CipherKey;
                    if (currentCipherKey.Length > 0)
                    {
                        try
                        {
                            PubnubCrypto aes = new PubnubCrypto(currentCipherKey, config, pubnubLog, null);
                            outputBytes = aes.Decrypt(item1Bytes, true);
                            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Stream length (after Decrypt)= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), item1Bytes.Length), config.LogVerbosity);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                    }
                    else
                    {
                        outputBytes = item1Bytes;
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


            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildGetFileUrlOrDeleteReqest("GET", "", this.channelName, this.currentFileId, this.currentFileName, externalQueryParam, PNOperationType.PNDownloadFileOperation);

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
                string currentCipherKey = !string.IsNullOrEmpty(this.currentFileCipherKey) ? this.currentFileCipherKey : config.CipherKey;
                if (currentCipherKey.Length > 0)
                {
                    try
                    {
                        PubnubCrypto aes = new PubnubCrypto(currentCipherKey, config, pubnubLog, null);
                        outputBytes = aes.Decrypt(item1Bytes, true);
                        LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, Stream length (after Decrypt)= {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), item1Bytes.Length), config.LogVerbosity);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    outputBytes = item1Bytes;
                }
                
                PNDownloadFileResult result = new PNDownloadFileResult();
                result.FileBytes = outputBytes;
                result.FileName = currentFileName;
                ret.Result = result;
            }

            return ret;
        }

    }
}
