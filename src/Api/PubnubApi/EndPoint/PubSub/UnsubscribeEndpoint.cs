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

		private readonly EndPoint.TokenManager pubnubTokenMgr;

		private string[] subscribeChannelNames;
		private string[] subscribeChannelGroupNames;
		private Dictionary<string, object> queryParam { get; set; }
		private Pubnub pubnubInstance { get; set; }
		private SubscribeEventEngine subscribeEventEngine { get; set; }
		private SubscribeEventEngineFactory subscribeEventEngineFactory { get; set; }
		private PresenceEventEngineFactory presenceEventEngineFactory;
		private string instanceId { get; set; }

		public UnsubscribeEndpoint(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, EndPoint.TokenManager tokenManager, SubscribeEventEngineFactory subscribeEventEngineFactory, PresenceEventEngineFactory presenceEventEngineFactory, Pubnub instance) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, tokenManager, instance)
		{
			pubnubInstance = instance;
			config = pubnubConfig;
			jsonLibrary = jsonPluggableLibrary;
			unit = pubnubUnit;

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
			logger?.Trace($"{GetType().Name} Execute invoked");
			Unsubscribe(subscribeChannelNames, subscribeChannelGroupNames);
		}

		private void Unsubscribe(string[] channels, string[] channelGroups)
		{
			if ((channels == null || channels.Length == 0) && (channelGroups == null || channelGroups.Length == 0)) {
				throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
			}

			logger?.Trace($" Unsubscribe request for channels: {string.Join(",", channels ?? [])}, channelGroups: {string.Join(",", channelGroups ?? [])} ");

			if (subscribeEventEngineFactory.HasEventEngine(instanceId)) {
				subscribeEventEngine = subscribeEventEngineFactory.GetEventEngine(instanceId);
				channels ??= [];
				channelGroups ??= [];
				var uniqueChannelsToRemove = new List<string>();
				var uniqueChannelGroupsToRemove = new List<string>();
				var channelNamesToRemove = new List<string>(channels);
				channelNamesToRemove =
					channelNamesToRemove.Concat(channelNamesToRemove.Where(c=>!c.EndsWith(Constants.Pnpres)).Select(c => $"{c}{Constants.Pnpres}")).ToList();
				var uniqueChannelNamesCount  = subscribeEventEngine.Channels.Distinct().Count();
				foreach (var c in channelNamesToRemove)
				{
					if (subscribeEventEngine.Channels.Contains(c))
					{
						subscribeEventEngine.Channels.Remove(c);
						if (!subscribeEventEngine.Channels.Contains(c) && !c.EndsWith(Constants.Pnpres))
							uniqueChannelsToRemove.Add(c);
					}
				}
				var uniqueChannelNamesCountAfterRemoval = subscribeEventEngine.Channels.Distinct().Count();
				bool isUniqueChannelCountChanged = uniqueChannelNamesCount != uniqueChannelNamesCountAfterRemoval;
				
				var channelGroupNamesToRemove = new List<string>(channelGroups);
				channelGroupNamesToRemove =
					channelGroupNamesToRemove.Concat(channelGroupNamesToRemove.Where(cg=>!cg.EndsWith(Constants.Pnpres)).Select(c => $"{c}{Constants.Pnpres}")).ToList();
				var uniqueChannelGroupNamesCount  = subscribeEventEngine.ChannelGroups.Distinct().Count();
				foreach (var cg in channelGroupNamesToRemove)
				{
					if (subscribeEventEngine.ChannelGroups.Contains(cg))
					{
						subscribeEventEngine.ChannelGroups.Remove(cg);
						if (!subscribeEventEngine.ChannelGroups.Contains(cg) && !cg.EndsWith(Constants.Pnpres))
							uniqueChannelGroupsToRemove.Add(cg);
					}
				}
				var uniqueChannelGroupNamesCountAfterRemoval = subscribeEventEngine.ChannelGroups.Distinct().Count();
				bool isUniqueChannelGroupCountChanged = uniqueChannelGroupNamesCount != uniqueChannelGroupNamesCountAfterRemoval;
				
				var isSubscriptionChanged = isUniqueChannelCountChanged || isUniqueChannelGroupCountChanged;
				if (isSubscriptionChanged)
				{
					subscribeEventEngine.Unsubscribe(subscribeEventEngine.Channels.ToArray(), subscribeEventEngine.ChannelGroups.ToArray());
					if (config.PresenceInterval > 0 && presenceEventEngineFactory.HasEventEngine(instanceId) && (uniqueChannelsToRemove.Count > 0 || uniqueChannelGroupsToRemove.Count > 0)) {
						PresenceEventEngine presenceEventEngine = presenceEventEngineFactory.GetEventEngine(instanceId);
						presenceEventEngine.EventQueue.Enqueue(new EventEngine.Presence.Events.LeftEvent()
						{
							Input = new EventEngine.Presence.Common.PresenceInput() 
								{ Channels = uniqueChannelsToRemove.ToArray(), ChannelGroups = uniqueChannelGroupsToRemove.ToArray() }
						});
					}
					if (config.MaintainPresenceState) {
						if (ChannelLocalUserState.TryGetValue(PubnubInstance.InstanceId, out
							    var userState)) {
							foreach (var channelName in uniqueChannelsToRemove ) {
								userState.TryRemove(channelName, out _);
							}
						}
						if (ChannelGroupLocalUserState.TryGetValue(PubnubInstance.InstanceId, out
							    var channelGroupUserState)) {
							foreach (var channelGroupName in uniqueChannelGroupsToRemove) {
								channelGroupUserState.TryRemove(channelGroupName, out _);
							}
						}
					}	
				}
			} else {
				logger?.Error($"Attempted unsubscribe without event engine");
			}

		}
	}
}
