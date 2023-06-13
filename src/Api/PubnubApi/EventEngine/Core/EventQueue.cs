﻿using System.Collections.Generic;
using System.Linq;

namespace PubnubApi.PubnubEventEngine.Core {
	internal class EventQueue {
		private volatile Queue<Event> eventQueue = new Queue<Event>();
		private object lockObj = new object();

		public event System.Action<EventQueue> onEventQueued;

		public void Enqueue(Event e) {
			lock (lockObj) {
				// TODO de-dupe? Throttle?
				eventQueue.Enqueue(e);
				onEventQueued?.Invoke(this);
			}
		}

		public Event Dequeue() {
			lock (lockObj) {
				return eventQueue.Any() ? eventQueue.Dequeue() : null;
			}
		}
	}
}