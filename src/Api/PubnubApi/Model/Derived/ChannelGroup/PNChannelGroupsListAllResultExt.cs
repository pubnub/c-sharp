using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNChannelGroupsListAllResultExt : PNCallback<PNChannelGroupsListAllResult>
    {
        Action<PNChannelGroupsListAllResult, PNStatus> callbackAction = null;

        public PNChannelGroupsListAllResultExt(Action<PNChannelGroupsListAllResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNChannelGroupsListAllResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
