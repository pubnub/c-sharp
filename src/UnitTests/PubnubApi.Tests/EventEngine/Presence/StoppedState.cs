using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class StoppedStateTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new StoppedState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new StoppedState(),
            },
            new object[] {
                new StoppedState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new StoppedState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new StoppedState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
            },
            new object[] {
                new StoppedState(),
                new LeftAllEvent(),
                new InactiveState(),
            },
            new object[] {
                new StoppedState(),
                new HeartbeatSuccessEvent(),
                null,
            },
            new object[] {
                new StoppedState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                null,
            },
            new object[] {
                new StoppedState(),
                new ReconnectEvent(),
                new HeartbeatingState(),
            },
            new object[] {
                new StoppedState(),
                new DisconnectEvent(),
                null,
            },
            new object[] {
                new StoppedState(),
                new TimesUpEvent(),
                null,
            },
        };

        [TestCaseSource(nameof(testCases))]
        public void TestTransition(State @sut, IEvent @ev, State @expected)
        {
            Assert.AreEqual(@expected.GetType(), @sut.Transition(@ev).State.GetType());
        }
    }
}
