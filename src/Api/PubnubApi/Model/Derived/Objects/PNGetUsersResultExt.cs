using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetUsersResultExt : PNCallback<PNGetUsersResult>
    {
        readonly Action<PNGetUsersResult, PNStatus> callbackAction;

        public PNGetUsersResultExt(Action<PNGetUsersResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetUsersResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
