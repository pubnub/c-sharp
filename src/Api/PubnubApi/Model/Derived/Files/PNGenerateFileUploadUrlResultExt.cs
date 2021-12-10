using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNGenerateFileUploadUrlResultExt : PNCallback<PNGenerateFileUploadUrlResult>
    {
        readonly Action<PNGenerateFileUploadUrlResult, PNStatus> callbackAction;

        public PNGenerateFileUploadUrlResultExt(Action<PNGenerateFileUploadUrlResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGenerateFileUploadUrlResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
