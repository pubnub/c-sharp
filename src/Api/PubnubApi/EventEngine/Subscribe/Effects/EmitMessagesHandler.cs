using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Common;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Subscribe.Context;
using PubnubApi.EventEngine.Subscribe.Events;
using PubnubApi.EventEngine.Subscribe.Invocations;
using PubnubApi.EventEngine.Subscribe.Common;

namespace PubnubApi.EventEngine.Subscribe.Effects
{
    internal class EmitMessagesHandler : IEffectHandler<Invocations.EmitMessagesInvocation>
    {
        private System.Action<Pubnub, PNMessageResult<object>> messageEmitterFunction;
        private Pubnub pubnubInstance;

        //Message<T>(Pubnub pubnub, PNMessageResult<T> message)
        public EmitMessagesHandler(Pubnub pubnubInstance,
            System.Action<Pubnub, PNMessageResult<object>> messageEmitterFunction)
        {
            this.messageEmitterFunction = messageEmitterFunction;
            this.pubnubInstance = pubnubInstance;
        }

        public async Task Run(EmitMessagesInvocation invocation)
        {
            var processedMessages = invocation.Messages.Messages.Select(m => new PNMessageResult<object>()
            {
                // TODO is this ok? :)
                Channel = m.Channel,
                Message = JsonConvert.DeserializeObject(m.Payload),
                Subscription = m.SubscriptionMatch, // ??
                Timetoken = m.Timetoken.Timestamp,
                UserMetadata = m.PublishMetadata,
                // Publisher = ??
            });

            foreach (var message in processedMessages)
            {
                messageEmitterFunction(pubnubInstance, message);
            }
        }

        public bool IsBackground(EmitMessagesInvocation invocation) => false;

        public Task Cancel()
        {
            throw new NotImplementedException();
        }
    }
}