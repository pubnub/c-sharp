using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetMembersResultExt : PNCallback<PNGetMembersResult>
    {
        readonly Action<PNGetMembersResult, PNStatus> callbackAction;

        public PNGetMembersResultExt(Action<PNGetMembersResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetMembersResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
