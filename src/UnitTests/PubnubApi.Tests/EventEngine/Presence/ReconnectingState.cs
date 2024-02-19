using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;
using System.Linq;

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
