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
    internal class StatelessSubscribeManager : Stateless_PubnubCoreBase, IDisposable
    {
        private static PNConfiguration config;
        private static IJsonPluggableLibrary jsonLibrary;
        private static IPubnubUnitTest unit;
        private static IPubnubLog pubnubLog;
        private static EndPoint.TelemetryManager pubnubTelemetryMgr;

        private Dictionary<string, object> customQueryParam;

        public StatelessSubscribeManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
            pubnubTelemetryMgr = telemetryManager;
        }

        internal async Task<Tuple<string, PNStatus>> Handshake<T>(PNOperationType responseType, string[] rawChannels, string[] rawChannelGroups, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam, CancellationToken cancellationToken)
        {
            return await Stateless_MultiChannelSubscribeInit<T>(responseType, rawChannels, rawChannelGroups, initialSubscribeUrlParams, externalQueryParam, cancellationToken);
        }

        internal async Task<Tuple<string, PNStatus>> ReceiveMessages<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken, int region, bool reconnect, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam, CancellationToken cancellationToken)
        {
            return await Stateless_MultiChannelSubscribeRequest<T>(type, channels, channelGroups, timetoken, region, reconnect, initialSubscribeUrlParams, externalQueryParam, cancellationToken);
        }

        internal async Task<Tuple<string, PNStatus>> IAmAway<T>(PNOperationType type, string channel, string channelGroup, Dictionary<string, object> externalQueryParam, CancellationToken cancellationToken)
        {
            return await Stateless_MultiChannelUnSubscribeInit<T>(type, channel, channelGroup, externalQueryParam, cancellationToken);
        }

        internal async Task<Tuple<string, PNStatus>> IamHere(PNOperationType type, string[] channels, string[] channelGroups, CancellationToken cancellationToken)
        {
            return await Stateless_PresenceHeartbeat(type, channels, channelGroups, cancellationToken);
        }

        internal async Task<PNResult<PNSetStateResult>> SetPresenceState(string[] channels, string[] channelGroups, string uuid, string jsonUserState, Dictionary<string, object> externalQueryParam, PNCallback<PNSetStateResult> callback)
        {
            EndPoint.SetStateOperation setStateOperation = new EndPoint.SetStateOperation(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, null, base.PubnubInstance);
            return await setStateOperation.Channels(channels).ChannelGroups(channelGroups).ExecuteAsync();
        }

        internal async Task<Tuple<string,PNStatus>> Stateless_MultiChannelUnSubscribeInit<T>(PNOperationType type, string channel, string channelGroup, Dictionary<string, object> externalQueryParam, CancellationToken cancellationToken)
        {
            List<string> validChannels = new List<string>();
            List<string> validChannelGroups = new List<string>();
            Tuple<string, PNStatus> ret = default(Tuple<string, PNStatus>);

            try
            {
                this.customQueryParam = externalQueryParam;

                if (PubnubInstance == null)
                {
                    LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, PubnubInstance is null. exiting MultiChannelUnSubscribeInit", DateTime.Now.ToString(CultureInfo.InvariantCulture)), config.LogVerbosity);
                    return ret;
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
                            if (string.IsNullOrEmpty(channelName))
                            {
                                continue;
                            }

                            if (MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelSubscribe[PubnubInstance.InstanceId] != null && !MultiChannelSubscribe[PubnubInstance.InstanceId].ContainsKey(channelName))
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
                                if (MultiChannelSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelSubscribe[PubnubInstance.InstanceId] != null && MultiChannelSubscribe[PubnubInstance.InstanceId].ContainsKey(presenceChannelName))
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
                            if (string.IsNullOrEmpty(channelGroupName))
                            {
                                continue;
                            }

                            if (MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelGroupSubscribe[PubnubInstance.InstanceId] != null && !MultiChannelGroupSubscribe[PubnubInstance.InstanceId].ContainsKey(channelGroupName))
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
                                if (MultiChannelGroupSubscribe.ContainsKey(PubnubInstance.InstanceId) && MultiChannelGroupSubscribe[PubnubInstance.InstanceId] != null && MultiChannelGroupSubscribe[PubnubInstance.InstanceId].ContainsKey(presenceChannelGroupName))
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

                        if (type == PNOperationType.PNUnsubscribeOperation)
                        {
                            //just fire leave() event to REST API for safeguard
                            string channelsJsonState = BuildJsonUserState(validChannels.ToArray(), validChannelGroups.ToArray(), false);
                            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

                            Uri request = urlBuilder.BuildMultiChannelLeaveRequest("GET", "", validChannels.ToArray(), validChannelGroups.ToArray(), channelsJsonState, externalQueryParam);

                            RequestState<T> requestState = new RequestState<T>();
                            requestState.Channels = new[] { channel };
                            requestState.ChannelGroups = new[] { channelGroup };
                            requestState.ResponseType = PNOperationType.Leave;
                            requestState.Reconnect = false;

                            ret = await UrlProcessRequest<T>(request, requestState, false, cancellationToken);
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
                    PNStatus failStatus = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, (int)HttpStatusCode.NotFound, new PNException("Unsubscribe Error. Please retry unsubscribe operation"));
                    bool successExist = false;
                    bool failExist = false;

                    //Remove the valid channels from subscribe list for unsubscribe 
                    for (int index = 0; index < validChannels.Count; index++)
                    {
                        long timetokenValue;
                        string channelToBeRemoved = validChannels[index];
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
                        string channelGroupToBeRemoved = validChannelGroups[index];
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

                    if (successExist && PubnubInstance != null)
                    {
                        Announce(successStatus);
                    }

                    if (failExist && PubnubInstance != null)
                    {
                        Announce(failStatus);
                    }

                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} SubscribeManager=> MultiChannelUnSubscribeInit \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", validChannels.OrderBy(x => x).ToArray()), string.Join(",", validChannelGroups.OrderBy(x => x).ToArray()), ex), config.LogVerbosity);
            }

            return ret;
        }

        internal async Task<Tuple<string, PNStatus>> Stateless_MultiChannelSubscribeInit<T>(PNOperationType responseType, string[] rawChannels, string[] rawChannelGroups, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam, CancellationToken cancellationToken)
        {
            List<string> validChannels = new List<string>();
            List<string> validChannelGroups = new List<string>();
            Tuple<string, PNStatus> ret = default(Tuple<string, PNStatus>);

            try
            {
                bool channelGroupSubscribeOnly = false;
                SubscribeDisconnected[PubnubInstance.InstanceId] = false;

                if (rawChannels != null && rawChannels.Length > 0)
                {
                    string[] rawChannelsFiltered = rawChannels;
                    if (rawChannels.Length != rawChannels.Distinct().Count())
                    {
                        rawChannelsFiltered = rawChannels.Distinct().ToArray();
                    }

                    for (int index = 0; index < rawChannelsFiltered.Length; index++)
                    {
                        if (rawChannelsFiltered[index].Trim().Length > 0)
                        {
                            string channelName = rawChannelsFiltered[index].Trim();
                            if (!string.IsNullOrEmpty(channelName))
                            {
                                validChannels.Add(channelName);
                            }
                        }
                    }
                }

                if (rawChannelGroups != null && rawChannelGroups.Length > 0)
                {
                    string[] rawChannelGroupsFiltered = rawChannelGroups;
                    if (rawChannelGroups.Length != rawChannelGroups.Distinct().Count())
                    {
                        rawChannelGroupsFiltered = rawChannelGroups.Distinct().ToArray();
                    }

                    for (int index = 0; index < rawChannelGroupsFiltered.Length; index++)
                    {
                        if (rawChannelGroupsFiltered[index].Trim().Length > 0)
                        {
                            string channelGroupName = rawChannelGroupsFiltered[index].Trim();
                            validChannelGroups.Add(channelGroupName);
                        }
                    }
                }

                if (validChannels.Count > 0 || validChannelGroups.Count > 0)
                {
                    //Get all the channels
                    string[] channels = validChannels.ToArray<string>();
                    string[] channelGroups = validChannelGroups.ToArray<string>();

                    if (channelGroups != null && channelGroups.Length > 0 && (channels == null || channels.Length == 0))
                    {
                        channelGroupSubscribeOnly = true;
                    }

                    ret = await Stateless_MultiChannelSubscribeRequest<T>(responseType, channels, channelGroups, 0, 0, false, initialSubscribeUrlParams, externalQueryParam, cancellationToken);

                }
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} SubscribeManager=> MultiChannelSubscribeInit \n channel(s)={1} \n cg(s)={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", validChannels.OrderBy(x => x).ToArray()), string.Join(",", validChannelGroups.OrderBy(x => x).ToArray()), ex), config.LogVerbosity);
            }

            return ret;
        }

        private async Task<Tuple<string,PNStatus>> Stateless_MultiChannelSubscribeRequest<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken, int region, bool reconnect, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam, CancellationToken cancellationToken)
        {
            string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
            string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            // Begin recursive subscribe
            RequestState<T> pubnubRequestState = null;
            try
            {
                this.customQueryParam = externalQueryParam;
                //RegisterPresenceHeartbeatTimer<T>(channels, channelGroups); REMOVED FOR STATELESS

                long lastTimetoken = 0;
                long minimumTimetoken1 = 0;
                long minimumTimetoken2 = 0;
                long minimumTimetoken = Math.Max(minimumTimetoken1, minimumTimetoken2);

                long maximumTimetoken1 = 0;
                long maximumTimetoken2 = 0;
                long maximumTimetoken = Math.Max(maximumTimetoken1, maximumTimetoken2);


                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0}, Building request for channel(s)={1}, channelgroup(s)={2} with timetoken={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), multiChannel, multiChannelGroup, lastTimetoken), config.LogVerbosity);
                // Build URL
                string channelsJsonState = "";// BuildJsonUserState(channels, channelGroups, false);
                config.Uuid = CurrentUuid; // to make sure we capture if UUID is changed
                IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

                Uri request = urlBuilder.BuildMultiChannelSubscribeRequest("GET", "", channels, channelGroups, (Convert.ToInt64(timetoken.ToString()) == 0) ? Convert.ToInt64(timetoken.ToString()) : lastTimetoken, region, channelsJsonState, initialSubscribeUrlParams, externalQueryParam);

                pubnubRequestState = new RequestState<T>();
                pubnubRequestState.Channels = channels;
                pubnubRequestState.ChannelGroups = channelGroups;
                pubnubRequestState.ResponseType = type;
                pubnubRequestState.Reconnect = reconnect;
                pubnubRequestState.Timetoken = Convert.ToInt64(timetoken.ToString());
                pubnubRequestState.Region = region;

                // Wait for message
                return await UrlProcessRequest<T>(request, pubnubRequestState, false, cancellationToken);
            }
            catch (Exception ex)
            {
                LoggingMethod.WriteToLog(pubnubLog, string.Format("DateTime {0} method:_subscribe \n channel={1} \n timetoken={2} \n Exception Details={3}", DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Join(",", channels.OrderBy(x => x).ToArray()), timetoken, ex), config.LogVerbosity);

                PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
                PNStatus status = new StatusBuilder(config, jsonLibrary).CreateStatusResponse<T>(type, errorCategory, pubnubRequestState, (int)HttpStatusCode.NotFound, new PNException(ex));
                if (channels != null && channels.Length > 0)
                {
                    status.AffectedChannels.AddRange(channels);
                }

                if (channelGroups != null && channelGroups.Length > 0)
                {
                    status.AffectedChannels.AddRange(channelGroups);
                }

                return new Tuple<string, PNStatus>(null, status);
            }

        }

        private async Task<Tuple<string, PNStatus>> Stateless_PresenceHeartbeat(PNOperationType type, string[] channels, string[] channelGroups, CancellationToken cancellationToken)
        {
            string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog, pubnubTelemetryMgr, (PubnubInstance != null && !string.IsNullOrEmpty(PubnubInstance.InstanceId) && PubnubTokenMgrCollection.ContainsKey(PubnubInstance.InstanceId)) ? PubnubTokenMgrCollection[PubnubInstance.InstanceId] : null, (PubnubInstance != null) ? PubnubInstance.InstanceId : "");

            Uri request = urlBuilder.BuildPresenceHeartbeatRequest("GET", "", channels, channelGroups, channelsJsonState);

            RequestState<PNHeartbeatResult> requestState = new RequestState<PNHeartbeatResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = PNOperationType.PNHeartbeatOperation;
            requestState.PubnubCallback = null;
            requestState.Reconnect = false;
            requestState.Response = null;

            return await UrlProcessRequest(request, requestState, false, cancellationToken);

        }


        internal void CurrentPubnubInstance(Pubnub instance)
        {
            PubnubInstance = instance;
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            DisposeInternal(true);
        }
        #endregion

    }
}
