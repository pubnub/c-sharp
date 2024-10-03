using System;
using System.Collections.Generic;
using System.Linq;
using PubnubApi.EventEngine.Common;

namespace PubnubApi
{
	public class Subscription : SubscribeCapable
	{
		public override List<string> ChannelNames { get; set; } = new List<string>();
		public override List<string> ChannelGroupNames { get; set; } = new List<string>();
		public override Pubnub Pubnub { get; set; }
		public override EventEmitter EventEmitter { get; set; }
		public override SubscriptionOptions? Options { get; set; }
		protected override SubscribeCallbackExt Listener { get; set; } = new SubscribeCallbackExt();

		public override event Action<Pubnub, PNMessageResult<object>> onMessage;
		public override event Action<Pubnub, PNPresenceEventResult> onPresence;
		public override event Action<Pubnub, PNSignalResult<object>> onSignal;
		public override event Action<Pubnub, PNObjectEventResult> onObject;
		public override event Action<Pubnub, PNMessageActionEventResult> onMessageAction;
		public override event Action<Pubnub, PNFileEventResult> onFile;
		public override event Action<Pubnub, PNStatus> onStatus;

		public Subscription(string[] channels, string[] channelGroups, SubscriptionOptions? options, Pubnub pubnub, EventEmitter eventEmitter)
		{
			this.ChannelNames = channels.ToList();
			this.ChannelGroupNames = channelGroups.ToList();
			this.Pubnub = pubnub;
			this.EventEmitter = eventEmitter;
			this.Options = options;

			this.Listener = new SubscribeCallbackExt(
				(pnObj, pubMsg) => {
					onMessage?.Invoke(pnObj, pubMsg);
				},
				(pnObj, presenceEvnt) => {
					onPresence?.Invoke(pnObj, presenceEvnt);
				},
				(pnObj, signalMsg) => {
					onSignal?.Invoke(pnObj, signalMsg);
				},
				(pnObj, objectEventObj) => {
					onObject?.Invoke(pnObj, objectEventObj);
				},
				(pnObj, msgActionEvent) => {
					onMessageAction?.Invoke(pnObj, msgActionEvent);
				},
				(pnObj, fileEvent) => {
					onFile?.Invoke(pnObj, fileEvent);
				},
				(pnObj, pnStatus) => {
					onStatus?.Invoke(pnObj, pnStatus);
				}
			);
			this.EventEmitter.AddListener(Listener, channels: channels, groups: channelGroups);
		}

		public SubscriptionSet Add(Subscription subscription)
		{
			this.ChannelNames.AddRange(subscription.ChannelNames);
			this.ChannelGroupNames.AddRange(subscription.ChannelGroupNames);
			return new SubscriptionSet(this.ChannelNames.ToArray(), this.ChannelGroupNames.ToArray(), this.Options, this.Pubnub, this.EventEmitter);
		}
	}
}