using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNDeleteFileResultExt : PNCallback<PNDeleteFileResult>
    {
        readonly Action<PNDeleteFileResult, PNStatus> callbackAction;

        public PNDeleteFileResultExt(Action<PNDeleteFileResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNDeleteFileResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
