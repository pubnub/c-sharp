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
    public class ManageMembersOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
        private readonly EndPoint.TokenManager pubnubTokenMgr;

        private string spcId = "";
        private Dictionary<string, object> mbrCustom;
        private List<PNMember> addMember;
        private List<PNMember> updMember;
        private List<string> delMember;
        private string commandDelimitedIncludeOptions = "";
        private PNPage page;
        private int limit = -1;
        private bool includeCount;

        private PNCallback<PNManageMembersResult> savedCallback;
        private Dictionary<string, object> queryParam;

        private class PNDeleteMember
        {
            [JsonProperty(PropertyName = "id")]
            public string UserId { get; set; }
        }

        public ManageMembersOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public ManageMembersOperation SpaceId(string id)
        {
            this.spcId = id;
            return this;
        }

        public ManageMembersOperation Add(List<PNMember> members)
        {
            this.addMember = members;
            return this;
        }

        public ManageMembersOperation Update(List<PNMember> members)
        {
            this.updMember = members;
            return this;
        }

        public ManageMembersOperation Remove(List<string> userIdList)
        {
            this.delMember = userIdList;
            return this;
        }

        public ManageMembersOperation Include(PNMemberField[] includeOptions)
        {
            if (includeOptions != null)
            {
                string[] arrayInclude = includeOptions.Select(x => MapEnumValueToEndpoint(x.ToString())).ToArray();
                this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
            }
            return this;
        }

        public ManageMembersOperation Page(PNPage bookmarkPage)
        {
            this.page = bookmarkPage;
            return this;
        }

        public ManageMembersOperation Limit(int numberOfObjects)
        {
            this.limit = numberOfObjects;
            return this;
        }

        public ManageMembersOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public ManageMembersOperation CustomObject(Dictionary<string, object> memberCustomObject)
        {
            this.mbrCustom = memberCustomObject;
            return this;
        }

        public ManageMembersOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNManageMembersResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                ProcessMembersOperationRequest(this.spcId, this.addMember, this.updMember, this.delMember, this.mbrCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                ProcessMembersOperationRequest(this.spcId, this.addMember, this.updMember, this.delMember, this.mbrCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                ProcessMembersOperationRequest(this.spcId, this.addMember, this.updMember, this.delMember, this.mbrCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                ProcessMembersOperationRequest(this.spcId, this.addMember, this.updMember, this.delMember, this.mbrCustom, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void ProcessMembersOperationRequest(string spaceId, List<PNMember> addMemberList, List<PNMember> updateMemberList, List<string> removeMemberList, Dictionary<string, object> custom, PNPage page, int limit, bool includeCount, string includeOptions, Dictionary<string, object> externalQueryParam, PNCallback<PNManageMembersResult> callback)
        {
            if (string.IsNullOrEmpty(spaceId) || string.IsNullOrEmpty(spaceId.Trim()))
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

            RequestState<PNManageMembersResult> requestState = new RequestState<PNManageMembersResult>();
            requestState.ResponseType = PNOperationType.PNManageMembersOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (addMemberList != null)
            {
                messageEnvelope.Add("add", addMemberList);
            }
            if (updateMemberList != null)
            {
                messageEnvelope.Add("update", updateMemberList);
            }
            if (removeMemberList != null)
            {
                List<PNDeleteMember> removeMbrFormat = new List<PNDeleteMember>();
                for (int index = 0; index < removeMemberList.Count; index++)
                {
                    if (!string.IsNullOrEmpty(removeMemberList[index]))
                    {
                        removeMbrFormat.Add(new PNDeleteMember { UserId = removeMemberList[index] });
                    }
                }
                messageEnvelope.Add("remove", removeMbrFormat);
            }
            if (custom != null)
            {
                messageEnvelope.Add("custom", custom);
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, pubnubTokenMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildMembersAddUpdateRemoveRequest("PATCH", patchMessage, spaceId, internalPage.Next, internalPage.Prev, limit, includeCount, includeOptions, externalQueryParam);

            string json = UrlProcessRequest<PNManageMembersResult>(request, requestState, false, patchMessage);

            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNManageMembersResult>(requestState, json);
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
            else if (enumValue.ToLowerInvariant() == "space")
            {
                ret = "space";
            }
            else if (enumValue.ToLowerInvariant() == "space_custom")
            {
                ret = "space.custom";
            }
            return ret;
        }

    }
}
