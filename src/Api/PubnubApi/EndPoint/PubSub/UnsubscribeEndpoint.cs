using PubnubApi.EventEngine.Presence;
using PubnubApi.EventEngine.Subscribe;
using PubnubApi.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PubnubApi.EndPoint
{
	public class UnsubscribeEndpoint<T> : PubnubCoreBase, IUnsubscribeOperation<T>
	{
		private readonly PNConfiguration config;
		private readonly IJsonPluggableLibrary jsonLibrary;
		private readonly IPubnubUnitTest unit;
		private readonly IPubnubLog pubnubLog;
		private readonly EndPoint.TelemetryManager pubnubTelemetryMgr;
		private readonly EndPoint.TokenManager pubnubTokenMgr;

		private string[] subscribeChannelNames;
		private string[] subscribeChannelGroupNames;
		private Dictionary<string, object> queryParam { get; set; }
		private Pubnub pubnubInstance { get; set; }
		private SubscribeEventEngine subscribeEventEngine { get; set; }
		private SubscribeEventEngineFactory subscribeEventEngineFactory { get; set; }
		private PresenceEventEngineFactory presenceEventEngineFactory;
		private string instanceId { get; set; }

		public UnsubscribeEndpoint(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log, EndPoint.TelemetryManager telemetryManager, EndPoint.TokenManager tokenManager, SubscribeEventEngineFactory subscribeEventEngineFactory, PresenceEventEngineFactory presenceEventEngineFactory, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log, telemetryManager, tokenManager, instance)
		{
			pubnubInstance = instance;
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;
			pubnubLog = log;
			pubnubTelemetryMgr = telemetryManager;
			pubnubTokenMgr = tokenManager;
			this.subscribeEventEngineFactory = subscribeEventEngineFactory;
			this.presenceEventEngineFactory = presenceEventEngineFactory;
			instanceId = instance.InstanceId;
		}

		public IUnsubscribeOperation<T> Channels(string[] channels)
		{
			this.subscribeChannelNames = channels;
			return this;
		}

		public IUnsubscribeOperation<T> ChannelGroups(string[] channelGroups)
		{
			this.subscribeChannelGroupNames = channelGroups;
			return this;
		}

		public IUnsubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam)
		{
			this.queryParam = customQueryParam;
			return this;
		}

		public void Execute()
		{
			Unsubscribe(subscribeChannelNames, subscribeChannelGroupNames);
		}

		private void Unsubscribe(string[] channels, string[] channelGroups)
		{
			if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0)) {
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}

			string channel = (channels != null) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
			string channelGroup = (channelGroups != null) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

			LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, "DateTime {0}, requested unsubscribe for channel(s)={1}, cg(s)={2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), channel, channelGroup), config.LogVerbosity);

			if (this.subscribeEventEngineFactory.HasEventEngine(instanceId)) {
				subscribeEventEngine = subscribeEventEngineFactory.GetEventEngine(instanceId);
				channels = channels ?? new string[] { };
				channelGroups = channelGroups ?? new string[] { };
				var channelsWithPresence = channels.Concat(channels.Select((c) => $"{c}-pnpres")).ToList();
				var filteredChannelNames = new List<string>(subscribeEventEngine.Channels);
				foreach (var c in channelsWithPresence) {
					filteredChannelNames.Remove(c);
				}
				var channelGroupsWithPresence = channelGroups.Concat(channelGroups.Select((cg) => $"{cg}-pnpres")).ToList();
				var filteredChannelGroupNames = new List<string>(subscribeEventEngine.ChannelGroups);
				foreach (var g in channelGroupsWithPresence) {
					filteredChannelGroupNames.Remove(g);
				}
				if (subscribeEventEngine.Channels.Distinct().Count() != filteredChannelNames.Distinct().Count() ||
					subscribeEventEngine.ChannelGroups.Distinct().Count() != filteredChannelGroupNames.Distinct().Count()) {

					var channelsToRemove = FindUniqueCommonElements(subscribeEventEngine.Channels, channels.ToList());
					var channelGroupsToRemove = FindUniqueCommonElements(subscribeEventEngine.ChannelGroups, channelGroups.ToList());

					subscribeEventEngine.Unsubscribe(channelsToRemove.ToArray(), channelGroupsToRemove.ToArray());

					if (config.PresenceInterval > 0 && presenceEventEngineFactory.HasEventEngine(instanceId)) {
						PresenceEventEngine presenceEventEngine = presenceEventEngineFactory.GetEventEngine(instanceId);
						presenceEventEngine.EventQueue.Enqueue(new EventEngine.Presence.Events.LeftEvent() { Input = new EventEngine.Presence.Common.PresenceInput() { Channels = channelsToRemove.ToArray(), ChannelGroups = channelGroupsToRemove.ToArray() } });
					}
					if (config.MaintainPresenceState) {
						if (ChannelLocalUserState.TryGetValue(PubnubInstance.InstanceId, out
							var userState)) {
							foreach (var channelName in channels ?? new string[0]) {
								userState.TryRemove(channelName, out _);
							}
						}
						if (ChannelGroupLocalUserState.TryGetValue(PubnubInstance.InstanceId, out
							var channelGroupUserState)) {
							foreach (var channelGroupName in channelGroups ?? new string[0]) {
								channelGroupUserState.TryRemove(channelGroupName, out _);
							}
						}
					}
				} else {
					subscribeEventEngine.Channels = filteredChannelNames;
					subscribeEventEngine.ChannelGroups = filteredChannelGroupNames;
				}

			} else {
				LoggingMethod.WriteToLog(pubnubLog, string.Format(CultureInfo.InvariantCulture, $"DateTime {DateTime.Now.ToString(CultureInfo.InvariantCulture)}, Attempted Unsubscribe without EventEngine subscribe."), config.LogVerbosity);
			}

		}

		private IEnumerable<string> FindUniqueCommonElements(List<string> a, List<string> b)
		{
			return a
				.Where(value =>
					b.Contains(value) &&
					a.IndexOf(value) == a.LastIndexOf(value) &&
					b.IndexOf(value) == b.LastIndexOf(value));
		}
	}
}
