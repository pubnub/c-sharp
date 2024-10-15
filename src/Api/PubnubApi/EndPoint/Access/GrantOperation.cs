using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;

namespace PubnubApi.EndPoint
{
	public class GrantOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;

        private string[] pubnubChannelNames;
        private string[] pubnubChannelGroupNames;
        private string[] pubnubTargetUuids;
        private string[] pamAuthenticationKeys;
        private bool grantWrite;
        private bool grantRead;
        private bool grantManage;
        private bool grantDelete;
        private bool grantGet;
        private bool grantUpdate;
        private bool grantJoin;
        private long grantTTL = -1;
        private PNCallback<PNAccessManagerGrantResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public GrantOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, null, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
        }

        public GrantOperation Channels(string[] channels)
        {
            this.pubnubChannelNames = channels;
            return this;
        }

        public GrantOperation ChannelGroups(string[] channelGroups)
        {
            this.pubnubChannelGroupNames = channelGroups;
            return this;
        }

        public GrantOperation Uuids(string[] targetUuids)
        {
            this.pubnubTargetUuids = targetUuids;
            return this;
        }

        public GrantOperation AuthKeys(string[] authKeys)
        {
            this.pamAuthenticationKeys = authKeys;
            return this;
        }

        public GrantOperation Write(bool write)
        {
            this.grantWrite = write;
            return this;
        }

        public GrantOperation Read(bool read)
        {
            this.grantRead = read;
            return this;
        }

        public GrantOperation Manage(bool manage)
        {
            this.grantManage = manage;
            return this;
        }

        public GrantOperation Delete(bool delete)
        {
            this.grantDelete = delete;
            return this;
        }

        public GrantOperation Get(bool get)
        {
            this.grantGet = get;
            return this;
        }

        public GrantOperation Update(bool update)
        {
            this.grantUpdate = update;
            return this;
        }

        public GrantOperation Join(bool join)
        {
            this.grantJoin = join;
            return this;
        }

        public GrantOperation TTL(long ttl)
        {
            this.grantTTL = ttl;
            return this;
        }

        public GrantOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNAccessManagerGrantResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNAccessManagerGrantResult> callback)
        {
            if ((this.pubnubChannelNames != null || this.pubnubChannelGroupNames != null) && this.pubnubTargetUuids != null)
            {
                throw new InvalidOperationException("Both channel/channelgroup and uuid cannot be used in the same request");
            }
            if (this.pubnubTargetUuids != null && (this.grantRead || this.grantWrite || this.grantManage || this.grantJoin))
            {
                throw new InvalidOperationException("Only Get/Update/Delete permissions are allowed for UUID");
            }
            this.savedCallback = callback;
            GrantAccess(callback);
        }

        public async Task<PNResult<PNAccessManagerGrantResult>> ExecuteAsync()
        {
            if ((this.pubnubChannelNames != null || this.pubnubChannelGroupNames != null) && this.pubnubTargetUuids != null)
            {
                throw new InvalidOperationException("Both channel/channelgroup and uuid cannot be used in the same request");
            }
            if (this.pubnubTargetUuids != null && (this.grantRead || this.grantWrite || this.grantManage || this.grantJoin))
            {
                throw new InvalidOperationException("Only Get/Update/Delete permissions are allowed for UUID");
            }
            return await GrantAccess().ConfigureAwait(false);
        }

        internal void Retry()
        {
            GrantAccess(savedCallback);
        }

        internal void GrantAccess(PNCallback<PNAccessManagerGrantResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }
            RequestState<PNAccessManagerGrantResult> requestState = new RequestState<PNAccessManagerGrantResult>();
            requestState.Channels = this.pubnubChannelNames;
            requestState.ChannelGroups = this.pubnubChannelGroupNames;
            requestState.ResponseType = PNOperationType.PNAccessManagerGrant;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;
            var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerGrant);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
                    requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
					} else {
						PNStatus errorStatus = GetStatusIfError(requestState, responseString);
						callback.OnResponse(default, errorStatus);
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerGrant, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default, status);
				}
			});
        }

        internal async Task<PNResult<PNAccessManagerGrantResult>> GrantAccess()
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            PNResult<PNAccessManagerGrantResult> returnValue = new PNResult<PNAccessManagerGrantResult>();
            RequestState<PNAccessManagerGrantResult> requestState = new RequestState<PNAccessManagerGrantResult>();
            requestState.Channels = this.pubnubChannelNames;
            requestState.ChannelGroups = this.pubnubChannelGroupNames;
            requestState.ResponseType = PNOperationType.PNAccessManagerGrant;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;
            var requestParameter = CreateRequestParameter();
			Tuple<string, PNStatus> JsonAndStatusTuple;
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerGrant);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest);
			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null) {
                    requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>(string.Empty, errorStatus);
				}
                returnValue.Status = JsonAndStatusTuple.Item2;
                string json = JsonAndStatusTuple.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> resultList = ProcessJsonResponse(requestState, json);
                    if (resultList != null && resultList.Count > 0)
                    {
                        ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                        PNAccessManagerGrantResult responseResult = responseBuilder.JsonToObject<PNAccessManagerGrantResult>(resultList, true);
                        if (responseResult != null)
                        {
                            returnValue.Result = responseResult;
                        }
                    }
                }
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerGrant, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
            return returnValue;
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
        }

        private RequestParameter CreateRequestParameter()
        {
            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            List<string> uuidList = new List<string>();
            List<string> authList = new List<string>();

            if (this.pubnubChannelNames != null && this.pubnubChannelNames.Length > 0)
            {
                channelList = new List<string>(this.pubnubChannelNames);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (this.pubnubChannelGroupNames != null && this.pubnubChannelGroupNames.Length > 0)
            {
                channelGroupList = new List<string>(this.pubnubChannelGroupNames);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (this.pubnubTargetUuids != null && this.pubnubTargetUuids.Length > 0)
            {
                uuidList = new List<string>(this.pubnubTargetUuids);
                uuidList = uuidList.Where(uuid => !string.IsNullOrEmpty(uuid) && uuid.Trim().Length > 0).Distinct().ToList();
            }

            if (this.pamAuthenticationKeys != null && this.pamAuthenticationKeys.Length > 0)
            {
                authList = new List<string>(this.pamAuthenticationKeys);
                authList = authList.Where(auth => !string.IsNullOrEmpty(auth) && auth.Trim().Length > 0).Distinct<string>().ToList();
            }

            string channelsCommaDelimited = string.Join(",", channelList.OrderBy(x => x).ToArray());
            string channelGroupsCommaDelimited = string.Join(",", channelGroupList.OrderBy(x => x).ToArray());
            string targetUuidsCommaDelimited = string.Join(",", uuidList.OrderBy(x => x).ToArray());
            string authKeysCommaDelimited = string.Join(",", authList.OrderBy(x => x).ToArray());

            List<string> pathSegments = new List<string>() {
                "v2",
                "auth",
                "grant",
                "sub-key",
                config.SubscribeKey
            };

            Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authKeysCommaDelimited))
            {
                requestQueryStringParams.Add("auth", UriUtil.EncodeUriComponent(authKeysCommaDelimited, PNOperationType.PNAccessManagerGrant, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelsCommaDelimited))
            {
                requestQueryStringParams.Add("channel", UriUtil.EncodeUriComponent(channelsCommaDelimited, PNOperationType.PNAccessManagerGrant, false, false, false));
            }

            if (!string.IsNullOrEmpty(channelGroupsCommaDelimited))
            {
                requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(channelGroupsCommaDelimited, PNOperationType.PNAccessManagerGrant, false, false, false));
            }

            if (!string.IsNullOrEmpty(targetUuidsCommaDelimited))
            {
                requestQueryStringParams.Add("target-uuid", UriUtil.EncodeUriComponent(targetUuidsCommaDelimited, PNOperationType.PNAccessManagerGrant, false, false, false));
            }

            if (grantTTL > -1)
            {
                requestQueryStringParams.Add("ttl", grantTTL.ToString(CultureInfo.InvariantCulture));
            }

            requestQueryStringParams.Add("r", Convert.ToInt32(grantRead).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("w", Convert.ToInt32(grantWrite).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("d", Convert.ToInt32(grantDelete).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("m", Convert.ToInt32(grantManage).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("g", Convert.ToInt32(grantGet).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("u", Convert.ToInt32(grantUpdate).ToString(CultureInfo.InvariantCulture));
            requestQueryStringParams.Add("j", Convert.ToInt32(grantJoin).ToString(CultureInfo.InvariantCulture));

            if (queryParam != null && queryParam.Count > 0)
            {
                foreach (KeyValuePair<string, object> kvp in queryParam)
                {
                    if (!requestQueryStringParams.ContainsKey(kvp.Key))
                    {
                        requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNAccessManagerGrant, false, false, false));
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
