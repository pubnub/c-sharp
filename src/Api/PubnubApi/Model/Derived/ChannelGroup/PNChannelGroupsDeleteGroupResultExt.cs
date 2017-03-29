using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNChannelGroupsDeleteGroupResultExt : PNCallback<PNChannelGroupsDeleteGroupResult>
    {
        Action<PNChannelGroupsDeleteGroupResult, PNStatus> callbackAction = null;

        public PNChannelGroupsDeleteGroupResultExt(Action<PNChannelGroupsDeleteGroupResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNChannelGroupsDeleteGroupResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
