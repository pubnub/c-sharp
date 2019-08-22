using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetMembershipsResultExt : PNCallback<PNGetMembershipsResult>
    {
        readonly Action<PNGetMembershipsResult, PNStatus> callbackAction;

        public PNGetMembershipsResultExt(Action<PNGetMembershipsResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetMembershipsResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
