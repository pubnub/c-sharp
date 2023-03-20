using System;

namespace PubnubApi.PubnubEventEngine
{
	public class EventEmitter
	{
		public Action<Event>? handler;

		public void RegisterHandler(Action<Event> eventHandler)
		{
			this.handler = eventHandler;
		}

		public void emit(Event e)
		{
			if (handler == null)
			{
				throw new MissingMemberException("eventHandler is missing");
			}
			this.handler(e);
		}
	}
}
