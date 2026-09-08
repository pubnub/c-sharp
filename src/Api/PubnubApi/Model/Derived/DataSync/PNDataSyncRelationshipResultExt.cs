using System;
using PubnubApi.EndPoint;

namespace PubnubApi;

public class PNDataSyncRelationshipResultExt : PNCallback<PNDataSyncRelationshipResult>
{
    readonly Action<PNDataSyncRelationshipResult, PNStatus> callbackAction;

    public PNDataSyncRelationshipResultExt(Action<PNDataSyncRelationshipResult, PNStatus> callback)
    {
        this.callbackAction = callback;
    }
    
    public override void OnResponse(PNDataSyncRelationshipResult result, PNStatus status)
    {
        callbackAction?.Invoke(result, status);
    }
}
