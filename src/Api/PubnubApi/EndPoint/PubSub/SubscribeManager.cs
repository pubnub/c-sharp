using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Collections.Concurrent;

namespace PubnubApi.EndPoint
{
	internal class SubscribeManager : PubnubCoreBase, IDisposable
	{
		private static ConcurrentDictionary<string, PNConfiguration> config { get; } = new();
		private static IJsonPluggableLibrary jsonLibrary;
		private static IPubnubUnitTest unit;
		private static IPubnubLog pubnubLog;

		private static Timer SubscribeHeartbeatCheckTimer;
		private Timer multiplexExceptionTimer;
		private Dictionary<string, object> customQueryParam;

		public SubscribeManager(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TokenManager tokenManager, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, tokenManager, instance)
		{
			config.AddOrUpdate(instance.InstanceId, pubnubConfig, (k, o) => pubnubConfig);
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
		}


		internal void MultiChannelUnSubscribeAll<T>(PNOperationType type, Dictionary<string, object> externalQueryParam)
		{
			//Retrieve the current channels already subscribed previously and terminate them
			TerminateCurrentSubscriberRequest();
			string[] currentChannels = SubscriptionChannels.ContainsKey(PubnubInstance.InstanceId)? SubscriptionChannels[PubnubInstance.InstanceId].Keys?.ToArray()??[] : [];
			string[] currentChannelGroups = SubscriptionChannelGroups.ContainsKey(PubnubInstance.InstanceId)? SubscriptionChannelGroups[PubnubInstance.InstanceId].Keys?.ToArray()??[] : [];
			
			if (type == PNOperationType.PNUnsubscribeOperation && config.ContainsKey(PubnubInstance.InstanceId) && !config[PubnubInstance.InstanceId].SuppressLeaveEvents) {
				//just fire leave() event to REST API for safeguard
				var leaveRequestParameter = CreateLeaveRequestParameter(currentChannels, currentChannelGroups);
				var leaveTransportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: leaveRequestParameter, operationType: PNOperationType.Leave);
				PubnubInstance.transportMiddleware.Send(transportRequest: leaveTransportRequest).ContinueWith(t => {
					SubscriptionChannels[PubnubInstance.InstanceId]?.Clear();
					SubscriptionChannelGroups[PubnubInstance.InstanceId]?.Clear();
				});
			}
			SubscriptionChannels[PubnubInstance.InstanceId]?.Clear();
			SubscriptionChannelGroups[PubnubInstance.InstanceId]?.Clear();
			TerminateCurrentSubscriberRequest();
			TerminateReconnectTimer();
			TerminatePresenceHeartbeatTimer();
		}

		internal void MultiChannelUnSubscribeInit<T>(PNOperationType type, string channel, string channelGroup, Dictionary<string, object> externalQueryParam)
		{
			List<string> validChannels = new List<string>();
			List<string> validChannelGroups = new List<string>();

			try {
				this.customQueryParam = externalQueryParam;

				if (PubnubInstance == null) {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] PubnubInstance is null. exiting MultiChannelUnSubscribeInit",  config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
					return;
				}

				string[] rawChannels = (channel != null && channel.Trim().Length > 0) ? channel.Split(',') : new string[] { };
				string[] rawChannelGroups = (channelGroup != null && channelGroup.Trim().Length > 0) ? channelGroup.Split(',') : new string[] { };

				if (rawChannels.Length > 0) {
					for (int index = 0; index < rawChannels.Length; index++) {
						if (rawChannels[index].Trim().Length > 0) {
							string channelName = rawChannels[index].Trim();
							if (string.IsNullOrEmpty(channelName)) {
								continue;
							}

							if (config.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannels.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannels[PubnubInstance.InstanceId] != null && !SubscriptionChannels[PubnubInstance.InstanceId].ContainsKey(channelName)) {
								PNStatus status = new StatusBuilder(config[PubnubInstance.InstanceId], jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, null, Constants.ResourceNotFoundStatusCode, null);
								if (!status.AffectedChannels.Contains(channelName)) {
									status.AffectedChannels.Add(channelName);
								}
								Announce(status);
							} else {
								validChannels.Add(channelName);
								string presenceChannelName = string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", channelName);
								if (SubscriptionChannels.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannels[PubnubInstance.InstanceId] != null && SubscriptionChannels[PubnubInstance.InstanceId].ContainsKey(presenceChannelName)) {
									validChannels.Add(presenceChannelName);
								}
							}
						}
					}
				}

				if (rawChannelGroups.Length > 0) {
					for (int index = 0; index < rawChannelGroups.Length; index++) {
						if (rawChannelGroups[index].Trim().Length > 0) {
							string channelGroupName = rawChannelGroups[index].Trim();
							if (string.IsNullOrEmpty(channelGroupName)) {
								continue;
							}

							if (config.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannelGroups.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannelGroups[PubnubInstance.InstanceId] != null && !SubscriptionChannelGroups[PubnubInstance.InstanceId].ContainsKey(channelGroupName)) {
								PNStatus status = new StatusBuilder(config[PubnubInstance.InstanceId], jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNUnexpectedDisconnectCategory, null, Constants.ResourceNotFoundStatusCode, null);
								if (!status.AffectedChannelGroups.Contains(channelGroupName)) {
									status.AffectedChannelGroups.Add(channelGroupName);
								}
								Announce(status);
							} else {
								validChannelGroups.Add(channelGroupName);
								string presenceChannelGroupName = string.Format(CultureInfo.InvariantCulture, "{0}-pnpres", channelGroupName);
								if (SubscriptionChannelGroups.ContainsKey(PubnubInstance.InstanceId) && SubscriptionChannelGroups[PubnubInstance.InstanceId] != null && SubscriptionChannelGroups[PubnubInstance.InstanceId].ContainsKey(presenceChannelGroupName)) {
									validChannelGroups.Add(presenceChannelGroupName);
								}
							}
						}
					}
				}

				if (validChannels.Count > 0 || validChannelGroups.Count > 0) {
					TerminateCurrentSubscriberRequest();
					if (type == PNOperationType.PNUnsubscribeOperation && config.ContainsKey(PubnubInstance.InstanceId)) {
						var leaveRequestParameter = CreateLeaveRequestParameter(validChannels.ToArray(), validChannelGroups.ToArray());
						var leaveTransportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: leaveRequestParameter, operationType: PNOperationType.Leave);
						PubnubInstance.transportMiddleware.Send(transportRequest: leaveTransportRequest).ContinueWith(t => { });
					}

					PNStatus successStatus = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNUnsubscribeOperation, PNStatusCategory.PNDisconnectedCategory, null, Constants.HttpRequestSuccessStatusCode, null);

