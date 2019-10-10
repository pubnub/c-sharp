using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetMessageActionsResultExt : PNCallback<PNGetMessageActionsResult>
    {
        readonly Action<PNGetMessageActionsResult, PNStatus> callbackAction;

        public PNGetMessageActionsResultExt(Action<PNGetMessageActionsResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetMessageActionsResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
