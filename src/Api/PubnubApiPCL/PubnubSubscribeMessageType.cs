using System;

namespace PubnubApi
{
    public class PubnubSubscribeMessageType : IPubnubSubscribeMessageType
    {
        public virtual dynamic GetSubscribeMessageType(Type messageType, object pubnubSubscribeCallbackObject, bool isChannelGroup)
        {
            return null;
        }
    }
}
