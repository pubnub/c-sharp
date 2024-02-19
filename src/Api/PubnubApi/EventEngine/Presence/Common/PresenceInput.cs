using System.Collections.Generic;
using System.Linq;

namespace PubnubApi.EventEngine.Presence.Common
{
    public class PresenceInput
    {
        public IEnumerable<string> Channels { get; set; }
        public IEnumerable<string> ChannelGroups { get; set; }

        public static PresenceInput operator +(PresenceInput a, PresenceInput b)
        {
            return new PresenceInput
            {
                Channels = a.Channels?.Union(b.Channels ?? new string[0]),
                ChannelGroups = a.ChannelGroups?.Union(b.ChannelGroups ?? new string[0]),
            };
        }

        public static PresenceInput operator -(PresenceInput a, PresenceInput b)
        {
            return new PresenceInput
            {
                Channels = a.Channels?.Except(b.Channels ?? new string[0]),
                ChannelGroups = a.ChannelGroups?.Except(b.ChannelGroups ?? new string[0]),
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
