using System;

namespace PubnubApi.PubnubEventEngine
{
	public class EventEmitter
	{
		private Action<Event>? handler;
		private Action<string, bool, int>? jsonListener;

		public void RegisterHandler(Action<Event> eventHandler)
		{
			this.handler = eventHandler;
		}

		public void RegisterJsonListener(Action<string, bool, int> listenerHandler)
		{
			this.jsonListener = listenerHandler;
		}

		public void emit(Event e)
		{
			if (handler == null)
			{
				throw new MissingMemberException("eventHandler is missing");
			}
			this.handler(e);
		}

		public void emit(string json, bool zeroTimetokenRequest, int messageCount)
		{
			if (jsonListener != null)
			{
				jsonListener(json, zeroTimetokenRequest, messageCount);
			}
		}
	}
}
