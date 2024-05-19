using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.States;
using System.Collections.Generic;
using System.Linq;
using PubnubApi.EventEngine.Subscribe.Common;
using System;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Common;

namespace PubnubApi.EventEngine.Subscribe
{
	public class SubscribeEventEngine : Engine
	{
		private SubscribeManager2 subscribeManager;
		private readonly Dictionary<string, Type> channelTypeMap = new Dictionary<string, Type>();
		private readonly Dictionary<string, Type> channelGroupTypeMap = new Dictionary<string, Type>();
		private readonly IJsonPluggableLibrary jsonPluggableLibrary;

		public string[] Channels { get; set; } = new string[] {};
		public string[] Channelgroups { get; set; } = new string[] {};

		internal SubscribeEventEngine(Pubnub pubnubInstance,
			PNConfiguration pubnubConfiguration,
			SubscribeManager2 subscribeManager,
            EventEmitter eventEmitter,
			IJsonPluggableLibrary jsonPluggableLibrary,
            Action<Pubnub, PNStatus> statusListener = null)
		{
			this.subscribeManager = subscribeManager;
			this.jsonPluggableLibrary = jsonPluggableLibrary;
			var handshakeHandler = new Effects.HandshakeEffectHandler(subscribeManager, EventQueue);
			var handshakeReconnectHandler = new Effects.HandshakeReconnectEffectHandler(pubnubConfiguration, EventQueue, handshakeHandler);

			dispatcher.Register<Invocations.HandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.CancelHandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.HandshakeReconnectInvocation, Effects.HandshakeReconnectEffectHandler>(handshakeReconnectHandler);
			dispatcher.Register<Invocations.CancelHandshakeReconnectInvocation, Effects.HandshakeReconnectEffectHandler>(handshakeReconnectHandler);

			var receiveHandler = new Effects.ReceivingEffectHandler(subscribeManager, EventQueue);
			var receiveReconnectHandler = new Effects.ReceivingReconnectEffectHandler(pubnubConfiguration, EventQueue, receiveHandler);

			dispatcher.Register<Invocations.ReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.CancelReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.ReceiveReconnectInvocation, Effects.ReceivingReconnectEffectHandler>(receiveReconnectHandler);
			dispatcher.Register<Invocations.CancelReceiveReconnectInvocation, Effects.ReceivingReconnectEffectHandler>(receiveReconnectHandler);

			var emitMessageHandler = new Effects.EmitMessagesHandler(eventEmitter, jsonPluggableLibrary, channelTypeMap, channelGroupTypeMap);
			dispatcher.Register<Invocations.EmitMessagesInvocation, Effects.EmitMessagesHandler>(emitMessageHandler);

			var emitStatusHandler = new Effects.EmitStatusEffectHandler(pubnubInstance, statusListener);
			dispatcher.Register<Invocations.EmitStatusInvocation, Effects.EmitStatusEffectHandler>(emitStatusHandler);

			currentState = new UnsubscribedState();
		}
		public void Subscribe<T>(string[] channels, string[] channelGroups, SubscriptionCursor cursor)
		{
			foreach (var c in channels)
			{
				channelTypeMap[c] = typeof(T);
			}
			foreach (var c in channelGroups)
			{
				channelGroupTypeMap[c] = typeof(T);
			}
			if (cursor != null)
			{
				EventQueue.Enqueue(new SubscriptionRestoredEvent() {
					Channels = Channels.Concat(channels ?? new string[] {}),
					ChannelGroups = Channelgroups.Concat(channelGroups ?? new string[] {}),
					Cursor = cursor
				});
			}
			else
			{
				EventQueue.Enqueue(new SubscriptionChangedEvent() {
					Channels = Channels.Concat(channels??new string[] {}),
					ChannelGroups = Channelgroups.Concat(channelGroups?? new string[] {})
				});
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
			var channelNames = new List<string>();
			var groupNames = new List<string>();
			foreach (var channel in channels?? Enumerable.Empty<string>()) {
				channelNames.Add($"{channel}-pnpres");
			}
			foreach (var cg in channelGroups ?? Enumerable.Empty<string>()) {
				groupNames.Add($"{cg}-pnpres");
			}
			this.EventQueue.Enqueue(new SubscriptionChangedEvent() {
				Channels = (this.currentState as SubscriptionState).Channels?.Except(channels?.Union(channelNames)??Enumerable.Empty<string>()),
				ChannelGroups = (this.currentState as SubscriptionState).ChannelGroups?.Except(channelGroups?.Union(groupNames)??Enumerable.Empty<string>())
			});
		}
	}
}