					//Remove the valid channels from subscribe list for unsubscribe 
					for (int index = 0; index < validChannels.Count; index++) {
						string channelToBeRemoved = validChannels[index];
						bool unsubscribeStatus = false;
						if (channelToBeRemoved.Contains("-pnpres")) {
							continue; //Do not send status for -pnpres channels
						}
						if (!successStatus.AffectedChannels.Contains(channelToBeRemoved)) {
							successStatus.AffectedChannels.Add(channelToBeRemoved);
						}
						base.DeleteLocalChannelUserState(channelToBeRemoved);
					}
					for (int index = 0; index < validChannelGroups.Count; index++) {
						string channelGroupToBeRemoved = validChannelGroups[index];
						if (channelGroupToBeRemoved.Contains("-pnpres")) {
							continue; //Do not send status for -pnpres channel-groups
						}
						if (!successStatus.AffectedChannelGroups.Contains(channelGroupToBeRemoved)) {
							successStatus.AffectedChannelGroups.Add(channelGroupToBeRemoved);
						}
						base.DeleteLocalChannelGroupUserState(channelGroupToBeRemoved);
					}
					if (PubnubInstance != null) {
						Announce(successStatus);
					}
					
					foreach (var channelToRemove in validChannels)
					{
						try
						{
							if (SubscriptionChannels[PubnubInstance.InstanceId].ContainsKey(channelToRemove))
								SubscriptionChannels[PubnubInstance.InstanceId].TryRemove(channelToRemove, out _);
						}
						catch { }
					}
					foreach (var groupToRemove in validChannelGroups)
					{
						try
						{
							if (SubscriptionChannelGroups[PubnubInstance.InstanceId].ContainsKey(groupToRemove))
								SubscriptionChannelGroups[PubnubInstance.InstanceId].TryRemove(groupToRemove, out _);
						}
						catch { }
					}
					
