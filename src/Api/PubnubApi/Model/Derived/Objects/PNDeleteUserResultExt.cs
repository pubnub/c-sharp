using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNDeleteUserResultExt : PNCallback<PNDeleteUserResult>
    {
        readonly Action<PNDeleteUserResult, PNStatus> callbackAction;

        public PNDeleteUserResultExt(Action<PNDeleteUserResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNDeleteUserResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
