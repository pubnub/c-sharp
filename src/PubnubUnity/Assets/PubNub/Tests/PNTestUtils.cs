using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PubnubApi;
using UnityEngine;


namespace PubnubApi.Unity.Tests {
    public sealed class AwaiterResult<T> where T : class {
        public T result = null;

        public static implicit operator T(AwaiterResult<T> t) {
            return t;
        }
    }


    public sealed class Awaiter<T> : CustomYieldInstruction {
        private T taskResult;
        private bool complete = false;
        private readonly Action<T> resultCallback;
        private readonly Task<T> task;
        private readonly float startTime;

        // hard-coded timeout for now
        public override bool keepWaiting => Time.time < startTime + 10f && !complete;

        public Awaiter(Task<T> task, Action<T> resultCallback) {
            this.task = task;
            this.resultCallback = resultCallback;
            RunTask();
            startTime = Time.time;
        }

        async void RunTask() {
            taskResult = await task;
            resultCallback?.Invoke(taskResult);
            await Task.Delay(1);
            complete = true;
        }
    }

    public static class TaskExtension {
        public static IEnumerator YieldTask<T>(this Task<T> t, out Func<AwaiterResult<T>> resultAssigner)
            where T : class {
            AwaiterResult<T> result = new();
            resultAssigner = () => result;
            return new Awaiter<T>(t, (res) => result.result = res);
        }
    }


    public sealed class CallbackResult<T> {
        public T result = default(T);
        public PNStatus status = null;
    }
}