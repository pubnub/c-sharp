using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNRemoveChannelMetadataResultExt : PNCallback<PNRemoveChannelMetadataResult>
    {
        readonly Action<PNRemoveChannelMetadataResult, PNStatus> callbackAction;

        public PNRemoveChannelMetadataResultExt(Action<PNRemoveChannelMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNRemoveChannelMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
