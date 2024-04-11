using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
	public class EmitMessagesHandler : EffectHandler<EmitMessagesInvocation>
	{
		private readonly Dictionary<string, Type> channelTypeMap;
		private readonly Dictionary<string, Type> channelGroupTypeMap;
		private readonly JsonSerializer serializer;
		private readonly EventEmitter eventEmitter;

		public EmitMessagesHandler(EventEmitter eventEmitter,
			JsonSerializer serializer,
			Dictionary<string, Type> channelTypeMap = null,
			Dictionary<string, Type> channelGroupTypeMap = null)
		{
			this.eventEmitter = eventEmitter;
			this.channelTypeMap = channelTypeMap;
			this.channelGroupTypeMap = channelGroupTypeMap;
			this.serializer = serializer;
		}

		public async override Task Run(EmitMessagesInvocation invocation)
		{
			var processedMessages = invocation.Messages?.Messages?.Select(m => {
				m.Payload = DeserializePayload(m.Channel, m.Payload);
				return m;
			});

			if (processedMessages is null) return;

			foreach (var message in processedMessages) {
				eventEmitter.EmitEvent<object>(message);
			}
		}

		private object DeserializePayload(string key, object rawMessage)
		{
			try {
				if (rawMessage is JObject message) {
					Type t;
					if ((channelTypeMap is not null && channelTypeMap.TryGetValue(key, out t) ||
						channelGroupTypeMap is not null && channelGroupTypeMap.TryGetValue(key, out t)) &&
						t != typeof(string)) {
						return message.ToObject(t, serializer);
					} else {
						return message.ToString(Formatting.None);
					}
				} else {
					return rawMessage;
				}
			} catch (Exception) {
				return rawMessage;
			}
		}

		public override bool IsBackground(EmitMessagesInvocation invocation) => false;

		public override Task Cancel()
		{
			throw new NotImplementedException();
		}
	}
}