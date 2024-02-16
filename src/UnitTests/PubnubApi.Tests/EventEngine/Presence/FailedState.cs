using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class FailedStateTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new FailedState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new FailedState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new FailedState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
            },
            new object[] {
                new FailedState(),
                new LeftAllEvent(),
                new InactiveState(),
            },
            new object[] {
                new FailedState(),
                new HeartbeatSuccessEvent(),
                null,
            },
            new object[] {
                new FailedState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                null,
            },
            new object[] {
                new FailedState(),
                new ReconnectEvent(),
                new HeartbeatingState(),
            },
            new object[] {
                new FailedState(),
                new DisconnectEvent(),
                new StoppedState(),
            },
            new object[] {
                new FailedState(),
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
