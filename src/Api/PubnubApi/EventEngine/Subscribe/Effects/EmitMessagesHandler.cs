using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Invocations;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
    public class EmitMessagesHandler : EffectHandler<Invocations.EmitMessagesInvocation>
    {
        private readonly System.Action<Pubnub, PNMessageResult<object>> messageEmitterFunction;
        private readonly Pubnub pubnubInstance;
        private readonly Dictionary<string, Type> channelTypeMap;
        private readonly Dictionary<string, Type> channelGroupTypeMap;
        
        public EmitMessagesHandler(Pubnub pubnubInstance,
            System.Action<Pubnub, PNMessageResult<object>> messageEmitterFunction, Dictionary<string, Type> channelTypeMap = null, Dictionary<string, Type> channelGroupTypeMap = null)
        {
            this.messageEmitterFunction = messageEmitterFunction;
            this.pubnubInstance = pubnubInstance;
        }

        public async override Task Run(EmitMessagesInvocation invocation)
        {
            var processedMessages = invocation.Messages?.Messages.Select(m =>
            {
                var msgResult = new PNMessageResult<object>()
                {
                    Channel = m.Channel,
                    Subscription = m.SubscriptionMatch,
                    Timetoken = m.Timetoken.Timestamp,
                    UserMetadata = m.PublishMetadata,
                    Publisher = m.IssuingClientId
                };
                
                if (!(channelTypeMap is null) && channelTypeMap.TryGetValue(m.Channel, out var msgType))
                {
                    msgResult.Message = JsonConvert.DeserializeObject(m.Payload, msgType);
                } else if (!(channelGroupTypeMap is null) && channelGroupTypeMap.TryGetValue(m.SubscriptionMatch, out var T))
                {
                    msgResult.Message = JsonConvert.DeserializeObject(m.Payload, T);
                }
                else
                {
                    msgResult.Message = m.Payload;
                }

                return msgResult;
            });

            if (processedMessages is null) return;
            
            foreach (var message in processedMessages)
            {
                messageEmitterFunction(pubnubInstance, message);
            }
        }

        public override bool IsBackground(EmitMessagesInvocation invocation) => false;

        public override Task Cancel()
        {
            throw new NotImplementedException();
        }
    }
}