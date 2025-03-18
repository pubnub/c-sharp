using System.Collections.Generic;

namespace PubnubApi.Interface
{
    public interface IUnsubscribeOperation<T>
    {
        IUnsubscribeOperation<T> Channels(string[] channels);
        IUnsubscribeOperation<T> ChannelGroups(string[] channelGroups);
        IUnsubscribeOperation<T> QueryParam(Dictionary<string, object> customQueryParam);
        void Execute();
    }
}
