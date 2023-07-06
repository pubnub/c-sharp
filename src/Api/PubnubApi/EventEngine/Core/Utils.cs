using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Core
{
    internal static class Utils
    {
        static Utils()
        {
            EmptyTask.Start();
        }
        
        internal static Task<IState> EmptyTask { get; } = new Task<IState>(() => null); 
        
        internal static Tuple<IState, IEnumerable<IEffectInvocation>> With(this IState state, params IEffectInvocation[] invocations)
        {
            return new Tuple<IState, IEnumerable<IEffectInvocation>>(state, invocations); 
        }

        internal static IEffectInvocation[] AsArray(this IEffectInvocation invocation)
        {
            return new IEffectInvocation[] { invocation };
        }
    }
}