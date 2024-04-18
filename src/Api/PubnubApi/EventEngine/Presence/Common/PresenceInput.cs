using System.Collections.Generic;
using System.Linq;

namespace PubnubApi.EventEngine.Presence.Common
{
    public class PresenceInput
    {
        public IEnumerable<string> Channels { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> ChannelGroups { get; set; } = Enumerable.Empty<string>();

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

        public override bool Equals(object obj)
        {
            if (obj is null || obj is not PresenceInput)
                return false;

            var typedObj = obj as PresenceInput;
            return this.Channels.SequenceEqual(typedObj.Channels)
                && this.ChannelGroups.SequenceEqual(typedObj.ChannelGroups);
        }
    }
}
