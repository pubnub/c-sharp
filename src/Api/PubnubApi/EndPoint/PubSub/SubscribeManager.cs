using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PubnubApi.Interface;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;

namespace PubnubApi.EndPoint
{
    internal class SubscribeManager : PubnubCoreBase
    {
        private readonly PNConfiguration config;
        private readonly IJsonPluggableLibrary jsonLibrary;
        private readonly IPubnubUnitTest unit;
        private readonly IPubnubLog pubnubLog;
        private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;

        private const int MINEXPONENTIALBACKOFF = 1;
        private const int MAXEXPONENTIALBACKOFF = 32;

        private Timer SubscribeHeartbeatCheckTimer;

        public SubscribeManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }


        internal void MultiChannelUnSubscribeAll<T>(PNOperationType type)
        {
            //Retrieve the current channels already subscribed previously and terminate them
            string[] currentChannels = MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();
            string[] currentChannelGroups = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();

            if (currentChannels != null && currentChannels.Length >= 0)
            {
                string multiChannelName = (currentChannels.Length > 0) ? string.Join(",", currentChannels.OrderBy(x => x).ToArray()) : ",";
                string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups.OrderBy(x => x).ToArray()) : "";

                Task.Factory.StartNew(() =>
                {
                    if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(multiChannelName))
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);

                        HttpWebRequest webRequest = ChannelRequest[PubnubInstance.InstanceId][multiChannelName];
                        ChannelRequest[PubnubInstance.InstanceId][multiChannelName] = null;

                        HttpWebRequest removedRequest;
                        bool removedChannel = ChannelRequest[PubnubInstance.InstanceId].TryRemove(multiChannelName, out removedRequest);
                        if (removedChannel)
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                        }
                        if (webRequest != null)
                            TerminatePendingWebRequest(webRequest);
                    }
                    else
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                    }
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

                if (type == PNOperationType.PNUnsubscribeOperation)
                {
                    //just fire leave() event to REST API for safeguard
                    string channelsJsonState = BuildJsonUserState(currentChannels, currentChannelGroups, false);
                    IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
                    urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
                    Uri request = urlBuilder.BuildMultiChannelLeaveRequest(currentChannels, currentChannelGroups, config.Uuid, channelsJsonState);

                    RequestState<T> requestState = new RequestState<T>();
                    requestState.Channels = currentChannels;
                    requestState.ChannelGroups = currentChannelGroups;
                    requestState.ResponseType = PNOperationType.Leave;
                    requestState.Reconnect = false;

                    string json = UrlProcessRequest<T>(request, requestState, false);

                    MultiChannelSubscribe[PubnubInstance.InstanceId].Clear();
                    MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Clear();
                }
            }

        }

        internal void MultiChannelUnSubscribeInit<T>(PNOperationType type, string channel, string channelGroup)
        {
            List<string> validChannels = new List<string>();
            List<string> validChannelGroups = new List<string>();

            try
            {
                if (!MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId))
                {
                    MultiChannelSubscribe.Add(PubnubInstance.InstanceId, new ConcurrentDictionary<string, long>());
                }
                if (!MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId))
                {
                    MultiChannelGroupSubscribe.Add(PubnubInstance.InstanceId, new ConcurrentDictionary<string, long>());
                }

                string[] rawChannels = (channel != null && channel.Trim().Length > 0) ? channel.Split(',') : new string[] { };
                string[] rawChannelGroups = (channelGroup != null && channelGroup.Trim().Length > 0) ? channelGroup.Split(',') : new string[] { };

                if (rawChannels.Length > 0)
                {
                    for (int index = 0; index < rawChannels.Length; index++)
                    {
                        if (rawChannels[index].Trim().Length > 0)
                        {
                            string channelName = rawChannels[index].Trim();
                            if (string.IsNullOrEmpty(channelName)) continue;

                            if (MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId) && !MultiChannelSubscribe[PubnubInstance.InstanceId].ContainsKey(channelName))
                            {
                                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, null, (int)HttpStatusCode.NotFound, null);
                                if (!status.AffectedChannels.Contains(channelName))
                                {
                                    status.AffectedChannels.Add(channelName);
                                }
                                Announce(status);
                            }
                            else
                            {
                                validChannels.Add(channelName);
                                string presenceChannelName = string.Format("{0}-pnpres", channelName);
                                if (MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelSubscribe[PubnubInstance.InstanceId].ContainsKey(presenceChannelName))
                                {
                                    validChannels.Add(presenceChannelName);
                                }
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

                            if (MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId) && !MultiChannelGroupSubscribe[PubnubInstance.InstanceId].ContainsKey(channelGroupName))
                            {
                                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, null, (int)HttpStatusCode.NotFound, null);
                                if (!status.AffectedChannelGroups.Contains(channelGroupName))
                                {
                                    status.AffectedChannelGroups.Add(channelGroupName);
                                }
                                Announce(status);
                            }
                            else
                            {
                                validChannelGroups.Add(channelGroupName);
                                string presenceChannelGroupName = string.Format("{0}-pnpres", channelGroupName);
                                if (MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelGroupSubscribe[PubnubInstance.InstanceId].ContainsKey(presenceChannelGroupName))
                                {
                                    validChannelGroups.Add(presenceChannelGroupName);
                                }
                            }
                        }
                    }
                }

                if (validChannels.Count > 0 || validChannelGroups.Count > 0)
                {
                    //Retrieve the current channels already subscribed previously and terminate them
                    string[] currentChannels = MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();
                    string[] currentChannelGroups = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();

                    if (currentChannels != null && currentChannels.Length >= 0)
                    {
                        string multiChannelName = (currentChannels.Length > 0) ? string.Join(",", currentChannels.OrderBy(x => x).ToArray()) : ",";
                        string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups.OrderBy(x => x).ToArray()) : "";

                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(multiChannelName))
                            {
                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);

                                HttpWebRequest webRequest = ChannelRequest[PubnubInstance.InstanceId][multiChannelName];
                                ChannelRequest[PubnubInstance.InstanceId][multiChannelName] = null;

                                HttpWebRequest removedRequest;
                                bool removedChannel = ChannelRequest[PubnubInstance.InstanceId].TryRemove(multiChannelName, out removedRequest);
                                if (removedChannel)
                                {
                                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                                }
                                else
                                {
                                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelUnSubscribeInit).", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                                }
                                if (webRequest != null)
                                    TerminatePendingWebRequest(webRequest);
                            }
                            else
                            {
                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                            }
                        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

                        if (type == PNOperationType.PNUnsubscribeOperation)
                        {
                            //just fire leave() event to REST API for safeguard
                            string channelsJsonState = BuildJsonUserState(validChannels.ToArray(), validChannelGroups.ToArray(), false);
                            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
                            urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
                            Uri request = urlBuilder.BuildMultiChannelLeaveRequest(validChannels.ToArray(), validChannelGroups.ToArray(), config.Uuid, channelsJsonState);

                            RequestState<T> requestState = new RequestState<T>();
                            requestState.Channels = new string[] { channel };
                            requestState.ChannelGroups = new string[] { channelGroup };
                            requestState.ResponseType = PNOperationType.Leave;
                            requestState.Reconnect = false;

                            string json = UrlProcessRequest<T>(request, requestState, false);
                        }
                    }

                    Dictionary<string, long> originalMultiChannelSubscribe = null;
                    Dictionary<string, long> originalMultiChannelGroupSubscribe = null;
                    if (PubnubInstance != null && MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId))
                    {
                        originalMultiChannelSubscribe = MultiChannelSubscribe[PubnubInstance.InstanceId].Count > 0 ? MultiChannelSubscribe[PubnubInstance.InstanceId].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
                    }
                    if (PubnubInstance != null && MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId))
                    {
                        originalMultiChannelGroupSubscribe = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count > 0 ? MultiChannelGroupSubscribe[PubnubInstance.InstanceId].ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : null;
                    }

                    PNStatus successStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, (int)HttpStatusCode.OK, null);
                    PNStatus failStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, (int)HttpStatusCode.NotFound, new Exception("Unsubscribe Error. Please retry unsubscribe operation"));
                    bool successExist = false;
                    bool failExist = false;

                    //Remove the valid channels from subscribe list for unsubscribe 
                    for (int index = 0; index < validChannels.Count; index++)
                    {
                        long timetokenValue;
                        string channelToBeRemoved = validChannels[index].ToString();
                        bool unsubscribeStatus = false;
                        if (PubnubInstance != null && MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId))
                        {
                            unsubscribeStatus = MultiChannelSubscribe[PubnubInstance.InstanceId].TryRemove(channelToBeRemoved, out timetokenValue);
                        }
                        if (channelToBeRemoved.Contains("-pnpres"))
                        {
                            continue; //Do not send status for -pnpres channels
                        }
                        if (unsubscribeStatus)
                        {
                            successExist = true;
                            if (!successStatus.AffectedChannels.Contains(channelToBeRemoved))
                            {
                                successStatus.AffectedChannels.Add(channelToBeRemoved);
                            }

                            base.DeleteLocalChannelUserState(channelToBeRemoved);
                        }
                        else
                        {
                            failExist = true;
                            if (!failStatus.AffectedChannels.Contains(channelToBeRemoved))
                            {
                                failStatus.AffectedChannels.Add(channelToBeRemoved);
                            }
                        }
                    }
                    for (int index = 0; index < validChannelGroups.Count; index++)
                    {
                        long timetokenValue;
                        string channelGroupToBeRemoved = validChannelGroups[index].ToString();
                        bool unsubscribeStatus = false;
                        if (PubnubInstance != null && MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId))
                        {
                            unsubscribeStatus = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].TryRemove(channelGroupToBeRemoved, out timetokenValue);
                        }
                        if (channelGroupToBeRemoved.Contains("-pnpres"))
                        {
                            continue; //Do not send status for -pnpres channel-groups
                        }
                        if (unsubscribeStatus)
                        {
                            successExist = true;
                            if (!successStatus.AffectedChannelGroups.Contains(channelGroupToBeRemoved))
                            {
                                successStatus.AffectedChannelGroups.Add(channelGroupToBeRemoved);
                            }

                            base.DeleteLocalChannelGroupUserState(channelGroupToBeRemoved);
                        }
                        else
                        {
                            failExist = true;
                            if (!failStatus.AffectedChannelGroups.Contains(channelGroupToBeRemoved))
                            {
                                failStatus.AffectedChannelGroups.Add(channelGroupToBeRemoved);
                            }
                        }
                    }

                    if (successExist)
                    {
                        Announce(successStatus);
                    }

                    if (failExist)
                    {
                        Announce(failStatus);
                    }

                    //Get all the channels
                    string[] channels = MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();
                    string[] channelGroups = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();

                    //Check any chained subscribes while unsubscribe 
                    for (int keyIndex = 0; keyIndex < MultiChannelSubscribe[PubnubInstance.InstanceId].Count; keyIndex++)
                    {
                        KeyValuePair<string, long> kvp = MultiChannelSubscribe[PubnubInstance.InstanceId].ElementAt(keyIndex);
                        if (originalMultiChannelSubscribe != null && !originalMultiChannelSubscribe.ContainsKey(kvp.Key))
                        {
                            return;
                        }
                    }

                    for (int keyIndex = 0; keyIndex < MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count; keyIndex++)
                    {
                        KeyValuePair<string, long> kvp = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].ElementAt(keyIndex);
                        if (originalMultiChannelGroupSubscribe != null && !originalMultiChannelGroupSubscribe.ContainsKey(kvp.Key))
                        {
                            return;
                        }
                    }

                    channels = (channels != null) ? channels : new string[] { };
                    channelGroups = (channelGroups != null) ? channelGroups : new string[] { };

                    if (channels.Length > 0 || channelGroups.Length > 0)
                    {
                        string multiChannel = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";

                        RequestState<T> state = new RequestState<T>();
                        ChannelRequest[PubnubInstance.InstanceId].AddOrUpdate(multiChannel, state.Request, (key, oldValue) => state.Request);

                        ResetInternetCheckSettings(channels, channelGroups);


                        //Continue with any remaining channels for subscribe/presence
                        MultiChannelSubscribeRequest<T>(PNOperationType.PNSubscribeOperation, channels, channelGroups, 0, false, null);
                    }
                    else
                    {
                        if (PresenceHeartbeatTimer != null)
                        {
                            // Stop the presence heartbeat timer if there are no channels subscribed
                            PresenceHeartbeatTimer.Dispose();
                            PresenceHeartbeatTimer = null;
                        }
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, All channels are Unsubscribed. Further subscription was stopped", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                    }
                }
            }
            catch(Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} SubscribeManager=> MultiChannelUnSubscribeInit \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", validChannels.OrderBy(x => x).ToArray()), string.Join(",", validChannelGroups.OrderBy(x => x).ToArray()), ex.ToString()), config.LogVerbosity);
            }
        }

        internal void MultiChannelSubscribeInit<T>(PNOperationType responseType, string[] rawChannels, string[] rawChannelGroups, Dictionary<string, string> initialSubscribeUrlParams)
        {
            List<string> validChannels = new List<string>();
            List<string> validChannelGroups = new List<string>();

            try
            {
                bool channelGroupSubscribeOnly = false;
                SubscribeDisconnected[PubnubInstance.InstanceId] = false;

                string channel = (rawChannels != null) ? string.Join(",", rawChannels.OrderBy(x => x).ToArray()) : "";
                string channelGroup = (rawChannelGroups != null) ? string.Join(",", rawChannelGroups.OrderBy(x => x).ToArray()) : "";

                if (rawChannels.Length > 0)
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
                                if (MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId) && !MultiChannelSubscribe[PubnubInstance.InstanceId].ContainsKey(channelName))
                                {
                                    validChannels.Add(channelName);
                                }
                            }
                        }
                    }
                }

                if (rawChannelGroups != null && rawChannelGroups.Length > 0)
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
                            if (MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId) && !MultiChannelGroupSubscribe[PubnubInstance.InstanceId].ContainsKey(channelGroupName))
                            {
                                validChannelGroups.Add(channelGroupName);
                            }
                        }
                    }
                }

                if (validChannels.Count > 0 || validChannelGroups.Count > 0)
                {
                    //Retrieve the current channels already subscribed previously and terminate them
                    string[] currentChannels = MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();
                    string[] currentChannelGroups = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();

                    if (currentChannels != null && currentChannels.Length >= 0)
                    {
                        string multiChannelGroupName = (currentChannelGroups.Length > 0) ? string.Join(",", currentChannelGroups.OrderBy(x => x).ToArray()) : "";
                        if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId))
                        {
                            List<string> keysList = ChannelRequest[PubnubInstance.InstanceId].Keys.ToList();
                            for (int keyIndex = 0; keyIndex < keysList.Count; keyIndex++)
                            {
                                string multiChannelName = keysList[keyIndex];
                                if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(multiChannelName))
                                {
                                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Aborting previous subscribe/presence requests having channel(s)={1}; channelgroup(s)={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                                    HttpWebRequest webRequest = ChannelRequest[PubnubInstance.InstanceId][multiChannelName];
                                    ChannelRequest[PubnubInstance.InstanceId][multiChannelName] = null;

                                    HttpWebRequest removedRequest;
                                    bool removedChannel = ChannelRequest[PubnubInstance.InstanceId].TryRemove(multiChannelName, out removedRequest);
                                    if (removedChannel)
                                    {
                                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Success to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                                    }
                                    else
                                    {
                                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unable to remove channel(s)={1}; channelgroup(s)={2} from _channelRequest (MultiChannelSubscribeInit).", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                                    }
                                    if (webRequest != null)
                                        TerminatePendingWebRequest(webRequest);
                                }
                                else
                                {
                                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unable to capture channel(s)={1}; channelgroup(s)={2} from _channelRequest to abort request.", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannelName, multiChannelGroupName), config.LogVerbosity);
                                }
                            }
                        }
                        else
                        {
                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unable to find instance id = {1} from _channelRequest.", DateTime.Now.ToString(CultureInfo.InvariantCulture), PubnubInstance.InstanceId), config.LogVerbosity);
                        }
                    }

                    TerminateCurrentSubscriberRequest();

                    //Add the valid channels to the channels subscribe list for tracking
                    for (int index = 0; index < validChannels.Count; index++)
                    {
                        string currentLoopChannel = validChannels[index].ToString();
                        MultiChannelSubscribe[PubnubInstance.InstanceId].GetOrAdd(currentLoopChannel, 0);
                    }


                    for (int index = 0; index < validChannelGroups.Count; index++)
                    {
                        string currentLoopChannelGroup = validChannelGroups[index].ToString();
                        MultiChannelGroupSubscribe[PubnubInstance.InstanceId].GetOrAdd(currentLoopChannelGroup, 0);
                    }

                    //Get all the channels
                    string[] channels = MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();
                    string[] channelGroups = MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.ToArray<string>();

                    if (channelGroups != null && channelGroups.Length > 0 && (channels == null || channels.Length == 0))
                    {
                        channelGroupSubscribeOnly = true;
                    }

                    RequestState<T> state = new RequestState<T>();
                    if (ChannelRequest.ContainsKey(PubnubInstance.InstanceId))
                    {
                        if (channelGroupSubscribeOnly)
                        {
                            ChannelRequest[PubnubInstance.InstanceId].AddOrUpdate(",", state.Request, (key, oldValue) => state.Request);
                        }
                        else
                        {
                            ChannelRequest[PubnubInstance.InstanceId].AddOrUpdate(string.Join(",", channels.OrderBy(x => x).ToArray()), state.Request, (key, oldValue) => state.Request);
                        }
                    }

                    ResetInternetCheckSettings(channels, channelGroups);
                    MultiChannelSubscribeRequest<T>(responseType, channels, channelGroups, 0, false, initialSubscribeUrlParams);

                    if (SubscribeHeartbeatCheckTimer != null)
                    {
                        try
                        {
                            SubscribeHeartbeatCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            SubscribeHeartbeatCheckTimer.Dispose();
                            SubscribeHeartbeatCheckTimer = null;
                        }
                        catch { }
                    }
                    SubscribeHeartbeatCheckTimer = new Timer(StartSubscribeHeartbeatCheckCallback<T>, null, config.SubscribeTimeout * 1000, config.SubscribeTimeout * 1000);
                }
            }
            catch(Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} SubscribeManager=> MultiChannelSubscribeInit \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", validChannels.OrderBy(x => x).ToArray()), string.Join(",", validChannelGroups.OrderBy(x => x).ToArray()), ex.ToString()), config.LogVerbosity);
            }
        }

        private void MultiChannelSubscribeRequest<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken, bool reconnect, Dictionary<string, string> initialSubscribeUrlParams)
        {
            if (SubscribeDisconnected[PubnubInstance.InstanceId])
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeDisconnected. Exiting MultiChannelSubscribeRequest", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                return;
            }

            //Exit if the channel is unsubscribed
            if (MultiChannelSubscribe != null && MultiChannelSubscribe[PubnubInstance.InstanceId].Count <= 0 && MultiChannelGroupSubscribe != null && MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count <= 0)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Zero channels/channelGroups. Further subscription was stopped", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                return;
            }

            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
            string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";
            if (!ChannelRequest.ContainsKey(PubnubInstance.InstanceId) || !ChannelRequest[PubnubInstance.InstanceId].ContainsKey(multiChannel))
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, PubnubInstance.InstanceId NOT matching", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                return;
            }

            bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, type, null, channels, channelGroups);

            if (!networkConnection)
            {
                ConnectionErrors++;
                UpdatePubnubNetworkTcpCheckIntervalInSeconds();
                ChannelInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(multiChannel, networkConnection, (key, oldValue) => networkConnection);
                ChannelGroupInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(multiChannelGroup, networkConnection, (key, oldValue) => networkConnection);
            }

            if (((ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(multiChannel) && !ChannelInternetStatus[PubnubInstance.InstanceId][multiChannel])
                || (multiChannelGroup != "" && ChannelGroupInternetStatus[PubnubInstance.InstanceId].ContainsKey(multiChannelGroup) && !ChannelGroupInternetStatus[PubnubInstance.InstanceId][multiChannelGroup]))
                && PubnetSystemActive)
            {
                if (ReconnectNetworkIfOverrideTcpKeepAlive<T>(type, channels, channelGroups, timetoken, networkConnection))
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
                long minimumTimetoken1 = (MultiChannelSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelSubscribe[PubnubInstance.InstanceId].Min(token => token.Value) : 0;
                long minimumTimetoken2 = (MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Min(token => token.Value) : 0;
                long minimumTimetoken = Math.Max(minimumTimetoken1, minimumTimetoken2);

                long maximumTimetoken1 = (MultiChannelSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelSubscribe[PubnubInstance.InstanceId].Max(token => token.Value) : 0;
                long maximumTimetoken2 = (MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Max(token => token.Value) : 0;
                long maximumTimetoken = Math.Max(maximumTimetoken1, maximumTimetoken2);


                if (minimumTimetoken == 0 || reconnect || UuidChanged)
                {
                    lastTimetoken = 0;
                    UuidChanged = false;
                }
                else
                {
                    if (LastSubscribeTimetoken[PubnubInstance.InstanceId] == maximumTimetoken)
                    {
                        lastTimetoken = maximumTimetoken;
                    }
                    else
                    {
                        lastTimetoken = LastSubscribeTimetoken[PubnubInstance.InstanceId];
                    }
                }
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Building request for channel(s)={1}, channelgroup(s)={2} with timetoken={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannel, multiChannelGroup, lastTimetoken), config.LogVerbosity);
                // Build URL
                string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
                config.Uuid = CurrentUuid; // to make sure we capture if UUID is changed
                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
                urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
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
                    string subscribedChannels = (MultiChannelSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.OrderBy(x=>x).Aggregate((x, y) => x + "," + y) : "";
                    string currentChannels = (channels != null && channels.Length > 0) ? channels.OrderBy(x => x).Aggregate((x, y) => x + "," + y) : "";

                    string subscribedChannelGroups = (MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.OrderBy(x => x).Aggregate((x, y) => x + "," + y) : "";
                    string currentChannelGroups = (channelGroups != null && channelGroups.Length > 0) ? channelGroups.OrderBy(x => x).Aggregate((x, y) => x + "," + y) : "";

                    if (subscribedChannels == currentChannels && subscribedChannelGroups == currentChannelGroups)
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
                                        MultiChannelSubscribe[PubnubInstance.InstanceId].AddOrUpdate(currentChannel, jsonTimetoken, (key, oldValue) => jsonTimetoken);
                                    }
                                }
                                if (pubnubRequestState.ChannelGroups != null && pubnubRequestState.ChannelGroups.Length > 0)
                                {
                                    foreach (string currentChannelGroup in pubnubRequestState.ChannelGroups)
                                    {
                                        MultiChannelGroupSubscribe[PubnubInstance.InstanceId].AddOrUpdate(currentChannelGroup, jsonTimetoken, (key, oldValue) => jsonTimetoken);
                                    }
                                }
                            }
                        }

                        switch (pubnubRequestState.ResponseType)
                        {
                            case PNOperationType.PNSubscribeOperation:
                                MultiplexInternalCallback<T>(pubnubRequestState.ResponseType, result);
                                break;
                            default:
                                break;
                        }

                    }

                }
                else
                {
                    MultiplexExceptionHandler<T>(type, channels, channelGroups, false, false);
                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} method:_subscribe \n channel={1} \n timetoken={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), timetoken.ToString(), ex.ToString()), config.LogVerbosity);

                PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, errorCategory, pubnubRequestState, (int)HttpStatusCode.NotFound, ex);
                if (channels != null)
                {
                    status.AffectedChannels.AddRange(channels);
                }

                if (channelGroups != null)
                {
                    status.AffectedChannels.AddRange(channelGroups);
                }

                Announce(status);

                MultiChannelSubscribeRequest<T>(type, channels, channelGroups, LastSubscribeTimetoken[PubnubInstance.InstanceId], false, null);
            }
        }

        private void MultiplexExceptionHandler<T>(PNOperationType type, string[] channels, string[] channelGroups, bool reconnectMaxTried, bool resumeOnReconnect)
        {
            string channel = "";
            string channelGroup = "";
            if (channels != null)
            {
                channel = string.Join(",", channels.OrderBy(x => x).ToArray());
            }
            if (channelGroups != null)
            {
                channelGroup = string.Join(",", channelGroups.OrderBy(x => x).ToArray());
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
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Lost Channel Name for resubscribe", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                return;
            }

            if (message != null && message.Count >= 3)
            {
                long timetoken = GetTimetokenFromMultiplexResult(message);
                System.Threading.Tasks.Task.Factory.StartNew(() => 
                {
                    MultiChannelSubscribeRequest<T>(type, channels, channelGroups, timetoken, false, null);
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }
        }

        private bool ReconnectNetworkIfOverrideTcpKeepAlive<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken, bool networkAvailable)
        {
            if (OverrideTcpKeepAlive)
            {
                ReconnectState<T> netState = new ReconnectState<T>();
                netState.Channels = channels;
                netState.ChannelGroups = channelGroups;
                netState.ResponseType = type;
                netState.Timetoken = timetoken;

                if (SubscribeDisconnected[PubnubInstance.InstanceId])
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Subscribe is still Disconnected. So no reconnect", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                }
                else if (config.ReconnectionPolicy != PNReconnectionPolicy.NONE)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Subscribe - No internet connection for channel={1} and channelgroup={2}; networkAvailable={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), channelGroups != null ? string.Join(",", channelGroups) : "", networkAvailable), config.LogVerbosity);
                    TerminateReconnectTimer();
                    ReconnectNetwork<T>(netState);
                }
                else
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, reconnection policy is DISABLED, please handle reconnection manually.", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                    if (!networkAvailable)
                    {
                        PNStatusCategory errorCategory =  PNStatusCategory.PNNetworkIssuesCategory;
                        PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, errorCategory, null, (int)HttpStatusCode.NotFound, new Exception("SDK Network related error"));
                        status.AffectedChannels.AddRange(channels);
                        status.AffectedChannels.AddRange(channelGroups);
                        Announce(status);

                    }
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
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeManager ReconnectNetwork interval = {1} sec", DateTime.Now.ToString(CultureInfo.InvariantCulture), PubnubNetworkTcpCheckIntervalInSeconds), config.LogVerbosity);

                System.Threading.Timer timer = new Timer(new TimerCallback(ReconnectNetworkCallback<T>), netState, 0,
                                                      (-1 == PubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : PubnubNetworkTcpCheckIntervalInSeconds * 1000);

                if (netState.Channels != null && netState.Channels.Length > 0)
                {
                    ChannelReconnectTimer[PubnubInstance.InstanceId].AddOrUpdate(string.Join(",", netState.Channels.OrderBy(x => x).ToArray()), timer, (key, oldState) => timer);
                }
                if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
                {
                    ChannelGroupReconnectTimer[PubnubInstance.InstanceId].AddOrUpdate(string.Join(",", netState.ChannelGroups.OrderBy(x => x).ToArray()), timer, (key, oldState) => timer);
                }
            }
        }

        internal void Reconnect<T>()
        {
            if (!SubscribeDisconnected[PubnubInstance.InstanceId]) //Check if disconnect is done before
            {
                return;
            }

            string[] channels = GetCurrentSubscriberChannels();
            string[] chananelGroups = GetCurrentSubscriberChannelGroups();

            if ((channels != null && channels.Length > 0) || (chananelGroups != null && chananelGroups.Length > 0))
            {
                string channel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
                string channelGroup = (chananelGroups != null && chananelGroups.Length > 0) ? string.Join(",", chananelGroups.OrderBy(x => x).ToArray()) : "";

                bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, PNOperationType.PNSubscribeOperation, null, channels, chananelGroups);
                if (!networkConnection)
                {
                    //Recheck for false alert with 1 sec delay
#if !NET35 && !NET40
                    Task.Delay(1000).Wait();
#else
                    Thread.Sleep(1000);
#endif

                    networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, PNOperationType.PNSubscribeOperation, null, channels, chananelGroups);
                }
                if (networkConnection)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Network available for SubscribeManager Manual Reconnect", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                    if (!string.IsNullOrEmpty(channel) && ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(channel))
                    {
                        ChannelInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(channel, networkConnection, (key, oldValue) => networkConnection);
                    }
                    if (!string.IsNullOrEmpty(channelGroup) && ChannelGroupInternetStatus[PubnubInstance.InstanceId].ContainsKey(channelGroup))
                    {
                        ChannelGroupInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(channelGroup, networkConnection, (key, oldValue) => networkConnection);
                    }
                }
                else
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, No network for SubscribeManager Manual Reconnect", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);

                    PNStatusCategory errorCategory = PNStatusCategory.PNNetworkIssuesCategory;
                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNSubscribeOperation, errorCategory, null, (int)HttpStatusCode.NotFound, new Exception("SDK Network related error"));
                    status.AffectedChannels.AddRange(channels);
                    status.AffectedChannels.AddRange(chananelGroups);
                    Announce(status);

                    return;
                }
            }
            else
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, No channels/channelgroups for SubscribeManager Manual Reconnect", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                return;
            }


            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeManager Manual Reconnect", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
            SubscribeDisconnected[PubnubInstance.InstanceId] = false;

            Task.Factory.StartNew(() =>
            {
                MultiChannelSubscribeRequest<T>(PNOperationType.PNSubscribeOperation, GetCurrentSubscriberChannels(), GetCurrentSubscriberChannelGroups(), LastSubscribeTimetoken[PubnubInstance.InstanceId], false, null);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Disconnect<T>()
        {
            if (SubscribeDisconnected[PubnubInstance.InstanceId])
            {
                return;
            }
            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeManager Manual Disconnect", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
            SubscribeDisconnected[PubnubInstance.InstanceId] = true;
            TerminateCurrentSubscriberRequest();
            TerminatePresenceHeartbeatTimer();
            TerminateReconnectTimer();
        }

        internal void StartSubscribeHeartbeatCheckCallback<T>(object state)
        {
            if (SubscribeDisconnected[PubnubInstance.InstanceId])
            {
                return;
            }

            string[] channels = GetCurrentSubscriberChannels();
            string[] chananelGroups = GetCurrentSubscriberChannelGroups();

            if ((channels != null && channels.Length > 0) || (chananelGroups != null && chananelGroups.Length > 0))
            {
                bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, PNOperationType.PNSubscribeOperation, null, channels, chananelGroups);
                if (networkConnection && PubnubInstance != null && SubscribeRequestTracker.ContainsKey(PubnubInstance.InstanceId))
                {
                    DateTime lastSubscribeRequestTime = SubscribeRequestTracker[PubnubInstance.InstanceId];
                    if ((DateTime.Now - lastSubscribeRequestTime).TotalSeconds <= config.SubscribeTimeout)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeManager - ok. expected subscribe within threshold limit of SubscribeTimeout. No action needed", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                    }
                    else if ((DateTime.Now - lastSubscribeRequestTime).TotalSeconds > 2*config.SubscribeTimeout)
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeManager - **No auto subscribe within threshold limit of SubscribeTimeout**. Calling MultiChannelSubscribeRequest", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                        Task.Factory.StartNew(() =>
                        {
                            TerminateCurrentSubscriberRequest();
                            MultiChannelSubscribeRequest<T>(PNOperationType.PNSubscribeOperation, channels, chananelGroups, LastSubscribeTimetoken[PubnubInstance.InstanceId], false, null);
                        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                    }
                    else
                    {
                        LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeManager - **No auto subscribe within threshold limit of SubscribeTimeout**. Calling TerminateCurrentSubscriberRequest", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                        Task.Factory.StartNew(() =>
                        {
                            TerminateCurrentSubscriberRequest();
                        }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                    }
                }
            }
            else
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, SubscribeManager - StartSubscribeHeartbeatCheckCallback - No channels/cgs avaialable", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                try
                {
                    SubscribeHeartbeatCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    SubscribeHeartbeatCheckTimer.Dispose();
                    SubscribeHeartbeatCheckTimer = null;

                    TerminateCurrentSubscriberRequest();
                }
                catch { }
            }
        }


        protected void ReconnectNetworkCallback<T>(System.Object reconnectState)
        {
            string channel = "";
            string channelGroup = "";

            ReconnectState<T> netState = reconnectState as ReconnectState<T>;
            try
            {
                string subscribedChannels = (MultiChannelSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelSubscribe[PubnubInstance.InstanceId].Keys.OrderBy(x => x).Aggregate((x, y) => x + "," + y) : "";
                string subscribedChannelGroups = (MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Count > 0) ? MultiChannelGroupSubscribe[PubnubInstance.InstanceId].Keys.OrderBy(x => x).Aggregate((x, y) => x + "," + y) : "";
                List<string> channelRequestKeyList = ChannelRequest[PubnubInstance.InstanceId].Keys.ToList();
                for(int keyIndex= 0; keyIndex < channelRequestKeyList.Count; keyIndex++)
                {
                    string keyChannel = channelRequestKeyList[keyIndex];
                    if (keyChannel != subscribedChannels)
                    {
                        if (ChannelRequest[PubnubInstance.InstanceId].ContainsKey(keyChannel))
                        {
                            if (ChannelRequest[PubnubInstance.InstanceId][keyChannel] != null)
                            {
                                try
                                {
                                    ChannelRequest[PubnubInstance.InstanceId][keyChannel].Abort();
                                }
                                catch { }
                                ChannelRequest[PubnubInstance.InstanceId][keyChannel] = null;
                            }
                            HttpWebRequest tempValue;
                            ChannelRequest[PubnubInstance.InstanceId].TryRemove(keyChannel, out tempValue);
                        }
                    }
                }


                if (netState != null && ((netState.Channels != null && netState.Channels.Length > 0) || (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)))
                {
                    if (netState.Channels != null && netState.Channels.Length > 0)
                    {
                        channel = (netState.Channels.Length > 0) ? string.Join(",", netState.Channels.OrderBy(x=>x).ToArray()) : ",";

                        if (channel == subscribedChannels && ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(channel)
                            && (netState.ResponseType == PNOperationType.PNSubscribeOperation || netState.ResponseType == PNOperationType.Presence))
                        {
                            bool networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
                            if (networkConnection) {
                                //Re-try to avoid false alert
                                networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
                            }

                            if (ChannelInternetStatus[PubnubInstance.InstanceId][channel])
                            {
                                //Reset Retry if previous state is true
                                //ChannelInternetRetry.AddOrUpdate(channel, 0, (key, oldValue) => 0);
                            }
                            else
                            {
                                ChannelInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(channel, networkConnection, (key, oldValue) => networkConnection);

                                ConnectionErrors++;
                                UpdatePubnubNetworkTcpCheckIntervalInSeconds();

                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, channel={1} {2} reconnectNetworkCallback. Retry", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel, netState.ResponseType), config.LogVerbosity);

                                if (netState.Channels != null && netState.Channels.Length > 0)
                                {
                                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.NotFound, new Exception("Internet connection problem. Retrying connection"));
                                    status.AffectedChannels.AddRange(netState.Channels.ToList());
                                    status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                                    Announce(status);
                                }

                            }
                        }

                        if (ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(channel) && ChannelInternetStatus[PubnubInstance.InstanceId][channel])
                        {
                            if (ChannelReconnectTimer[PubnubInstance.InstanceId].ContainsKey(channel))
                            {
                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, {1} {2} terminating ch reconnectimer", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel, netState.ResponseType), config.LogVerbosity);
                                TerminateReconnectTimer();
                            }

                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.OK, null);
                            status.AffectedChannels.AddRange(netState.Channels.ToList());
                            status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                            Announce(status);

                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, channel={1} {2} reconnectNetworkCallback. Internet Available : {3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel, netState.ResponseType, ChannelInternetStatus[PubnubInstance.InstanceId][channel]), config.LogVerbosity);
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
                        channelGroup = string.Join(",", netState.ChannelGroups.OrderBy(x => x).ToArray());

                        if (subscribedChannelGroups == channelGroup && channelGroup != "" && ChannelGroupInternetStatus[PubnubInstance.InstanceId].ContainsKey(channelGroup)
                            && (netState.ResponseType == PNOperationType.PNSubscribeOperation || netState.ResponseType == PNOperationType.Presence))
                        {
                            bool networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
                            if (networkConnection)
                            {
                                //Re-try to avoid false alert
                                networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
                            }

                            if (ChannelGroupInternetStatus[PubnubInstance.InstanceId][channelGroup])
                            {
                                //Reset Retry if previous state is true
                                //ChannelGroupInternetRetry.AddOrUpdate(channelGroup, 0, (key, oldValue) => 0);
                            }
                            else
                            {
                                ChannelGroupInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(channelGroup, networkConnection, (key, oldValue) => networkConnection);

                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, channelgroup={1} {2} reconnectNetworkCallback. Retrying", DateTime.Now.ToString(CultureInfo.InvariantCulture), channelGroup, netState.ResponseType), config.LogVerbosity);

                                if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0)
                                {
                                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.NotFound, new Exception("Internet connection problem. Retrying connection"));
                                    status.AffectedChannels.AddRange(netState.Channels.ToList());
                                    status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                                    Announce(status);
                                }
                            }
                        }

                        if (ChannelGroupInternetStatus[PubnubInstance.InstanceId][channelGroup])
                        {
                            if (ChannelGroupReconnectTimer.ContainsKey(channelGroup))
                            {
                                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, {1} {2} terminating cg reconnectimer", DateTime.Now.ToString(CultureInfo.InvariantCulture), channelGroup, netState.ResponseType), config.LogVerbosity);
                                TerminateReconnectTimer();
                            }

                            PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.OK, null);
                            status.AffectedChannels.AddRange(netState.Channels.ToList());
                            status.AffectedChannelGroups.AddRange(netState.Channels.ToList());
                            Announce(status);

                            LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, channelgroup={1} {2} reconnectNetworkCallback. Internet Available", DateTime.Now.ToString(CultureInfo.InvariantCulture), channelGroup, netState.ResponseType), config.LogVerbosity);
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
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Unknown request state in reconnectNetworkCallback", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                }
            }
            catch (Exception ex)
            {
                if (netState != null)
                {
                    string multiChannel = (netState.Channels != null) ? string.Join(",", netState.Channels.OrderBy(x => x).ToArray()) : "";
                    string multiChannelGroup = (netState.ChannelGroups != null) ? string.Join(",", netState.ChannelGroups.OrderBy(x => x).ToArray()) : "";

                    PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                    PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, errorCategory, null, (int)HttpStatusCode.NotFound, ex);
                    status.AffectedChannels.AddRange(netState.Channels);
                    status.AffectedChannels.AddRange(netState.ChannelGroups);
                    Announce(status);
                }

                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} method:reconnectNetworkCallback \n Exception Details={1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), ex.ToString()), config.LogVerbosity);
            }
        }

        private void RegisterPresenceHeartbeatTimer<T>(string[] channels, string[] channelGroups)
        {
            if (PresenceHeartbeatTimer != null)
            {
                try
                {
                    PresenceHeartbeatTimer.Dispose();
                    PresenceHeartbeatTimer = null;
                }
                catch { }
            }
            if ((channels != null && channels.Length > 0 && channels.Where(s => s.Contains("-pnpres") == false).ToArray().Length > 0)
                || (channelGroups != null && channelGroups.Length > 0 && channelGroups.Where(s => s.Contains("-pnpres") == false).ToArray().Length > 0))
            {
                RequestState<T> presenceHeartbeatState = new RequestState<T>();
                presenceHeartbeatState.Channels = channels;
                presenceHeartbeatState.ChannelGroups = channelGroups;
                presenceHeartbeatState.ResponseType = PNOperationType.PNHeartbeatOperation;
                presenceHeartbeatState.Request = null;
                presenceHeartbeatState.Response = null;

                if (config.PresenceInterval > 0)
                {
                    PresenceHeartbeatTimer = new Timer(OnPresenceHeartbeatIntervalTimeout<T>, presenceHeartbeatState, config.PresenceInterval * 1000, config.PresenceInterval * 1000);
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
                        IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr);
                        urlBuilder.PubnubInstanceId = (PubnubInstance != null) ? PubnubInstance.InstanceId : "";
                        Uri request = urlBuilder.BuildPresenceHeartbeatRequest(subscriberChannels, subscriberChannelGroups, channelsJsonState);

                        RequestState<PNHeartbeatResult> requestState = new RequestState<PNHeartbeatResult>();
                        requestState.Channels = currentState.Channels;
                        requestState.ChannelGroups = currentState.ChannelGroups;
                        requestState.ResponseType = PNOperationType.PNHeartbeatOperation;
                        requestState.PubnubCallback = null;
                        requestState.Reconnect = false;
                        requestState.Response = null;

                        string json = UrlProcessRequest<PNHeartbeatResult>(request, requestState, false);
                        if (!string.IsNullOrEmpty(json))
                        {
                            List<object> result = ProcessJsonResponse<PNHeartbeatResult>(requestState, json);
                            ProcessResponseCallbacks(result, requestState);
                        }
                    }
                }

            }

        }

        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
        }

    }
}
