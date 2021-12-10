using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNGetChannelMetadataResultExt : PNCallback<PNGetChannelMetadataResult>
    {
        readonly Action<PNGetChannelMetadataResult, PNStatus> callbackAction;

        public PNGetChannelMetadataResultExt(Action<PNGetChannelMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNGetChannelMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
