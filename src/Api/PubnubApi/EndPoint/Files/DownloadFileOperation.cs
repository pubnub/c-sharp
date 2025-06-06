using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq;
using PubnubApi.Security.Crypto;
using PubnubApi.Security.Crypto.Cryptors;

namespace PubnubApi.EndPoint
{
    public class DownloadFileOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;

        private PNCallback<PNDownloadFileResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private string channelName;
        private string currentFileId;
        private string currentFileName;
        private string currentFileCipherKey;

        public DownloadFileOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary,
            IPubnubUnitTest pubnubUnit, TokenManager tokenManager, Pubnub instance) : base(pubnubConfig,
            jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }

        public DownloadFileOperation Channel(string channel)
        {
            channelName = channel;
            return this;
        }

        public DownloadFileOperation FileId(string fileId)
        {
            currentFileId = fileId;
            return this;
        }

        public DownloadFileOperation FileName(string fileName)
        {
            currentFileName = fileName;
            return this;
        }

        public DownloadFileOperation CipherKey(string cipherKeyForFile)
        {
            currentFileCipherKey = cipherKeyForFile;
            return this;
        }

        public DownloadFileOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNDownloadFileResult> callback)
        {
            logger?.Trace($"{GetType().Name} Execute invoked");
            if (string.IsNullOrEmpty(currentFileId))
            {
                throw new ArgumentException("Missing File Id");
            }

            if (string.IsNullOrEmpty(currentFileName))
            {
                throw new ArgumentException("Missing File Name");
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            savedCallback = callback ?? throw new ArgumentException("Missing callback");
            ProcessFileDownloadRequest(savedCallback);
        }

        public async Task<PNResult<PNDownloadFileResult>> ExecuteAsync()
        {
            logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
            return await ProcessFileDownloadRequest().ConfigureAwait(false);
        }

        internal void Retry()
        {
            ProcessFileDownloadRequest(savedCallback);
        }

        private void ProcessFileDownloadRequest(PNCallback<PNDownloadFileResult> callback)
        {
            var processTask = ProcessFileDownloadRequest();
            processTask.ContinueWith((t) =>
            {
                var result = t.Result;
                callback.OnResponse(result.Result, result.Status);
            });
        }

        private async Task<PNResult<PNDownloadFileResult>> ProcessFileDownloadRequest()
        {
            PNResult<PNDownloadFileResult> returnValue = new PNResult<PNDownloadFileResult>();

            if (string.IsNullOrEmpty(currentFileId))
            {
                PNStatus errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Missing File Id", new ArgumentException("Missing File Id"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            if (string.IsNullOrEmpty(currentFileName))
            {
                PNStatus errStatus = new PNStatus
                {
                    Error = true,
                    ErrorData = new PNErrorData("Invalid file name", new ArgumentException("Invalid file name"))
                };
                returnValue.Status = errStatus;
                return returnValue;
            }

            logger?.Debug($"{GetType().Name} parameter validated.");
            var requestParameter = CreateRequestParameter();
            var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(
                requestParameter: requestParameter, operationType: PNOperationType.PNDownloadFileOperation);
            var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest)
                .ConfigureAwait(false);
            RequestState<PNDownloadFileResult> requestState = new RequestState<PNDownloadFileResult>
            {
                ResponseType = PNOperationType.PNDownloadFileOperation,
                Response = transportResponse,
                Reconnect = false,
                EndPointOperation = this
            };
            if (transportResponse.Error == null)
            {
                var fileContentBytes = transportResponse.Content;
                if (fileContentBytes != null)
                {
                    byte[] outputBytes;
                    requestState.GotJsonResponse = true;
                    if (string.IsNullOrEmpty(currentFileCipherKey) && string.IsNullOrEmpty(config.CipherKey) &&
                        config.CryptoModule == null)
                    {
                        outputBytes = fileContentBytes;
                    }
                    else
                    {
                        CryptoModule currentCryptoModule = !string.IsNullOrEmpty(currentFileCipherKey)
                            ? new CryptoModule(new LegacyCryptor(currentFileCipherKey, true), null)
                            : (config.CryptoModule ??=
                                new CryptoModule(new LegacyCryptor(config.CipherKey, true), null));
                        try
                        {
                            outputBytes = currentCryptoModule.Decrypt(fileContentBytes);
                            logger?.Debug($"Stream length (after Decrypt)= {fileContentBytes.Length}");
                        }
                        catch (Exception ex)
                        {
                            logger?.Error(
                                $" Error while decrypting file content.File might be not encrypted, returning as it is. exception: {ex}");
                            outputBytes = fileContentBytes;
                            returnValue.Status = new PNStatus
                                { Error = true, ErrorData = new PNErrorData("Decryption error", ex) };
                        }
                    }
                    
                    //Parsing for AWS errors (httpClient doesn't throw an exception but returns a 4xx status code with error in body)
                    if (transportResponse.StatusCode is >= 400 and < 500)
                    {
                        var stringResult = System.Text.Encoding.UTF8.GetString(outputBytes);
                        PNStatus errorStatus = GetStatusIfError(requestState, stringResult);
                        returnValue.Status = errorStatus;
                    }
                    else
                    {
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                            requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState,
                            transportResponse.StatusCode, null);
                        PNDownloadFileResult result = new PNDownloadFileResult
                        {
                            FileBytes = outputBytes,
                            FileName = currentFileName
                        };
                        returnValue.Result = result;
                        returnValue.Status = status;
                    }
                }
                else
                {
                    PNStatus errorStatus = GetStatusIfError(requestState, null);
                    returnValue.Status = errorStatus;
                }
            }
            else
            {
                int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
                PNStatusCategory category =
                    PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(
                    PNOperationType.PNDownloadFileOperation, category, requestState, statusCode,
                    new PNException(transportResponse.Error.Message, transportResponse.Error));
                returnValue.Status = status;
            }
            logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status?.StatusCode}");
            return returnValue;
        }

        private RequestParameter CreateRequestParameter()
        {
            var pathSegments = new List<string>
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
                        requestQueryStringParams.Add(kvp.Key,
                            UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNDownloadFileOperation,
                                false, false, false));
                    }
                }
            }

            var requestParameter = new RequestParameter()
            {
                RequestType = Constants.GET,
                PathSegment = pathSegments,
                Query = requestQueryStringParams
            };

            return requestParameter;
        }
    }
}