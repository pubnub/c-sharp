using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Globalization;
#if !NET35 && !NET40
using System.Collections.Concurrent;
#endif

namespace PubnubApi.EndPoint
{
    public class SetStateOperation : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string[] channelNames;
        private string[] channelGroupNames;
        private Dictionary<string, object> userState;
        private string channelUUID = "";
        private PNCallback<PNSetStateResult> savedCallback;
        private Dictionary<string, object> queryParam;

        public SetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;

            PubnubInstance = instance;

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
            if (!ChannelUserState.ContainsKey(instance.InstanceId))
            {
                ChannelUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelLocalUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupLocalUserState.GetOrAdd(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
        }

        public SetStateOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public SetStateOperation ChannelGroups(string[] channelGroups)
        {
            this.channelGroupNames = channelGroups;
            return this;
        }

        public SetStateOperation State(Dictionary<string, object> state)
        {
            this.userState = state;
            return this;
        }

        public SetStateOperation Uuid(string uuid)
        {
            this.channelUUID = uuid;
            return this;
        }

        public SetStateOperation QueryParam(Dictionary<string, object> customQueryParam)
        {
            this.queryParam = customQueryParam;
            return this;
        }

        [Obsolete("Async is deprecated, please use Execute instead.")]
        public void Async(PNCallback<PNSetStateResult> callback)
        {
            Execute(callback);
        }

        public void Execute(PNCallback<PNSetStateResult> callback)
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
                SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, this.queryParam, callback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                this.savedCallback = callback;
                string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
                SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, this.queryParam, callback);
            })
            { IsBackground = true }.Start();
