
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

        PNSetUuidMetadataOperation,
        PNGetUuidMetadataOperation,
        PNGetAllUuidMetadataOperation,
        PNDeleteUuidMetadataOperation,

        PNSetChannelMetadataOperation,
        PNGetChannelMetadataOperation,
        PNGetAllChannelMetadataOperation,
        PNDeleteChannelMetadataOperation,


        PNGetChannelMembersOperation,
        PNSetChannelMembersOperation,
        PNRemoveChannelMembersOperation,
        PNManageChannelMembersOperation,

        PNGetMembershipsOperation,
        PNSetMembershipsOperation,
        PNRemoveMembershipsOperation,
        PNManageMembershipsOperation,

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
        PNAccessManagerGrant,
        PNGenerateFileUploadUrlOperation,
        PNFileUploadOperation,
        PNPublishFileMessageOperation,
        PNFileUrlOperation,
        PNDownloadFileOperation,
        PNListFilesOperation,
        PNDeleteFileOperation
    }
}
