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
    public class SetStateOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;
        private EndPoint.TelemetryManager pubnubTelemetryMgr;

        private string[] channelNames = null;
        private string[] channelGroupNames = null;
        private Dictionary<string, object> userState = null;
        private string channelUUID = "";
        private PNCallback<PNSetStateResult> savedCallback = null;

        public SetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
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

        public void Async(PNCallback<PNSetStateResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
                SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
                SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void SetUserState(string[] channels, string[] channelGroups, string uuid, string jsonUserState, PNCallback<PNSetStateResult> callback)
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

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
                channels = channelList.ToArray();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
                channelGroups = channelGroupList.ToArray();
            }

            string commaDelimitedChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string commaDelimitedChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            if (!jsonLibrary.IsDictionaryCompatible(jsonUserState))
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

            SharedSetUserState(channels, channelGroups, uuid, jsonUserState, jsonUserState, callback);
        }

        internal void SetUserState(string[] channels, string[] channelGroups, string uuid, KeyValuePair<string, object> keyValuePair, PNCallback<PNSetStateResult> callback)
        {
            if ((channels == null && channelGroups != null) || (channels.Length == 0 && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
                channels = channelList.ToArray();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
                channelGroups = channelGroupList.ToArray();
            }

            string commaDelimitedChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string commaDelimitedChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            string key = keyValuePair.Key;

            int valueInt;
            double valueDouble;
            bool stateChanged = false;
            string currentChannelUserState = "";
            string currentChannelGroupUserState = "";

            for (int channelIndex = 0; channelIndex < channelList.Count; channelIndex++)
            {
                string currentChannel = channelList[channelIndex];

                string oldJsonChannelState = GetLocalUserState(currentChannel, "");

                if (keyValuePair.Value == null)
                {
                    currentChannelUserState = SetLocalUserState(currentChannel, "", key, null);
                }
                else if (Int32.TryParse(keyValuePair.Value.ToString(), out valueInt))
                {
                    currentChannelUserState = SetLocalUserState(currentChannel, "", key, valueInt);
                }
                else if (Double.TryParse(keyValuePair.Value.ToString(), out valueDouble))
                {
                    currentChannelUserState = SetLocalUserState(currentChannel, "", key, valueDouble);
                }
                else
                {
                    currentChannelUserState = SetLocalUserState(currentChannel, "", key, keyValuePair.Value.ToString());
                }
                if (oldJsonChannelState != currentChannelUserState)
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
                    currentChannelGroupUserState = "";

                    if (keyValuePair.Value == null)
                    {
                        currentChannelGroupUserState = SetLocalUserState("", currentChannelGroup, key, null);
                    }
                    else if (Int32.TryParse(keyValuePair.Value.ToString(), out valueInt))
                    {
                        currentChannelGroupUserState = SetLocalUserState("", currentChannelGroup, key, valueInt);
                    }
                    else if (Double.TryParse(keyValuePair.Value.ToString(), out valueDouble))
                    {
                        currentChannelGroupUserState = SetLocalUserState("", currentChannelGroup, key, valueDouble);
                    }
                    else
                    {
                        currentChannelGroupUserState = SetLocalUserState("", currentChannelGroup, key, keyValuePair.Value.ToString());
                    }

                    if (oldJsonChannelGroupState != currentChannelGroupUserState)
                    {
                        stateChanged = true;
                        break;
                    }
                }
            }


            if (!stateChanged)
            {
                StatusBuilder statusBuilder = new StatusBuilder(config, jsonLibrary);
                PNStatus status = statusBuilder.CreateStatusResponse<PNSetStateResult>(PNOperationType.PNSetStateOperation, PNStatusCategory.PNUnknownCategory, null, (int)System.Net.HttpStatusCode.NotModified, null);

                Announce(status);
                return;
            }

            if (currentChannelUserState.Trim() == "")
            {
                currentChannelUserState = "{}";
            }
            if (currentChannelGroupUserState == "")
            {
                currentChannelGroupUserState = "{}";
            }

            SharedSetUserState(channels, channelGroups, uuid, currentChannelUserState, currentChannelGroupUserState, callback);
        }

        private void SharedSetUserState(string[] channels, string[] channelGroups, string uuid, string jsonChannelUserState, string jsonChannelGroupUserState, PNCallback<PNSetStateResult> callback)
        {
            List<string> channelList = new List<string>();
            List<string> channelGroupList = new List<string>();

            if (channels != null && channels.Length > 0)
            {
                channelList = new List<string>(channels);
                channelList = channelList.Where(ch => !string.IsNullOrEmpty(ch) && ch.Trim().Length > 0).Distinct<string>().ToList();
                channels = channelList.ToArray();
            }

            if (channelGroups != null && channelGroups.Length > 0)
            {
                channelGroupList = new List<string>(channelGroups);
                channelGroupList = channelGroupList.Where(cg => !string.IsNullOrEmpty(cg) && cg.Trim().Length > 0).Distinct<string>().ToList();
                channelGroups = channelGroupList.ToArray();
            }

            string commaDelimitedChannels = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string commaDelimitedChannelGroups = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.Uuid;
            }

            Dictionary<string, object> deserializeChannelUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonChannelUserState);
            Dictionary<string, object> deserializeChannelGroupUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonChannelGroupUserState);

            for (int channelIndex=0; channelIndex < channelList.Count; channelIndex++)
            {
                string currentChannel = channelList[channelIndex];

                ChannelUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
                ChannelLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
            }

            for (int channelGroupIndex=0; channelGroupIndex < channelGroupList.Count; channelGroupIndex++)
            {
                string currentChannelGroup = channelGroupList[channelGroupIndex];

                ChannelGroupUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
                ChannelGroupLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(currentChannelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
            }

            string jsonUserState = "{}";

            if (jsonChannelUserState == jsonChannelGroupUserState)
            {
                jsonUserState = jsonChannelUserState;
            }
            else if (jsonChannelUserState == "{}" && jsonChannelGroupUserState != "{}")
            {
                jsonUserState = jsonChannelGroupUserState;
            }
            else if (jsonChannelUserState != "{}" && jsonChannelGroupUserState == "{}")
            {
                jsonUserState = jsonChannelUserState;
            }
            else if (jsonChannelUserState != "{}" && jsonChannelGroupUserState != "{}")
            {
                jsonUserState = "";
                for (int channelIndex = 0; channelIndex < channelList.Count; channelIndex++)
                {
                    string currentChannel = channelList[channelIndex];

                    if (jsonUserState == "")
                    {
                        jsonUserState = string.Format("\"{0}\":{{{1}}}", currentChannel, jsonChannelUserState);
                    }
                    else
                    {
                        jsonUserState = string.Format("{0},\"{1}\":{{{2}}}", jsonUserState, currentChannel, jsonChannelUserState);
                    }
                }
                for (int channelGroupIndex = 0; channelGroupIndex < channelGroupList.Count; channelGroupIndex++)
                {
                    string currentChannelGroup = channelGroupList[channelGroupIndex];

                    if (jsonUserState == "")
                    {
                        jsonUserState = string.Format("\"{0}\":{{{1}}}", currentChannelGroup, jsonChannelGroupUserState);
                    }
                    else
                    {
                        jsonUserState = string.Format("{0},\"{1}\":{{{2}}}", jsonUserState, currentChannelGroup, jsonChannelGroupUserState);
                    }
                }
                jsonUserState = string.Format("{{{0}}}", jsonUserState);
                //jsonUserState = string.Format("{{\"{0}\":{{{1}}},\"{2}\":{{{3}}}}}", channel, jsonChannelUserState, channelGroup, jsonChannelGroupUserState);
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
            Uri request = urlBuilder.BuildSetUserStateRequest(commaDelimitedChannels, commaDelimitedChannelGroups, uuid, jsonUserState);

            RequestState<PNSetStateResult> requestState = new RequestState<PNSetStateResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = PNOperationType.PNSetStateOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            //Set TerminateSubRequest to true to bounce the long-polling subscribe requests to update user state
            string json = UrlProcessRequest<PNSetStateResult>(request, requestState, true);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNSetStateResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

        private string AddOrUpdateOrDeleteLocalUserState(string channel, string channelGroup, string userStateKey, object userStateValue)
        {
            string retJsonUserState = "";

            Dictionary<string, object> channelUserStateDictionary = null;
            Dictionary<string, object> channelGroupUserStateDictionary = null;

            if (!string.IsNullOrEmpty(channel) && channel.Trim().Length > 0)
            {
                if (ChannelLocalUserState[PubnubInstance.InstanceId].ContainsKey(channel))
                {
                    channelUserStateDictionary = ChannelLocalUserState[PubnubInstance.InstanceId][channel];
                    if (channelUserStateDictionary != null)
                    {
                        if (channelUserStateDictionary.ContainsKey(userStateKey))
                        {
                            if (userStateValue != null)
                            {
                                channelUserStateDictionary[userStateKey] = userStateValue;
                            }
                            else
                            {
                                channelUserStateDictionary.Remove(userStateKey);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                            {
                                channelUserStateDictionary.Add(userStateKey, userStateValue);
                            }
                        }
                    }
                    else
                    {
                        channelUserStateDictionary = new Dictionary<string, object>();
                        channelUserStateDictionary.Add(userStateKey, userStateValue);
                    }

                    ChannelLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(channel, channelUserStateDictionary, (oldData, newData) => channelUserStateDictionary);
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                    {
                        channelUserStateDictionary = new Dictionary<string, object>();
                        channelUserStateDictionary.Add(userStateKey, userStateValue);

                        ChannelLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(channel, channelUserStateDictionary, (oldData, newData) => channelUserStateDictionary);
                    }
                }
            }
            //
            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                if (ChannelGroupLocalUserState[PubnubInstance.InstanceId].ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = ChannelGroupLocalUserState[PubnubInstance.InstanceId][channelGroup];
                    if (channelGroupUserStateDictionary != null)
                    {
                        if (channelGroupUserStateDictionary.ContainsKey(userStateKey))
                        {
                            if (userStateValue != null)
                            {
                                channelGroupUserStateDictionary[userStateKey] = userStateValue;
                            }
                            else
                            {
                                channelGroupUserStateDictionary.Remove(userStateKey);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                            {
                                channelGroupUserStateDictionary.Add(userStateKey, userStateValue);
                            }
                        }
                    }
                    else
                    {
                        channelGroupUserStateDictionary = new Dictionary<string, object>();
                        channelGroupUserStateDictionary.Add(userStateKey, userStateValue);
                    }

                    ChannelGroupLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(channelGroup, channelGroupUserStateDictionary, (oldData, newData) => channelGroupUserStateDictionary);
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                    {
                        channelGroupUserStateDictionary = new Dictionary<string, object>();
                        channelGroupUserStateDictionary.Add(userStateKey, userStateValue);

                        ChannelGroupLocalUserState[PubnubInstance.InstanceId].AddOrUpdate(channelGroup, channelGroupUserStateDictionary, (oldData, newData) => channelGroupUserStateDictionary);
                    }
                }
            }

            string jsonChannelUserState = BuildJsonUserState(channel, "", true);
            string jsonChannelGroupUserState = BuildJsonUserState("", channelGroup, true);
            if (jsonChannelUserState != "" && jsonChannelGroupUserState != "")
            {
                retJsonUserState = string.Format("{{\"{0}\":{{{1}}},\"{2}\":{{{3}}}}}", channel, jsonChannelUserState, channelGroup, jsonChannelGroupUserState);
            }
            else if (jsonChannelUserState != "")
            {
                retJsonUserState = string.Format("{{{0}}}", jsonChannelUserState);
            }
            else if (jsonChannelGroupUserState != "")
            {
                retJsonUserState = string.Format("{{{0}}}", jsonChannelGroupUserState);
            }
            return retJsonUserState;
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
                jsonStateBuilder.AppendFormat("{0}:{1},{2}:{3}", channel, channelJsonUserState, channelGroup, channelGroupJsonUserState);
            }

            if (jsonStateBuilder.Length > 0)
            {
                retJsonUserState = string.Format("{{{0}}}", jsonStateBuilder.ToString());
            }

            return retJsonUserState;
        }

        private string SetLocalUserState(string channel, string channelGroup, string userStateKey, int userStateValue)
        {
            return AddOrUpdateOrDeleteLocalUserState(channel, channelGroup, userStateKey, userStateValue);
        }

        private string SetLocalUserState(string channel, string channelGroup, string userStateKey, double userStateValue)
        {
            return AddOrUpdateOrDeleteLocalUserState(channel, channelGroup, userStateKey, userStateValue);
        }

        private string SetLocalUserState(string channel, string channelGroup, string userStateKey, string userStateValue)
        {
            return AddOrUpdateOrDeleteLocalUserState(channel, channelGroup, userStateKey, userStateValue);
        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;

            if (!ChannelRequest.ContainsKey(instance.InstanceId))
            {
                ChannelRequest.Add(instance.InstanceId, new ConcurrentDictionary<string, HttpWebRequest>());
            }
            if (!ChannelInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelGroupInternetStatus.ContainsKey(instance.InstanceId))
            {
                ChannelGroupInternetStatus.Add(instance.InstanceId, new ConcurrentDictionary<string, bool>());
            }
            if (!ChannelUserState.ContainsKey(instance.InstanceId))
            {
                ChannelUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelLocalUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
            if (!ChannelGroupLocalUserState.ContainsKey(instance.InstanceId))
            {
                ChannelGroupLocalUserState.Add(instance.InstanceId, new ConcurrentDictionary<string, Dictionary<string, object>>());
            }
        }
    }
}
