using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading
{
#if NETSTANDARD10 || NETSTANDARD11 || PORTABLE111
    public delegate void TimerCallback(object state);

    public sealed class Timer : CancellationTokenSource, IDisposable
    {
        TimerCallback callback;
        object state;

        public Timer(TimerCallback callback, object state, int dueTime, int period)
        {
            Init(callback, state, dueTime, period);
        }

        public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            Init(callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
        }

        public Timer(TimerCallback callback)
        {
            Init(callback, this, Timeout.Infinite, Timeout.Infinite);
        }

        void Init(TimerCallback callback, object state, long dueTime, long period)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            this.callback = callback;
            this.state = state;

            Change(dueTime, period, true);
        }


        public new void Dispose()
        {
            base.Cancel();
        }

        public bool Change(int dueTime, int period)
        {
            return Change(dueTime, period, false);
        }


        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return Change((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds, false);
        }


        public bool Change(uint dueTime, uint period)
        {
            // convert all values to long - with a special case for -1 / 0xffffffff
            long d = (dueTime == UInt32.MaxValue) ? Timeout.Infinite : (long)dueTime;
            long p = (period == UInt32.MaxValue) ? Timeout.Infinite : (long)period;
            return Change(d, p, false);
        }

        public bool Change(long dueTime, long period)
        {
            return Change(dueTime, period, false);
        }

        bool Change(long dueTime, long period, bool first)
        {
            bool status = false;

            if (dueTime < Timeout.Infinite)
                throw new ArgumentOutOfRangeException("dueTime");


            if (period < Timeout.Infinite)
                throw new ArgumentOutOfRangeException("period");


            Task.Delay((int)dueTime, Token).ContinueWith(async (t, s) =>
            {
                var tuple = (Tuple<TimerCallback, object>)s;

                while (true)
                {
                    if (IsCancellationRequested)
                    {
                        status = false;
                        break;
                    }
                    await Task.Run(() => tuple.Item1(tuple.Item2));
                    await Task.Delay((int)period);

                    status = true;
                }

            }, Tuple.Create(callback, state), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);

            return status;
        }


    }

#endif
}

