// TODO: Dummy Invocation until we have real ones
namespace PubnubApi.EventEngine.Presence.Invocations {
    public class DummyInvocation : Core.IEffectInvocation {}

    DummyInvocation[] DummyInvocations() {
        return new DummyInvocation[] {
            new DummyInvocation(),
        };
    }
}
