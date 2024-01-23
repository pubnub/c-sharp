using System.Collections.Generic;
using System.Linq;

namespace PubnubApi.EventEngine.Presence.Common
{
    public class PresenceInput
    {
        public List<string> Channels { get; set; }
        public List<string> ChannelGroups { get; set; }

        public static PresenceInput operator +(PresenceInput a, PresenceInput b)
        {
            return new PresenceInput
            {
                Channels = a.Channels?.Union(b.Channels ?? new List<string>()).ToList(),
                ChannelGroups = a.ChannelGroups?.Union(b.ChannelGroups ?? new List<string>()).ToList(),
            };
        }

        public static PresenceInput operator -(PresenceInput a, PresenceInput b)
        {
            return new PresenceInput
            {
                Channels = a.Channels?.Except(b.Channels ?? new List<string>()).ToList(),
                ChannelGroups = a.ChannelGroups?.Except(b.ChannelGroups ?? new List<string>()).ToList(),
            };
        }

        public bool IsEmpty()
        {
            return (Channels == null && ChannelGroups == null)
                || ((Channels != null && Channels.Count == 0)
                        && (ChannelGroups != null && ChannelGroups.Count == 0));
        }
    }
}
