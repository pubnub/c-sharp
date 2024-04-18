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
    public class RemoveMembershipsOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string uuidMetadataId = "";
        private List<string> delMembership;
        private string commandDelimitedIncludeOptions = "";
        private PNPageObject page;
        private int limit = -1;
        private bool includeCount;
        private List<string> sortField;

        private PNCallback<PNMembershipsResult> savedCallback;
        private Dictionary<string, object> queryParam;
        public RemoveMembershipsOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
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

        public RemoveMembershipsOperation Uuid(string id)
        {
            this.uuidMetadataId = id;
            return this;
        }

        public RemoveMembershipsOperation Channels(List<string> channelIdList)
        {
            this.delMembership = channelIdList;
            return this;
        }

        public RemoveMembershipsOperation Include(PNMembershipField[] includeOptions)
        {
            if (includeOptions != null)
            {
                string[] arrayInclude = includeOptions.Select(x => MapEnumValueToEndpoint(x.ToString())).ToArray();
                this.commandDelimitedIncludeOptions = string.Join(",", arrayInclude);
            }
            return this;
        }

        public RemoveMembershipsOperation Page(PNPageObject pageObject)
        {
            this.page = pageObject;
            return this;
        }

        public RemoveMembershipsOperation Limit(int numberOfObjects)
        {
            this.limit = numberOfObjects;
            return this;
        }

        public RemoveMembershipsOperation IncludeCount(bool includeTotalCount)
        {
            this.includeCount = includeTotalCount;
            return this;
        }

        public RemoveMembershipsOperation Sort(List<string> sortByField)
        {
            this.sortField = sortByField;
            return this;
        }

        public RemoveMembershipsOperation QueryParam(Dictionary<string, object> customQueryParam)
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
                RemoveUuidMemberships(this.uuidMetadataId, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                RemoveUuidMemberships(this.uuidMetadataId, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNMembershipsResult>> ExecuteAsync()
        {
            return await RemoveUuidMemberships(this.uuidMetadataId, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                RemoveUuidMemberships(this.uuidMetadataId, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                RemoveUuidMemberships(this.uuidMetadataId, this.delMembership, this.page, this.limit, this.includeCount, this.commandDelimitedIncludeOptions, this.sortField, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        private void RemoveUuidMemberships(string uuid, List<string> removeMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam, PNCallback<PNMembershipsResult> callback)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.UserId;
            }

            PNPageObject internalPage;
            if (page == null) { internalPage = new PNPageObject(); }
            else { internalPage = page; }

            RequestState<PNMembershipsResult> requestState = new RequestState<PNMembershipsResult>();
            requestState.ResponseType = PNOperationType.PNRemoveMembershipsOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (removeMembership != null)
            {
                List<Dictionary<string, Dictionary<string, string>>> removeMembershipFormatList = new List<Dictionary<string, Dictionary<string, string>>>();
                for (int index = 0; index < removeMembership.Count; index++)
                {
                    Dictionary<string, Dictionary<string, string>> currentMembershipFormat = new Dictionary<string, Dictionary<string, string>>();
                    if (!string.IsNullOrEmpty(removeMembership[index]))
                    {
                        currentMembershipFormat.Add("channel", new Dictionary<string, string> { { "id", removeMembership[index] } });
                        removeMembershipFormatList.Add(currentMembershipFormat);
                    }
                }
                if (removeMembershipFormatList.Count > 0)
                {
                    messageEnvelope.Add("delete", removeMembershipFormatList);
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

        private async Task<PNResult<PNMembershipsResult>> RemoveUuidMemberships(string uuid, List<string> removeMembership, PNPageObject page, int limit, bool includeCount, string includeOptions, List<string> sort, Dictionary<string, object> externalQueryParam)
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
            requestState.ResponseType = PNOperationType.PNRemoveMembershipsOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            requestState.UsePatchMethod = true;
            Dictionary<string, object> messageEnvelope = new Dictionary<string, object>();
            if (removeMembership != null)
            {
                List<Dictionary<string, Dictionary<string, string>>> removeMembershipFormatList = new List<Dictionary<string, Dictionary<string, string>>>();
                for (int index = 0; index < removeMembership.Count; index++)
                {
                    Dictionary<string, Dictionary<string, string>> currentMembershipFormat = new Dictionary<string, Dictionary<string, string>>();
                    if (!string.IsNullOrEmpty(removeMembership[index]))
                    {
                        currentMembershipFormat.Add("channel", new Dictionary<string, string> { { "id", removeMembership[index] } });
                        removeMembershipFormatList.Add(currentMembershipFormat);
                    }
                }
                if (removeMembershipFormatList.Count > 0)
                {
                    messageEnvelope.Add("delete", removeMembershipFormatList);
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
