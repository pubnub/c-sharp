using System;

namespace PubnubApi
{
    public abstract class SubscribeCallback
    {
        public abstract void Status(Pubnub pubnub, PNStatus status);

        public abstract void Message<T>(Pubnub pubnub, PNMessageResult<T> message);

        public abstract void Presence(Pubnub pubnub, PNPresenceEventResult presence);

        public abstract void Signal<T>(Pubnub pubnub, PNSignalResult<T> signal);

        public abstract void ObjectEvent(Pubnub pubnub, PNObjectEventResult objectEvent);

        public abstract void MessageAction(Pubnub pubnub, PNMessageActionEventResult messageAction);

        public abstract void File<T>(Pubnub pubnub, PNFileEventResult<T> fileEvent);
    }
}
