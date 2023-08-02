using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

namespace PubnubApi.EventEngine.Core
{
    internal static class Utils
    {
        static Utils()
        {
            EmptyTask.Start();
        }
        
        internal static Task<State> EmptyTask { get; } = new Task<State>(() => null);

        internal static IEffectInvocation[] AsArray(this IEffectInvocation invocation)
        {
            return new IEffectInvocation[] { invocation };
        }

        internal static bool IsBackground(this IEffectHandler handler, IEffectInvocation invocation)
        {
            return (bool)handler.GetType()
                .GetMethod("IsBackground")
                .Invoke(handler, new object[] { invocation });
        }
        
        internal static Task Run(this IEffectHandler handler, IEffectInvocation invocation)
        {
            return (Task)handler.GetType()
                .GetMethod("Run")
                .Invoke(handler, new object[] { invocation });
        }
    }

    public class TransitionResult
    {
        public State State => tuple.Item1;
        public IEnumerable<IEffectInvocation> Invocations => tuple.Item2;
        
        private readonly Tuple<State, IEnumerable<IEffectInvocation>> tuple;

        /// <summary>
        /// Create a state-invocation pair with empty invocations
        /// </summary>
        public TransitionResult(State state)
        {
            this.tuple = new Tuple<State, IEnumerable<IEffectInvocation>>(state, new IEffectInvocation[0]);
        }

        /// <summary>
        /// Create a state-invocation pair
        /// </summary>
        public TransitionResult(State state, IEnumerable<IEffectInvocation> invocations)
        {
            this.tuple = new Tuple<State, IEnumerable<IEffectInvocation>>(state, invocations);
        }

        /// <summary>
        /// Create a state-invocation pair
        /// </summary>
        public TransitionResult(State state, params IEffectInvocation[] invocations) : this(state, invocations as IEnumerable<IEffectInvocation>) { }

        public static implicit operator Tuple<State, IEnumerable<IEffectInvocation>>(TransitionResult t)
        {
            return t.tuple;
        }

        public static implicit operator TransitionResult(Tuple<State, IEnumerable<IEffectInvocation>> t)
        {
            return new TransitionResult(t.Item1, t.Item2);
        }

        public override int GetHashCode()
        {
            return tuple.GetHashCode();
        }
    }
}