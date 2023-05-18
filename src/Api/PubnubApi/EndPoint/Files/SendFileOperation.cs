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
    public class SendFileOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private Dictionary<string, object> queryParam;

        private string channelName;
        private object publishMessage;
        private string sendFileFullPath;
        private string sendFileName = "";
        private byte[] sendFileBytes = null;
        private string currentFileCipherKey;
        private string currentFileId;
        private bool storeInHistory = true;
        private Dictionary<string, object> userMetadata;
        private int ttl = -1;

        public SendFileOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public SendFileOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public SendFileOperation Message(object message)
        {
            this.publishMessage = message;
            return this;
        }

        public SendFileOperation ShouldStore(bool store)
        {
            this.storeInHistory = store;
            return this;
        }

        public SendFileOperation Meta(Dictionary<string, object> metadata)
        {
            this.userMetadata = metadata;
            return this;
        }

        public SendFileOperation Ttl(int ttl)
        {
            this.ttl = ttl;
            return this;
        }

        public SendFileOperation File(string fileNameWithFullPath)
        {
            this.sendFileFullPath = fileNameWithFullPath;
#if !NETSTANDARD10 && !NETSTANDARD11
            // manually set filename should take precedence
            if (System.IO.File.Exists(fileNameWithFullPath) && string.IsNullOrEmpty(sendFileName))
            {
                sendFileName = System.IO.Path.GetFileName(fileNameWithFullPath);
            }
            return this;
#else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif      
        }

        public SendFileOperation File(byte[] byteArray)
        {
            this.sendFileBytes = byteArray ?? throw new ArgumentException("File byte array not provided.");
            return this;
        }

        public SendFileOperation FileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name is missing");
            }

            if (fileName.Trim() != fileName)
            {
                throw new ArgumentException("File name should not contain leading or trailing whitespace");
            }
            
            this.sendFileName = fileName;
            return this;
        }

        public SendFileOperation CipherKey(string cipherKeyForFile)
        {
            this.currentFileCipherKey = cipherKeyForFile;
            return this;
        }

        public SendFileOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNFileUploadResult> callback)
        {
            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

            if (string.IsNullOrEmpty(this.sendFileName))
            {
                throw new ArgumentException("Missing File");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                ProcessFileUpload(this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                ProcessFileUpload(this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNFileUploadResult>> ExecuteAsync()
        {
            return await ProcessFileUpload(this.queryParam).ConfigureAwait(false);
        }

        private void ProcessFileUpload(Dictionary<string, object> externalQueryParam, PNCallback<PNFileUploadResult> callback)
        {
            PNResult<PNGenerateFileUploadUrlResult> generateFileUploadUrl = GenerateFileUploadUrl(externalQueryParam).Result;
            PNGenerateFileUploadUrlResult generateFileUploadUrlResult = generateFileUploadUrl.Result;
            PNStatus generateFileUploadUrlStatus = generateFileUploadUrl.Status;
            if (generateFileUploadUrlStatus.Error || generateFileUploadUrlResult == null)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Error in GenerateFileUploadUrl. Try again.", new ArgumentException("Error in GenerateFileUploadUrl. Try again.")) };
                if (callback != null)
                {
                    callback.OnResponse(null, errStatus);
                }
                return;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);

            RequestState<PNFileUploadResult> requestState = new RequestState<PNFileUploadResult>();
            requestState.ResponseType = PNOperationType.PNFileUploadOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = true;

            byte[] sendFileByteArray = sendFileBytes ?? GetByteArrayFromFilePath(sendFileFullPath);

            
            string dataBoundary = String.Format(CultureInfo.InvariantCulture, "----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + dataBoundary;
            string currentCipherKey = !string.IsNullOrEmpty(this.currentFileCipherKey) ? this.currentFileCipherKey : config.CipherKey;
            byte[] postData = GetMultipartFormData(sendFileByteArray,generateFileUploadUrlResult.FileName, generateFileUploadUrlResult.FileUploadRequest.FormFields, dataBoundary, currentCipherKey, config, pubnubLog);

            string json;
            UrlProcessRequest(new Uri(generateFileUploadUrlResult.FileUploadRequest.Url), requestState, false, postData, contentType).ContinueWith(r =>
            {
                json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json) && string.Equals(json,"{}", StringComparison.OrdinalIgnoreCase))
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl -> file upload OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                    //do internal publish after successful file upload

                    Dictionary<string, object> publishPayload = new Dictionary<string, object>();
                    if (this.publishMessage != null && !string.IsNullOrEmpty(this.publishMessage.ToString())){
                        publishPayload.Add("message", this.publishMessage);
                    }
                    publishPayload.Add("file", new Dictionary<string, string> { 
                        { "id", generateFileUploadUrlResult.FileId },
                        { "name", generateFileUploadUrlResult.FileName } });

                    int publishFileRetryLimit = config.FileMessagePublishRetryLimit;
                    int currentFileRetryCount = 0;
                    bool publishFailed = false;
                    do
                    {
                        currentFileRetryCount += 1;
                        PNResult<PNPublishFileMessageResult> publishFileMessageResponse = PublishFileMessage(publishPayload, queryParam).Result;
                        PNPublishFileMessageResult publishFileMessage = publishFileMessageResponse.Result;
                        PNStatus publishFileMessageStatus = publishFileMessageResponse.Status;
                        if (publishFileMessageStatus != null && !publishFileMessageStatus.Error && publishFileMessage != null)
                        {
                            publishFailed = false;
                            PNFileUploadResult result = new PNFileUploadResult();
                            result.Timetoken = publishFileMessage.Timetoken;
                            result.FileId = generateFileUploadUrlResult.FileId;
                            result.FileName = generateFileUploadUrlResult.FileName;
                            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl -> file upload -> PublishFileMessage -> OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                            r.Result.Item2.Error = false;
                            callback.OnResponse(result, r.Result.Item2);
                        }
                        else
                        {
                            publishFailed = true;
                            if (currentFileRetryCount == publishFileRetryLimit)
                            {
                                callback.OnResponse(null, publishFileMessageStatus);
                            }
                            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} PublishFileMessage Failed. currentFileRetryCount={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), currentFileRetryCount), config.LogVerbosity);
#if !NET35 && !NET40
                            Task.Delay(1000).Wait();
#else
                            Thread.Sleep(1000);
#endif
                        }
                    }
                    while (publishFailed && currentFileRetryCount <= publishFileRetryLimit);
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

        private async Task<PNResult<PNFileUploadResult>> ProcessFileUpload(Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNFileUploadResult> ret = new PNResult<PNFileUploadResult>();

            if (string.IsNullOrEmpty(this.sendFileName))
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Missing File", new ArgumentException("Missing File")) };
                ret.Status = errStatus;
                return ret;
            }


            PNResult<PNGenerateFileUploadUrlResult> generateFileUploadUrl = await GenerateFileUploadUrl(externalQueryParam).ConfigureAwait(false);
            PNGenerateFileUploadUrlResult generateFileUploadUrlResult = generateFileUploadUrl.Result;
            PNStatus generateFileUploadUrlStatus = generateFileUploadUrl.Status;
            if (generateFileUploadUrlStatus.Error || generateFileUploadUrlResult == null)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Error in GenerateFileUploadUrl. Try again.", new ArgumentException("Error in GenerateFileUploadUrl. Try again.")) };
                ret.Status = errStatus;
                return ret;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);

            RequestState<PNFileUploadResult> requestState = new RequestState<PNFileUploadResult>();
            requestState.ResponseType = PNOperationType.PNFileUploadOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePostMethod = true;

            byte[] sendFileByteArray = sendFileBytes ?? GetByteArrayFromFilePath(sendFileFullPath);

            string dataBoundary = String.Format(CultureInfo.InvariantCulture, "----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + dataBoundary;
            string currentCipherKey = !string.IsNullOrEmpty(this.currentFileCipherKey) ? this.currentFileCipherKey : config.CipherKey;
            byte[] postData = GetMultipartFormData(sendFileByteArray, generateFileUploadUrlResult.FileName, generateFileUploadUrlResult.FileUploadRequest.FormFields, dataBoundary, currentCipherKey, config, pubnubLog);

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(new Uri(generateFileUploadUrlResult.FileUploadRequest.Url), requestState, false, postData, contentType).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl -> file upload OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);                //do internal publish after successful file upload

                Dictionary<string, object> publishPayload = new Dictionary<string, object>();
                if (this.publishMessage != null && !string.IsNullOrEmpty(this.publishMessage.ToString()))
                {
                    publishPayload.Add("message", this.publishMessage);
                }
                currentFileId = generateFileUploadUrlResult.FileId;
                publishPayload.Add("file", new Dictionary<string, string> {
                        { "id", generateFileUploadUrlResult.FileId },
                        { "name", generateFileUploadUrlResult.FileName } });

                int publishFileRetryLimit = config.FileMessagePublishRetryLimit;
                int currentFileRetryCount = 0;
                bool publishFailed = false;
                do
                {
                    currentFileRetryCount += 1;
                    PNResult<PNPublishFileMessageResult> publishFileMessageResponse = await PublishFileMessage(publishPayload, queryParam).ConfigureAwait(false);
                    PNPublishFileMessageResult publishFileMessage = publishFileMessageResponse.Result;
                    PNStatus publishFileMessageStatus = publishFileMessageResponse.Status;
                    if (publishFileMessageStatus != null && !publishFileMessageStatus.Error && publishFileMessage != null)
                    {
                        publishFailed = false;
                        PNFileUploadResult result = new PNFileUploadResult();
                        result.Timetoken = publishFileMessage.Timetoken;
                        result.FileId = generateFileUploadUrlResult.FileId;
                        result.FileName = generateFileUploadUrlResult.FileName;
                        ret.Result = result;
                        ret.Status.Error = false;
                        LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} GenerateFileUploadUrl -> file upload -> PublishFileMessage -> OK.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);                //do internal publish after successful file upload
                    }
                    else
                    {
                        publishFailed = true;
                        ret.Status = publishFileMessageStatus;
                        LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0} PublishFileMessage Failed. currentFileRetryCount={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), currentFileRetryCount), config.LogVerbosity);