					var channelsToKeepSubscription = SubscriptionChannels[PubnubInstance.InstanceId].Keys.ToArray();
					var groupsToKeepSubscription = SubscriptionChannelGroups[PubnubInstance.InstanceId].Keys.ToArray();
					if (channelsToKeepSubscription.Length > 0 || groupsToKeepSubscription.Length > 0) {
						MultiChannelSubscribeRequest<T>(PNOperationType.PNSubscribeOperation, channelsToKeepSubscription, groupsToKeepSubscription, 0, 0, false, null, this.customQueryParam);
					} else {
						if (PresenceHeartbeatTimer != null) {
							PresenceHeartbeatTimer.Dispose();
							PresenceHeartbeatTimer = null;
						}
						LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] All channels are Unsubscribed. Further subscription was stopped", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
					}
				}
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: At SubscribeManager.MultiChannelUnSubscribeInit() \n channel(s)={string.Join(",", validChannels.OrderBy(x => x).ToArray())} \n cg(s)={string.Join(",", validChannelGroups.OrderBy(x => x).ToArray())} \n Exception Details={ex}", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
			}
		}

		internal void MultiChannelSubscribeInit<T>(PNOperationType responseType, string[] rawChannels, string[] rawChannelGroups, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
		{
			try {
				bool channelGroupSubscribeOnly = false;
				SubscribeDisconnected[PubnubInstance.InstanceId] = false;
				bool isSubscriptionChanged = false;
				if (!SubscriptionChannels.ContainsKey(PubnubInstance.InstanceId))
					SubscriptionChannels[PubnubInstance.InstanceId] = new();
				if (!SubscriptionChannelGroups.ContainsKey(PubnubInstance.InstanceId))
					SubscriptionChannelGroups[PubnubInstance.InstanceId] = new();
				foreach (string channel in rawChannels.Distinct())
				{
					try { isSubscriptionChanged |= SubscriptionChannels[PubnubInstance.InstanceId].TryAdd(channel.Trim(), true); } catch { isSubscriptionChanged = true; }
				}
				
				foreach (string group in rawChannelGroups.Distinct())
				{
					try {isSubscriptionChanged |= SubscriptionChannelGroups[PubnubInstance.InstanceId].TryAdd(group.Trim(),true);} catch{isSubscriptionChanged = true;}
				}
				if (isSubscriptionChanged && config.ContainsKey(PubnubInstance.InstanceId)) {
					//Retrieve the current channels already subscribed previously and terminate them
					string[] channels = SubscriptionChannels[PubnubInstance.InstanceId].Keys.ToArray();
					string[] channelGroups = SubscriptionChannelGroups[PubnubInstance.InstanceId].Keys.ToArray();
					TerminateCurrentSubscriberRequest();
					if (channelGroups != null && channelGroups.Length > 0 && (channels == null || channels.Length == 0)) {
						channelGroupSubscribeOnly = true;
					}
					LoggingMethod.WriteToLog(pubnubLog,$"MultiChannelSubscribeRequest with tt=0", PNLogVerbosity.BODY);
					MultiChannelSubscribeRequest<T>(responseType, channels, channelGroups, 0, 0, false, initialSubscribeUrlParams, externalQueryParam);

					if (SubscribeHeartbeatCheckTimer != null) {
						try {
							SubscribeHeartbeatCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
						} catch {  /* ignore */ }
					}
				}
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, $"subscribe initialisation opeartion encountered error {ex.Message}", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
			}
		}

		private void MultiChannelSubscribeRequest<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken, int region, bool reconnect, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
		{
			if (!config.ContainsKey(PubnubInstance.InstanceId)) {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] InstanceId Not Available. Exiting MultiChannelSubscribeRequest", PNLogVerbosity.BODY);
				return;
			}
			if (SubscribeDisconnected[PubnubInstance.InstanceId]) {
				LoggingMethod.WriteToLog(pubnubLog,$"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeDisconnected. Exiting MultiChannelSubscribeRequest", config[PubnubInstance.InstanceId].LogVerbosity);
				return;
			}
			//Exit if the channel is unsubscribed
			if (SubscriptionChannels[PubnubInstance.InstanceId].Count <= 0 && SubscriptionChannelGroups[PubnubInstance.InstanceId].Count <= 0) {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Zero channels/channelGroups. Further subscription was stopped", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
				return;
			}
			TerminateCurrentSubscriberRequest();
			string multiChannel = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
			string multiChannelGroup = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

			bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, type, null, channels, channelGroups);

			if (!networkConnection) {
				ConnectionErrors++;
				UpdatePubnubNetworkTcpCheckIntervalInSeconds();
			}
			
			if (!networkConnection) {
				PNStatusCategory errorCategory = PNStatusCategory.PNNetworkIssuesCategory;
				PNStatus status = new StatusBuilder(config[PubnubInstance.InstanceId], jsonLibrary).CreateStatusResponse<T>(type, errorCategory, null, Constants.ResourceNotFoundStatusCode, new PNException("SDK Network related error"));
				if (channels != null && channels.Length > 0) {
					status.AffectedChannels.AddRange(channels);
				}
				if (channelGroups != null && channelGroups.Length > 0) {
					status.AffectedChannels.AddRange(channelGroups);
				}
				Announce(status);
			}
			
			// Begin recursive subscribe
			RequestState<T> pubnubRequestState = null;
			try {
				this.customQueryParam = externalQueryParam;
				RegisterPresenceHeartbeatTimer<T>(channels, channelGroups);
				long lastTimetoken = LastSubscribeTimetoken.ContainsKey(PubnubInstance.InstanceId)? LastSubscribeTimetoken[PubnubInstance.InstanceId] : 0;
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Building request for channel(s)={multiChannel}, channelgroup(s)={multiChannelGroup} with timetoken={lastTimetoken}", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
				string channelsJsonState = BuildJsonUserState(channels, channelGroups, false);
				config[PubnubInstance.InstanceId].UserId = CurrentUserId[PubnubInstance.InstanceId]; // to make sure we capture if UUID is changed
				pubnubRequestState = new RequestState<T>
				{
					Channels = channels,
					ChannelGroups = channelGroups,
					ResponseType = type,
					Reconnect = reconnect,
					Timetoken = Convert.ToInt64(timetoken.ToString(), CultureInfo.InvariantCulture),
					Region = region,
					TimeQueued = DateTime.Now
				};
				var subscribeRequestParameter = CreateSubscribeRequestParameter(channels: channels, channelGroups: channelGroups,timetoken: (Convert.ToInt64(timetoken.ToString(), CultureInfo.InvariantCulture) == 0) ? Convert.ToInt64(timetoken.ToString(), CultureInfo.InvariantCulture) : lastTimetoken,region: region,stateJsonValue: channelsJsonState, initialSubscribeUrlParams: initialSubscribeUrlParams, externalQueryParam: externalQueryParam);
				var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: subscribeRequestParameter, operationType: PNOperationType.PNSubscribeOperation);
				OngoingSubscriptionCancellationTokenSources[PubnubInstance.InstanceId] =
					transportRequest.CancellationTokenSource;
				PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith( t => {
					var transportResponse = t.Result;
					if (transportResponse.Error == null) {
						var json = Encoding.UTF8.GetString(transportResponse.Content);
						pubnubRequestState.GotJsonResponse = true;
						if (!string.IsNullOrEmpty(json)) {
								List<object> result = ProcessJsonResponse<T>(pubnubRequestState, json);
								LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] result count of ProcessJsonResponse = {result?.Count ?? -1}", config[PubnubInstance.InstanceId].LogVerbosity);

								ProcessResponseCallbacks<T>(result, pubnubRequestState);

								if ((pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation || pubnubRequestState.ResponseType == PNOperationType.Presence) && (result != null) && (result.Count > 0)) {
									long jsonTimetoken = GetTimetokenFromMultiplexResult(result);
									LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] jsonTimetoken = {jsonTimetoken}", config[PubnubInstance.InstanceId].LogVerbosity);
								}
								if (pubnubRequestState.ResponseType == PNOperationType.PNSubscribeOperation) {
									MultiplexInternalCallback<T>(pubnubRequestState.ResponseType, result);
								}
						} 
					} else {
						LoggingMethod.WriteToLog(pubnubLog,$"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: Exception from TransportLayer\n\n transportResponse.Error.Message => {transportResponse.Error.Message} \n\n\n  inner{transportResponse.Error.InnerException?.Message}", config[PubnubInstance.InstanceId].LogVerbosity);
						if (multiplexExceptionTimer != null) {
							multiplexExceptionTimer.Change(Timeout.Infinite, Timeout.Infinite);
						}
						ConnectionErrors++;
						UpdatePubnubNetworkTcpCheckIntervalInSeconds();
						multiplexExceptionTimer = new Timer(new TimerCallback(MultiplexExceptionHandlerTimerCallback<T>), pubnubRequestState,
							(-1 == PubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : PubnubNetworkTcpCheckIntervalInSeconds * 1000,
							Timeout.Infinite);
					}
				});
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] method:_subscribe \n channel={string.Join(",", channels.OrderBy(x => x).ToArray())} \n timetoken={timetoken} \n Exception Details={ex}", config[PubnubInstance.InstanceId].LogVerbosity);
				PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
				PNStatus status = new StatusBuilder(config[PubnubInstance.InstanceId], jsonLibrary).CreateStatusResponse<T>(type, errorCategory, pubnubRequestState, Constants.ResourceNotFoundStatusCode, new PNException(ex));
				if (channels != null && channels.Length > 0) {
					status.AffectedChannels.AddRange(channels);
				}
				if (channelGroups != null && channelGroups.Length > 0) {
					status.AffectedChannels.AddRange(channelGroups);
				}
				Announce(status);
				MultiChannelSubscribeRequest<T>(type, channels, channelGroups, LastSubscribeTimetoken[PubnubInstance.InstanceId], LastSubscribeRegion[PubnubInstance.InstanceId], false, null, externalQueryParam);
			}
		}

		private void MultiplexExceptionHandlerTimerCallback<T>(object state)
		{
			LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] MultiplexExceptionHandlerTimerCallback", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
			RequestState<T> currentState = state as RequestState<T>;
			if (currentState != null) {
				MultiplexExceptionHandler<T>(currentState.ResponseType, currentState.Channels, currentState.ChannelGroups, false);
			}
		}

		private void MultiplexExceptionHandler<T>(PNOperationType type, string[] channels, string[] channelGroups, bool resumeOnReconnect)
		{
			List<object> result = new List<object>();
			result.Add("0");
			if (resumeOnReconnect || LastSubscribeTimetoken == null || !LastSubscribeTimetoken.ContainsKey(PubnubInstance.InstanceId)) {
				result.Add(0); //send 0 time token to enable presence event
			} else {
				long lastTT = LastSubscribeTimetoken[PubnubInstance.InstanceId]; //get last timetoken of the current instance
				int lastRegionId = (LastSubscribeRegion != null && LastSubscribeRegion.ContainsKey(PubnubInstance.InstanceId)) ? LastSubscribeRegion[PubnubInstance.InstanceId] : 0; //get last region of the current instance

				Dictionary<string, object> dictTimetokenAndRegion = new Dictionary<string, object>();
				dictTimetokenAndRegion.Add("t", lastTT);
				dictTimetokenAndRegion.Add("r", lastRegionId);

				Dictionary<string, object> dictEnvelope = new Dictionary<string, object>();
				dictEnvelope.Add("t", dictTimetokenAndRegion);
				result.Add(dictEnvelope);
			}

			if (channelGroups != null && channelGroups.Length > 0) {
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
			if (message != null && message.Count >= 3) {
				if (message[message.Count - 1] is string[]) {
					channels = message[message.Count - 1] as string[];
				} else {
					channels = message[message.Count - 1].ToString().Split(',');
				}

				if (channels.Length == 1 && channels[0] == "") {
					channels = new string[] { };
				}
				if (message.Count >= 4) {
					if (message[message.Count - 2] is string[]) {
						channelGroups = message[message.Count - 2] as string[];
					} else if (message[message.Count - 2].ToString() != "") {
						channelGroups = message[message.Count - 2].ToString().Split(',');
					}
				}

				long timetoken = GetTimetokenFromMultiplexResult(message);
				int region = GetRegionFromMultiplexResult(message);
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] MultiplexInternalCallback timetoken = {timetoken}; region = {region}", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Calling MultiChannelSubscribeRequest tt {timetoken}",PNLogVerbosity.BODY);
				MultiChannelSubscribeRequest<T>(type, channels, channelGroups, timetoken, region, false, null, this.customQueryParam);
			} else {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Error: Lost Channel Name for resubscribe", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
			}
		}

		private bool ReconnectNetworkIfOverrideTcpKeepAlive<T>(PNOperationType type, string[] channels, string[] channelGroups, object timetoken, int region, bool networkAvailable)
		{
			if (OverrideTcpKeepAlive) {
				ReconnectState<T> netState = new ReconnectState<T>();
				netState.Channels = channels;
				netState.ChannelGroups = channelGroups;
				netState.ResponseType = type;
				netState.Timetoken = timetoken;
				netState.Region = region;
				if (!config.ContainsKey(PubnubInstance.InstanceId)) {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}], InstanceId Not Available. So no reconnect", PNLogVerbosity.BODY);
				}

				if (SubscribeDisconnected[PubnubInstance.InstanceId]) {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Subscribe is still Disconnected. So no reconnect", config[PubnubInstance.InstanceId].LogVerbosity);
				} else if (config[PubnubInstance.InstanceId].ReconnectionPolicy != PNReconnectionPolicy.NONE) {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Subscribe - No internet connection for channel={string.Join(",", channels.OrderBy(x => x).ToArray())} and channelgroup={(channelGroups != null ? string.Join(",", channelGroups) : "")}; networkAvailable={networkAvailable}", config[PubnubInstance.InstanceId].LogVerbosity);
					TerminateReconnectTimer();
					ReconnectNetwork<T>(netState);
				} else {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] reconnection policy is DISABLED, please handle reconnection manually.", config[PubnubInstance.InstanceId].LogVerbosity);
					if (!networkAvailable) {
						PNStatusCategory errorCategory = PNStatusCategory.PNNetworkIssuesCategory;
						PNStatus status = new StatusBuilder(config[PubnubInstance.InstanceId], jsonLibrary).CreateStatusResponse<T>(type, errorCategory, null, Constants.ResourceNotFoundStatusCode, new PNException("SDK Network related error"));
						if (channels != null && channels.Length > 0) {
							status.AffectedChannels.AddRange(channels);
						}
						if (channelGroups != null && channelGroups.Length > 0) {
							status.AffectedChannels.AddRange(channelGroups);
						}
						Announce(status);

					}
				}
				return true;
			} else {
				return false;
			}
		}

		private void ReconnectNetwork<T>(ReconnectState<T> netState)
		{
			if (netState != null && ((netState.Channels != null && netState.Channels.Length > 0) || (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0))) {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager ReconnectNetwork interval = {PubnubNetworkTcpCheckIntervalInSeconds} sec", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);

				System.Threading.Timer timer;

				if (netState.Channels != null && netState.Channels.Length > 0) {
					string reconnectChannelTimerKey = string.Join(",", netState.Channels.OrderBy(x => x).ToArray());

					if (!ChannelReconnectTimer[PubnubInstance.InstanceId].ContainsKey(reconnectChannelTimerKey)) {
						timer = new Timer(new TimerCallback(ReconnectNetworkCallback<T>), netState, 0,
															  (-1 == PubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : PubnubNetworkTcpCheckIntervalInSeconds * 1000);
						ChannelReconnectTimer[PubnubInstance.InstanceId].AddOrUpdate(reconnectChannelTimerKey, timer, (key, oldState) => timer);
					}
				} else if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
					string reconnectChannelGroupTimerKey = string.Join(",", netState.ChannelGroups.OrderBy(x => x).ToArray());

					if (!ChannelGroupReconnectTimer[PubnubInstance.InstanceId].ContainsKey(reconnectChannelGroupTimerKey)) {
						timer = new Timer(new TimerCallback(ReconnectNetworkCallback<T>), netState, 0,
															  (-1 == PubnubNetworkTcpCheckIntervalInSeconds) ? Timeout.Infinite : PubnubNetworkTcpCheckIntervalInSeconds * 1000);
						ChannelGroupReconnectTimer[PubnubInstance.InstanceId].AddOrUpdate(reconnectChannelGroupTimerKey, timer, (key, oldState) => timer);
					}
				}
			}
		}

		internal bool Reconnect<T>(bool resetSubscribeTimetoken)
		{
			if (!SubscribeDisconnected[PubnubInstance.InstanceId]) //Check if disconnect is done before
			{
				return false;
			}

			string[] channels = GetCurrentSubscriberChannels();
			string[] channelGroups = GetCurrentSubscriberChannelGroups();

			if ((channels != null && channels.Length > 0) || (channelGroups != null && channelGroups.Length > 0)) {
				bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, PNOperationType.PNSubscribeOperation, null, channels, channelGroups);
				if (!networkConnection) {
					networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, PNOperationType.PNSubscribeOperation, null, channels, channelGroups);
				}
				if (!networkConnection)  {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] No network for SubscribeManager Manual Reconnect", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);

					PNStatusCategory errorCategory = PNStatusCategory.PNNetworkIssuesCategory;
					PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNSubscribeOperation, errorCategory, null, Constants.ResourceNotFoundStatusCode, new PNException("SDK Network related error"));
					if (channels != null && channels.Length > 0) {
						status.AffectedChannels.AddRange(channels);
					}
					if (channelGroups != null && channelGroups.Length > 0) {
						status.AffectedChannels.AddRange(channelGroups);
					}
					Announce(status);

					return false;
				}
			} else {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] No channels/channelgroups for SubscribeManager Manual Reconnect", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
				return false;
			}


			LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager Manual Reconnect", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
			SubscribeDisconnected[PubnubInstance.InstanceId] = false;

			Task.Factory.StartNew(() => {
				if (resetSubscribeTimetoken) {
					LastSubscribeTimetoken[PubnubInstance.InstanceId] = 0;
					LastSubscribeRegion[PubnubInstance.InstanceId] = 0;
				}
				MultiChannelSubscribeRequest<T>(PNOperationType.PNSubscribeOperation, GetCurrentSubscriberChannels(), GetCurrentSubscriberChannelGroups(), LastSubscribeTimetoken[PubnubInstance.InstanceId], LastSubscribeRegion[PubnubInstance.InstanceId], false, null, this.customQueryParam);
			}, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);

			return true;
		}

		internal bool Disconnect()
		{
			if (SubscribeDisconnected[PubnubInstance.InstanceId]) {
				return false;
			}
			LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager Manual Disconnect", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
			SubscribeDisconnected[PubnubInstance.InstanceId] = true;
			TerminateCurrentSubscriberRequest();
			TerminatePresenceHeartbeatTimer();
			TerminateReconnectTimer();

			return true;
		}

		internal void StartSubscribeHeartbeatCheckCallback<T>(object state)
		{
			try {
				if (SubscribeDisconnected[PubnubInstance.InstanceId]) {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager - SubscribeDisconnected. No heartbeat check.", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
					return;
				}
				if (!config.ContainsKey(PubnubInstance.InstanceId)) {
					LoggingMethod.WriteToLog(pubnubLog,$"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] InstanceId Not Available. So No heartbeat check.", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
					return;
				}

				string[] channels = GetCurrentSubscriberChannels();
				string[] chananelGroups = GetCurrentSubscriberChannelGroups();

				if ((channels != null && channels.Length > 0) || (chananelGroups != null && chananelGroups.Length > 0)) {
					bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, PNOperationType.PNSubscribeOperation, null, channels, chananelGroups);
					if (networkConnection && PubnubInstance != null && SubscribeRequestTracker.ContainsKey(PubnubInstance.InstanceId)) {
						DateTime lastSubscribeRequestTime = SubscribeRequestTracker[PubnubInstance.InstanceId];
						if ((DateTime.Now - lastSubscribeRequestTime).TotalSeconds < config[PubnubInstance.InstanceId].SubscribeTimeout) {
							LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager: expected subscribe within threshold limit of SubscribeTimeout", config[PubnubInstance.InstanceId].LogVerbosity);
						} else if ((DateTime.Now - lastSubscribeRequestTime).TotalSeconds > 2 * (config[PubnubInstance.InstanceId].SubscribeTimeout - config[PubnubInstance.InstanceId].SubscribeTimeout / 2)) {
							LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager - **No auto subscribe within threshold limit of SubscribeTimeout**. Calling MultiChannelSubscribeRequest", config[PubnubInstance.InstanceId].LogVerbosity);
							Task.Factory.StartNew(() => {
								TerminateCurrentSubscriberRequest();
								MultiChannelSubscribeRequest<T>(PNOperationType.PNSubscribeOperation, channels, chananelGroups, LastSubscribeTimetoken[PubnubInstance.InstanceId], LastSubscribeRegion[PubnubInstance.InstanceId], false, null, this.customQueryParam);
							}, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
						} else {
							LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager - **No auto subscribe within threshold limit of SubscribeTimeout**. Calling TerminateCurrentSubscriberRequest", config[PubnubInstance.InstanceId].LogVerbosity);
							Task.Factory.StartNew(() => {
								TerminateCurrentSubscriberRequest();
							}, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default).ConfigureAwait(false);
						}
					} else {
						LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager - StartSubscribeHeartbeatCheckCallback - No network or no pubnub instance mapping", config[PubnubInstance.InstanceId].LogVerbosity);
						if (PubnubInstance != null && !networkConnection) {
							PNStatus status = new StatusBuilder(config[PubnubInstance.InstanceId], jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNSubscribeOperation, PNStatusCategory.PNNetworkIssuesCategory, null, (int)System.Net.HttpStatusCode.NotFound, new PNException("Internet connection problem during subscribe heartbeat."));
							if (channels != null && channels.Length > 0) {
								status.AffectedChannels.AddRange(channels.ToList());
							}
							if (chananelGroups != null && chananelGroups.Length > 0) {
								status.AffectedChannelGroups.AddRange(chananelGroups.ToList());
							}
							Announce(status);
							
							TerminateCurrentSubscriberRequest();
							MultiChannelSubscribeRequest<T>(PNOperationType.PNSubscribeOperation, GetCurrentSubscriberChannels(), GetCurrentSubscriberChannelGroups(), LastSubscribeTimetoken[PubnubInstance.InstanceId], LastSubscribeRegion[PubnubInstance.InstanceId], false, null, this.customQueryParam);
							
						}
					}
				} else {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager - StartSubscribeHeartbeatCheckCallback - No channels/cgs avaialable", config[PubnubInstance.InstanceId].LogVerbosity);
					try {
						SubscribeHeartbeatCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
						TerminateCurrentSubscriberRequest();
					} catch {  /* ignore */ }
				}
			} catch (Exception ex) {
				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] SubscribeManager - StartSubscribeHeartbeatCheckCallback - EXCEPTION: {ex}", config[PubnubInstance.InstanceId].LogVerbosity);
			}
		}


		protected void ReconnectNetworkCallback<T>(System.Object reconnectState)
		{
			string channel = "";
			string channelGroup = "";

			ReconnectState<T> netState = reconnectState as ReconnectState<T>;
			try {
				string subscribedChannels = (SubscriptionChannels[PubnubInstance.InstanceId].Count > 0) ? SubscriptionChannels[PubnubInstance.InstanceId].Keys.OrderBy(x => x).Aggregate((x, y) => x + "," + y) : "";
				string subscribedChannelGroups = (SubscriptionChannelGroups[PubnubInstance.InstanceId].Count > 0) ? SubscriptionChannelGroups[PubnubInstance.InstanceId].Keys.OrderBy(x => x).Aggregate((x, y) => x + "," + y) : "";


				if (netState != null && ((netState.Channels != null && netState.Channels.Length > 0) || (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0))) {
					if (netState.Channels == null) {
						netState.Channels = new string[] { };
					}

					if (netState.ChannelGroups == null) {
						netState.ChannelGroups = new string[] { };
					}

					bool channelInternetFlag;
					bool channelGroupInternetFlag;
					if (netState.Channels != null && netState.Channels.Length > 0) {
						channel = (netState.Channels.Length > 0) ? string.Join(",", netState.Channels.OrderBy(x => x).ToArray()) : ",";
						channelGroup = (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) ? string.Join(",", netState.ChannelGroups.OrderBy(x => x).ToArray()) : "";

						if (netState.ResponseType == PNOperationType.PNSubscribeOperation || netState.ResponseType == PNOperationType.Presence) {
							bool networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
							if (networkConnection) {
								//Re-try to avoid false alert
								networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
							}
							if(!networkConnection){
								ConnectionErrors++;
								UpdatePubnubNetworkTcpCheckIntervalInSeconds();
								LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] channel={channel} {netState.ResponseType} reconnectNetworkCallback. Retry", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
								if (netState.Channels != null && netState.Channels.Length > 0) {
									PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNNetworkIssuesCategory, null, (int)System.Net.HttpStatusCode.NotFound, new PNException("Internet connection problem. Retrying connection"));
									if (netState.Channels != null && netState.Channels.Length > 0) {
										status.AffectedChannels.AddRange(netState.Channels.ToList());
									}
									if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
										status.AffectedChannelGroups.AddRange(netState.ChannelGroups.ToList());
									}
									Announce(status);
								}
							}
						}

						if (ChannelInternetStatus[PubnubInstance.InstanceId].ContainsKey(channel) && ChannelInternetStatus[PubnubInstance.InstanceId].TryGetValue(channel, out channelInternetFlag) && channelInternetFlag) {
							if (ChannelReconnectTimer[PubnubInstance.InstanceId].ContainsKey(channel)) {
								LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}], {channel} {netState.ResponseType} terminating ch reconnectimer", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
								TerminateReconnectTimer();
							}

							PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.OK, null);
							if (netState.Channels != null && netState.Channels.Length > 0) {
								status.AffectedChannels.AddRange(netState.Channels.ToList());
							}
							if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
								status.AffectedChannelGroups.AddRange(netState.ChannelGroups.ToList());
							}
							Announce(status);

							LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] channel={channel} {netState.ResponseType} reconnectNetworkCallback. Internet Available : {channelInternetFlag}", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
							switch (netState.ResponseType) {
								case PNOperationType.PNSubscribeOperation:
								case PNOperationType.Presence:
									MultiChannelSubscribeRequest<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.Timetoken, netState.Region, true, null, this.customQueryParam);
									break;
								default:
									break;
							}
						}
					} else if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
						channelGroup = string.Join(",", netState.ChannelGroups.OrderBy(x => x).ToArray());
						channel = (netState.Channels != null && netState.Channels.Length > 0) ? string.Join(",", netState.Channels.OrderBy(x => x).ToArray()) : ",";

						if (subscribedChannelGroups == channelGroup && channelGroup != "" && ChannelGroupInternetStatus[PubnubInstance.InstanceId].ContainsKey(channelGroup)
							&& (netState.ResponseType == PNOperationType.PNSubscribeOperation || netState.ResponseType == PNOperationType.Presence)) {
							bool networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
							if (networkConnection) {
								//Re-try to avoid false alert
								networkConnection = CheckInternetConnectionStatus(PubnetSystemActive, netState.ResponseType, netState.PubnubCallback, netState.Channels, netState.ChannelGroups);
							}

							if (ChannelGroupInternetStatus[PubnubInstance.InstanceId].TryGetValue(channelGroup, out channelGroupInternetFlag) && channelGroupInternetFlag) {
								//do nothing
							} else {
								ChannelGroupInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(channelGroup, networkConnection, (key, oldValue) => networkConnection);
								if (!string.IsNullOrEmpty(channel) && channel.Length > 0) {
									ChannelInternetStatus[PubnubInstance.InstanceId].AddOrUpdate(channel, networkConnection, (key, oldValue) => networkConnection);
								}

								LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] channelgroup={channelGroup} {netState.ResponseType} reconnectNetworkCallback. Retrying",config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);

								if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
									PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.NotFound, new PNException("Internet connection problem. Retrying connection"));
									if (netState.Channels != null && netState.Channels.Length > 0) {
										status.AffectedChannels.AddRange(netState.Channels.ToList());
									}
									if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
										status.AffectedChannelGroups.AddRange(netState.ChannelGroups.ToList());
									}
									Announce(status);
								}
							}
						}

						if (ChannelGroupInternetStatus[PubnubInstance.InstanceId].TryGetValue(channelGroup, out channelGroupInternetFlag) && channelGroupInternetFlag) {
							if (ChannelGroupReconnectTimer[PubnubInstance.InstanceId].ContainsKey(channelGroup)) {
								LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] {channelGroup} {netState.ResponseType} terminating cg reconnectimer", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
								TerminateReconnectTimer();
							}

							//Send one ReConnectedCategory message. If Channels NOT available then use this
							if (netState.Channels.Length == 0 && netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
								PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, PNStatusCategory.PNReconnectedCategory, null, (int)System.Net.HttpStatusCode.OK, null);
								if (netState.Channels != null && netState.Channels.Length > 0) {
									status.AffectedChannels.AddRange(netState.Channels.ToList());
								}
								if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
									status.AffectedChannelGroups.AddRange(netState.ChannelGroups.ToList());
								}
								Announce(status);
							}

							LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] channelgroup={channelGroup} {netState.ResponseType} reconnectNetworkCallback. Internet Available", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
							switch (netState.ResponseType) {
								case PNOperationType.PNSubscribeOperation:
								case PNOperationType.Presence:
									MultiChannelSubscribeRequest<T>(netState.ResponseType, netState.Channels, netState.ChannelGroups, netState.Timetoken, netState.Region, true, null, this.customQueryParam);
									break;
								default:
									break;
							}
						}
					}
				} else {
					LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] Unknown request state in reconnectNetworkCallback", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
				}
			} catch (Exception ex) {
				if (netState != null) {
					PNStatusCategory errorCategory = PNStatusCategoryHelper.GetPNStatusCategory(ex);
					PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(netState.ResponseType, errorCategory, null, Constants.ResourceNotFoundStatusCode, new PNException(ex));
					if (netState.Channels != null && netState.Channels.Length > 0) {
						status.AffectedChannels.AddRange(netState.Channels.ToList());
					}
					if (netState.ChannelGroups != null && netState.ChannelGroups.Length > 0) {
						status.AffectedChannels.AddRange(netState.ChannelGroups.ToList());
					}
					Announce(status);
				}

				LoggingMethod.WriteToLog(pubnubLog, $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] method:reconnectNetworkCallback \n Exception Details={1}", config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].LogVerbosity : PNLogVerbosity.NONE);
			}
		}

		private void RegisterPresenceHeartbeatTimer<T>(string[] channels, string[] channelGroups)
		{
			if (PresenceHeartbeatTimer != null) {
				try {
					PresenceHeartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
					PresenceHeartbeatTimer.Dispose();
					PresenceHeartbeatTimer = null;
				} catch {  /* ignore */ }
			}
			if ((channels != null && channels.Length > 0 && channels.Where(s => s != null && s.Contains("-pnpres") == false).Any())
				|| (channelGroups != null && channelGroups.Length > 0 && channelGroups.Where(s => s != null && s.Contains("-pnpres") == false).Any())) {
				RequestState<T> presenceHeartbeatState = new RequestState<T>();
				presenceHeartbeatState.Channels = channels;
				presenceHeartbeatState.ChannelGroups = channelGroups;
				presenceHeartbeatState.ResponseType = PNOperationType.PNHeartbeatOperation;
				presenceHeartbeatState.RequestCancellationTokenSource = null;
				presenceHeartbeatState.Response = null;

				if (config.ContainsKey(PubnubInstance.InstanceId) && config[PubnubInstance.InstanceId].PresenceInterval > 0) {
					PresenceHeartbeatTimer = new Timer(OnPresenceHeartbeatIntervalTimeout<T>, presenceHeartbeatState, config[PubnubInstance.InstanceId].PresenceInterval * 1000, config[PubnubInstance.InstanceId].PresenceInterval * 1000);
				}
			}
		}
		
		void OnPresenceHeartbeatIntervalTimeout<T>(System.Object presenceHeartbeatState)
		{
			//Make presence heartbeat call
			RequestState<T> currentState = presenceHeartbeatState as RequestState<T>;
			if (currentState != null) {
				string[] subscriberChannels = (currentState.Channels != null) ? currentState.Channels.Where(s => s.Contains("-pnpres") == false).ToArray() : null;
				string[] subscriberChannelGroups = (currentState.ChannelGroups != null) ? currentState.ChannelGroups.Where(s => s.Contains("-pnpres") == false).ToArray() : null;

				bool networkConnection = CheckInternetConnectionStatus<T>(PubnetSystemActive, currentState.ResponseType, currentState.PubnubCallback, currentState.Channels, currentState.ChannelGroups);
				if (networkConnection) {
					if ((subscriberChannels != null && subscriberChannels.Length > 0) || (subscriberChannelGroups != null && subscriberChannelGroups.Length > 0)) {
						RequestState<PNHeartbeatResult> requestState = new RequestState<PNHeartbeatResult>();
						requestState.Channels = currentState.Channels;
						requestState.ChannelGroups = currentState.ChannelGroups;
						requestState.ResponseType = PNOperationType.PNHeartbeatOperation;
						requestState.PubnubCallback = null;
						requestState.Reconnect = false;
						requestState.Response = null;
						requestState.TimeQueued = DateTime.Now;
						var heartbeatRequestParameter = CreateHeartbeatRequestParameter(subscriberChannels, subscriberChannelGroups);
						var transportRequest = PubnubInstance.transportMiddleware.PreapareTransportRequest(requestParameter: heartbeatRequestParameter, operationType: PNOperationType.PNHeartbeatOperation);
						PNStatus responseStatus;
						PubnubInstance.transportMiddleware.Send(transportRequest: transportRequest).ContinueWith(t => {
							var transportResponse = t.Result;
							if (transportResponse.Error == null) {
								var responseString = Encoding.UTF8.GetString(transportResponse.Content);
								PNStatus errorStatus = GetStatusIfError(requestState, responseString);
								if (errorStatus == null)
								{
									requestState.GotJsonResponse = true;
									List<object> result = ProcessJsonResponse(requestState, responseString);
									responseStatus = new StatusBuilder(config[PubnubInstance.InstanceId], jsonLibrary).CreateStatusResponse(requestState.ResponseType, PNStatusCategory.PNAcknowledgmentCategory, requestState, 200, null);
									ProcessResponseCallbacks(result, requestState);
								} else {
									responseStatus = errorStatus;
									ProcessResponseCallbacks(default, requestState);
								}
							}
						});
					}
				} else {
					if (PubnubInstance != null && !networkConnection) {
						PNStatus status = new StatusBuilder(config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId] : null, jsonLibrary).CreateStatusResponse<T>(PNOperationType.PNSubscribeOperation, PNStatusCategory.PNNetworkIssuesCategory, null, (int)System.Net.HttpStatusCode.NotFound, new PNException("Internet connection problem during presence heartbeat."));
						if (subscriberChannels != null && subscriberChannels.Length > 0) {
							status.AffectedChannels.AddRange(subscriberChannels.ToList());
						}
						if (subscriberChannelGroups != null && subscriberChannelGroups.Length > 0) {
							status.AffectedChannelGroups.AddRange(subscriberChannelGroups.ToList());
						}
						Announce(status);
					}
				}
			}
		}

		internal void CurrentPubnubInstance(Pubnub instance)
		{
			PubnubInstance = instance;
		}

		private RequestParameter CreateLeaveRequestParameter(string[] channels, string[] channelGroups)
		{
			string channleString = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
			List<string> pathSegments = new List<string>
			{
				"v2",
				"presence",
				"sub_key",
				config[PubnubInstance.InstanceId].SubscribeKey,
				"channel",
				channleString,
				"leave"
			};

			var requestQueryStringParams = new Dictionary<string, string>();

			if (channelGroups != null && channelGroups.Length > 0) {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), PNOperationType.Leave, false, false, false));
			}
			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}

		private RequestParameter CreateHeartbeatRequestParameter(string[] channels, string[] channelGroups)
		{
			string channelString = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
			List<string> pathSegments = new List<string>
			{
				"v2",
				"presence",
				"sub_key",
				config[PubnubInstance.InstanceId].SubscribeKey,
				"channel",
				channelString,
				"heartbeat"
			};
			string presenceState = string.Empty;

			presenceState = BuildJsonUserState(channels, channelGroups, false);
			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>();

			string channelsJsonState = presenceState;
			if (channelsJsonState != "{}" && channelsJsonState != string.Empty) {
				requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(channelsJsonState, PNOperationType.PNHeartbeatOperation, false, false, false));
			}

			if (channelGroups != null && channelGroups.Length > 0) {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), PNOperationType.PNHeartbeatOperation, false, false, false));
			}

			if (config[PubnubInstance.InstanceId].PresenceTimeout != 0) {
				requestQueryStringParams.Add("heartbeat", config[PubnubInstance.InstanceId].PresenceTimeout.ToString(CultureInfo.InvariantCulture));
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams
			};
			return requestParameter;
		}

		private RequestParameter CreateSubscribeRequestParameter(string[] channels, string[] channelGroups, long timetoken, int region, string stateJsonValue, Dictionary<string, string> initialSubscribeUrlParams, Dictionary<string, object> externalQueryParam)
		{
			string channelsSegment = (channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : ",";
			List<string> pathSegments = new List<string>
			{
				"v2",
				"subscribe",
				config.ContainsKey(PubnubInstance.InstanceId) ? config[PubnubInstance.InstanceId].SubscribeKey : "",
				channelsSegment,
				"0"
			};

			Dictionary<string, string> internalInitialSubscribeUrlParams = new Dictionary<string, string>();
			if (initialSubscribeUrlParams != null) {
				internalInitialSubscribeUrlParams = initialSubscribeUrlParams;
			}

			Dictionary<string, string> requestQueryStringParams = new Dictionary<string, string>(internalInitialSubscribeUrlParams);

			if (!requestQueryStringParams.ContainsKey("filter-expr") && config.ContainsKey(PubnubInstance.InstanceId) && !string.IsNullOrEmpty(config[PubnubInstance.InstanceId].FilterExpression)) {
				requestQueryStringParams.Add("filter-expr", UriUtil.EncodeUriComponent(config[PubnubInstance.InstanceId].FilterExpression, PNOperationType.PNSubscribeOperation, false, false, false));
			}

			if (!requestQueryStringParams.ContainsKey("ee") && config.ContainsKey(PubnubInstance.InstanceId) && config[PubnubInstance.InstanceId].EnableEventEngine) {
				requestQueryStringParams.Add("ee", "");
			}

			if (!requestQueryStringParams.ContainsKey("tt")) {
				requestQueryStringParams.Add("tt", timetoken.ToString(CultureInfo.InvariantCulture));
			}

			if (!requestQueryStringParams.ContainsKey("tr") && region > 0) {
				requestQueryStringParams.Add("tr", region.ToString(CultureInfo.InvariantCulture));
			}

			if (config.ContainsKey(PubnubInstance.InstanceId) && config[PubnubInstance.InstanceId].PresenceTimeout != 0) {
				requestQueryStringParams.Add("heartbeat", config[PubnubInstance.InstanceId].PresenceTimeout.ToString(CultureInfo.InvariantCulture));
			}

			if (channelGroups != null && channelGroups.Length > 0 && channelGroups[0] != "") {
				requestQueryStringParams.Add("channel-group", UriUtil.EncodeUriComponent(string.Join(",", channelGroups.OrderBy(x => x).ToArray()), PNOperationType.PNSubscribeOperation, false, false, false));
			}

			if (stateJsonValue != "{}" && stateJsonValue != "") {
				requestQueryStringParams.Add("state", UriUtil.EncodeUriComponent(stateJsonValue, PNOperationType.PNSubscribeOperation, false, false, false));
			}

			if (externalQueryParam != null && externalQueryParam.Count > 0) {
				foreach (KeyValuePair<string, object> kvp in externalQueryParam) {
					if (!requestQueryStringParams.ContainsKey(kvp.Key)) {
						requestQueryStringParams.Add(kvp.Key, UriUtil.EncodeUriComponent(kvp.Value.ToString(), PNOperationType.PNSubscribeOperation, false, false, false));
					}
				}
			}

			var requestParameter = new RequestParameter() {
				RequestType = Constants.GET,
				PathSegment = pathSegments,
				Query = requestQueryStringParams,
			};
			return requestParameter;
		}
		
		
		private bool disposedValue;

		protected virtual void DisposeInternal(bool disposing)
		{
			if (!disposedValue) {
				if (SubscribeHeartbeatCheckTimer != null) {
					SubscribeHeartbeatCheckTimer.Dispose();
				}

				disposedValue = true;
			}
		}

		void IDisposable.Dispose()
		{
			DisposeInternal(true);
		}

	}
}
