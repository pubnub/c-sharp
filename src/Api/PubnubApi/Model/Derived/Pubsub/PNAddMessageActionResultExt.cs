using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNAddMessageActionResultExt : PNCallback<PNAddMessageActionResult>
    {
        readonly Action<PNAddMessageActionResult, PNStatus> callbackAction;

        public PNAddMessageActionResultExt(Action<PNAddMessageActionResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNAddMessageActionResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
