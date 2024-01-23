namespace PubnubApi.EventEngine.Presence.Common
{
    public class PresenceInput
    {
        public string[] Channels { get; set; }
        public string[] ChannelGroups { get; set; }

        public static PresenceInput operator +(PresenceInput a, PresenceInput b)
        {
            return new PresenceInput
            {
                Channels = a.Channels?.Union(b.Channels ?? new string[0]).ToArray(),
                ChannelGroups = a.ChannelGroups?.Union(b.ChannelGroups ?? new string[0]).ToArray(),
            };
        }

        public static PresenceInput operator -(PresenceInput a, PresenceInput b)
        {
            return new PresenceInput
            {
                Channels = a.Channels?.Except(b.Channels ?? new string[0]).ToArray(),
                ChannelGroups = a.ChannelGroups?.Except(b.ChannelGroups ?? new string[0]).ToArray(),
            };
        }

        public bool IsEmpty()
        {
            return (Channels == null && ChannelGroups == null)
                || ((Channels != null && Channels.Count() == 0)
                        && (ChannelGroups != null && ChannelGroups.Count() == 0));
        }
    }
}
