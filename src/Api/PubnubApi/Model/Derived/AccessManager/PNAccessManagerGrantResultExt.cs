using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNAccessManagerGrantResultExt: PNCallback<PNAccessManagerGrantResult>
    {
        Action<PNAccessManagerGrantResult, PNStatus> callbackAction = null;

        public PNAccessManagerGrantResultExt(Action<PNAccessManagerGrantResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
