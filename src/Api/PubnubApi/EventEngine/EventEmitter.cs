using System;

namespace PubnubApi.PubnubEventEngine
{
	public class EventEmitter
	{
		private Action<Event>? handler;
		private Action<string, bool>? jsonListener;

		public void RegisterHandler(Action<Event> eventHandler)
		{
			this.handler = eventHandler;
		}

		public void RegisterJsonListener(Action<string, bool> listenerHandler)
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

		public void emit(string json, bool zeroTT)
		{
			if (jsonListener != null)
			{
				jsonListener(json, zeroTT);
			}
		}
	}
}
