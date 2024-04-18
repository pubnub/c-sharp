using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class SetMembershipsOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string uuidMetadataId = "";
        private List<PNMembership> addMembership;
        private string commandDelimitedIncludeOptions = "";
        private PNPageObject page;
        private int limit = -1;
        private bool includeCount;
        private List<string> sortField;

        private PNCallback<PNMembershipsResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public SetMembershipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public SetMembershipsOperation Uuid(string id)
        {
            this.uuidMetadataId = id;
            return this;
        }

        public SetMembershipsOperation Channels(List<PNMembership> memberships)
        {
            this.addMembership = memberships;
            return this;
        }

        public SetMembershipsOperation Include(PNMembershipField[] includeOptions)
        {
            if (includeOptions != null)
            {
                string[] arrayInclude = includeOptions.Select(x => MapEnumValueToEndpoint(x.ToString())).ToArray();
                this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
            }
            return this;
        }

        public SetMembershipsOperation Page(PNPageObject pageObject)
        {
            this.page = pageObject;
            return this;
        }

        public SetMembershipsOperation Limit(int numberOfObjects)
        {
            this.limit = numberOfObjects;
            return this;
        }

        public SetMembershipsOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public SetMembershipsOperation Sort(List<string> sortByField)
        {
            this.sortField = sortByField;
            return this;
        }

        public SetMembershipsOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        public void Execute(PNCallback<PNMembershipsResult> callback)
        {
            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid subscribe key");
            }

            if (callback == null)
            {
                throw new ArgumentException("Missing callback");
            }

#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                SetChannelMembershipWithUuid(this.uuidMetadataId, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                SetChannelMembershipWithUuid(this.uuidMetadataId, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNMembershipsResult>> ExecuteAsync()
        {
            return await SetChannelMembershipWithUuid(this.uuidMetadataId, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                SetChannelMembershipWithUuid(this.uuidMetadataId, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                SetChannelMembershipWithUuid(this.uuidMetadataId, this.addMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void SetChannelMembershipWithUuid(string uuid, List<PNMembership> setMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNMembershipsResult> callback)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.UserId;
            }

            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>();
            requestState.ResponseType = PNOperationType.PNSetMembershipsOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (setMembership != null)
            {
                List<Dictionary<string, object>> setMembershipFormatList = new List<Dictionary<string, object>>();
                for (int index = 0; index < setMembership.Count; index++)
                {
                    Dictionary<string, object> currentMembershipFormat = new Dictionary<string, object>();
                    currentMembershipFormat.Add("channel", new Dictionary<string, string> { { "id", setMembership[index].Channel } });
                    if (setMembership[index].Custom != null)
                    {
                        currentMembershipFormat.Add("custom", setMembership[index].Custom);
                    }
                    setMembershipFormatList.Add(currentMembershipFormat);
                }
                if (setMembershipFormatList.Count > 0)
                {
                    messageEnvelope.Add("set", setMembershipFormatList);
                }
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildMembershipSetRemoveManageUserRequest(requestState.ResponseType, "PATCH", patchMessage, uuid, internalPage.Next, internalPage.Prev, limit, includeCount, includeOptions, sort, externalQueryParam);

            UrlProcessRequest(request, requestState, false, patchData).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
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

        private async Task<PNResult<PNMembershipsResult>> SetChannelMembershipWithUuid(string uuid, List<PNMembership> setMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNMembershipsResult> ret = new PNResult<PNMembershipsResult>();

            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.UserId;
            }

            if (string.IsNullOrEmpty(config.SubscribeKey) || string.IsNullOrEmpty(config.SubscribeKey.Trim()) || config.SubscribeKey.Length <= 0)
            {
                PNStatus errStatus = new PNStatus { Error = true, ErrorData = new PNErrorData("Invalid Subscribe key", new ArgumentException("Invalid Subscribe key")) };
                ret.Status = errStatus;
                return ret;
            }

            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>();
            requestState.ResponseType = PNOperationType.PNSetMembershipsOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (setMembership != null)
            {
                List<Dictionary<string, object>> setMembershipFormatList = new List<Dictionary<string, object>>();
                for (int index = 0; index < setMembership.Count; index++)
                {
                    Dictionary<string, object> currentMembershipFormat = new Dictionary<string, object>();
                    currentMembershipFormat.Add("channel", new Dictionary<string, string> { { "id", setMembership[index].Channel } });
                    if (setMembership[index].Custom != null)
                    {
                        currentMembershipFormat.Add("custom", setMembership[index].Custom);
                    }
                    setMembershipFormatList.Add(currentMembershipFormat);
                }
                if (setMembershipFormatList.Count > 0)
                {
                    messageEnvelope.Add("set", setMembershipFormatList);
                }
            }
            string patchMessage = jsonLibrary.SerializeToJsonString(messageEnvelope);
            byte[] patchData = Encoding.UTF8.GetBytes(patchMessage);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            Uri request = urlBuilder.BuildMembershipSetRemoveManageUserRequest(requestState.ResponseType, "PATCH", patchMessage, uuid, internalPage.Next, internalPage.Prev, limit, includeCount, includeOptions, sort, externalQueryParam);

            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false, patchData).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNMembershipsResult responseResult = responseBuilder.JsonToObject<PNMembershipsResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }

        private static string MapEnumValueToEndpoint(string enumValue)
        {
            string ret = "";
            if (enumValue.ToLowerInvariant() == "custom")
            {
                ret = "custom";
            }
            else if (enumValue.ToLowerInvariant() == "uuid")
            {
                ret = "uuid";
            }
            else if (enumValue.ToLowerInvariant() == "channel")
            {
                ret = "channel";
            }
            else if (enumValue.ToLowerInvariant() == "channel_custom")
            {
                ret = "channel.custom";
            }
            else if (enumValue.ToLowerInvariant() == "uuid_custom")
            {
                ret = "uuid.custom";
            }
            return ret;
        }

    }
}
