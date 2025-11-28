using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.States;
using System.Collections.Generic;
using System.Linq;
using System;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Subscribe.Effects;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe
{
	public class SubscribeEventEngine : Engine
	{
		private SubscribeManager2 subscribeManager;
		private readonly Dictionary<string, Type> channelTypeMap = new();
		private readonly Dictionary<string, Type> channelGroupTypeMap = new Dictionary<string, Type>();
		private readonly IJsonPluggableLibrary jsonPluggableLibrary;

		public List<string> Channels { get; } = [];
		public List<string> ChannelGroups { get; } = [];
		private EmitStatusEffectHandler emitStatusHandler = null; 

		internal SubscribeEventEngine(Pubnub pubnubInstance,
			PNConfiguration pubnubConfiguration,
			SubscribeManager2 subscribeManager,
			EventEmitter eventEmitter,
			IJsonPluggableLibrary jsonPluggableLibrary,
			Action<Pubnub, PNStatus> statusListener = null)
		{
			this.subscribeManager = subscribeManager;
			this.jsonPluggableLibrary = jsonPluggableLibrary;
			var handshakeHandler = new HandshakeEffectHandler(subscribeManager, EventQueue);
			var handshakeReconnectHandler = new HandshakeReconnectEffectHandler(pubnubConfiguration, EventQueue, handshakeHandler);

			dispatcher.Register<HandshakeInvocation, HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<CancelHandshakeInvocation, HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<HandshakeReconnectInvocation, HandshakeReconnectEffectHandler>(handshakeReconnectHandler);
			dispatcher.Register<CancelHandshakeReconnectInvocation, HandshakeReconnectEffectHandler>(handshakeReconnectHandler);

			var receiveHandler = new ReceivingEffectHandler(subscribeManager, EventQueue);
			var receiveReconnectHandler = new ReceivingReconnectEffectHandler(pubnubConfiguration, EventQueue, receiveHandler);

			dispatcher.Register<ReceiveMessagesInvocation, ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<CancelReceiveMessagesInvocation, ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<ReceiveReconnectInvocation, ReceivingReconnectEffectHandler>(receiveReconnectHandler);
			dispatcher.Register<CancelReceiveReconnectInvocation, ReceivingReconnectEffectHandler>(receiveReconnectHandler);

			var emitMessageHandler = new EmitMessagesHandler(eventEmitter, jsonPluggableLibrary, channelTypeMap, channelGroupTypeMap);
			dispatcher.Register<EmitMessagesInvocation, EmitMessagesHandler>(emitMessageHandler);

			emitStatusHandler = new EmitStatusEffectHandler(pubnubInstance, statusListener);
			dispatcher.Register<EmitStatusInvocation, EmitStatusEffectHandler>(emitStatusHandler);

			currentState = new UnsubscribedState();
			logger = pubnubConfiguration.Logger;
		}
		public void Subscribe<T>(string[] channels, string[] channelGroups, SubscriptionCursor cursor)
		{
			bool allChannelsExist = channels.All(c => Channels.Contains(c));
			bool allChannelGroupsExist = channelGroups.All(cg => ChannelGroups.Contains(cg));
			bool isNewSubscription = !(allChannelsExist && allChannelGroupsExist);
			
			Channels.AddRange(channels);
			ChannelGroups.AddRange(channelGroups);

			foreach (var c in channels) {
				channelTypeMap[c] = typeof(T);
			}
			foreach (var c in channelGroups) {
				channelGroupTypeMap[c] = typeof(T);
			}
			if (cursor != null) {
				EventQueue.Enqueue(new SubscriptionRestoredEvent() {
					Channels = Channels.Distinct(),
					ChannelGroups = ChannelGroups.Distinct(),
					Cursor = cursor
				});
			} else {
				if (isNewSubscription) {
					EventQueue.Enqueue(new SubscriptionChangedEvent() {
						Channels = Channels.Distinct(),
						ChannelGroups = ChannelGroups.Distinct()
					});
				} else {
					var status = new PNStatus(null, PNOperationType.PNSubscribeOperation, PNStatusCategory.PNSubscriptionChangedCategory, Channels, ChannelGroups, Constants.HttpRequestSuccessStatusCode);
					var invocation = new EmitStatusInvocation(status);
					_ = emitStatusHandler.Run(invocation);
				}
			}
		}

		public void Subscribe(string[] channels, string[] channelGroups, SubscriptionCursor cursor)
		{
			Subscribe<string>(channels, channelGroups, cursor);
		}

		public void UnsubscribeAll()
		{
			EventQueue.Enqueue(new UnsubscribeAllEvent());
		}

		public void Unsubscribe(string[] channels, string[] channelGroups)
		{
			if (channels.Length > 0 || channelGroups.Length > 0)
			{
				EventQueue.Enqueue(new SubscriptionChangedEvent()
				{
					Channels = Channels.Distinct(),
					ChannelGroups = ChannelGroups.Distinct()
				});
			}
			else
			{
				EventQueue.Enqueue(new UnsubscribeAllEvent());
			}
		}
	}
}