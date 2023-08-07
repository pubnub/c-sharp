﻿using System;
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
        
        public EmitMessagesHandler(Pubnub pubnubInstance,
            System.Action<Pubnub, PNMessageResult<object>> messageEmitterFunction)
        {
            this.messageEmitterFunction = messageEmitterFunction;
            this.pubnubInstance = pubnubInstance;
        }

        public async override Task Run(EmitMessagesInvocation invocation)
        {
            var processedMessages = invocation.Messages?.Messages.Select(m => new PNMessageResult<object>()
            {
                Channel = m.Channel,
                Message = JsonConvert.DeserializeObject(m.Payload),
                Subscription = m.SubscriptionMatch,
                Timetoken = m.Timetoken.Timestamp,
                UserMetadata = m.PublishMetadata,
                Publisher = m.IssuingClientId
            });

            processedMessages?.ToList().ForEach(message => messageEmitterFunction(pubnubInstance, message));

        }

        public override bool IsBackground(EmitMessagesInvocation invocation) => false;

        public override Task Cancel()
        {
            throw new NotImplementedException();
        }
    }
}