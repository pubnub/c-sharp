using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace PubnubApi.EndPoint
{
    public class UpdateUserOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string usrId = "";
        private string usrName = "";
        private string usrExternalId;
        private string usrProfileUrl;
        private string usrEmail;
        private Dictionary<string, object> usrCustom;

        private PNCallback<PNUpdateUserResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public UpdateUserOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
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

        public UpdateUserOperation Id(string userId)
        {
            this.usrId = userId;
            return this;
        }

        public UpdateUserOperation Name(string userName)
        {
            this.usrName = userName;
            return this;
        }

        public UpdateUserOperation ExternalId(string userExternalId)
        {
            this.usrExternalId = userExternalId;
            return this;
        }

        public UpdateUserOperation ProfileUrl(string userProfileUrl)
        {
            this.usrProfileUrl = userProfileUrl;
            return this;
        }

        public UpdateUserOperation Email(string userEmail)
        {
            this.usrEmail = userEmail;
            return this;
        }

        public UpdateUserOperation CustomObject(Dictionary<string, object> userCustomObject)
        {
            this.usrCustom = userCustomObject;
            return this;
        }

        public UpdateUserOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNUpdateUserResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                UpdateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                UpdateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                UpdateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                UpdateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void UpdateUser(string userId, string userName, string userExternalId, string userProfileUrl, string userEmail, Dictionary<string, object> userCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNUpdateUserResult> callback)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userId.Trim()) || userName == null)
            {
                throw new ArgumentException("Missing Id or Name");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }


            RequestState<PNUpdateUserResult> requestState = new RequestState<PNUpdateUserResult>();
            requestState.ResponseType = PNOperationType.PNUpdateUserOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            messageEnvelope.Add("id", userId);
            messageEnvelope.Add("name", userName);
            if (userExternalId != null)
            {
                messageEnvelope.Add("externalId", userExternalId);
            }
            if (userProfileUrl != null)
            {
                messageEnvelope.Add("profileUrl", userProfileUrl);
            }
            if (userEmail != null)
            {
                messageEnvelope.Add("email", userEmail);
            }
            if (userCustom != null)
            {
                messageEnvelope.Add("custom", userCustom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildUpdateUserRequest("PATCH", patchMessage, userId, userCustom, externalQueryParam);

            string json = UrlProcessRequest<PNUpdateUserResult>(request, requestState, false, patchMessage);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNUpdateUserResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
