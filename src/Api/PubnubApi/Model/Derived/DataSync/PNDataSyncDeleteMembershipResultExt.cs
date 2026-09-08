using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncDeleteMembershipResultExt : PNCallback<PNDataSyncDeleteMembershipResult>
{
    readonly Action<PNDataSyncDeleteMembershipResult, PNStatus> callbackAction;

    public PNDataSyncDeleteMembershipResultExt(Action<PNDataSyncDeleteMembershipResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncDeleteMembershipResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
