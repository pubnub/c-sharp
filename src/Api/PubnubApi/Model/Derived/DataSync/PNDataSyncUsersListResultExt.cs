using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncUsersListResultExt : PNCallback<PNDataSyncUsersListResult>
{
    readonly Action<PNDataSyncUsersListResult, PNStatus> callbackAction;

    public PNDataSyncUsersListResultExt(Action<PNDataSyncUsersListResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncUsersListResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
