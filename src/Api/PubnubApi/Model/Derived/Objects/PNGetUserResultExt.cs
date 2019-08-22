using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetUserResultExt : PNCallback<PNGetUserResult>
    {
        readonly Action<PNGetUserResult, PNStatus> callbackAction;

        public PNGetUserResultExt(Action<PNGetUserResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetUserResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
