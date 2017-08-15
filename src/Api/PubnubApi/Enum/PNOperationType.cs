
namespace PubnubApi
{
    public enum PNOperationType
    {
        None,
        PNSubscribeOperation,
        PNUnsubscribeOperation,
        PNPublishOperation,
        PNFireOperation,
        PNHistoryOperation,
        PNDeleteMessageOperation,
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
        ChannelGroupAllGet,
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
