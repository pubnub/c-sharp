
namespace PubnubApi
{
    public enum PNOperationType
    {
        None,
        PNSubscribeOperation,
        PNUnsubscribeOperation,
        PNPublishOperation,
        PNHistoryOperation,
        PNWhereNowOperation,

        PNHeartbeatOperation,
        PNSetStateOperation,
        PNAddChannelsToGroupOperation,
        PNRemoveChannelsFromGroupOperation,
        PNChannelGroupsOperation,
        PNRemoveGroupOperation,
        PNChannelsForGroupOperation,

        Presence,
        Leave,
        PresenceUnsubscribe,
        RevokeAccess,
        PushRegister,
        PushRemove,
        PushGet,
        PushUnregister,
        ChannelGroupGet,
        ChannelGroupGrantAccess,
        ChannelGroupAuditAccess,
        ChannelGroupRevokeAccess,

        PNTimeOperation,

        PNHereNowOperation,
        PNGetStateOperation,
        PNAccessManagerAudit,
        PNAccessManagerGrant
    }
}
