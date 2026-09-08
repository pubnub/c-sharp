using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncDeleteUserResultExt : PNCallback<PNDataSyncDeleteUserResult>
{
    readonly Action<PNDataSyncDeleteUserResult, PNStatus> callbackAction;

    public PNDataSyncDeleteUserResultExt(Action<PNDataSyncDeleteUserResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncDeleteUserResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
