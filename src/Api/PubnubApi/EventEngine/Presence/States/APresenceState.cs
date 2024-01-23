using PubnubApi.EventEngine.Core;

namespace PubnubApi.EventEngine.Presence.States
{
    public abstract class APresenceState : Core.State
    {
        public IEnumerable<string> Channels { get; set; }
        public IEnumerable<string> ChannelGroups { get; set; }

        public bool IsEmpty()
        {
            return (Channels == null && ChannelGroups == null)
                || ((Channels != null && Channels.Count() == 0)
                        && (ChannelGroups != null && ChannelGroups.Count() == 0));
        }
    }
}
