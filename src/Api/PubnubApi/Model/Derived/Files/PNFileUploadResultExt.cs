using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNFileUploadResultExt : PNCallback<PNFileUploadResult>
    {
        readonly Action<PNFileUploadResult, PNStatus> callbackAction;

        public PNFileUploadResultExt(Action<PNFileUploadResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNFileUploadResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
