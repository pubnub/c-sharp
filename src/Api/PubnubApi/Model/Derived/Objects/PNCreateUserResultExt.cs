using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNCreateUserResultExt : PNCallback<PNCreateUserResult>
    {
        readonly Action<PNCreateUserResult, PNStatus> callbackAction;

        public PNCreateUserResultExt(Action<PNCreateUserResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNCreateUserResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
