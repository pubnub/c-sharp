using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.PubnubEventEngine.Core
{
    internal static class Utils
    {
        internal static Tuple<IState, IEnumerable<IEffectInvocation>> With(this IState state, params IEffectInvocation[] invocations)
        {
            return new Tuple<IState, IEnumerable<IEffectInvocation>>(state, invocations); 
        }
    }
}