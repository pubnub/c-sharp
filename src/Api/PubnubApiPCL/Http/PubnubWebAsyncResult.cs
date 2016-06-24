using System;
using System.Threading;

namespace PubnubApi
{
    internal class PubnubWebAsyncResult : IAsyncResult
    {
        private const int pubnubDefaultLatencyInMilliSeconds = 1;
        //PubnubDefaultLatencyInMilliSeconds
        private readonly AsyncCallback _callback;
        private readonly object _state;
        private readonly ManualResetEvent _waitHandle;
        private readonly Timer _timer;

        public bool IsCompleted
        {
            get;
            private set;
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _waitHandle; }
        }

        public object AsyncState
        {
            get { return _state; }
        }

        public bool CompletedSynchronously
        {
            get { return IsCompleted; }
        }

        public PubnubWebAsyncResult(AsyncCallback callback, object state)
            : this(callback, state, TimeSpan.FromMilliseconds(pubnubDefaultLatencyInMilliSeconds))
        {
        }

        public PubnubWebAsyncResult(AsyncCallback callback, object state, TimeSpan latency)
        {
            IsCompleted = false;
            _callback = callback;
            _state = state;
            _waitHandle = new ManualResetEvent(false);
            _timer = new Timer(onTimer => NotifyComplete(), null, latency, TimeSpan.FromMilliseconds(-1));
        }

        public void Abort()
        {
            _timer.Dispose();
            NotifyComplete();
        }

        private void NotifyComplete()
        {
            IsCompleted = true;
            _waitHandle.Set();
            if (_callback != null)
                _callback(this);
        }
    }
}
