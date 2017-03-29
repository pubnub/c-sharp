using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNChannelGroupsRemoveChannelResultExt : PNCallback<PNChannelGroupsRemoveChannelResult>
    {
        Action<PNChannelGroupsRemoveChannelResult, PNStatus> callbackAction = null;

        public PNChannelGroupsRemoveChannelResultExt(Action<PNChannelGroupsRemoveChannelResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNChannelGroupsRemoveChannelResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
