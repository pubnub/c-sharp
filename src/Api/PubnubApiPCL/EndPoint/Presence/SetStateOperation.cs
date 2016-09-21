using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;

namespace PubnubApi.EndPoint
{
    public class SetStateOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private string[] channelNames = null;
        private string[] channelGroupNames = null;
        private Dictionary<string, object> userState = null;
        private string channelUUID = "";

        public SetStateOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public SetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
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

        public SetStateOperation uuid(string uuid)
        {
            this.channelUUID = uuid;
            return this;
        }

        public void Async(PNCallback<PNSetStateResult> callback)
        {
            string serializedState = jsonLibrary.SerializeToJsonString(this.userState);
            SetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, serializedState, callback);
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

            string commaDelimitedChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";
            string commaDelimitedChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

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
                        PNStatus status = statusBuilder.CreateStatusResponse< PNSetStateResult>(PNOperationType.PNSetStateOperation, PNStatusCategory.PNUnknownCategory, null, System.Net.HttpStatusCode.NotModified, null);

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

            string commaDelimitedChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";
            string commaDelimitedChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

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
                currentChannelUserState = "";

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
                PNStatus status = statusBuilder.CreateStatusResponse<PNSetStateResult>(PNOperationType.PNSetStateOperation, PNStatusCategory.PNUnknownCategory, null, System.Net.HttpStatusCode.NotModified, null);

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

            string commaDelimitedChannels = (channels != null && channels.Length > 0) ? string.Join(",", channels) : "";
            string commaDelimitedChannelGroups = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";

            if (string.IsNullOrEmpty(uuid))
            {
                uuid = config.Uuid;
            }

