using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNChannelMembersResultExt : PNCallback<PNChannelMembersResult>
    {
        readonly Action<PNChannelMembersResult, PNStatus> callbackAction;

        public PNChannelMembersResultExt(Action<PNChannelMembersResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNChannelMembersResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
