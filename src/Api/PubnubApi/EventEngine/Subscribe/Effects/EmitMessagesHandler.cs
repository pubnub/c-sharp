using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
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
                if (CheckForNonGenericSerialization(m))
                {
                    return m;
                }
                m.Payload = DeserializePayload(m.Channel, m.Payload);
                return m;
            });

            if (processedMessages is null) return;

            foreach (var message in processedMessages)
            {
                Type type = MessageTypeValue(message.SubscriptionMatch??message.Channel);
                var methodInfo = eventEmitter.GetType().GetMethod("EmitEvent");
                var genericMethod = methodInfo?.MakeGenericMethod([type]);
                genericMethod?.Invoke(eventEmitter, [message]);
            }
        }

        private bool CheckForNonGenericSerialization<T>(Message<T> message)
        {
            var channel = message.Channel;
            var channelGroup = message.SubscriptionMatch;
            var type = message.MessageType;
            if (channel.EndsWith(Constants.Pnpres)) return true;
            if (!string.IsNullOrEmpty(channelGroup) && channelGroup.EndsWith(Constants.Pnpres)) return true;
            if (type == 0) return false;
            return true;
        }

        private Type MessageTypeValue(string channel)
        {
            try {
                Type t;
                if ((channelTypeMap is not null && channelTypeMap.TryGetValue(channel, out t) ||
                     channelGroupTypeMap is not null && channelGroupTypeMap.TryGetValue(channel, out t)) &&
                    t != typeof(string))
                {
                    return t;
                }
                return typeof(string);
            } catch (Exception) {
                return typeof(object);
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

        public override Task Cancel() => Task.CompletedTask;
        
    }
}
