using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class InactiveStateTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new InactiveState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
            },
            new object[] {
                new InactiveState(),
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                null,
            },
            new object[] {
                new InactiveState(),
                new LeftAllEvent(),
                null,
            },
            new object[] {
                new InactiveState(),
                new HeartbeatSuccessEvent(),
                null,
            },
            new object[] {
                new InactiveState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                null,
            },
            new object[] {
                new InactiveState(),
                new ReconnectEvent(),
                null,
            },
            new object[] {
                new InactiveState(),
                new DisconnectEvent(),
                null,
            },
            new object[] {
                new InactiveState(),
                new TimesUpEvent(),
                null,
            },
        };

        [TestCaseSource(nameof(testCases))]
        public void TestTransition(State @sut, IEvent @ev, State @expected)
        {
            var result = @sut.Transition(@ev);

            if (result == null && expected == null)
            {
                // it's expected result
                return;
            }

            Assert.AreEqual(@expected.GetType(), result.State.GetType());
        }
    }
}
