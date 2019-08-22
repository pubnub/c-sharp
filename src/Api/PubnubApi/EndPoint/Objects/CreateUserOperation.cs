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
    public class CreateUserOperation : PubnubCoreBase
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

        private PNCallback<PNCreateUserResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public CreateUserOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, instance)
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

        public CreateUserOperation Id(string userId)
        {
            this.usrId = userId;
            return this;
        }

        public CreateUserOperation Name(string userName)
        {
            this.usrName = userName;
            return this;
        }

        public CreateUserOperation ExternalId(string userExternalId)
        {
            this.usrExternalId = userExternalId;
            return this;
        }

        public CreateUserOperation ProfileUrl(string userProfileUrl)
        {
            this.usrProfileUrl = userProfileUrl;
            return this;
        }

        public CreateUserOperation Email(string userEmail)
        {
            this.usrEmail = userEmail;
            return this;
        }

        public CreateUserOperation CustomObject(Dictionary<string, object> userCustomObject)
        {
            this.usrCustom = userCustomObject;
            return this;
        }

        public CreateUserOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNCreateUserResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                CreateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                CreateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                CreateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                CreateUser(this.usrId, this.usrName, this.usrExternalId, this.usrProfileUrl, this.usrEmail, this.usrCustom, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void CreateUser(string userId, string userName, string userExternalId, string  userProfileUrl, string userEmail, Dictionary<string, object> userCustom, Dictionary<string, object> externalQueryParam, PNCallback<PNCreateUserResult> callback)
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


            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildCreateUserRequest(userCustom, externalQueryParam);

            RequestState<PNCreateUserResult> requestState = new RequestState<PNCreateUserResult>();
            requestState.ResponseType = PNOperationType.PNCreateUserOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = "";

            requestState.UsePostMethod = true;
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
            string postMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            json = UrlProcessRequest<PNCreateUserResult>(request, requestState, false, postMessage);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNCreateUserResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
