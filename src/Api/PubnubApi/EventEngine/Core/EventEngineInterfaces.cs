using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PubnubApi.EventEngine.Core
{
    /// <summary>
    /// Generic effect handler.
    /// </summary>
    public interface IEffectHandler
    {
        Task Cancel();
        Task Run(IEffectInvocation invocation);
        bool IsBackground(IEffectInvocation invocation);
    }

    /// <summary>
    /// Handler (implementation) for a given invocation. The invocation represents the input arguments of a handler.
    /// </summary>
    /// <typeparam name="T">Associated invocation</typeparam>
    public interface IEffectHandler<T> : IEffectHandler where T : IEffectInvocation
    {
        Task Run(T invocation);
        bool IsBackground(T invocation);
    }

    public abstract class EffectHandler<T> : IEffectHandler<T>
        where T : class, IEffectInvocation
    {
        public abstract Task Cancel();

        public Task Run(IEffectInvocation invocation) => Run(invocation as T);

        public bool IsBackground(IEffectInvocation invocation) => IsBackground(invocation as T);

        public abstract Task Run(T invocation);

        public abstract bool IsBackground(T invocation);
    }

    public abstract class EffectCancellableHandler<T1, T2> : EffectHandler<T1>, IEffectHandler<T2>
        where T1 : class, IEffectInvocation
        where T2 : class, IEffectCancelInvocation
    {
        public Task Run(T2 invocation)
        {
            throw new NotImplementedException();
        }

        public bool IsBackground(T2 invocation) => false;
    }
   

    public abstract class EffectDoubleHandler<T1, T2> : EffectHandler<T1>, IEffectHandler<T2>
        where T1 : class, IEffectInvocation
        where T2 : class, IEffectInvocation
    {

        public new Task Run(IEffectInvocation invocation) =>
            invocation is T1 effectInvocation ? Run(effectInvocation) : Run(invocation as T2);

        public new bool IsBackground(IEffectInvocation invocation) => 
            invocation is T1 effectInvocation ? IsBackground(effectInvocation) : IsBackground(invocation as T2);

        public abstract Task Run(T2 invocation);

        public abstract bool IsBackground(T2 invocation);
    }

    public abstract class EffectDoubleCancellableHandler<T1, T2, T3> : EffectDoubleHandler<T1, T2>, IEffectHandler<T3>
        where T1 : class, IEffectInvocation
        where T2 : class, IEffectInvocation
        where T3 : class, IEffectCancelInvocation
    {
        public Task Run(T3 invocation)
        {
            throw new NotImplementedException();
        }

        public bool IsBackground(T3 invocation) => false;
    }


    /// <summary>
    /// An effect invocation. It represents calling <c>Run()</c> on a registered effect handler - calling it is orchestrated by the dispatcher.
    /// </summary>
    public interface IEffectInvocation
    {
    }

    /// <summary>
    /// A cancel effect invocation. It represents calling <c>Cancel()</c> on a registered effect handler - calling it is orchestrated by the dispatcher.
    /// </summary>
    public interface IEffectCancelInvocation : IEffectInvocation
    {
    }

    public interface IEvent
    {
    };

    public abstract class State
    {
        public virtual IEnumerable<IEffectInvocation> OnEntry { get; } = null;
        public virtual IEnumerable<IEffectInvocation> OnExit { get; } = null;

        /// <summary>
        /// The EE transition pure function.
        /// </summary>
        /// <param name="e">Input event</param>
        /// <returns>Target state and invocation list, or null for no-transition</returns>
        public abstract TransitionResult Transition(IEvent e);

        public TransitionResult With(params IEffectInvocation[] invocations)
        {
            return new TransitionResult(this, invocations);
        }

        public static implicit operator TransitionResult(State s)
        {
            return new TransitionResult(s);
        }
    }
}