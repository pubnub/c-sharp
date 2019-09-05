using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNAccessManagerTokenResultExt : PNCallback<PNAccessManagerTokenResult>
    {
        readonly Action<PNAccessManagerTokenResult, PNStatus> callbackAction;

        public PNAccessManagerTokenResultExt(Action<PNAccessManagerTokenResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNAccessManagerTokenResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
