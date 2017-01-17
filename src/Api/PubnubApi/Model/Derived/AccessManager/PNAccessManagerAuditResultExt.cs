using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNAccessManagerAuditResultExt: PNCallback<PNAccessManagerAuditResult>
    {
        Action<PNAccessManagerAuditResult, PNStatus> callbackAction = null;

        public PNAccessManagerAuditResultExt(Action<PNAccessManagerAuditResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNAccessManagerAuditResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
