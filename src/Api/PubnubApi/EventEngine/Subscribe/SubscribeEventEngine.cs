using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.States;
using System.Collections.Generic;
using System.Linq;
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

		public List<string> Channels { get; set; } = new List<string>();
		public List<string> ChannelGroups { get; set; } = new List<string>();

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
					Channels = Channels.Concat(channels ?? new string[] { }).Distinct(),
					ChannelGroups = ChannelGroups.Concat(channelGroups ?? new string[] { }).Distinct(),
					Cursor = cursor
				});
			} else {
				EventQueue.Enqueue(new SubscriptionChangedEvent() {
					Channels = Channels.Concat(channels ?? new string[] { }).Distinct(),
					ChannelGroups = ChannelGroups.Concat(channelGroups ?? new string[] { }).Distinct()
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
			if(channels.Length>0 || channelGroups.Length>0)
				this.EventQueue.Enqueue(new SubscriptionChangedEvent() {
					Channels = this.Channels?.Except(channels ?? Enumerable.Empty<string>()),
					ChannelGroups = this.ChannelGroups?.Except(channelGroups ?? Enumerable.Empty<string>())
				});
		}
	}
}