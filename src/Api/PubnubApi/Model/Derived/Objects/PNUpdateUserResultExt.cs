using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNUpdateUserResultExt : PNCallback<PNUpdateUserResult>
    {
        readonly Action<PNUpdateUserResult, PNStatus> callbackAction;

        public PNUpdateUserResultExt(Action<PNUpdateUserResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNUpdateUserResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
