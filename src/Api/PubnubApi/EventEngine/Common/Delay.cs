using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EventEngine.Common
{
    public class Delay
    {
        public bool Cancelled { get; private set; } = false;
        private TaskCompletionSource<object> taskCompletionSource;
        private readonly object monitor = new object();
        private readonly int milliseconds;

        public Delay(int milliseconds)
        {
            this.milliseconds = milliseconds;
        }

        public Task Start()
        {
            taskCompletionSource = new TaskCompletionSource<object>();
            Cancelled = false;
            #if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task taskAwaiter = Task.Factory.StartNew(AwaiterLoop);
            #else
            Thread awaiterThread = new Thread(AwaiterLoop);
            awaiterThread.Start();
            #endif
            return taskCompletionSource.Task;
        }

        public void Cancel()
        {
            if (Cancelled) return;
            lock (monitor)
            {
                Cancelled = true;
                Monitor.Pulse(monitor);
            }
        }

        private void AwaiterLoop()
        {
            lock (monitor)
            {
                if (Cancelled)
                {
                    taskCompletionSource.SetCanceled();
                    return;
                }
                Monitor.Wait(monitor, milliseconds);
                if (Cancelled)
                {
                    taskCompletionSource.SetCanceled();
                    return;
                }
                taskCompletionSource.SetResult(null);
                Cancelled = true;
            }
        }
    }
}