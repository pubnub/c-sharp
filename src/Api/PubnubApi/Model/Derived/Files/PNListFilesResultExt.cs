using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNListFilesResultExt : PNCallback<PNListFilesResult>
    {
        readonly Action<PNListFilesResult, PNStatus> callbackAction;

        public PNListFilesResultExt(Action<PNListFilesResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNListFilesResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
