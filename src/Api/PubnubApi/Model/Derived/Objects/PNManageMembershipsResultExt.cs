using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNManageMembershipsResultExt : PNCallback<PNManageMembershipsResult>
    {
        readonly Action<PNManageMembershipsResult, PNStatus> callbackAction;

        public PNManageMembershipsResultExt(Action<PNManageMembershipsResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNManageMembershipsResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
