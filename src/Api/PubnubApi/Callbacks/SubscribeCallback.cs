using System;

namespace PubnubApi
{
    public abstract class SubscribeCallback
    {
        public abstract void Status(Pubnub pubnub, PNStatus status);

        public abstract void Message<T>(Pubnub pubnub, PNMessageResult<T> message);

        public abstract void Presence(Pubnub pubnub, PNPresenceEventResult presence);

    }
}
