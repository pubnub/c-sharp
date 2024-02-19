using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class CooldownTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new CooldownState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new CooldownState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new CooldownState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
            },
            new object[] {
                new CooldownState(),
                new LeftAllEvent(),
                new InactiveState(),
            },
            new object[] {
                new CooldownState(),
                new HeartbeatSuccessEvent(),
                null,
            },
            new object[] {
                new CooldownState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                null,
            },
            new object[] {
                new CooldownState(),
                new HeartbeatGiveUpEvent() { Status = new PNStatus() { Category = PNStatusCategory.PNCancelledCategory } },
                null,
            },
            new object[] {
                new CooldownState(),
                new ReconnectEvent(),
                null,
            },
            new object[] {
                new CooldownState(),
                new DisconnectEvent(),
                new StoppedState(),
            },
            new object[] {
                new CooldownState(),
                new TimesUpEvent(),
                new HeartbeatingState(),
            },
        };

        [TestCasesSource(nameof(testCases))]
        public void TestTransition(State @sut, IEvent @ev, State @expected)
        {
            Assert.AreEqual(expected, sut.Transition(ev));
        }
    }
}
