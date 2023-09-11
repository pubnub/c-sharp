using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EventEngine.Common
{
    public class Delay
    {
        public bool Cancelled { get; private set; } = false;
        private readonly TaskCompletionSource<object> taskCompletionSource  = new TaskCompletionSource<object>();
        private readonly object monitor = new object();
        private readonly int milliseconds;

        public Delay(int milliseconds)
        {
            this.milliseconds = milliseconds;
        }

        public Task Start()
        {
            #if NETFX_CORE || WINDOWS_UWP || UAP || NETSTANDARD10 || NETSTANDARD11 || NETSTANDARD12
            Task taskAwaiter = Task.Factory.StartNew(AwaiterLoop);
            taskAwaiter.Wait();
            #else
            Thread awaiterThread = new Thread(AwaiterLoop);
            awaiterThread.Start();
            #endif
            return taskCompletionSource.Task;        }

        public void Cancel()
        {
            lock (monitor)
            {
                Cancelled = true;
                Monitor.Pulse(monitor);
            }
        }

        private void AwaiterLoop()
        {
            while(true)
            {
                lock (monitor)
                {
                    if (Cancelled)
                    {
                        taskCompletionSource.SetCanceled();
                        break;
                    }
                    Monitor.Wait(monitor, milliseconds);
                    if (Cancelled)
                    {
                        taskCompletionSource.SetCanceled();
                        break;
                    }
                    taskCompletionSource.SetResult(null);
                }
            }
        }
    }
}