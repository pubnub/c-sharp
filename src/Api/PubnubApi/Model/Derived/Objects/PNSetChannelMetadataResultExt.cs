using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNSetChannelMetadataResultExt : PNCallback<PNSetChannelMetadataResult>
    {
        readonly Action<PNSetChannelMetadataResult, PNStatus> callbackAction;

        public PNSetChannelMetadataResultExt(Action<PNSetChannelMetadataResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNSetChannelMetadataResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
