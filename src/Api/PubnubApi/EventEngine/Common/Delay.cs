using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EventEngine.Common
{
    public class Delay
    {
        public bool Cancelled { get; private set; } = false;
        private readonly TaskCompletionSource<object> taskCompletionSource  = new ();
        private readonly CancellationTokenSource cancellationTokenSource = new ();
        private readonly int milliseconds;

        public Delay(int milliseconds)
        {
            this.milliseconds = milliseconds;
        }

        public Task Start()
        {
            AwaiterLoop();
            return taskCompletionSource.Task;        }

        public void Cancel()
        {
            Cancelled = true;
            cancellationTokenSource.Cancel();
        }

        private async void AwaiterLoop()
        {
            if (Cancelled)
            {
                taskCompletionSource.TrySetCanceled();
                return;
            }
            try
            {
                await Task.Delay(milliseconds, cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException e)
            {
                taskCompletionSource.TrySetCanceled();
                return;
            }
            if (Cancelled)
            {
                taskCompletionSource.TrySetCanceled();
                return;
            }
            taskCompletionSource.TrySetResult(null);
        }
    }
}