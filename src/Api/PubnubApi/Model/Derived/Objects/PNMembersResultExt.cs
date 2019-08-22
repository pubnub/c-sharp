using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembersResultExt : PNCallback<PNMembersResult>
    {
        readonly Action<PNMembersResult, PNStatus> callbackAction;

        public PNMembersResultExt(Action<PNMembersResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNMembersResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
