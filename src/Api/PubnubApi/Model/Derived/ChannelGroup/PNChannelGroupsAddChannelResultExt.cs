using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNChannelGroupsAddChannelResultExt : PNCallback<PNChannelGroupsAddChannelResult>
    {
        Action<PNChannelGroupsAddChannelResult, PNStatus> callbackAction = null;

        public PNChannelGroupsAddChannelResultExt(Action<PNChannelGroupsAddChannelResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNChannelGroupsAddChannelResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
