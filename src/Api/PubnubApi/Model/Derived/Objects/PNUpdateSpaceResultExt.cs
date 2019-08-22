using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNUpdateSpaceResultExt : PNCallback<PNUpdateSpaceResult>
    {
        readonly Action<PNUpdateSpaceResult, PNStatus> callbackAction;

        public PNUpdateSpaceResultExt(Action<PNUpdateSpaceResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNUpdateSpaceResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
