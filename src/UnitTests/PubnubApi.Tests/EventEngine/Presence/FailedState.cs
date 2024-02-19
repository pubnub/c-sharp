using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;
using PubnubApi.EventEngine.Presence.Invocations;
using System.Linq;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class FailedStateTransitions
    {
        private static readonly object[] testCases = {
            new object[] {
                new FailedState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                null
            },
            new object[] {
                new FailedState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() { Channels = new [] { "b" } } } }
            },
            new object[] {
                new FailedState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() { Channels = new [] { "a" } } } }
            },
            new object[] {
                new FailedState(),
                new LeftAllEvent(),
                new InactiveState(),
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() { Channels = new string[] { } } } }
            },
            new object[] {
                new FailedState(),
                new HeartbeatSuccessEvent(),
                null,
                null
            },
            new object[] {
                new FailedState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                null,
                null
            },
            new object[] {
                new FailedState(),
                new ReconnectEvent(),
                new HeartbeatingState(),
                null
            },
            new object[] {
                new FailedState(),
                new DisconnectEvent(),
                new StoppedState(),
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() } }
            },
            new object[] {
                new FailedState(),
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
