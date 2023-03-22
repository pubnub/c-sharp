using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class SubscribeCallbackExt : SubscribeCallback
    {
        readonly Action<Pubnub, PNMessageResult<object>> subscribeAction;
        readonly Action<Pubnub, PNPresenceEventResult> presenceAction;
        readonly Action<Pubnub, PNSignalResult<object>> signalAction;
        readonly Action<Pubnub, PNStatus> statusAction;
        readonly Action<Pubnub, PNObjectEventResult> objectEventAction;
        readonly Action<Pubnub, PNMessageActionEventResult> messageAction;
        readonly Action<Pubnub, PNFileEventResult> fileAction;

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, Action<Pubnub, PNPresenceEventResult> presenceCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = null;
            objectEventAction = null;
            fileAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNSignalResult<object>> signalCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = null;
            presenceAction = null;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectEventAction = null;
            fileAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNObjectEventResult> objectEventCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = null;
            presenceAction = null;
            signalAction = null;
            statusAction = statusCallback;
            objectEventAction = objectEventCallback;
            fileAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageActionEventResult> messageActionCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = null;
            presenceAction = null;
            signalAction = null;
            statusAction = statusCallback;
            objectEventAction = null;
            messageAction = messageActionCallback;
            fileAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNFileEventResult> fileCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = null;
            presenceAction = null;
            statusAction = statusCallback;
            signalAction = null;
            objectEventAction = null;
            fileAction = fileCallback;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, 
            Action<Pubnub, PNPresenceEventResult> presenceCallback, 
            Action<Pubnub, PNSignalResult<object>> signalCallback, 
            Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectEventAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, 
            Action<Pubnub, PNPresenceEventResult> presenceCallback, 
            Action<Pubnub, PNSignalResult<object>> signalCallback, 
            Action<Pubnub, PNObjectEventResult> objectEventCallback, 
            Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectEventAction = objectEventCallback;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, 
            Action<Pubnub, PNPresenceEventResult> presenceCallback, 
            Action<Pubnub, PNSignalResult<object>> signalCallback, 
            Action<Pubnub, PNObjectEventResult> objectEventCallback, 
            Action<Pubnub, PNMessageActionEventResult> messageActionCallback, 
            Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectEventAction = objectEventCallback;
            messageAction = messageActionCallback;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback,
            Action<Pubnub, PNPresenceEventResult> presenceCallback,
            Action<Pubnub, PNSignalResult<object>> signalCallback,
            Action<Pubnub, PNObjectEventResult> objectEventCallback,
            Action<Pubnub, PNMessageActionEventResult> messageActionCallback,
            Action<Pubnub, PNFileEventResult> fileCallback,
            Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectEventAction = objectEventCallback;
            messageAction = messageActionCallback;
            fileAction = fileCallback;
        }

        public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
        {
            PNMessageResult<object> message1 = new PNMessageResult<object>
            {
                Channel = message.Channel,
                Message = (T)(object)message.Message,
                Subscription = message.Subscription,
                Timetoken = message.Timetoken,
                UserMetadata = message.UserMetadata,
                Publisher = message.Publisher,
                Type = message.Type,
                SpaceId = message.SpaceId
            };

            subscribeAction?.Invoke(pubnub, message1);
        }

        public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
        {
            presenceAction?.Invoke(pubnub, presence);
        }

        public override void Status(Pubnub pubnub, PNStatus status)
        {
            statusAction?.Invoke(pubnub, status);
        }

        public override void Signal<T>(Pubnub pubnub, PNSignalResult<T> signalMessage)
        {
            PNSignalResult<object> message1 = new PNSignalResult<object>();
            message1.Channel = signalMessage.Channel;
            message1.Message = (T)(object)signalMessage.Message;
            message1.Subscription = signalMessage.Subscription;
            message1.Timetoken = signalMessage.Timetoken;
            message1.UserMetadata = signalMessage.UserMetadata;
            message1.Publisher = signalMessage.Publisher;
            message1.Type = signalMessage.Type;
            message1.SpaceId = signalMessage.SpaceId;

            signalAction?.Invoke(pubnub, message1);
        }

        public override void ObjectEvent(Pubnub pubnub, PNObjectEventResult objectEvent)
        {
            objectEventAction?.Invoke(pubnub, objectEvent);
        }

        public override void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageActionEvent)
        {
            messageAction?.Invoke(pubnub, messageActionEvent);
        }

        public override void File(Pubnub pubnub, PNFileEventResult fileEvent)
        {
            PNFileEventResult message1 = new PNFileEventResult();
            message1.Channel = fileEvent.Channel;
            message1.Message = fileEvent.Message;
            message1.Subscription = fileEvent.Subscription;
            message1.Timetoken = fileEvent.Timetoken;
            message1.Publisher = fileEvent.Publisher;
            message1.File = fileEvent.File;

            fileAction?.Invoke(pubnub, message1);
        }
    }
}
