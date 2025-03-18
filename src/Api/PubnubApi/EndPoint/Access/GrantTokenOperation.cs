using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PubnubApi.EndPoint
{
	public class GrantTokenOperation : PubnubCoreBase
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;

		
		private PNTokenResources pubnubResources = new PNTokenResources {
			Channels = new Dictionary<string, PNTokenAuthValues>(),
			Spaces = new Dictionary<string, PNTokenAuthValues>(),
			ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
			Uuids = new Dictionary<string, PNTokenAuthValues>(),
			Users = new Dictionary<string, PNTokenAuthValues>()
		};
		private PNTokenPatterns pubnubPatterns = new PNTokenPatterns {
			Channels = new Dictionary<string, PNTokenAuthValues>(),
			Spaces = new Dictionary<string, PNTokenAuthValues>(),
			ChannelGroups = new Dictionary<string, PNTokenAuthValues>(),
			Uuids = new Dictionary<string, PNTokenAuthValues>(),
			Users = new Dictionary<string, PNTokenAuthValues>()
		};

		private int grantTTL = -1;
		private PNCallback<PNAccessManagerTokenResult> savedCallbackGrantToken;
		private Dictionary<string, object> queryParam;
		private Dictionary<string, object> grantMeta;
		private string pubnubAuthorizedUuid = string.Empty;
		private string pubnubAuthorizedUserId = string.Empty;
		
		public GrantTokenOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;

			PubnubInstance = instance;
			InitializeDefaultVariableObjectStates();
		}

		public GrantTokenOperation AuthorizedUuid(string uuid)
		{
			if (!string.IsNullOrEmpty(pubnubAuthorizedUserId)) {
				throw new ArgumentException("Either UUID or UserId can be used. Not both.");
			}
			pubnubAuthorizedUuid = uuid;
			return this;
		}

		public GrantTokenOperation AuthorizedUserId(UserId user)
		{
			if (!string.IsNullOrEmpty(pubnubAuthorizedUuid)) {
				throw new ArgumentException("Either UUID or UserId can be used. Not both.");
			}
			pubnubAuthorizedUserId = user;
			return this;
		}

		public GrantTokenOperation Resources(PNTokenResources resources)
		{
			if (pubnubResources != null && resources != null) {
				if (resources.Channels != null && resources.Channels.Count > 0 &&
					resources.Spaces != null && resources.Spaces.Count > 0) {
					throw new ArgumentException("Either Channels or Spaces can be used. Not both.");
				}
				if (resources.Uuids != null && resources.Uuids.Count > 0 &&
					resources.Users != null && resources.Users.Count > 0) {
					throw new ArgumentException("Either Uuids or Users can be used. Not both.");
				}
				pubnubResources = resources;
				if (pubnubResources.Channels == null) {
					pubnubResources.Channels = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubResources.Spaces == null) {
					pubnubResources.Spaces = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubResources.ChannelGroups == null) {
					pubnubResources.ChannelGroups = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubResources.Uuids == null) {
					pubnubResources.Uuids = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubResources.Users == null) {
					pubnubResources.Users = new Dictionary<string, PNTokenAuthValues>();
				}
			}
			return this;
		}

		public GrantTokenOperation Patterns(PNTokenPatterns patterns)
		{
			if (pubnubPatterns != null && patterns != null) {
				if (patterns.Channels != null && patterns.Channels.Count > 0 &&
					patterns.Spaces != null && patterns.Spaces.Count > 0) {
					throw new ArgumentException("Either Channels or Spaces can be used. Not both.");
				}
				if (patterns.Uuids != null && patterns.Uuids.Count > 0 &&
					patterns.Users != null && patterns.Users.Count > 0) {
					throw new ArgumentException("Either Uuids or Users can be used. Not both.");
				}

				pubnubPatterns = patterns;
				if (pubnubPatterns.Channels == null) {
					pubnubPatterns.Channels = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubPatterns.Spaces == null) {
					pubnubPatterns.Spaces = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubPatterns.ChannelGroups == null) {
					pubnubPatterns.ChannelGroups = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubPatterns.Uuids == null) {
					pubnubPatterns.Uuids = new Dictionary<string, PNTokenAuthValues>();
				}
				if (pubnubPatterns.Users == null) {
					pubnubPatterns.Users = new Dictionary<string, PNTokenAuthValues>();
				}

			}
			return this;
		}

		public GrantTokenOperation TTL(int ttl)
		{
			this.grantTTL = ttl;
			return this;
		}

		public GrantTokenOperation Meta(Dictionary<string, object> metaObject)
		{
			this.grantMeta = metaObject;
			return this;
		}

		public GrantTokenOperation QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute(PNCallback<PNAccessManagerTokenResult> callback)
		{
			logger?.Trace($"{GetType().Name} Execute invoked");
			GrantAccess(callback);
		}

		public async Task<PNResult<PNAccessManagerTokenResult>> ExecuteAsync()
		{
			logger?.Trace($"{GetType().Name} ExecuteAsync invoked.");
			return await GrantAccess().ConfigureAwait(false);
		}

		internal void Retry()
		{
			GrantAccess(savedCallbackGrantToken);
		}

		internal void GrantAccess(PNCallback<PNAccessManagerTokenResult> callback)
		{
			if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0) {
				throw new MissingMemberException("Invalid secret key");
			}

			if (this.grantTTL <= 0) {
				throw new MissingMemberException("Invalid TTL value");
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNAccessManagerTokenResult> requestState = new RequestState<PNAccessManagerTokenResult>
				{
					Channels = pubnubResources.Channels.Keys.ToArray(),
					ChannelGroups = pubnubResources.ChannelGroups.Keys.ToArray(),
					ResponseType = PNOperationType.PNAccessManagerGrantToken,
					PubnubCallback = callback,
					Reconnect = false,
					EndPointOperation = this
				};

			var requestParameter = CreateRequestParameter();

			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerGrantToken);
			PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
				var transportResponse = t.Result;
				if (transportResponse.Error == null) {
					var responseString = Encoding.UTF8.GetString(transportResponse.Content);
					requestState.GotJsonResponse = true;
					if (!string.IsNullOrEmpty(responseString)) {
						List<object> result = ProcessJsonResponse(requestState, responseString);
						ProcessResponseCallbacks(result, requestState);
						logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
					}
				} else {
					int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
					PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerGrantToken, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
					requestState.PubnubCallback.OnResponse(default(PNAccessManagerTokenResult), status);
					logger?.Info($"{GetType().Name} request finished with status code {requestState.Response?.StatusCode}");
				}
			});
		}

		private bool FillPermissionMappingWithMaskValues(Dictionary<string, PNTokenAuthValues> dPerms, bool currentAtleastOnePermission, out Dictionary<string, int> dPermsWithMaskValues)
		{
			dPermsWithMaskValues = new Dictionary<string, int>();
			bool internalAtleastOnePermission = currentAtleastOnePermission;
			foreach (KeyValuePair<string, PNTokenAuthValues> kvp in dPerms) {
				PNTokenAuthValues perm = kvp.Value;
				int bitMaskPermissionValue = 0;
				if (!string.IsNullOrEmpty(kvp.Key) && kvp.Key.Trim().Length > 0 && perm != null) {
					bitMaskPermissionValue = CalculateGrantBitMaskValue(perm);
					if (!internalAtleastOnePermission && bitMaskPermissionValue > 0) { internalAtleastOnePermission = true; }
				}
				dPermsWithMaskValues.Add(kvp.Key, bitMaskPermissionValue);
			}
			return internalAtleastOnePermission;
		}

		internal async Task<PNResult<PNAccessManagerTokenResult>> GrantAccess()
		{
			if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0) {
				throw new MissingMemberException("Invalid secret key");
			}

			if (this.grantTTL <= 0) {
				throw new MissingMemberException("Invalid TTL value");
			}
			logger?.Debug($"{GetType().Name} parameter validated.");
			RequestState<PNAccessManagerTokenResult> requestState = new RequestState<PNAccessManagerTokenResult>();
			requestState.Channels = pubnubResources.Channels.Keys.ToArray();
			requestState.ChannelGroups = pubnubResources.ChannelGroups.Keys.ToArray();
			requestState.ResponseType = PNOperationType.PNAccessManagerGrantToken;
			requestState.Reconnect = false;
			requestState.EndPointOperation = this;

			Tuple<string, PNStatus> JsonAndStatusTuple;

			PNResult<PNAccessManagerTokenResult> returnValue = new PNResult<PNAccessManagerTokenResult>();
			var requestParameter = CreateRequestParameter();
			var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: requestParameter, operationType: PNOperationType.PNAccessManagerGrantToken);
			var transportResponse = await PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ConfigureAwait(false);

			if (transportResponse.Error == null) {
				var responseString = Encoding.UTF8.GetString(transportResponse.Content);
				PNStatus errorStatus = GetStatusIfError(requestState, responseString);
				if (errorStatus == null && transportResponse.StatusCode == Constants.HttpRequestSuccessStatusCode) {
					requestState.GotJsonResponse = true;
					PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, transportResponse.StatusCode, null);
					JsonAndStatusTuple = new Tuple<string, PNStatus>(responseString, status);
				} else {
					JsonAndStatusTuple = new Tuple<string, PNStatus>("", errorStatus);
				}
				returnValue.Status = JsonAndStatusTuple.Item2;
				string json = JsonAndStatusTuple.Item1;
				if (!string.IsNullOrEmpty(json)) {
					List<object> resultList = ProcessJsonResponse(requestState, json);
					if (resultList != null && resultList.Count > 0) {
						ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary);
						PNAccessManagerTokenResult responseResult = responseBuilder.JsonToObject<PNAccessManagerTokenResult>(resultList, true);
						if (responseResult != null) {
							returnValue.Result = responseResult;
						}
					}
				}
			} else {
				int statusCode = PNStatusCodeHelper.GetHttpStatusCode(transportResponse.Error.Message);
				PNStatusCategory category = PNStatusCategoryHelper.GetPNStatusCategory(statusCode, transportResponse.Error.Message);
				PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse(PNOperationType.PNAccessManagerGrantToken, category, requestState, statusCode, new PNException(transportResponse.Error.Message, transportResponse.Error));
				returnValue.Status = status;
			}
			logger?.Info($"{GetType().Name} request finished with status code {returnValue.Status.StatusCode}");
			return returnValue;
		}

		private static int CalculateGrantBitMaskValue(PNTokenAuthValues perm)
		{
			int result = 0;

			if (perm.Read) {
				result = (int)GrantBitFlag.READ;
			}
			if (perm.Write) {
				result = result + (int)GrantBitFlag.WRITE;
			}
			if (perm.Manage) {
				result = result + (int)GrantBitFlag.MANAGE;
			}
			if (perm.Delete) {
				result = result + (int)GrantBitFlag.DELETE;
			}
			if (perm.Create) {
				result = result + (int)GrantBitFlag.CREATE;
			}
			if (perm.Get) {
				result = result + (int)GrantBitFlag.GET;
			}
			if (perm.Update) {
				result = result + (int)GrantBitFlag.UPDATE;
			}
			if (perm.Join) {
				result = result + (int)GrantBitFlag.JOIN;
			}

			return result;
		}

		private RequestParameter CreateRequestParameter()
		{
			bool atleastOnePermission = false;
			Dictionary<string, int> chBitmaskPermCollection = null;
			atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Channels, atleastOnePermission, out chBitmaskPermCollection);

			Dictionary<string, int> chPatternBitmaskPermCollection = null;
			atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Channels, atleastOnePermission, out chPatternBitmaskPermCollection);

			Dictionary<string, int> spBitmaskPermCollection = null;
			Dictionary<string, int> spPatternBitmaskPermCollection = null;
			if (pubnubResources.Channels.Count == 0 && pubnubPatterns.Channels.Count == 0) {
				atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Spaces, atleastOnePermission, out spBitmaskPermCollection);
				atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Spaces, atleastOnePermission, out spPatternBitmaskPermCollection);
			} else {
				spBitmaskPermCollection = new Dictionary<string, int>();
				spPatternBitmaskPermCollection = new Dictionary<string, int>();
			}

			Dictionary<string, int> cgBitmaskPermCollection = null;
			atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.ChannelGroups, atleastOnePermission, out cgBitmaskPermCollection);

			Dictionary<string, int> cgPatternBitmaskPermCollection = null;
			atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.ChannelGroups, atleastOnePermission, out cgPatternBitmaskPermCollection);

			Dictionary<string, int> uuidBitmaskPermCollection = null;
			atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Uuids, atleastOnePermission, out uuidBitmaskPermCollection);

			Dictionary<string, int> uuidPatternBitmaskPermCollection = null;
			atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Uuids, atleastOnePermission, out uuidPatternBitmaskPermCollection);

			Dictionary<string, int> userBitmaskPermCollection = null;
			Dictionary<string, int> userPatternBitmaskPermCollection = null;
			if (pubnubResources.Uuids.Count == 0 && pubnubPatterns.Uuids.Count == 0) {
				atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubResources.Users, atleastOnePermission, out userBitmaskPermCollection);
				atleastOnePermission = FillPermissionMappingWithMaskValues(this.pubnubPatterns.Users, atleastOnePermission, out userPatternBitmaskPermCollection);
			} else {
				userBitmaskPermCollection = new Dictionary<string, int>();
				userPatternBitmaskPermCollection = new Dictionary<string, int>();
			}

			if (!atleastOnePermission) {
				config?.Logger?.Warn("GrantToken At least one permission is needed for at least one or more of uuids/users, channels/spaces or groups");
			}

			Dictionary<string, object> resourcesCollection = new Dictionary<string, object>
			{
				{ "channels", chBitmaskPermCollection },
				{ "groups", cgBitmaskPermCollection },
				{ "uuids", uuidBitmaskPermCollection },
				{ "users", userBitmaskPermCollection },
				{ "spaces", spBitmaskPermCollection }
			};

			Dictionary<string, object> patternsCollection = new Dictionary<string, object>
			{
				{ "channels", chPatternBitmaskPermCollection },
				{ "groups", cgPatternBitmaskPermCollection },
				{ "uuids", uuidPatternBitmaskPermCollection },
				{ "users", userPatternBitmaskPermCollection },
				{ "spaces", spPatternBitmaskPermCollection }
			};

			Dictionary<string, object> optimizedMeta = new Dictionary<string, object>();
			if (this.grantMeta != null) {
				optimizedMeta = this.grantMeta;
			}

			Dictionary<string, object> permissionCollection = new Dictionary<string, object>
			{
				{ "resources", resourcesCollection },
				{ "patterns", patternsCollection },
				{ "meta", optimizedMeta }
			};
			if (!string.IsNullOrEmpty(this.pubnubAuthorizedUuid) && this.pubnubAuthorizedUuid.Trim().Length > 0) {
				permissionCollection.Add("uuid", this.pubnubAuthorizedUuid);
			} else if (!string.IsNullOrEmpty(this.pubnubAuthorizedUserId) && this.pubnubAuthorizedUserId.Trim().Length > 0) {
				permissionCollection.Add("uuid", this.pubnubAuthorizedUserId);
			}
			Dictionary<string, object> messageEnvelope = new Dictionary<string, object>
			{
				{ "ttl", this.grantTTL },
				{ "permissions", permissionCollection }
			};
			string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
			byte[] postData = Encoding.UTF8.GetBytes(postMessage);

			List<string> pathSegments = new List<string>() {
				"v3",
				"pam",
				config.SubscribeKey,
				"grant"
			};


			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			if (queryParam != null && queryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in queryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNAccessManagerGrantToken, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.POST,
				PathSegment = pathSegments,
				Query = requestQueryStringParams,
				BodyContentString = postMessage
			};
			return requestParameter;
		}
	}
}
