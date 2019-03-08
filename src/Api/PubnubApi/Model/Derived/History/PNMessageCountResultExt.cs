using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNMessageCountResultExt : PNCallback<PNMessageCountResult>
    {
        readonly Action<PNMessageCountResult, PNStatus> callbackAction;

        public PNMessageCountResultExt(Action<PNMessageCountResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNMessageCountResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
