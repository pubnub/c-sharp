using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Newtonsoft.Json;

namespace PubnubApi.EndPoint
{
    public class ManageMembershipsOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private string usrId = "";
        private Dictionary<string, object> mbrshipCustom;
        private List<PNMembership> addMembership;
        private List<PNMembership> updMembership;
        private List<string> delMembership;
        private string commandDelimitedIncludeOptions = "";
        private PNPage page;
        private int limit = -1;
        private bool includeCount;

        private PNCallback<PNManageMembershipsResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private class PNDeleteMembership
        {
            [JsonProperty(PropertyName = "id")]
            public string SpaceId { get; set; }
        }

        public ManageMembershipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
            pubnubTokenMgr = tokenManager;

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

        public ManageMembershipsOperation UserId(string id)
        {
            this.usrId = id;
            return this;
        }

        public ManageMembershipsOperation Add(List<PNMembership> membership)
        {
            this.addMembership = membership;
            return this;
        }

        public ManageMembershipsOperation Update(List<PNMembership> membership)
        {
            this.updMembership = membership;
            return this;
        }

        public ManageMembershipsOperation Remove(List<string> spaceIdList)
        {
            this.delMembership = spaceIdList;
            return this;
        }

        public ManageMembershipsOperation Include(PNMembershipField[] includeOptions)
        {
            if (includeOptions != null)
            {
                string[] arrayInclude = includeOptions.Select(x => MapEnumValueToEndpoint(x.ToString())).ToArray();
                this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
            }
            return this;
        }

        public ManageMembershipsOperation Page(PNPage bookmarkPage)
        {
            this.page = bookmarkPage;
            return this;
        }

        public ManageMembershipsOperation Limit(int numberOfObjects)
        {
            this.limit = numberOfObjects;
            return this;
        }

        public ManageMembershipsOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public ManageMembershipsOperation CustomObject(Dictionary<string, object> membershipCustomObject)
        {
            this.mbrshipCustom = membershipCustomObject;
            return this;
        }

        public ManageMembershipsOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNManageMembershipsResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                UpdateSpaceMembershipWithUser(this.usrId, this.addMembership, this.updMembership, this.delMembership, this.mbrshipCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                UpdateSpaceMembershipWithUser(this.usrId, this.addMembership, this.updMembership, this.delMembership, this.mbrshipCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                UpdateSpaceMembershipWithUser(this.usrId, this.addMembership, this.updMembership, this.delMembership, this.mbrshipCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                UpdateSpaceMembershipWithUser(this.usrId, this.addMembership, this.updMembership, this.delMembership, this.mbrshipCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void UpdateSpaceMembershipWithUser(string userId, List<PNMembership> addMembership, List<PNMembership> updateMembership, List<string> removeMembership, Dictionary<string, object> custom, PNPage page, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam, PNCallback<PNManageMembershipsResult> callback)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userId.Trim()))
            {
                throw new ArgumentException("Missing Id");
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing userCallback");
            }

            PNPage internalPage;
            if (page == null) { internalPage = new PNPage(); }
            else { internalPage = page; }

            RequestState<PNManageMembershipsResult> requestState = new RequestState<PNManageMembershipsResult>();
            requestState.ResponseType = PNOperationType.PNManageMembershipsOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (addMembership != null)
            {
                messageEnvelope.Add("add", addMembership);
            }
            if (updateMembership != null)
            {
                messageEnvelope.Add("update", updateMembership);
            }
            if (removeMembership != null)
            {
                List<PNDeleteMembership> removeMbrshipFormat = new List<PNDeleteMembership>();
                for (int index=0; index < removeMembership.Count; index++)
                {
                    if (!string.IsNullOrEmpty(removeMembership[index]))
                    {
                        removeMbrshipFormat.Add(new PNDeleteMembership { SpaceId = removeMembership[index] });
                    }
                }
                messageEnvelope.Add("remove", removeMbrshipFormat);
            }
            if (custom != null)
            {
                messageEnvelope.Add("custom", custom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildUpdateSpaceMembershipsWithUserRequest("PATCH", patchMessage, userId, internalPage.Next, internalPage.Prev, limit, includeCount, includeOptions, externalQueryParam);

            string json = UrlProcessRequest<PNManageMembershipsResult>(request, requestState, false, patchMessage);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNManageMembershipsResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        private static string MapEnumValueToEndpoint(string enumValue)
        {
            string ret = "";
            if (enumValue.ToLowerInvariant() == "custom")
            {
                ret = "custom";
            }
            else if (enumValue.ToLowerInvariant() == "user")
            {
                ret = "user";
            }
            else if (enumValue.ToLowerInvariant() == "space")
            {
                ret = "space";
            }
            else if (enumValue.ToLowerInvariant() == "space_custom")
            {
                ret = "space.custom";
            }
            else if (enumValue.ToLowerInvariant() == "user_custom")
            {
                ret = "user.custom";
            }
            return ret;
        }

    }
}
