using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMembershipsResultExt : PNCallback<PNMembershipsResult>
    {
        readonly Action<PNMembershipsResult, PNStatus> callbackAction;

        public PNMembershipsResultExt(Action<PNMembershipsResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNMembershipsResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
