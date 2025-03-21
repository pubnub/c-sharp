﻿using System.Collections.Generic;

namespace PubnubApi
{
    public interface ISubscribeOperation<T>
    {
        List<SubscribeCallback> SubscribeListenerList { get; set;}
        ISubscribeOperation<T> Channels(string[] channels);
        ISubscribeOperation<T> ChannelGroups(string[] channelGroups);
        ISubscribeOperation<T> WithTimetoken(long timetoken);
        ISubscribeOperation<T> WithPresence();
        ISubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam);
        void Execute();
    }
}
