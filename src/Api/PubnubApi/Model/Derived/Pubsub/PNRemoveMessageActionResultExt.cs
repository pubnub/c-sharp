using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNRemoveMessageActionResultExt : PNCallback<PNRemoveMessageActionResult>
    {
        readonly Action<PNRemoveMessageActionResult, PNStatus> callbackAction;

        public PNRemoveMessageActionResultExt(Action<PNRemoveMessageActionResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNRemoveMessageActionResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
