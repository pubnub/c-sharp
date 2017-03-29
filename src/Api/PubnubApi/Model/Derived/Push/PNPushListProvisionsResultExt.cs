using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNPushListProvisionsResultExt : PNCallback<PNPushListProvisionsResult>
    {
        Action<PNPushListProvisionsResult, PNStatus> callbackAction = null;

        public PNPushListProvisionsResultExt(Action<PNPushListProvisionsResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNPushListProvisionsResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
