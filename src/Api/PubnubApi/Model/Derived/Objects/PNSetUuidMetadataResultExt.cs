using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNSetUuidMetadataResultExt : PNCallback<PNSetUuidMetadataResult>
    {
        readonly Action<PNSetUuidMetadataResult, PNStatus> callbackAction;

        public PNSetUuidMetadataResultExt(Action<PNSetUuidMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNSetUuidMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
