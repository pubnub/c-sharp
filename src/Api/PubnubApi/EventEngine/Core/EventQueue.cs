using System.Collections.Generic;
using System.Linq;

namespace PubnubApi.PubnubEventEngine.Core {
	internal class EventQueue {
		private volatile Queue<IEvent> eventQueue = new Queue<IEvent>();
		private object lockObj = new object();

		public event System.Action<EventQueue> onEventQueued;

		public void Enqueue(IEvent e) {
			lock (lockObj) {
				// TODO de-dupe? Throttle?
				eventQueue.Enqueue(e);
				onEventQueued?.Invoke(this);
			}
		}

		public IEvent Dequeue() {
			lock (lockObj) {
				return eventQueue.Any() ? eventQueue.Dequeue() : null;
			}
		}
	}
}