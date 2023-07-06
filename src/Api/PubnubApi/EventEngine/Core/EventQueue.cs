using System.Collections.Generic;
using System.Linq;

namespace PubnubApi.PubnubEventEngine.Core {
	internal class EventQueue {
		private volatile Queue<IEvent> eventQueue = new Queue<IEvent>();
		private object lockObj = new object();

		public event System.Action<EventQueue> onEventQueued;

		/// <summary>
		/// Enqueue (fire) an event to the Event Engine. Handling that event is covered by the Engine itself.
		/// </summary>
		/// <param name="e">Event to be fired</param>
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