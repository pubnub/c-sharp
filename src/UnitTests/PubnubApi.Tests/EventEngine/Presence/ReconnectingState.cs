using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class ReconnectingStateTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new ReconnectingState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new ReconnectingState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new ReconnectingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
            },
            new object[] {
                new ReconnectingState(),
                new LeftAllEvent(),
                new InactiveState(),
            },
            new object[] {
                new ReconnectingState(),
                new HeartbeatSuccessEvent(),
                new CooldownState(),
            },
            new object[] {
                new ReconnectingState() { RetryCount = 1 },
                new HeartbeatFailureEvent(),
                new ReconnectingState() { RetryCount = 2 },
            },
            new object[] {
                new ReconnectingState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() { Category = PNStatusCategory.PNCancelledCategory} },
                null,
            },
            new object[] {
                new ReconnectingState(),
                new HeartbeatGiveUpEvent(),
                new FailedState(),
            },
            new object[] {
                new ReconnectingState(),
                new ReconnectEvent(),
                null,
            },
            new object[] {
                new ReconnectingState(),
                new DisconnectEvent(),
                new StoppedState(),
            },
            new object[] {
                new ReconnectingState(),
                new TimesUpEvent(),
                null,
            },
        };

        [TestCasesSource(nameof(testCases))]
        public void TestTransition(State @sut, IEvent @ev, State @expected)
        {
            Assert.AreEqual(expected, sut.Transition(ev));
        }
    }
}
