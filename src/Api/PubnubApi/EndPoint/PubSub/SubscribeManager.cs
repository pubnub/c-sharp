using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PubnubApi.Interface;
using System.Net;

namespace PubnubApi.EndPoint
{
    internal class SubscribeManager : PubnubCoreBase
    {
        private static PNConfiguration config = null;
        private static IJsonPluggableLibrary jsonLibrary = null;
        private static IPubnubUnitTest unit = null;
        private const int MINEXPONENTIALBACKOFF = 1;
        private const int MAXEXPONENTIALBACKOFF = 32;
        private const int INTERVAL = 3;
        private const int MILLISECONDS = 1000;

        public SubscribeManager(PNConfiguration pubnubConfig) :base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public SubscribeManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public SubscribeManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit) : base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
        }


        internal void MultiChannelUnSubscribeAll<T>(PNOperationType type)
        {
            //Retrieve the current channels already subscribed previously and terminate them
            string[] currentChannels = MultiChannelSubscribe.Keys.ToArray<string>();
            string[] currentChannelGroups = MultiChannelGroupSubscribe.Keys.ToArray<string>();

            if (currentChannels != null && currentChannels.Length >= 0)
            {
                string multiChannelName = (currentChannels.Length > 0) ? string.Join(",", currentChannels) : ",";
                string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups) : "";

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    if (ChannelRequest.ContainsKey(multiChannelName))
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);

                        HttpWebRequest webRequest = ChannelRequest[multiChannelName];
                        ChannelRequest[multiChannelName] = null;

                        if (webRequest != null)
                        {
                            TerminateLocalClientHeartbeatTimer(webRequest.RequestUri);
                        }

                        HttpWebRequest removedRequest;
                        bool removedChannel = ChannelRequest.TryRemove(multiChannelName, out removedRequest);
                        if (removedChannel)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        }
                        if (webRequest != null)
                            TerminatePendingWebRequest(webRequest);
                    }
                    else
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                    }
                });

                if (type == PNOperationType.PNUnsubscribeOperation)
                {
                    //just fire leave() event to REST API for safeguard
                    string channelsJsonState = BuildJsonUserState(currentChannels, currentChannelGroups, false);
                    IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
                    Uri request = urlBuilder.BuildMultiChannelLeaveRequest(currentChannels, currentChannelGroups, config.Uuid, channelsJsonState);

                    RequestState<T> requestState = new RequestState<T>();
                    requestState.Channels = currentChannels;
                    requestState.ChannelGroups = currentChannelGroups;
                    requestState.ResponseType = PNOperationType.Leave;
                    requestState.Reconnect = false;

                    string json = UrlProcessRequest<T>(request, requestState, false);
                }
            }

        }

        internal void MultiChannelUnSubscribeInit<T>(PNOperationType type, string channel, string channelGroup)
        {
            string[] rawChannels = (channel != null && channel.Trim().Length > 0) ? channel.Split(',') : new string[] { };
            string[] rawChannelGroups = (channelGroup != null && channelGroup.Trim().Length > 0) ? channelGroup.Split(',') : new string[] { };

            List<string> validChannels = new List<string>();
            List<string> validChannelGroups = new List<string>();

            if (rawChannels.Length > 0)
            {
                for (int index = 0; index < rawChannels.Length; index++)
                {
                    if (rawChannels[index].Trim().Length > 0)
                    {
                        string channelName = rawChannels[index].Trim();
                        if (string.IsNullOrEmpty(channelName)) continue;

                        if (!MultiChannelSubscribe.ContainsKey(channelName))
                        {
                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, null, (int)HttpStatusCode.NotFound, null);
                            Announce(status);
                        }
                        else
                        {
                            validChannels.Add(channelName);
                        }
                    }
                }
            }

            if (rawChannelGroups.Length > 0)
            {
                for (int index = 0; index < rawChannelGroups.Length; index++)
                {
                    if (rawChannelGroups[index].Trim().Length > 0)
                    {
                        string channelGroupName = rawChannelGroups[index].Trim();
                        if (string.IsNullOrEmpty(channelGroupName)) continue;

                        if (!MultiChannelGroupSubscribe.ContainsKey(channelGroupName))
                        {
                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, null, (int)HttpStatusCode.NotFound, null);
                            Announce(status);
                        }
                        else
                        {
                            validChannelGroups.Add(channelGroupName);
                        }
                    }
                }
            }

            if (validChannels.Count > 0 || validChannelGroups.Count > 0)
            {
                //Retrieve the current channels already subscribed previously and terminate them
                string[] currentChannels = MultiChannelSubscribe.Keys.ToArray<string>();
                string[] currentChannelGroups = MultiChannelGroupSubscribe.Keys.ToArray<string>();

                if (currentChannels != null && currentChannels.Length >= 0)
                {
                    string multiChannelName = (currentChannels.Length > 0) ? string.Join(",", currentChannels) : ",";
                    string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups) : "";

                    System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        if (ChannelRequest.ContainsKey(multiChannelName))
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);

                            HttpWebRequest webRequest = ChannelRequest[multiChannelName];
                            ChannelRequest[multiChannelName] = null;

                            if (webRequest != null)
                            {
                                TerminateLocalClientHeartbeatTimer(webRequest.RequestUri);
                            }

                            HttpWebRequest removedRequest;
                            bool removedChannel = ChannelRequest.TryRemove(multiChannelName, out removedRequest);
                            if (removedChannel)
                            {
                                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                            }
                            if (webRequest != null)
                                TerminatePendingWebRequest(webRequest);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        }
                    });

                    if (type == PNOperationType.PNUnsubscribeOperation)
                    {
                        //just fire leave() event to REST API for safeguard
                        string channelsJsonState = BuildJsonUserState(validChannels.ToArray(), validChannelGroups.ToArray(), false);
                        IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
                        Uri request = urlBuilder.BuildMultiChannelLeaveRequest(validChannels.ToArray(), validChannelGroups.ToArray(), config.Uuid, channelsJsonState);

                        RequestState<T> requestState = new RequestState<T>();
                        requestState.Channels = new string[] { channel };
                        requestState.ChannelGroups = new string[] { channelGroup };
                        requestState.ResponseType = PNOperationType.Leave;
                        requestState.Reconnect = false;

                        string json = UrlProcessRequest<T>(request, requestState, false);
                    }
                }

                Dictionary<string, long> originalMultiChannelSubscribe = MultiChannelSubscribe.Count > 0 ? MultiChannelSubscribe.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
                Dictionary<string, long> originalMultiChannelGroupSubscribe = MultiChannelGroupSubscribe.Count > 0 ? MultiChannelGroupSubscribe.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;

                //Remove the valid channels from subscribe list for unsubscribe 
                for (int index = 0; index < validChannels.Count; index++)
                {
                    long timetokenValue;
                    string channelToBeRemoved = validChannels[index].ToString();
                    bool unsubscribeStatus = MultiChannelSubscribe.TryRemove(channelToBeRemoved, out timetokenValue);
                    if (unsubscribeStatus)
                    {
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, (int)HttpStatusCode.OK, null);
                        if (!status.AffectedChannels.Contains(channelToBeRemoved))
                        {
                            status.AffectedChannels.Add(channelToBeRemoved);
                        }
                        Announce(status);

                        base.DeleteLocalChannelUserState(channelToBeRemoved);
                    }
                    else
                    {
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, (int)HttpStatusCode.NotFound, new Exception("Unsubscribe Error. Please retry the channel unsubscribe operation"));
                        if (!status.AffectedChannels.Contains(channelToBeRemoved))
                        {
                            status.AffectedChannels.Add(channelToBeRemoved);
                        }
                        Announce(status);
                    }
                }
                for (int index = 0; index < validChannelGroups.Count; index++)
                {
                    long timetokenValue;
                    string channelGroupToBeRemoved = validChannelGroups[index].ToString();
                    bool unsubscribeStatus = MultiChannelGroupSubscribe.TryRemove(channelGroupToBeRemoved, out timetokenValue);
                    if (unsubscribeStatus)
                    {
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, (int)HttpStatusCode.OK, null);
                        if (!status.AffectedChannelGroups.Contains(channelGroupToBeRemoved))
                        {
                            status.AffectedChannelGroups.Add(channelGroupToBeRemoved);
                        }
                        Announce(status);

                        base.DeleteLocalChannelGroupUserState(channelGroupToBeRemoved);
                    }
                    else
                    {
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, (int)HttpStatusCode.NotFound, new Exception("Unsubscribe Error. Please retry the channelgroup unsubscribe operation"));
                        if (!status.AffectedChannelGroups.Contains(channelGroupToBeRemoved))
                        {
                            status.AffectedChannelGroups.Add(channelGroupToBeRemoved);
                        }
                        Announce(status);
                    }
                }

                //Get all the channels
                string[] channels = MultiChannelSubscribe.Keys.ToArray<string>();
                string[] channelGroups = MultiChannelGroupSubscribe.Keys.ToArray<string>();

                //Check any chained subscribes while unsubscribe 
                for(int keyIndex=0; keyIndex < MultiChannelSubscribe.Count; keyIndex++)
                {
                    KeyValuePair<string, long> kvp = MultiChannelSubscribe.ElementAt(keyIndex);
                    if (originalMultiChannelSubscribe != null && !originalMultiChannelSubscribe.ContainsKey(kvp.Key))
                    {
                        return;
                    }
                }

                for (int keyIndex = 0; keyIndex < MultiChannelGroupSubscribe.Count; keyIndex++)
                {
                    KeyValuePair<string, long> kvp = MultiChannelGroupSubscribe.ElementAt(keyIndex);
                    if (originalMultiChannelGroupSubscribe != null && !originalMultiChannelGroupSubscribe.ContainsKey(kvp.Key))
                    {
                        return;
                    }
                }

                channels = (channels != null) ? channels : new string[] { };
                channelGroups = (channelGroups != null) ? channelGroups : new string[] { };

                if (channels.Length > 0 || channelGroups.Length > 0)
                {
                    string multiChannel = (channels.Length > 0) ? string.Join(",", channels) : ",";

                    RequestState<T> state = new RequestState<T>();
                    ChannelRequest.AddOrUpdate(multiChannel, state.Request, (key, oldValue) => state.Request);

                    ResetInternetCheckSettings(channels, channelGroups);


                    //Continue with any remaining channels for subscribe/presence
                    MultiChannelSubscribeRequest<T>(type, channels, channelGroups, 0, false, null);
                }
                else
                {
                    if (PresenceHeartbeatTimer != null)
                    {
                        // Stop the presence heartbeat timer if there are no channels subscribed
                        PresenceHeartbeatTimer.Dispose();
                        PresenceHeartbeatTimer = null;
                    }
                    LoggingMethod.WriteToLog(string.Format("DateTime {0}, All channels are Unsubscribed. Further subscription was stopped", DateTime.Now.ToString()), config.LogVerbosity);
                }
            }

        }

        internal void MultiChannelSubscribeInit<T>(PNOperationType responseType, string[] rawChannels, string[] rawChannelGroups, Dictionary<string, string> initialSubscribeUrlParams)
        {
            bool channelGroupSubscribeOnly = false;

            string channel = (rawChannels != null) ? string.Join(",", rawChannels) : "";
            string channelGroup = (rawChannelGroups != null) ? string.Join(",", rawChannelGroups) : "";

            List<string> validChannels = new List<string>();
            List<string> validChannelGroups = new List<string>();

            bool networkConnection = InternetConnectionStatusInitialCheck<T>(responseType, null, rawChannels, rawChannelGroups);

            if (rawChannels.Length > 0 && networkConnection)
            {
                if (rawChannels.Length != rawChannels.Distinct().Count())
                {
                    rawChannels = rawChannels.Distinct().ToArray();
                }

                for (int index = 0; index < rawChannels.Length; index++)
                {
                    if (rawChannels[index].Trim().Length > 0)
                    {
                        string channelName = rawChannels[index].Trim();
                        if (!string.IsNullOrEmpty(channelName))
                        {
                            validChannels.Add(channelName);
                        }
                    }
                }
            }

            if (rawChannelGroups != null && rawChannelGroups.Length > 0 && networkConnection)
            {
                if (rawChannelGroups.Length != rawChannelGroups.Distinct().Count())
                {
                    rawChannelGroups = rawChannelGroups.Distinct().ToArray();
                }

                for (int index = 0; index < rawChannelGroups.Length; index++)
                {
                    if (rawChannelGroups[index].Trim().Length > 0)
                    {
                        string channelGroupName = rawChannelGroups[index].Trim();
                        validChannelGroups.Add(channelGroupName);
                    }
                }
            }

            if (validChannels.Count > 0 || validChannelGroups.Count > 0)
            {
                //Retrieve the current channels already subscribed previously and terminate them
                string[] currentChannels = MultiChannelSubscribe.Keys.ToArray<string>();
                string[] currentChannelGroups = MultiChannelGroupSubscribe.Keys.ToArray<string>();

                if (currentChannels != null && currentChannels.Length >= 0)
                {
                    string multiChannelName = (currentChannels.Length > 0) ? string.Join(",", currentChannels) : ",";
                    string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups) : "";

                    if (ChannelRequest.ContainsKey(multiChannelName))
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        HttpWebRequest webRequest = ChannelRequest[multiChannelName];
                        ChannelRequest[multiChannelName] = null;

                        if (webRequest != null)
                            TerminateLocalClientHeartbeatTimer(webRequest.RequestUri);

                        HttpWebRequest removedRequest;
                        ChannelRequest.TryRemove(multiChannelName, out removedRequest);
                        bool removedChannel = ChannelRequest.TryRemove(multiChannelName, out removedRequest);
                        if (removedChannel)
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        }
                        if (webRequest != null)
                            TerminatePendingWebRequest(webRequest);
                    }
                    else
                    {
                        LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                    }
                }

                //Add the valid channels to the channels subscribe list for tracking
                for (int index = 0; index < validChannels.Count; index++)
                {
                    string currentLoopChannel = validChannels[index].ToString();
                    MultiChannelSubscribe.GetOrAdd(currentLoopChannel, 0);
                }


                for (int index = 0; index < validChannelGroups.Count; index++)
                {
                    string currentLoopChannelGroup = validChannelGroups[index].ToString();
                    MultiChannelGroupSubscribe.GetOrAdd(currentLoopChannelGroup, 0);
                }

                //Get all the channels
                string[] channels = MultiChannelSubscribe.Keys.ToArray<string>();
                string[] channelGroups = MultiChannelGroupSubscribe.Keys.ToArray<string>();

                if (channelGroups != null && channelGroups.Length > 0 && (channels == null || channels.Length == 0))
                {
                    channelGroupSubscribeOnly = true;
                }

                RequestState<T> state = new RequestState<T>();
                if (channelGroupSubscribeOnly)
                {
                    ChannelRequest.AddOrUpdate(",", state.Request, (key, oldValue) => state.Request);
                }
                else
                {
                    ChannelRequest.AddOrUpdate(string.Join(",", channels), state.Request, (key, oldValue) => state.Request);
                }

                ResetInternetCheckSettings(channels, channelGroups);
                MultiChannelSubscribeRequest<T>(responseType, channels, channelGroups, 0, false, initialSubscribeUrlParams);
            }
        }

        private void MultiChannelSubscribeRequest<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken, bool reconnect, Dictionary<string, string> initialSubscribeUrlParams)
        {
            //Exit if the channel is unsubscribed
            if (MultiChannelSubscribe != null && MultiChannelSubscribe.Count <= 0 && MultiChannelGroupSubscribe != null && MultiChannelGroupSubscribe.Count <= 0)
            {
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Zero channels/channelGroups. Further subscription was stopped", DateTime.Now.ToString()), config.LogVerbosity);
                return;
            }

            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels) : ",";
            string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups) : "";
            if (!ChannelRequest.ContainsKey(multiChannel))
            {
                return;
            }

            bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, type, null, channels, channelGroups);

            if (!networkConnection)
            {
                ConnectionErrors++;
                UpdatePubnubNetworkTcpCheckIntervalInSeconds();
                ChannelInternetStatus.AddOrUpdate(multiChannel, networkConnection, (key, oldValue) => networkConnection);
                ChannelGroupInternetStatus.AddOrUpdate(multiChannelGroup, networkConnection, (key, oldValue) => networkConnection);
            }

            if (((ChannelInternetStatus.ContainsKey(multiChannel) && !ChannelInternetStatus[multiChannel])
                || (multiChannelGroup != "" && ChannelGroupInternetStatus.ContainsKey(multiChannelGroup) && !ChannelGroupInternetStatus[multiChannelGroup]))
                && PubnetSystemActive)
            {
                if (ReconnectNetworkIfOverrideTcpKeepAlive<T>(type, channels, channelGroups, timetoken))
                {
                    return;
                }

            }

            // Begin recursive subscribe
            RequestState<T> pubnubRequestState = null;
            try
            {
                RegisterPresenceHeartbeatTimer<T>(channels, channelGroups);

                long lastTimetoken = 0;
                long minimumTimetoken1 = (MultiChannelSubscribe.Count > 0) ? MultiChannelSubscribe.Min(token => token.Value) : 0;
                long minimumTimetoken2 = (MultiChannelGroupSubscribe.Count > 0) ? MultiChannelGroupSubscribe.Min(token => token.Value) : 0;
                long minimumTimetoken = Math.Max(minimumTimetoken1, minimumTimetoken2);

                long maximumTimetoken1 = (MultiChannelSubscribe.Count > 0) ? MultiChannelSubscribe.Max(token => token.Value) : 0;
                long maximumTimetoken2 = (MultiChannelGroupSubscribe.Count > 0) ? MultiChannelGroupSubscribe.Max(token => token.Value) : 0;
                long maximumTimetoken = Math.Max(maximumTimetoken1, maximumTimetoken2);


                if (minimumTimetoken == 0 || reconnect || UuidChanged)
                {
                    lastTimetoken = 0;
                    UuidChanged = false;
                }
                else
                {
                    if (LastSubscribeTimetoken == maximumTimetoken)
                    {
                        lastTimetoken = maximumTimetoken;
                    }
                    else
                    {
                        lastTimetoken = LastSubscribeTimetoken;
                    }
                }
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Building request for channel(s)={1}, channelgroup(s)={2} with timetoken={3}", DateTime.Now.ToString(), multiChannel, multiChannelGroup, lastTimetoken), config.LogVerbosity);
                // Build URL
                string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
                config.Uuid = CurrentUuid; // to make sure we capture if UUID is changed
                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
                Uri request = urlBuilder.BuildMultiChannelSubscribeRequest(channels, channelGroups, (Convert.ToInt64(timetoken.ToString()) == 0) ? Convert.ToInt64(timetoken.ToString()) : lastTimetoken, channelsJsonState, initialSubscribeUrlParams);

                pubnubRequestState = new RequestState<T>();
                pubnubRequestState.Channels = channels;
                pubnubRequestState.ChannelGroups = channelGroups;
                pubnubRequestState.ResponseType = type;
                pubnubRequestState.Reconnect = reconnect;
                pubnubRequestState.Timetoken = Convert.ToInt64(timetoken.ToString());

                // Wait for message
                string json = UrlProcessRequest<T>(request, pubnubRequestState, false);
                if (!string.IsNullOrEmpty(json))
                {
                    List<object> result = ProcessJsonResponse<T>(pubnubRequestState, json);
                    ProcessResponseCallbacks<T>(result, pubnubRequestState);

                    if ((pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation || pubnubRequestState.ResponseType == PNOperationType.Presence) && (result != null) && (result.Count > 0))
                    {
                        long jsonTimetoken = GetTimetokenFromMultiplexResult(result);

                        if (jsonTimetoken > 0)
                        {
                            if (pubnubRequestState.Channels != null)
                            {
                                foreach (string currentChannel in pubnubRequestState.Channels)
                                {
                                    MultiChannelSubscribe.AddOrUpdate(currentChannel, jsonTimetoken, (key, oldValue) => jsonTimetoken);
                                }
                            }
                            if (pubnubRequestState.ChannelGroups != null && pubnubRequestState.ChannelGroups.Length > 0)
                            {
                                foreach (string currentChannelGroup in pubnubRequestState.ChannelGroups)
                                {
                                    MultiChannelGroupSubscribe.AddOrUpdate(currentChannelGroup, jsonTimetoken, (key, oldValue) => jsonTimetoken);
                                }
                            }
                        }
                    }

                    switch (pubnubRequestState.ResponseType)
                    {
                        case PNOperationType.PNSubscribeOperation:
                        case PNOperationType.Presence:
                            MultiplexInternalCallback<T>(pubnubRequestState.ResponseType, result);
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    MultiplexExceptionHandler<T>(type, channels, channelGroups, false, false);
                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(string.Format("DateTime {0} method:_subscribe \n channel={1} \n timetoken={2} \n Exception Details={3}", DateTime.Now.ToString(), string.Join(",", channels), timetoken.ToString(), ex.ToString()), config.LogVerbosity);

                PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, errorCategory, pubnubRequestState, (int)HttpStatusCode.NotFound, ex);
                status.AffectedChannels.AddRange(channels);
                status.AffectedChannels.AddRange(channelGroups);
                Announce(status);

                MultiChannelSubscribeRequest<T>(type, channels, channelGroups, LastSubscribeTimetoken, false, null);
            }
        }

        private bool InternetConnectionStatusInitialCheck<T>(PNOperationType responseType, PNCallback<T> callback, string[] channels, string[] channelGroups)
        {
            bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, responseType, callback, channels, channelGroups);
            if (!networkConnection)
            {
                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(responseType, PNStatusCategory.PNNetworkIssuesCategory, null, (int)HttpStatusCode.NotFound, new Exception("Internet connection is not available"));
                status.AffectedChannels.AddRange(channels);
                status.AffectedChannelGroups.AddRange(channelGroups);
                Announce(status);
            }

            return networkConnection;
        }

        private void MultiplexExceptionHandler<T>(PNOperationType type, string[] channels, string[] channelGroups, bool reconnectMaxTried, bool resumeOnReconnect)
        {
            string channel = "";
            string channelGroup = "";
            if (channels != null)
            {
                channel = string.Join(",", channels);
            }
            if (channelGroups != null)
            {
                channelGroup = string.Join(",", channelGroups);
            }

            List<object> result = new List<object>();
            result.Add("0");
            if (resumeOnReconnect)
            {
                result.Add(0); //send 0 time token to enable presence event
            }
            else
            {
                result.Add(LastSubscribeTimetoken); //get last timetoken
            }
            if (channelGroups != null && channelGroups.Length > 0)
            {
                result.Add(channelGroups);
            }
            result.Add(channels); //send channel name

            MultiplexInternalCallback<T>(type, result);
        }

        private void MultiplexInternalCallback<T>(PNOperationType type, object multiplexResult)
        {
            List<object> message = multiplexResult as List<object>;
            string[] channels = null;
            string[] channelGroups = null;
            if (message != null && message.Count >= 3)
            {
                if (message[message.Count - 1] is string[])
                {
                    channels = message[message.Count - 1] as string[];
                }
                else
                {
                    channels = message[message.Count - 1].ToString().Split(',') as string[];
                }

                if (channels.Length == 1 && channels[0] == "")
                {
                    channels = new string[] { };
                }
                if (message.Count >= 4)
                {
                    if (message[message.Count - 2] is string[])
                    {
                        channelGroups = message[message.Count - 2] as string[];
                    }
                    else if (message[message.Count - 2].ToString() != "")
                    {
                        channelGroups = message[message.Count - 2].ToString().Split(',') as string[];
                    }
                }
            }
            else
            {
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, Lost Channel Name for resubscribe", DateTime.Now.ToString()), config.LogVerbosity);
                return;
            }

            if (message != null && message.Count >= 3)
            {
                long timetoken = GetTimetokenFromMultiplexResult(message);
                MultiChannelSubscribeRequest<T>(type, channels, channelGroups, timetoken, false, null);
            }
        }

        private bool ReconnectNetworkIfOverrideTcpKeepAlive<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken)
        {
            if (OverrideTcpKeepAlive)
            {
                if (config.ReconnectionPolicy != PNReconnectionPolicy.NONE)
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0}, Subscribe - No internet connection for channel={1} and channelgroup={2}", DateTime.Now.ToString(), string.Join(",", channels), channelGroups != null ? string.Join(",", channelGroups) : ""), config.LogVerbosity);
                    ReconnectState<T> netState = new ReconnectState<T>();
                    netState.Channels = channels;
                    netState.ChannelGroups = channelGroups;
                    netState.ResponseType = type;
                    netState.Timetoken = timetoken;
                    ReconnectNetwork<T>(netState);
                }
                else
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0}, reconnection policy is DISABLED, please handle reconnection manually.", DateTime.Now.ToString()), config.LogVerbosity);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ReconnectNetwork<T>(ReconnectState<T> netState)
        {
            if (netState != null && ((netState.Channels != null && netState.Channels.Length > 0) || (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)))
            {
                LoggingMethod.WriteToLog(string.Format("DateTime {0}, SubscribeManager ReconnectNetwork interval = {1} sec", DateTime.Now.ToString(), PubnubNetworkTcpCheckIntervalInSeconds), config.LogVerbosity);

                System.Threading.Timer timer = new Timer(new TimerCallback(ReconnectNetworkCallback<T>), netState, 0,
                                                      (-1 == PubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : PubnubNetworkTcpCheckIntervalInSeconds * 1000);

                if (netState.Channels != null && netState.Channels.Length > 0)
                {
                    ChannelReconnectTimer.AddOrUpdate(string.Join(",", netState.Channels), timer, (key, oldState) => timer);
                }
                if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
                {
                    ChannelGroupReconnectTimer.AddOrUpdate(string.Join(",", netState.ChannelGroups), timer, (key, oldState) => timer);
                }
            }
        }

        protected void ReconnectNetworkCallback<T>(System.Object reconnectState)
        {
            string channel = "";
            string channelGroup = "";

            ReconnectState<T> netState = reconnectState as ReconnectState<T>;
            try
            {
                if (netState != null && ((netState.Channels != null && netState.Channels.Length > 0) || (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)))
                {
                    if (netState.Channels != null && netState.Channels.Length > 0)
                    {
                        channel = (netState.Channels.Length > 0) ? string.Join(",", netState.Channels) : ",";

                        if (ChannelInternetStatus.ContainsKey(channel)
                            && (netState.ResponseType == PNOperationType.PNSubscribeOperation || netState.ResponseType == PNOperationType.Presence))
                        {
                            bool networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
                            if (ChannelInternetStatus[channel])
                            {
                                //Reset Retry if previous state is true
                                //ChannelInternetRetry.AddOrUpdate(channel, 0, (key, oldValue) => 0);
                            }
                            else
                            {
                                ChannelInternetStatus.AddOrUpdate(channel, networkConnection, (key, oldValue) => networkConnection);
                                ConnectionErrors++;
                                UpdatePubnubNetworkTcpCheckIntervalInSeconds();

                                LoggingMethod.WriteToLog(string.Format("DateTime {0}, channel={1} {2} reconnectNetworkCallback. Retry", DateTime.Now.ToString(), channel, netState.ResponseType), config.LogVerbosity);

                                if (netState.Channels != null && netState.Channels.Length > 0)
                                {
                                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.NotFound, new Exception("Internet connection problem. Retrying connection"));
                                    status.AffectedChannels.AddRange(netState.Channels.ToList());
                                    status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                                    Announce(status);
                                }

                            }
                        }

                        if (ChannelInternetStatus.ContainsKey(channel) && ChannelInternetStatus[channel])
                        {
                            if (ChannelReconnectTimer.ContainsKey(channel))
                            {
                                try
                                {
                                    ChannelReconnectTimer[channel].Change(Timeout.Infinite, Timeout.Infinite);
                                    ChannelReconnectTimer[channel].Dispose();
                                }
                                catch { }
                            }

                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.OK, null);
                            status.AffectedChannels.AddRange(netState.Channels.ToList());
                            status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                            Announce(status);

                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, {1} {2} reconnectNetworkCallback. Internet Available : {3}", DateTime.Now.ToString(), channel, netState.ResponseType, ChannelInternetStatus[channel]), config.LogVerbosity);
                            switch (netState.ResponseType)
                            {
                                case PNOperationType.PNSubscribeOperation:
                                case PNOperationType.Presence:
                                    MultiChannelSubscribeRequest<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.Timetoken, true, null);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
                    {
                        channelGroup = string.Join(",", netState.ChannelGroups);

                        if (channelGroup != "" && ChannelGroupInternetStatus.ContainsKey(channelGroup)
                            && (netState.ResponseType == PNOperationType.PNSubscribeOperation || netState.ResponseType == PNOperationType.Presence))
                        {
                            bool networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);

                            if (ChannelGroupInternetStatus[channelGroup])
                            {
                                //Reset Retry if previous state is true
                                //ChannelGroupInternetRetry.AddOrUpdate(channelGroup, 0, (key, oldValue) => 0);
                            }
                            else
                            {
                                ChannelGroupInternetStatus.AddOrUpdate(channelGroup, networkConnection, (key, oldValue) => networkConnection);

                                LoggingMethod.WriteToLog(string.Format("DateTime {0}, channelgroup={1} {2} reconnectNetworkCallback. Retrying", DateTime.Now.ToString(), channelGroup, netState.ResponseType), config.LogVerbosity);

                                if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
                                {
                                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.NotFound, new Exception("Internet connection problem. Retrying connection"));
                                    status.AffectedChannels.AddRange(netState.Channels.ToList());
                                    status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                                    Announce(status);
                                }
                            }
                        }

                        if (ChannelGroupInternetStatus[channelGroup])
                        {
                            if (ChannelGroupReconnectTimer.ContainsKey(channelGroup))
                            {
                                try
                                {
                                    ChannelGroupReconnectTimer[channelGroup].Change(Timeout.Infinite, Timeout.Infinite);
                                    ChannelGroupReconnectTimer[channelGroup].Dispose();
                                }
                                catch { }
                            }

                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.OK, null);
                            status.AffectedChannels.AddRange(netState.Channels.ToList());
                            status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                            Announce(status);

                            LoggingMethod.WriteToLog(string.Format("DateTime {0}, channelgroup={1} {2} reconnectNetworkCallback. Internet Available", DateTime.Now.ToString(), channelGroup, netState.ResponseType), config.LogVerbosity);
                            switch (netState.ResponseType)
                            {
                                case PNOperationType.PNSubscribeOperation:
                                case PNOperationType.Presence:
                                    MultiChannelSubscribeRequest<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.Timetoken, true, null);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(string.Format("DateTime {0}, Unknown request state in reconnectNetworkCallback", DateTime.Now.ToString()), config.LogVerbosity);
                }
            }
            catch (Exception ex)
            {
                if (netState != null)
                {
                    string multiChannel = (netState.Channels != null) ? string.Join(",", netState.Channels) : "";
                    string multiChannelGroup = (netState.ChannelGroups != null) ? string.Join(",", netState.ChannelGroups) : "";

                    PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, errorCategory, null, (int)HttpStatusCode.NotFound, ex);
                    status.AffectedChannels.AddRange(netState.Channels);
                    status.AffectedChannels.AddRange(netState.ChannelGroups);
                    Announce(status);
                }

                LoggingMethod.WriteToLog(string.Format("DateTime {0} method:reconnectNetworkCallback \n Exception Details={1}", DateTime.Now.ToString(), ex.ToString()), config.LogVerbosity);
            }
        }

        private void RegisterPresenceHeartbeatTimer<T>(string[] channels, string[] channelGroups)
        {
            if (PresenceHeartbeatTimer != null)
            {
                PresenceHeartbeatTimer.Dispose();
                PresenceHeartbeatTimer = null;
            }
            if ((channels != null && channels.Length > 0 && channels.Where(s => s.Contains("-pnpres") == false).ToArray().Length > 0)
                || (channelGroups != null && channelGroups.Length > 0 && channelGroups.Where(s => s.Contains("-pnpres") == false).ToArray().Length > 0))
            {
                RequestState<T> presenceHeartbeatState = new RequestState<T>();
                presenceHeartbeatState.Channels = channels;
                presenceHeartbeatState.ChannelGroups = channelGroups;
                presenceHeartbeatState.ResponseType = PNOperationType.PNHeartbeatOperation;
                //presenceHeartbeatState.ErrorCallback = errorCallback;
                presenceHeartbeatState.Request = null;
                presenceHeartbeatState.Response = null;

                if (config.PresenceHeartbeatInterval > 0)
                {
                    PresenceHeartbeatTimer = new Timer(OnPresenceHeartbeatIntervalTimeout<T>, presenceHeartbeatState, config.PresenceHeartbeatInterval * 1000, config.PresenceHeartbeatInterval * 1000);
                }
            }
        }

        private void OnPresenceHeartbeatIntervalTimeout<T>(System.Object presenceHeartbeatState)
        {
            //Make presence heartbeat call
            RequestState<T> currentState = presenceHeartbeatState as RequestState<T>;
            if (currentState != null)
            {
                bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, currentState.ResponseType, currentState.PubnubCallback, currentState.Channels, currentState.ChannelGroups);
                if (networkConnection)
                {
                    string[] subscriberChannels = (currentState.Channels != null) ? currentState.Channels.Where(s => s.Contains("-pnpres") == false).ToArray() : null;
                    string[] subscriberChannelGroups = (currentState.ChannelGroups != null) ? currentState.ChannelGroups.Where(s => s.Contains("-pnpres") == false).ToArray() : null;

                    if ((subscriberChannels != null && subscriberChannels.Length > 0) || (subscriberChannelGroups != null && subscriberChannelGroups.Length > 0))
                    {
                        string channelsJsonState = BuildJsonUserState(subscriberChannels, subscriberChannelGroups, false);
                        IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit);
                        Uri request = urlBuilder.BuildPresenceHeartbeatRequest(subscriberChannels, subscriberChannelGroups, channelsJsonState);

                        RequestState<T> requestState = new RequestState<T>();
                        requestState.Channels = currentState.Channels;
                        requestState.ChannelGroups = currentState.ChannelGroups;
                        requestState.ResponseType = PNOperationType.PNHeartbeatOperation;
                        requestState.PubnubCallback = currentState.PubnubCallback;
                        requestState.Reconnect = false;
                        requestState.Response = null;

                        string json = UrlProcessRequest<T>(request, requestState, false);
                    }
                }

            }

        }

        

    }
}
