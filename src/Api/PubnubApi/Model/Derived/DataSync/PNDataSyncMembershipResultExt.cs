using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncMembershipResultExt : PNCallback<PNDataSyncMembershipResult>
{
    readonly Action<PNDataSyncMembershipResult, PNStatus> callbackAction;

    public PNDataSyncMembershipResultExt(Action<PNDataSyncMembershipResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncMembershipResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
