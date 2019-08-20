using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNCreateSpaceResultExt : PNCallback<PNCreateSpaceResult>
    {
        readonly Action<PNCreateSpaceResult, PNStatus> callbackAction;

        public PNCreateSpaceResultExt(Action<PNCreateSpaceResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNCreateSpaceResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