#endif
        }

        public async Task<PNResult<PNSetStateResult>> ExecuteAsync()
        {
            string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
            return await SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, this.queryParam).ConfigureAwait(false);
        }

        internal void Retry()
        {
#if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task.Factory.StartNew(() =>
            {
                string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
                SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, this.queryParam, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
#else
            new Thread(() =>
            {
                string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
                SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, this.queryParam, savedCallback);
            })
            { IsBackground = true }.Start();
#endif
        }

        internal void SetUserState(string[] channels, string[] channelGroups, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam, PNCallback<PNSetStateResult> callback)
        {
            if ((channels == null && channelGroups == null)
                            || (channels != null && channelGroups != null && channels.Length == 0 && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided");
            }

            if (string.IsNullOrEmpty(jsonUserState) || string.IsNullOrEmpty(jsonUserState.Trim()))
            {
                throw new ArgumentException("Missing User State");
            }

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            string[] filteredChannels = channels;
            string[] filteredChannelGroups = channelGroups;

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
                filteredChannels = channelList.ToArray();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
                filteredChannelGroups = channelGroupList.ToArray();
            }

            if (!jsonLibrary.IsDictionaryCompatible(jsonUserState, PNOperationType.PNSetStateOperation))
            {
                throw new MissingMemberException("Missing json format for user state");
            }
            else
            {
                Dictionary<string, object> deserializeUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonUserState);
                if (deserializeUserState == null)
                {
                    throw new MissingMemberException("Missing json format user state");
                }
                else
                {
                    bool stateChanged = false;

                    for (int channelIndex = 0; channelIndex < channelList.Count; channelIndex++)
                    {
                        string currentChannel = channelList[channelIndex];

                        string oldJsonChannelState = GetLocalUserState(currentChannel, "");

                        if (oldJsonChannelState != jsonUserState)
                        {
                            stateChanged = true;
                            break;
                        }
                    }

                    if (!stateChanged)
                    {
                        for (int channelGroupIndex = 0; channelGroupIndex < channelGroupList.Count; channelGroupIndex++)
                        {
                            string currentChannelGroup = channelGroupList[channelGroupIndex];

                            string oldJsonChannelGroupState = GetLocalUserState("", currentChannelGroup);

                            if (oldJsonChannelGroupState != jsonUserState)
                            {
                                stateChanged = true;
                                break;
                            }
                        }
                    }

                    if (!stateChanged)
                    {
                        StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
                        PNStatus status = statusBuilder.CreateStatusResponse< PNSetStateResult>(PNOperationType.PNSetStateOperation, PNStatusCategory.PNUnknownCategory, null, (int)System.Net.HttpStatusCode.NotModified, null);

                        Announce(status);
                        return;
                    }

                }
            }

            SharedSetUserState(filteredChannels, filteredChannelGroups, uuid, jsonUserState, jsonUserState, externalQueryParam, callback);
        }

        internal async Task<PNResult<PNSetStateResult>> SetUserState(string[] channels, string[] channelGroups, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam)
        {
            if ((channels == null && channelGroups == null)
                            || (channels != null && channelGroups != null && channels.Length == 0 && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided");
            }

            if (string.IsNullOrEmpty(jsonUserState) || string.IsNullOrEmpty(jsonUserState.Trim()))
            {
                throw new ArgumentException("Missing User State");
            }

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();
            string[] filteredChannels = channels;
            string[] filteredChannelGroups = channelGroups;

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
                filteredChannels = channelList.ToArray();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
                filteredChannelGroups = channelGroupList.ToArray();
            }

            if (!jsonLibrary.IsDictionaryCompatible(jsonUserState, PNOperationType.PNSetStateOperation))
            {
                throw new MissingMemberException("Missing json format for user state");
            }
            else
            {
                Dictionary<string, object> deserializeUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonUserState);
                if (deserializeUserState == null)
                {
                    throw new MissingMemberException("Missing json format user state");
                }
                else
                {
                    bool stateChanged = false;

                    for (int channelIndex = 0; channelIndex < channelList.Count; channelIndex++)
                    {
                        string currentChannel = channelList[channelIndex];

                        string oldJsonChannelState = GetLocalUserState(currentChannel, "");

                        if (oldJsonChannelState != jsonUserState)
                        {
                            stateChanged = true;
                            break;
                        }
                    }

                    if (!stateChanged)
                    {
                        for (int channelGroupIndex = 0; channelGroupIndex < channelGroupList.Count; channelGroupIndex++)
                        {
                            string currentChannelGroup = channelGroupList[channelGroupIndex];

                            string oldJsonChannelGroupState = GetLocalUserState("", currentChannelGroup);

                            if (oldJsonChannelGroupState != jsonUserState)
                            {
                                stateChanged = true;
                                break;
                            }
                        }
                    }

                    if (!stateChanged)
                    {
                        PNResult<PNSetStateResult> errRet = new PNResult<PNSetStateResult>();

                        StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
                        PNStatus status = statusBuilder.CreateStatusResponse<PNSetStateResult>(PNOperationType.PNSetStateOperation, PNStatusCategory.PNUnknownCategory, null, (int)System.Net.HttpStatusCode.NotModified, null);
                        errRet.Status = status;
                        return errRet;
                    }

                }
            }

            return await SharedSetUserState(filteredChannels, filteredChannelGroups, uuid, jsonUserState, jsonUserState, externalQueryParam).ConfigureAwait(false);
        }

        private void SharedSetUserState(string[] channels, string[] channelGroups, string uuid, string jsonChannelUserState, string jsonChannelGroupUserState, Dictionary<string, object> externalQueryParam, PNCallback<PNSetStateResult> callback)
        {
            List<string> channelList;
            List<string> channelGroupList;
            string currentUuid = uuid;

            string[] channelArray = null;
            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
                channelArray = channelList.ToArray();
            }

            string[] channelGroupsArray = null;
            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
                channelGroupsArray = channelGroupList.ToArray();
            }

            string commaDelimitedChannels = (channelArray != null && channelArray.Length > 0) ? string.Join(",", channelArray.OrderBy(x => x).ToArray()) : "";
            string commaDelimitedChannelGroups = (channelGroupsArray != null && channelGroupsArray.Length > 0) ? string.Join(",", channelGroupsArray.OrderBy(x => x).ToArray()) : "";

            if (string.IsNullOrEmpty(uuid))
            {
                currentUuid = config.UserId;
            }            

            string jsonUserState = GetJsonSharedSetUserStateInternal(channels, channelGroups, jsonChannelUserState, jsonChannelGroupUserState);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildSetUserStateRequest("GET", "", commaDelimitedChannels, commaDelimitedChannelGroups, currentUuid, jsonUserState, externalQueryParam);

            RequestState<PNSetStateResult> requestState = new RequestState<PNSetStateResult>();
            requestState.Channels = channelArray;
            requestState.ChannelGroups = channelGroupsArray;
            requestState.ResponseType = PNOperationType.PNSetStateOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            //Set TerminateSubRequest to true to bounce the long-polling subscribe requests to update user state
            UrlProcessRequest(request, requestState, false).ContinueWith(r =>
            {
                string json = r.Result.Item1;
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse(requestState, json);
                    ProcessResponseCallbacks(result, requestState);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Wait();
        }

        private async Task<PNResult<PNSetStateResult>> SharedSetUserState(string[] channels, string[] channelGroups, string uuid, string jsonChannelUserState, string jsonChannelGroupUserState, Dictionary<string, object> externalQueryParam)
        {
            PNResult<PNSetStateResult> ret = new PNResult<PNSetStateResult>();

            List<string> channelList;
            List<string> channelGroupList;
            string currentUuid = uuid;

            string[] channelArray = null;
            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
                channelArray = channelList.ToArray();
            }

            string[] channelGroupsArray = null;
            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
                channelGroupsArray = channelGroupList.ToArray();
            }

            string commaDelimitedChannels = (channelArray != null && channelArray.Length > 0) ? string.Join(",", channelArray.OrderBy(x => x).ToArray()) : "";
            string commaDelimitedChannelGroups = (channelGroupsArray != null && channelGroupsArray.Length > 0) ? string.Join(",", channelGroupsArray.OrderBy(x => x).ToArray()) : "";

            if (string.IsNullOrEmpty(uuid))
            {
                currentUuid = config.UserId;
            }

            string jsonUserState = GetJsonSharedSetUserStateInternal(channels, channelGroups, jsonChannelUserState, jsonChannelGroupUserState);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");
            
            Uri request = urlBuilder.BuildSetUserStateRequest("GET", "", commaDelimitedChannels, commaDelimitedChannelGroups, currentUuid, jsonUserState, externalQueryParam);

            RequestState<PNSetStateResult> requestState = new RequestState<PNSetStateResult>();
            requestState.Channels = channelArray;
            requestState.ChannelGroups = channelGroupsArray;
            requestState.ResponseType = PNOperationType.PNSetStateOperation;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            //Set TerminateSubRequest to true to bounce the long-polling subscribe requests to update user state
            Tuple<string, PNStatus> JsonAndStatusTuple = await UrlProcessRequest(request, requestState, false).ConfigureAwait(false);
            ret.Status = JsonAndStatusTuple.Item2;
            string json = JsonAndStatusTuple.Item1;
            if (!string.IsNullOrEmpty(json))
            {
                List<object> resultList = ProcessJsonResponse(requestState, json);
                ResponseBuilder responseBuilder = new ResponseBuilder(config, jsonLibrary, pubnubLog);
                PNSetStateResult responseResult = responseBuilder.JsonToObject<PNSetStateResult>(resultList, true);
                if (responseResult != null)
                {
                    ret.Result = responseResult;
                }
            }

            return ret;
        }

        private string GetJsonSharedSetUserStateInternal(string[] channels, string[] channelGroups, string jsonChannelUserState, string jsonChannelGroupUserState)
        {
            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
            }

            Dictionary<string, object> deserializeChannelUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonChannelUserState);
            Dictionary<string, object> deserializeChannelGroupUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonChannelGroupUserState);

            for (int channelIndex = 0; channelIndex < channelList.Count; channelIndex++)
            {
                string currentChannel = channelList[channelIndex];

                ChannelUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
                ChannelLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
            }

            for (int channelGroupIndex = 0; channelGroupIndex < channelGroupList.Count; channelGroupIndex++)
            {
                string currentChannelGroup = channelGroupList[channelGroupIndex];

                ChannelGroupUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
                ChannelGroupLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
            }

            string jsonUserState = "{}";

            if ((jsonChannelUserState == jsonChannelGroupUserState) || (jsonChannelUserState != "{}" && jsonChannelGroupUserState == "{}"))
            {
                jsonUserState = jsonChannelUserState;
            }
            else if (jsonChannelUserState == "{}" && jsonChannelGroupUserState != "{}")
            {
                jsonUserState = jsonChannelGroupUserState;
            }
            else if (jsonChannelUserState != "{}" && jsonChannelGroupUserState != "{}")
            {
                jsonUserState = "";
                for (int channelIndex = 0; channelIndex < channelList.Count; channelIndex++)
                {
                    string currentChannel = channelList[channelIndex];

                    if (jsonUserState == "")
                    {
                        jsonUserState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", currentChannel, jsonChannelUserState);
                    }
                    else
                    {
                        jsonUserState = string.Format(CultureInfo.InvariantCulture, "{0},\"{1}\":{{{2}}}", jsonUserState, currentChannel, jsonChannelUserState);
                    }
                }
                for (int channelGroupIndex = 0; channelGroupIndex < channelGroupList.Count; channelGroupIndex++)
                {
                    string currentChannelGroup = channelGroupList[channelGroupIndex];

                    if (jsonUserState == "")
                    {
                        jsonUserState = string.Format(CultureInfo.InvariantCulture, "\"{0}\":{{{1}}}", currentChannelGroup, jsonChannelGroupUserState);
                    }
                    else
                    {
                        jsonUserState = string.Format(CultureInfo.InvariantCulture, "{0},\"{1}\":{{{2}}}", jsonUserState, currentChannelGroup, jsonChannelGroupUserState);
                    }
                }
                jsonUserState = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", jsonUserState);
            }

            return jsonUserState;
        }

        private string GetLocalUserState(string channel, string channelGroup)
        {
            string retJsonUserState = "";
            StringBuilder jsonStateBuilder = new StringBuilder();

            string channelJsonUserState = BuildJsonUserState(channel, "", false);
            string channelGroupJsonUserState = BuildJsonUserState("", channelGroup, false);

            if (channelJsonUserState.Trim().Length > 0 && channelGroupJsonUserState.Trim().Length <= 0)
            {
                jsonStateBuilder.Append(channelJsonUserState);
            }
            else if (channelJsonUserState.Trim().Length <= 0 && channelGroupJsonUserState.Trim().Length > 0)
            {
                jsonStateBuilder.Append(channelGroupJsonUserState);
            }
            else if (channelJsonUserState.Trim().Length > 0 && channelGroupJsonUserState.Trim().Length > 0)
            {
                jsonStateBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}:{1},{2}:{3}", channel, channelJsonUserState, channelGroup, channelGroupJsonUserState);
            }

            if (jsonStateBuilder.Length > 0)
            {
                retJsonUserState = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", jsonStateBuilder);
            }

            return retJsonUserState;
        }
    }
}
