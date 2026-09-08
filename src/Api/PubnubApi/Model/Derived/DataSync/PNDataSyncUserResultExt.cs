using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncUserResultExt : PNCallback<PNDataSyncUserResult>
{
    readonly Action<PNDataSyncUserResult, PNStatus> callbackAction;

    public PNDataSyncUserResultExt(Action<PNDataSyncUserResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncUserResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
