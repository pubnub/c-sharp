using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubnubApi
{
    public class PNPushAddChannelResultExt : PNCallback<PNPushAddChannelResult>
    {
        Action<PNPushAddChannelResult, PNStatus> callbackAction = null;

        public PNPushAddChannelResultExt(Action<PNPushAddChannelResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNPushAddChannelResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }
}