            Dictionary<string, object> deserializeChannelUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonChannelUserState);
            Dictionary<string, object> deserializeChannelGroupUserState = jsonLibrary.DeserializeToDictionaryOfObject(jsonChannelGroupUserState);

            for (int channelIndex=0; channelIndex < channelList.Count; channelIndex++)
            {
                string currentChannel = channelList[channelIndex];

                ChannelUserState.AddOrUpdate(currentChannel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
                ChannelLocalUserState.AddOrUpdate(currentChannel.Trim(), deserializeChannelUserState, (oldState, newState) => deserializeChannelUserState);
            }

            for (int channelGroupIndex=0; channelGroupIndex < channelGroupList.Count; channelGroupIndex++)
            {
                string currentChannelGroup = channelGroupList[channelGroupIndex];

                ChannelGroupUserState.AddOrUpdate(currentChannelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
                ChannelGroupLocalUserState.AddOrUpdate(currentChannelGroup.Trim(), deserializeChannelGroupUserState, (oldState, newState) => deserializeChannelGroupUserState);
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

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildSetUserStateRequest(commaDelimitedChannels, commaDelimitedChannelGroups, uuid, jsonUserState);

            RequestState<PNSetStateResult> requestState = new RequestState<PNSetStateResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = PNOperationType.PNSetStateOperation;
            requestState.Callback = callback;
            requestState.Reconnect = false;

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
                if (ChannelLocalUserState.ContainsKey(channel))
                {
                    channelUserStateDictionary = ChannelLocalUserState[channel];
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

                    ChannelLocalUserState.AddOrUpdate(channel, channelUserStateDictionary, (oldData, newData) => channelUserStateDictionary);
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                    {
                        channelUserStateDictionary = new Dictionary<string, object>();
                        channelUserStateDictionary.Add(userStateKey, userStateValue);

                        ChannelLocalUserState.AddOrUpdate(channel, channelUserStateDictionary, (oldData, newData) => channelUserStateDictionary);
                    }
                }
            }
            //
            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.Trim().Length > 0)
            {
                if (ChannelGroupLocalUserState.ContainsKey(channelGroup))
                {
                    channelGroupUserStateDictionary = ChannelGroupLocalUserState[channelGroup];
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

                    ChannelGroupLocalUserState.AddOrUpdate(channelGroup, channelGroupUserStateDictionary, (oldData, newData) => channelGroupUserStateDictionary);
                }
                else
                {
                    if (!string.IsNullOrEmpty(userStateKey) && userStateKey.Trim().Length > 0 && userStateValue != null)
                    {
                        channelGroupUserStateDictionary = new Dictionary<string, object>();
                        channelGroupUserStateDictionary.Add(userStateKey, userStateValue);

                        ChannelGroupLocalUserState.AddOrUpdate(channelGroup, channelGroupUserStateDictionary, (oldData, newData) => channelGroupUserStateDictionary);
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

        //private bool DeleteLocalChannelUserState(string channel)
        //{
        //    bool userStateDeleted = false;

        //    if (ChannelLocalUserState.ContainsKey(channel))
        //    {
        //        Dictionary<string, object> returnedUserState = null;
        //        userStateDeleted = ChannelLocalUserState.TryRemove(channel, out returnedUserState);
        //    }

        //    return userStateDeleted;
        //}

        //private bool DeleteLocalChannelGroupUserState(string channelGroup)
        //{
        //    bool userStateDeleted = false;

        //    if (base.ChannelGroupLocalUserState.ContainsKey(channelGroup))
        //    {
        //        Dictionary<string, object> returnedUserState = null;
        //        userStateDeleted = base.ChannelGroupLocalUserState.TryRemove(channelGroup, out returnedUserState);
        //    }

        //    return userStateDeleted;
        //}

        //private string BuildJsonUserState(string channel, string channelGroup, bool local)
        //{
        //    Dictionary<string, object> channelUserStateDictionary = null;
        //    Dictionary<string, object> channelGroupUserStateDictionary = null;

        //    if (!string.IsNullOrEmpty(channel) && !string.IsNullOrEmpty(channelGroup))
        //    {
        //        throw new ArgumentException("BuildJsonUserState takes either channel or channelGroup at one time. Send one at a time by passing empty value for other.");
        //    }

        //    if (local)
        //    {
        //        if (!string.IsNullOrEmpty(channel) && base.channelLocalUserState.ContainsKey(channel))
        //        {
        //            channelUserStateDictionary = base.channelLocalUserState[channel];
        //        }
        //        if (!string.IsNullOrEmpty(channelGroup) && base.channelGroupLocalUserState.ContainsKey(channelGroup))
        //        {
        //            channelGroupUserStateDictionary = base.channelGroupLocalUserState[channelGroup];
        //        }
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(channel) && base.channelUserState.ContainsKey(channel))
        //        {
        //            channelUserStateDictionary = base.channelUserState[channel];
        //        }
        //        if (!string.IsNullOrEmpty(channelGroup) && base.channelGroupUserState.ContainsKey(channelGroup))
        //        {
        //            channelGroupUserStateDictionary = base.channelGroupUserState[channelGroup];
        //        }
        //    }

        //    StringBuilder jsonStateBuilder = new StringBuilder();

        //    if (channelUserStateDictionary != null)
        //    {
        //        string[] channelUserStateKeys = channelUserStateDictionary.Keys.ToArray<string>();

        //        for (int keyIndex = 0; keyIndex < channelUserStateKeys.Length; keyIndex++)
        //        {
        //            string channelUserStateKey = channelUserStateKeys[keyIndex];
        //            object channelUserStateValue = channelUserStateDictionary[channelUserStateKey];
        //            if (channelUserStateValue == null)
        //            {
        //                jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelUserStateKey, string.Format("\"{0}\"", "null"));
        //            }
        //            else
        //            {
        //                jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelUserStateKey, (channelUserStateValue.GetType().ToString() == "System.String") ? string.Format("\"{0}\"", channelUserStateValue) : channelUserStateValue);
        //            }
        //            if (keyIndex < channelUserStateKeys.Length - 1)
        //            {
        //                jsonStateBuilder.Append(",");
        //            }
        //        }
        //    }
        //    if (channelGroupUserStateDictionary != null)
        //    {
        //        string[] channelGroupUserStateKeys = channelGroupUserStateDictionary.Keys.ToArray<string>();

        //        for (int keyIndex = 0; keyIndex < channelGroupUserStateKeys.Length; keyIndex++)
        //        {
        //            string channelGroupUserStateKey = channelGroupUserStateKeys[keyIndex];
        //            object channelGroupUserStateValue = channelGroupUserStateDictionary[channelGroupUserStateKey];
        //            if (channelGroupUserStateValue == null)
        //            {
        //                jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelGroupUserStateKey, string.Format("\"{0}\"", "null"));
        //            }
        //            else
        //            {
        //                jsonStateBuilder.AppendFormat("\"{0}\":{1}", channelGroupUserStateKey, (channelGroupUserStateValue.GetType().ToString() == "System.String") ? string.Format("\"{0}\"", channelGroupUserStateValue) : channelGroupUserStateValue);
        //            }
        //            if (keyIndex < channelGroupUserStateKeys.Length - 1)
        //            {
        //                jsonStateBuilder.Append(",");
        //            }
        //        }
        //    }

        //    return jsonStateBuilder.ToString();
        //}

        //private string BuildJsonUserState(string[] channels, string[] channelGroups, bool local)
        //{
        //    string retJsonUserState = "";

        //    StringBuilder jsonStateBuilder = new StringBuilder();

        //    if (channels != null && channels.Length > 0)
        //    {
        //        for (int index = 0; index < channels.Length; index++)
        //        {
        //            string currentJsonState = BuildJsonUserState(channels[index].ToString(), "", local);
        //            if (!string.IsNullOrEmpty(currentJsonState))
        //            {
        //                currentJsonState = string.Format("\"{0}\":{{{1}}}", channels[index].ToString(), currentJsonState);
        //                if (jsonStateBuilder.Length > 0)
        //                {
        //                    jsonStateBuilder.Append(",");
        //                }
        //                jsonStateBuilder.Append(currentJsonState);
        //            }
        //        }
        //    }

        //    if (channelGroups != null && channelGroups.Length > 0)
        //    {
        //        for (int index = 0; index < channelGroups.Length; index++)
        //        {
        //            string currentJsonState = BuildJsonUserState("", channelGroups[index].ToString(), local);
        //            if (!string.IsNullOrEmpty(currentJsonState))
        //            {
        //                currentJsonState = string.Format("\"{0}\":{{{1}}}", channelGroups[index].ToString(), currentJsonState);
        //                if (jsonStateBuilder.Length > 0)
        //                {
        //                    jsonStateBuilder.Append(",");
        //                }
        //                jsonStateBuilder.Append(currentJsonState);
        //            }
        //        }
        //    }

        //    if (jsonStateBuilder.Length > 0)
        //    {
        //        retJsonUserState = string.Format("{{{0}}}", jsonStateBuilder.ToString());
        //    }

        //    return retJsonUserState;
        //}

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


    }
}
