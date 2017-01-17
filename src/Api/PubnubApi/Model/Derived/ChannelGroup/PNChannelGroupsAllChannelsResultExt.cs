using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNChannelGroupsAllChannelsResultExt : PNCallback<PNChannelGroupsAllChannelsResult>
    {
        Action<PNChannelGroupsAllChannelsResult, PNStatus> callbackAction = null;

        public PNChannelGroupsAllChannelsResultExt(Action<PNChannelGroupsAllChannelsResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNChannelGroupsAllChannelsResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
