using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNManageMembersResultExt : PNCallback<PNManageMembersResult>
    {
        readonly Action<PNManageMembersResult, PNStatus> callbackAction;

        public PNManageMembersResultExt(Action<PNManageMembersResult, PNStatus> callback)
        {
            this.callbackAction = callback;
        }

        public override void OnResponse(PNManageMembersResult result, PNStatus status)
        {
            callbackAction?.Invoke(result, status);
        }
    }

}
