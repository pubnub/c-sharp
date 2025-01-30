using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.Security.Crypto.Common;

namespace PubnubApi
{
	public class Middleware : ITransportMiddleware
	{
		private PNConfiguration configuration;
		private Pubnub pnInstance;
		private TokenManager tokenManager;
		private IHttpClientService httpClientService;

		public Middleware(IHttpClientService httpClientService, PNConfiguration configuration, Pubnub pnInstance, TokenManager tokenManager)
		{
			this.configuration = configuration;
			this.pnInstance = pnInstance;
			this.tokenManager = tokenManager;
			this.httpClientService = httpClientService;
		}

		public TransportRequest PreapareTransportRequest(RequestParameter requestParameter, PNOperationType operationType)
		{
			long timeStamp = TranslateUtcDateTimeToSeconds(DateTime.UtcNow);
			string requestid = Guid.NewGuid().ToString();
			string instanceId = pnInstance.InstanceId;

			Dictionary<string, string> commonQueryParameters = new Dictionary<string, string>
			{
				{ "uuid",UriUtil.EncodeUriComponent(configuration.UserId.ToString(),PNOperationType.PNSubscribeOperation, false, false, true)},
				{ "pnsdk", UriUtil.EncodeUriComponent(pnInstance.Version, PNOperationType.PNSubscribeOperation, false, false, true) }
			};

			if (configuration.IncludeInstanceIdentifier)
			{
				commonQueryParameters.Add("requestid", requestid);
			}
			if (configuration.IncludeInstanceIdentifier && !string.IsNullOrEmpty(instanceId) && instanceId.Trim().Length > 0)
			{
				commonQueryParameters.Add("instanceid", instanceId);
			}
			if (!string.IsNullOrEmpty(configuration.SecretKey))
			{
				commonQueryParameters.Add("timestamp", timeStamp.ToString(CultureInfo.InvariantCulture));
			}

			var excludedAuthOperationTypes = new[] {
				PNOperationType.PNTimeOperation,
				PNOperationType.PNAccessManagerGrant,
				PNOperationType.PNAccessManagerGrantToken,
				PNOperationType.PNAccessManagerRevokeToken,
				PNOperationType.ChannelGroupGrantAccess,
				PNOperationType.PNAccessManagerAudit,
				PNOperationType.ChannelGroupAuditAccess
			};
			if (!excludedAuthOperationTypes.Contains(operationType))
			{
				string authToken = tokenManager?.AuthToken?.Trim();
				string authKey = configuration.AuthKey?.Trim();

				if (!string.IsNullOrEmpty(authToken))
				{
					commonQueryParameters.Add("auth", UriUtil.EncodeUriComponent(authToken, operationType, false, false, false));
				}
				else if (!string.IsNullOrEmpty(authKey))
				{
					commonQueryParameters.Add("auth", UriUtil.EncodeUriComponent(authKey, operationType, false, false, false));
				}
			}
			requestParameter.Query = requestParameter.Query.Union(commonQueryParameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			var queryString = UriUtil.BuildQueryString(requestParameter.Query);
			var pathString = GeneratePathString(requestParameter.PathSegment, operationType);
			if (!string.IsNullOrEmpty(configuration.SecretKey))
			{
				string signature = string.Empty;
				StringBuilder stringToSign = new StringBuilder();
				stringToSign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", requestParameter.RequestType);
				stringToSign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", configuration.PublishKey);
				stringToSign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", pathString);
				stringToSign.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", queryString);
				if (!string.IsNullOrEmpty(requestParameter.BodyContentString)) stringToSign.Append(requestParameter.BodyContentString);
				signature = Util.PubnubAccessManagerSign(configuration.SecretKey, stringToSign.ToString());
				signature = string.Format(CultureInfo.InvariantCulture, "v2.{0}", signature.TrimEnd(new[] { '=' }));
				requestParameter.Query.Add("signature", signature);
			}
			var urlString = $"{(configuration.Secure ? "https://" : "http://")}{configuration.Origin}{pathString}?{UriUtil.BuildQueryString(requestParameter.Query)}";

			var cts = new CancellationTokenSource();
			cts.CancelAfter(operationType == PNOperationType.PNSubscribeOperation ? configuration.SubscribeTimeout * 1000 : configuration.NonSubscribeRequestTimeout* 1000);
			var transportRequest = new TransportRequest()
			{
				RequestType = requestParameter.RequestType,
				RequestUrl = urlString,
				BodyContentString = requestParameter.BodyContentString,
				FormData = requestParameter.FormData,
				CancellationToken = cts.Token,
			};
			if(requestParameter.Headers.Count>0) transportRequest.Headers = requestParameter.Headers;
			return transportRequest;
		}

		public Task<TransportResponse> Send(TransportRequest transportRequest)
		{
			LoggingMethod.WriteToLog(pnInstance.PNConfig.PubnubLog,$"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Sending request with url = {transportRequest.RequestUrl}" , pnInstance.PNConfig.LogVerbosity);
			switch (transportRequest.RequestType)
			{
				case Constants.GET:
					return httpClientService.GetRequest(transportRequest);
				case Constants.POST:
					return httpClientService.PostRequest(transportRequest);
				case Constants.PATCH:
					return httpClientService.PatchRequest(transportRequest);
				case Constants.PUT:
					return httpClientService.PutRequest(transportRequest);
				case Constants.DELETE:
					return httpClientService.DeleteRequest(transportRequest);
				default:
					return httpClientService.GetRequest(transportRequest);
			}
		}

		private string GeneratePathString(List<string> pathSegments, PNOperationType operationType)
		{
			StringBuilder pathString = new StringBuilder();
			foreach (var component in pathSegments)
			{
				pathString.Append('/');

				if ((operationType == PNOperationType.PNPublishOperation || operationType == PNOperationType.PNPublishFileMessageOperation) && component == pathSegments.Last())
				{
					pathString.Append(UriUtil.EncodeUriComponent(component, operationType, false, true, false));
				}
				else if (operationType == PNOperationType.PNAccessManagerRevokeToken)
				{
					pathString.Append(UriUtil.EncodeUriComponent(component, operationType, false, false, false));
				}
				else
				{
					pathString.Append(UriUtil.EncodeUriComponent(component, operationType, true, false, false));
				}
			}
			return pathString.ToString();
		}

		private long TranslateUtcDateTimeToSeconds(DateTime dotNetUTCDateTime)
		{
			TimeSpan timeSpan = dotNetUTCDateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long timeStamp = Convert.ToInt64(timeSpan.TotalSeconds);
			return timeStamp;
		}
	}
}
