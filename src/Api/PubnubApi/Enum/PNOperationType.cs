
namespace PubnubApi
{
    public enum PNOperationType
    {
        None,
        PNSubscribeOperation,
        PNUnsubscribeOperation,
        PNPublishOperation,
        PNFireOperation,
        PNSignalOperation,
        PNHistoryOperation,
        PNFetchHistoryOperation,
        PNDeleteMessageOperation,
        PNMessageCountsOperation,
        PNWhereNowOperation,

        PNHeartbeatOperation,
        PNSetStateOperation,
        PNAddChannelsToGroupOperation,
        PNRemoveChannelsFromGroupOperation,
        PNChannelGroupsOperation,
        PNRemoveGroupOperation,
        PNChannelsForGroupOperation,

        PNCreateUserOperation,
        PNUpdateUserOperation,
        PNDeleteUserOperation,
        PNGetUserOperation,
        PNGetUsersOperation,

        PNCreateSpaceOperation,
        PNUpdateSpaceOperation,
        PNDeleteSpaceOperation,
        PNGetSpacesOperation,
        PNGetSpaceOperation,

        PNManageMembershipsOperation,
        PNManageMembersOperation,
        PNGetMembershipsOperation,
        PNGetMembersOperation,

        PNAddMessageActionOperation,
        PNRemoveMessageActionOperation,
        PNGetMessageActionsOperation,

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
        PNAccessManagerGrantToken,
        PNAccessManagerGrant
    }
}
