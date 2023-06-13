using System.Collections.Generic;
using System.Linq;

namespace PubnubApi.PubnubEventEngine.Core {
	public class EventQueue {
		private volatile Queue<Event> eventQueue = new Queue<Event>();
		private object lockObj = new object();

		public event System.Action onEventQueued;

		public void Enqueue(Event e) {
			lock (lockObj) {
				// TODO de-dupe? Throttle?
				eventQueue.Enqueue(e);
				onEventQueued?.Invoke();
			}
		}

		public Event Dequeue() {
			lock (lockObj) {
				return eventQueue.Any() ? eventQueue.Dequeue() : null;
			}
		}
	}
}