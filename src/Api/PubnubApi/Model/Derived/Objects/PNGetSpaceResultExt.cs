using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetSpaceResultExt : PNCallback<PNGetSpaceResult>
    {
        readonly Action<PNGetSpaceResult, PNStatus> callbackAction;

        public PNGetSpaceResultExt(Action<PNGetSpaceResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetSpaceResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
