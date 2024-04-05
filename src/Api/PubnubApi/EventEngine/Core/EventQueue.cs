using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PubnubApi.EventEngine.Core
{
    public class EventQueue
    {
        private volatile Queue<IEvent> eventQueue = new Queue<IEvent>();
        private object lockObj = new object();

        public event System.Action<EventQueue> OnEventQueued;

        /// <summary>
        /// Enqueue (fire) an event to the Event Engine. Handling that event is covered by the Engine itself.
        /// </summary>
        /// <param name="e">Event to be fired</param>
        public void Enqueue(IEvent e)
        {
            lock (lockObj)
            {
                // TODO de-dupe? Throttle?
                eventQueue.Enqueue(e);
                OnEventQueued?.Invoke(this);
            }
        }

        private IEvent Dequeue()
        {
            lock (lockObj)
            {
                return eventQueue.Any() ? eventQueue.Dequeue() : null;
            }
        }

        public IEvent Peek()
        {
            lock (lockObj)
            {
                return eventQueue.Peek();
            }
        }

        public async Task Loop<T>(System.Func<IEvent, Task<T>> function)
        {
            while (Count > 0)
            {
                await function(Dequeue());
            }
        }

        public int Count
        {
            get
            {
                lock (lockObj)
                {
                    return eventQueue.Count;
                }
            }
        }
    }
}