using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNRemoveUuidMetadataResultExt : PNCallback<PNRemoveUuidMetadataResult>
    {
        readonly Action<PNRemoveUuidMetadataResult, PNStatus> callbackAction;

        public PNRemoveUuidMetadataResultExt(Action<PNRemoveUuidMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNRemoveUuidMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
