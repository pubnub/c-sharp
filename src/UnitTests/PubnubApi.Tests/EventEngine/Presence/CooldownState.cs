using NUnit.Framework;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using PubnubApi.EventEngine.Presence.Events;
using PubnubApi.EventEngine.Presence.States;
using PubnubApi.EventEngine.Presence.Invocations;

namespace PubnubApi.Tests.EventEngine.Presence
{
    internal class CooldownTransitions
    {
        // Test case:
        // - Current state
        // - Event
        // - Expected next state
        // - Invocations
        private static readonly object[] testCases = {
            new object[] {
                new CooldownState(),
                new JoinedEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                null
            },
            new object[] {
                new CooldownState() { Input = new PresenceInput() { Channels = new [] { "a", "b" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "b" } } },
                new HeartbeatingState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() { Channels = new [] { "b" } } } }
            },
            new object[] {
                new CooldownState() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new LeftEvent() { Input = new PresenceInput() { Channels = new [] { "a" } } },
                new InactiveState(),
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() { Channels = new [] { "a" } } } }
            },
            new object[] {
                new CooldownState(),
                new LeftAllEvent(),
                new InactiveState(),
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() { Channels = new string[] { } } } }
            },
            new object[] {
                new CooldownState(),
                new HeartbeatSuccessEvent(),
                null,
                null
            },
            new object[] {
                new CooldownState(),
                new HeartbeatFailureEvent() { Status = new PNStatus() },
                null,
                null
            },
            new object[] {
                new CooldownState(),
                new ReconnectEvent(),
                null,
                null
            },
            new object[] {
                new CooldownState(),
                new DisconnectEvent(),
                new StoppedState(),
                new IEffectInvocation[] { new LeaveInvocation() { Input = new PresenceInput() { Channels = new string[] { } } } }
            },
            new object[] {
                new CooldownState(),
                new TimesUpEvent(),
                new HeartbeatingState(),
                null
            },
        };

        [TestCaseSource(nameof(testCases))]
        public void TestTransition(State @sut, IEvent @ev, State @expected, IEffectInvocation[] @_)
        {
            Assert.AreEqual(@expected, @sut.Transition(@ev));
        }

        [TestCaseSource(nameof(testCases))]
        public void TestReturnedInvocations(State @sut, IEvent @ev, State @_, IEffectInvocation[] @expected)
        {
            CollectionAssert.AreEqual(@expected, @sut.Transition(@ev).Invocations);
        }
    }
}
