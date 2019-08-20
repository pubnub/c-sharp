using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNDeleteSpaceResultExt : PNCallback<PNDeleteSpaceResult>
    {
        readonly Action<PNDeleteSpaceResult, PNStatus> callbackAction;

        public PNDeleteSpaceResultExt(Action<PNDeleteSpaceResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNDeleteSpaceResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
