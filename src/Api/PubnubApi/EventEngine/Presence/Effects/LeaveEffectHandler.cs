using System;
using System.Linq;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Invocations;

namespace PubnubApi.EventEngine.Presence.Effects
{
	public class LeaveEffectHandler : EffectHandler<Invocations.LeaveInvocation>
	{
		private LeaveOperation leaveOperation;
		private PNConfiguration configuration;

		public LeaveEffectHandler(PNConfiguration configuration, LeaveOperation leaveOperation)
		{
			this.leaveOperation = leaveOperation;
			this.configuration = configuration;
		}

		public override async Task Run(LeaveInvocation invocation)
		{
			try {
				if (!configuration.SuppressLeaveEvents)
					await leaveOperation.LeaveRequest<string>(
						invocation.Input.Channels?.ToArray(),
						invocation.Input.ChannelGroups?.ToArray()
					).ConfigureAwait(false);
			} catch (Exception) { }
		}

		public override bool IsBackground(LeaveInvocation invocation) => true;

		public override Task Cancel()
		{
			throw new NotImplementedException();
		}
	}
}

