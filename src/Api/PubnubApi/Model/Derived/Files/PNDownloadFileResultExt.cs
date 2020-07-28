using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNDownloadFileResultExt : PNCallback<PNDownloadFileResult>
    {
        readonly Action<PNDownloadFileResult, PNStatus> callbackAction;

        public PNDownloadFileResultExt(Action<PNDownloadFileResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNDownloadFileResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