#if !NET35 && !NET40
                        Task.Delay(1000).Wait();
#else
                        Thread.Sleep(1000);
#endif
                    }
                }
                while (publishFailed && currentFileRetryCount <= publishFileRetryLimit);
            }

            return ret;
        }

        private async Task<PNResult<PNGenerateFileUploadUrlResult>> GenerateFileUploadUrl(Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNGenerateFileUploadUrlResult> ret = new PNResult<PNGenerateFileUploadUrlResult>();

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

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, postData, "application/json").ConfigureAwait(false);
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

        private async Task<PNResult<PNPublishFileMessageResult>> PublishFileMessage(object message, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNPublishFileMessageResult> ret = new PNResult<PNPublishFileMessageResult>();

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildPublishFileMessageRequest("GET", "", this.channelName, message, this.storeInHistory, this.ttl, this.userMetadata, null, externalQueryParam);

            RequestState<PNPublishFileMessageResult> requestState = new RequestState<PNPublishFileMessageResult>();
            requestState.Channels = new[] { this.channelName };
            requestState.ResponseType = PNOperationType.PNPublishFileMessageOperation;
            requestState.PubnubCallback = null;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = "";

            await UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                    PNPublishFileMessageResult publishResult = responseBuilder.JsonToObject<PNPublishFileMessageResult>(result, true);
                    StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
                    if (publishResult != null)
                    {
                        ret.Result = publishResult;
                        PNStatus status = statusBuilder.CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, (int)HttpStatusCode.OK, null);
                        ret.Status = status;
                    }
                    else
                    {
                        PNException ex = new PNException("File has been upload but the notification couldn't be sent to the subscribed users");
                        PNStatus status = statusBuilder.CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNUnknownCategory, requestState, (int)HttpStatusCode.BadRequest, ex);
                        status.AdditonalData = new Dictionary<string, string> { {"FileId", currentFileId }, {"FileName", sendFileName } };
                        ret.Status = status;
                    }
                }
                else
                {
                    ret.Status = r.Result.Item2;
                    if (ret.Status == null)
                    {
                        PNException ex = new PNException("PublishFileMessage failed.");
                        StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
                        PNStatus status = statusBuilder.CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNUnknownCategory, requestState, (int)HttpStatusCode.BadRequest, ex);
                        status.AdditonalData = new Dictionary<string, string> { { "FileId", currentFileId }, { "FileName", sendFileName } };
                        ret.Status = status;
                    }
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            return ret;
        }

        private static byte[] GetByteArrayFromFilePath(string filePath)
        {
#if !NETSTANDARD10 && !NETSTANDARD11
            byte[] byteArray = null;
            if (!string.IsNullOrEmpty(filePath))
            {
                byteArray = System.IO.File.ReadAllBytes(filePath);
            }
            return byteArray;
#else
            throw new NotSupportedException("FileSystem not supported in NetStandard 1.0/1.1. Consider higher version of .NetStandard.");
#endif

        }

        private static byte[] GetMultipartFormData(byte[] sendFileByteArray, string fileName, Dictionary<string, object> formFields, string dataBoundary, string currentCipherKey, PNConfiguration config, IPubnubLog pubnubLog)
        {
            byte[] ret = null;
            string fileContentType = "application/octet-stream";
            using (Stream dataStream = new System.IO.MemoryStream())
            {
                foreach (var kvp in formFields)
                {
                    if (kvp.Key == "Content-Type" && kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.ToString()))
                    {
                        fileContentType = kvp.Value.ToString();
                    }
                    string postParamData = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                            dataBoundary,
                            kvp.Key,
                            kvp.Value);
                    byte[] postParam = Encoding.UTF8.GetBytes(postParamData);
                    dataStream.Write(postParam, 0, postParam.Length);

                    string emptyLine = "\r\n";
                    byte[] emptyData = Encoding.UTF8.GetBytes(emptyLine);
                    dataStream.Write(emptyData, 0, emptyData.Length);
                }
                string header = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                            dataBoundary,
                            "file",
                            fileName,
                            fileContentType);
                byte[] postHeaderData = Encoding.UTF8.GetBytes(header);

                dataStream.Write(postHeaderData, 0, postHeaderData.Length);
                if (currentCipherKey.Length > 0)
                {
                    try
                    {
                        PubnubCrypto aes = new PubnubCrypto(currentCipherKey, config, pubnubLog, null);
                        byte[] encryptBytes = aes.Encrypt(sendFileByteArray, true);
                        dataStream.Write(encryptBytes, 0, encryptBytes.Length);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    dataStream.Write(sendFileByteArray, 0, sendFileByteArray.Length);
                }

                string footer = "\r\n--" + dataBoundary + "--\r\n";
                byte[] postFooterData = Encoding.UTF8.GetBytes(footer);
                dataStream.Write(postFooterData, 0, postFooterData.Length);

                dataStream.Position = 0;
                ret = new byte[dataStream.Length];
                int bytesRead = dataStream.Read(ret, 0, ret.Length);
                System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "MultipartFormData byte count = {0}", bytesRead));
#if NET35 || NET40 || NET45 || NET461 || NET48
                dataStream.Close();
#endif
            }
            return ret;
        }
    }
}
