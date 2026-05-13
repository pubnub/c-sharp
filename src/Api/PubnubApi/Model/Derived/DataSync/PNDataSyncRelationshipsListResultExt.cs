using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncRelationshipsListResultExt : PNCallback<PNDataSyncRelationshipsListResult>
{
    readonly Action<PNDataSyncRelationshipsListResult, PNStatus> callbackAction;

    public PNDataSyncRelationshipsListResultExt(Action<PNDataSyncRelationshipsListResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncRelationshipsListResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
