using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.States;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Effects;
using PubnubApi.EventEngine.Subscribe.Invocations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe {
	public class SubscribeEventEngine : Engine {
		private SubscribeManager2 subscribeManager;

		private readonly Dictionary<string, Type> channelTypeMap = new Dictionary<string, Type>();
		private readonly Dictionary<string, Type> channelGroupTypeMap = new Dictionary<string, Type>();
		
		private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
			{ Formatting = Formatting.None, DateParseHandling = DateParseHandling.None };
		private static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings); 

		internal SubscribeEventEngine(Pubnub pubnubInstance,
			PNConfiguration pubnubConfiguration,
			SubscribeManager2 subscribeManager,
			Action<Pubnub, PNStatus> statusListener = null,
			Action<Pubnub, PNMessageResult<object>> messageListener = null)
		{
			this.subscribeManager = subscribeManager;

			// initialize the handler, pass dependencies
			var handshakeHandler = new Effects.HandshakeEffectHandler(subscribeManager, EventQueue);
			var handshakeReconnectHandler = new Effects.HandshakeReconnectEffectHandler(subscribeManager, EventQueue, handshakeHandler);
			dispatcher.Register<Invocations.HandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.CancelHandshakeInvocation, Effects.HandshakeEffectHandler>(handshakeHandler);
			dispatcher.Register<Invocations.HandshakeReconnectInvocation, Effects.HandshakeReconnectEffectHandler>(handshakeReconnectHandler);
			dispatcher.Register<Invocations.CancelHandshakeReconnectInvocation, Effects.HandshakeReconnectEffectHandler>(handshakeReconnectHandler);

			var receiveHandler = new Effects.ReceivingEffectHandler(subscribeManager, EventQueue);
			var receiveReconnectHandler = new Effects.ReceivingReconnectEffectHandler(subscribeManager, EventQueue, receiveHandler);
			dispatcher.Register<Invocations.ReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.CancelReceiveMessagesInvocation, Effects.ReceivingEffectHandler>(receiveHandler);
			dispatcher.Register<Invocations.ReceiveReconnectInvocation, Effects.ReceivingReconnectEffectHandler>(receiveReconnectHandler);
			dispatcher.Register<Invocations.CancelReceiveReconnectInvocation, Effects.ReceivingReconnectEffectHandler>(receiveReconnectHandler);

			var emitMessageHandler = new Effects.EmitMessagesHandler(pubnubInstance, messageListener, Serializer, channelTypeMap, channelGroupTypeMap);
			dispatcher.Register<Invocations.EmitMessagesInvocation, Effects.EmitMessagesHandler>(emitMessageHandler);

			var emitStatusHandler = new Effects.EmitStatusEffectHandler(pubnubInstance, statusListener);
			dispatcher.Register<Invocations.EmitStatusInvocation, Effects.EmitStatusEffectHandler>(emitStatusHandler);

			currentState = new UnsubscribedState() { ReconnectionConfiguration = new Context.ReconnectionConfiguration(pubnubConfiguration.ReconnectionPolicy, pubnubConfiguration.ConnectionMaxRetries) };
		}
		public void Subscribe<T>(string[] channels, string[] channelGroups)
		{
			foreach (var c in channels)
			{
				channelTypeMap[c] = typeof(T);
			}
			foreach (var c in channelGroups)
			{
				channelGroupTypeMap[c] = typeof(T);
			}
			this.EventQueue.Enqueue(new SubscriptionChangedEvent() { Channels = channels, ChannelGroups = channelGroups });
		}

		public void Subscribe(string[] channels, string[] channelGroups)
		{
			Subscribe<string>(channels, channelGroups);
		}
		
		public void UnsubscribeAll()
		{
			this.EventQueue.Enqueue(new UnsubscribeAllEvent());
		}

		public void Unsubscribe(string[] channels, string[] channelGroups)
		{
			this.EventQueue.Enqueue(new SubscriptionChangedEvent() {
				Channels = (this.currentState as SubscriptionState).Channels.Except(channels),
				ChannelGroups = (this.currentState as SubscriptionState).ChannelGroups.Except(channelGroups)
			});
		}
	}
}