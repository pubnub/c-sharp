using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public interface IPubnubSubscribeMessageType
    {
        dynamic GetSubscribeMessageType(Type messageType, object pubnubSubscribeCallbackObject, bool isChannelGroup);
    }
}
