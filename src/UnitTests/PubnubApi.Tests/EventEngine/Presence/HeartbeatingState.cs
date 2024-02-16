using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class HeartbeatingStateTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new HeartbeatingState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
            },
            new object[] {
                new HeartbeatingState(),
                new LeftAllEvent(),
                new InactiveState(),
            },
            new object[] {
                new HeartbeatingState(),
                new HeartbeatSuccessEvent(),
                new CooldownState(),
            },
            new object[] {
                new HeartbeatingState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                new ReconnectingState(),
            },
            new object[] {
                new HeartbeatingState(),
                new ReconnectEvent(),
                null,
            },
            new object[] {
                new HeartbeatingState(),
                new DisconnectEvent(),
                new StoppedState(),
            },
            new object[] {
                new HeartbeatingState(),
                new TimesUpEvent(),
                null,
            },
        };

        [TestCaseSource(nameof(testCases))]
        public void TestTransition(State @sut, IEvent @ev, State @expected)
        {
            Assert.AreEqual(expected, sut.Transition(ev).State);
        }
    }
}
