using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
	public class EmitMessagesHandler : EffectHandler<EmitMessagesInvocation>
	{
		private readonly Dictionary<string, Type> channelTypeMap;
		private readonly Dictionary<string, Type> channelGroupTypeMap;
		private readonly IJsonPluggableLibrary jsonPluggableLibrary;
		private readonly EventEmitter eventEmitter;

		public EmitMessagesHandler(EventEmitter eventEmitter,
			IJsonPluggableLibrary jsonPluggableLibrary,
			Dictionary<string, Type> channelTypeMap = null,
			Dictionary<string, Type> channelGroupTypeMap = null)
		{
			this.eventEmitter = eventEmitter;
			this.channelTypeMap = channelTypeMap;
			this.channelGroupTypeMap = channelGroupTypeMap;
			this.jsonPluggableLibrary = jsonPluggableLibrary;
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
				Type t;
				if ((channelTypeMap is not null && channelTypeMap.TryGetValue(key, out t) ||
					channelGroupTypeMap is not null && channelGroupTypeMap.TryGetValue(key, out t)) &&
					t != typeof(string))
				{
					return jsonPluggableLibrary.DeserializeToObject(rawMessage, t);
				} else {
					return rawMessage.ToString();
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