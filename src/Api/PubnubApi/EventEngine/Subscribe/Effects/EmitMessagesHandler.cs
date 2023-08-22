using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Common;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
    public class EmitMessagesHandler : EffectHandler<Invocations.EmitMessagesInvocation>
    {
        private readonly System.Action<Pubnub, PNMessageResult<object>> messageEmitterFunction;
        private readonly Pubnub pubnubInstance;
        private readonly Dictionary<string, Type> channelTypeMap;
        private readonly Dictionary<string, Type> channelGroupTypeMap;
        private readonly JsonSerializer serializer;

        public EmitMessagesHandler(Pubnub pubnubInstance,
            System.Action<Pubnub, PNMessageResult<object>> messageEmitterFunction,
            JsonSerializer serializer,
            Dictionary<string, Type> channelTypeMap = null,
            Dictionary<string, Type> channelGroupTypeMap = null)
        {
            this.messageEmitterFunction = messageEmitterFunction;
            this.pubnubInstance = pubnubInstance;

            this.channelTypeMap = channelTypeMap;
            this.channelGroupTypeMap = channelGroupTypeMap;

            this.serializer = serializer;
        }

        public async override Task Run(EmitMessagesInvocation invocation)
        {
            var processedMessages = invocation.Messages?.Messages?.Select(m =>
            {
                var msgResult = new PNMessageResult<object>()
                {
                    Channel = m.Channel,
                    Subscription = m.SubscriptionMatch,
                    Timetoken = invocation.Cursor.Timetoken.Value,
                    UserMetadata = (m.UserMetadata as object),
                    // TODO Where do we put the publish timetoken metadata?
                    // UserMetadata = (m.PublishMetadata as JObject)?.ToObject<Timetoken>(),
                    Publisher = m.IssuingClientId,
                };

                try
                {
                    DeserializeMessage(channelTypeMap, m.Channel, msgResult, m.Payload);
                    DeserializeMessage(channelGroupTypeMap, m.Channel, msgResult, m.Payload);
                }
                catch (Exception e)
                {
                    // TODO pass the exception
                    throw e;
                }

                return msgResult;
            });

            if (processedMessages is null) return;

            foreach (var message in processedMessages)
            {
                messageEmitterFunction(pubnubInstance, message);
            }
        }

        private void DeserializeMessage(Dictionary<string, Type> dict, string key, PNMessageResult<object> msg,
            object rawMessage)
        {
            if (dict is null) return;
            if (rawMessage is JObject message)
            {
                Type t;
                msg.Message = dict.TryGetValue(key, out t) && t != typeof(string)
                    ? message.ToObject(t, serializer)
                    : message.ToString(Formatting.None);
            }
            else
            {
                msg.Message = rawMessage;
            }
        }

        public override bool IsBackground(EmitMessagesInvocation invocation) => false;

        public override Task Cancel()
        {
            throw new NotImplementedException();
        }
    }
}