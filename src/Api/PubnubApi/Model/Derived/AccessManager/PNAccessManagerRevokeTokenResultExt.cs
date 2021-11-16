using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNAccessManagerRevokeTokenResultExt : PNCallback<PNAccessManagerRevokeTokenResult>
    {
        readonly Action<PNAccessManagerRevokeTokenResult, PNStatus> callbackAction;

        public PNAccessManagerRevokeTokenResultExt(Action<PNAccessManagerRevokeTokenResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNAccessManagerRevokeTokenResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
