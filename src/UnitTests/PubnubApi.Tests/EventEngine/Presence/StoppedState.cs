using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;
using System.Linq;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class StoppedStateTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new StoppedState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new StoppedState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                null
            },
            new object[] {
                new StoppedState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new StoppedState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                null
            },
            new object[] {
                new StoppedState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
                null
            },
            new object[] {
                new StoppedState(),
                new LeftAllEvent(),
                new InactiveState(),
                null
            },
            new object[] {
                new StoppedState(),
                new HeartbeatSuccessEvent(),
                null,
                null
            },
            new object[] {
                new StoppedState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                null,
                null
            },
            new object[] {
                new StoppedState(),
                new ReconnectEvent(),
                new HeartbeatingState(),
                null
            },
            new object[] {
                new StoppedState(),
                new DisconnectEvent(),
                null,
                null
            },
            new object[] {
                new StoppedState(),
                new TimesUpEvent(),
                null,
                null
            },
        };

        [TestCaseSource(nameof(testCases))]
        public void TestTransition(APresenceState @sut, IEvent @ev, APresenceState @expected, IEffectInvocation[] @_)
        {
            var result = @sut.Transition(@ev);

            if (result == null && expected == null)
            {
                // it's expected result
                return;
            }

            Assert.AreEqual(@expected, result.State);
        }

        [TestCaseSource(nameof(testCases))]
        public void TestReturnedInvocations(State @sut, IEvent @ev, State @_, IEffectInvocation[] @expected)
        {
            var result = @sut.Transition(@ev);

            if (result == null && expected == null)
            {
                // it's expected result
                return;
            }

            foreach (var item in result.Invocations)
            {
                Assert.True(expected.Select(i => i.GetType()).Contains(item.GetType()));
            }
        }
    }
}
