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
        readonly Action<Pubnub, PNObjectApiEventResult> objectApiAction;
        readonly Action<Pubnub, PNMessageActionEventResult> messageAction;

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, Action<Pubnub, PNPresenceEventResult> presenceCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = null;
            objectApiAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNSignalResult<object>> signalCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = null;
            presenceAction = null;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectApiAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNObjectApiEventResult> objectApiCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = null;
            presenceAction = null;
            signalAction = null;
            statusAction = statusCallback;
            objectApiAction = objectApiCallback;

        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageActionEventResult> messageActionCallback, Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = null;
            presenceAction = null;
            signalAction = null;
            statusAction = null;
            objectApiAction = null;
            messageAction = messageActionCallback;
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
            objectApiAction = null;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, 
            Action<Pubnub, PNPresenceEventResult> presenceCallback, 
            Action<Pubnub, PNSignalResult<object>> signalCallback, 
            Action<Pubnub, PNObjectApiEventResult> objectApiCallback, 
            Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectApiAction = objectApiCallback;
        }

        public SubscribeCallbackExt(Action<Pubnub, PNMessageResult<object>> messageCallback, 
            Action<Pubnub, PNPresenceEventResult> presenceCallback, 
            Action<Pubnub, PNSignalResult<object>> signalCallback, 
            Action<Pubnub, PNObjectApiEventResult> objectApiCallback, 
            Action<Pubnub, PNMessageActionEventResult> messageActionCallback, 
            Action<Pubnub, PNStatus> statusCallback)
        {
            subscribeAction = messageCallback;
            presenceAction = presenceCallback;
            statusAction = statusCallback;
            signalAction = signalCallback;
            objectApiAction = objectApiCallback;
            messageAction = messageActionCallback;
        }

        public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
        {
            PNMessageResult<object> message1 = new PNMessageResult<object>();
            message1.Channel = message.Channel;
            message1.Message = (T)(object)message.Message;
            message1.Subscription = message.Subscription;
            message1.Timetoken = message.Timetoken;
            message1.UserMetadata = message.UserMetadata;
            message1.Publisher = message.Publisher;

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

            signalAction?.Invoke(pubnub, message1);
        }

        public override void ObjectEvent(Pubnub pubnub, PNObjectApiEventResult objectEvent)
        {
            objectApiAction?.Invoke(pubnub, objectEvent);
        }

        public override void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageActionEvent)
        {
            messageAction?.Invoke(pubnub, messageActionEvent);
        }
    }
}
