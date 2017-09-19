using System;

namespace PubnubApi
{
    internal class InternetState<T>
    {
        public Action<bool> InternalCallback { get; set; }
        public PNCallback<T> PubnubCallbacck { get; set; }
        public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }
        public PNOperationType ResponseType { get; set; }

        public InternetState()
        {
            InternalCallback = null;
            Channels = null;
            ChannelGroups = null;
        }
    }
}