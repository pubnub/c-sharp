using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.EndPoint;

namespace PubnubApi
{
    public class PNStatus
    {
        private object savedEndpointOperation { get; set; }

        public PNStatus() { }

        public PNStatus(Exception e, PNOperationType operationType, PNStatusCategory category, IEnumerable<string> affectedChannels = null, IEnumerable<string> affectedChannelGroups = null)
        {
            this.Error = true;
            this.Operation = operationType;
            this.ErrorData = new PNErrorData(e.Message, e);
            this.AffectedChannels = affectedChannels?.ToList(); 
            this.AffectedChannelGroups = affectedChannelGroups?.ToList();
            this.Category = category;
        }

        internal PNStatus(object endpointOperation)
        {
            this.savedEndpointOperation = endpointOperation;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public PNStatusCategory Category { get; internal set; }

        public PNErrorData ErrorData { get; internal set; }
        public bool Error { get; internal set; }

        public int StatusCode { get; internal set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PNOperationType Operation { get; internal set; }

        public bool TlsEnabled { get; internal set; }

        public string Uuid { get; internal set; }
        public string AuthKey { get; internal set; }
        public string Origin { get; internal set; }
        public object ClientRequest { get; internal set; }

        // send back channel, channel groups that were affected by this operation
        public List<string> AffectedChannels { get; internal set; } = new List<string>();
        public List<string> AffectedChannelGroups { get; internal set; } = new List<string>();

        public object AdditonalData { get; internal set; } = new object();

        public void Retry()
        {
            if (savedEndpointOperation != null)
            {
                switch (Operation)
                {
                    case PNOperationType.PNAccessManagerAudit:
                        if (savedEndpointOperation is AuditOperation)
                        {
                            AuditOperation endpoint = savedEndpointOperation as AuditOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNAccessManagerGrantToken:
                        if (savedEndpointOperation is GrantTokenOperation)
                        {
                            GrantTokenOperation endpoint = savedEndpointOperation as GrantTokenOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNAccessManagerRevokeToken:
                        if (savedEndpointOperation is RevokeTokenOperation)
                        {
                            RevokeTokenOperation endpoint = savedEndpointOperation as RevokeTokenOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNAccessManagerGrant:
                        if (savedEndpointOperation is GrantOperation)
                        {
                            GrantOperation endpoint = savedEndpointOperation as GrantOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNAddChannelsToGroupOperation:
                        if (savedEndpointOperation is AddChannelsToChannelGroupOperation)
                        {
                            AddChannelsToChannelGroupOperation endpoint = savedEndpointOperation as AddChannelsToChannelGroupOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNRemoveGroupOperation:
                        if (savedEndpointOperation is DeleteChannelGroupOperation)
                        {
                            DeleteChannelGroupOperation endpoint = savedEndpointOperation as DeleteChannelGroupOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.ChannelGroupAllGet:
                        if (savedEndpointOperation is ListAllChannelGroupOperation)
                        {
                            ListAllChannelGroupOperation endpoint = savedEndpointOperation as ListAllChannelGroupOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.ChannelGroupGet:
                        if (savedEndpointOperation is ListChannelsForChannelGroupOperation)
                        {
                            ListChannelsForChannelGroupOperation endpoint = savedEndpointOperation as ListChannelsForChannelGroupOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNRemoveChannelsFromGroupOperation:
                        if (savedEndpointOperation is RemoveChannelsFromChannelGroupOperation)
                        {
                            RemoveChannelsFromChannelGroupOperation endpoint = savedEndpointOperation as RemoveChannelsFromChannelGroupOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetStateOperation:
                        if (savedEndpointOperation is GetStateOperation)
                        {
                            GetStateOperation endpoint = savedEndpointOperation as GetStateOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNHereNowOperation:
                        if (savedEndpointOperation is HereNowOperation)
                        {
                            HereNowOperation endpoint = savedEndpointOperation as HereNowOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNSetStateOperation:
                        if (savedEndpointOperation is SetStateOperation)
                        {
                            SetStateOperation endpoint = savedEndpointOperation as SetStateOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNWhereNowOperation:
                        if (savedEndpointOperation is WhereNowOperation)
                        {
                            WhereNowOperation endpoint = savedEndpointOperation as WhereNowOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNFireOperation:
                        if (savedEndpointOperation is FireOperation)
                        {
                            FireOperation endpoint = savedEndpointOperation as FireOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNPublishOperation:
                        if (savedEndpointOperation is PublishOperation)
                        {
                            PublishOperation endpoint = savedEndpointOperation as PublishOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PushRegister:
                        if (savedEndpointOperation is AddPushChannelOperation)
                        {
                            AddPushChannelOperation endpoint = savedEndpointOperation as AddPushChannelOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PushGet:
                        if (savedEndpointOperation is AuditPushChannelOperation)
                        {
                            AuditPushChannelOperation endpoint = savedEndpointOperation as AuditPushChannelOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PushUnregister:
                        if (savedEndpointOperation is RemovePushChannelOperation)
                        {
                            RemovePushChannelOperation endpoint = savedEndpointOperation as RemovePushChannelOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNHistoryOperation:
                        if (savedEndpointOperation is HistoryOperation)
                        {
                            HistoryOperation endpoint = savedEndpointOperation as HistoryOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNFetchHistoryOperation:
                        if (savedEndpointOperation is FetchHistoryOperation)
                        {
                            FetchHistoryOperation endpoint = savedEndpointOperation as FetchHistoryOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNMessageCountsOperation:
                        if (savedEndpointOperation is MessageCountsOperation)
                        {
                            MessageCountsOperation endpoint = savedEndpointOperation as MessageCountsOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNTimeOperation:
                        if (savedEndpointOperation is TimeOperation)
                        {
                            TimeOperation endpoint = savedEndpointOperation as TimeOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNSetUuidMetadataOperation:
                        if (savedEndpointOperation is SetUuidMetadataOperation)
                        {
                            SetUuidMetadataOperation endpoint = savedEndpointOperation as SetUuidMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNSetChannelMetadataOperation:
                        if (savedEndpointOperation is SetChannelMetadataOperation)
                        {
                            SetChannelMetadataOperation endpoint = savedEndpointOperation as SetChannelMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNDeleteUuidMetadataOperation:
                        if (savedEndpointOperation is RemoveUuidMetadataOperation)
                        {
                            RemoveUuidMetadataOperation endpoint = savedEndpointOperation as RemoveUuidMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNDeleteChannelMetadataOperation:
                        if (savedEndpointOperation is RemoveChannelMetadataOperation)
                        {
                            RemoveChannelMetadataOperation endpoint = savedEndpointOperation as RemoveChannelMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetUuidMetadataOperation:
                        if (savedEndpointOperation is GetUuidMetadataOperation)
                        {
                            GetUuidMetadataOperation endpoint = savedEndpointOperation as GetUuidMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetChannelMetadataOperation:
                        if (savedEndpointOperation is GetChannelMetadataOperation)
                        {
                            GetChannelMetadataOperation endpoint = savedEndpointOperation as GetChannelMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetAllUuidMetadataOperation:
                        if (savedEndpointOperation is GetAllUuidMetadataOperation)
                        {
                            GetAllUuidMetadataOperation endpoint = savedEndpointOperation as GetAllUuidMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetAllChannelMetadataOperation:
                        if (savedEndpointOperation is GetAllChannelMetadataOperation)
                        {
                            GetAllChannelMetadataOperation endpoint = savedEndpointOperation as GetAllChannelMetadataOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNManageMembershipsOperation:
                        if (savedEndpointOperation is ManageMembershipsOperation)
                        {
                            ManageMembershipsOperation endpoint = savedEndpointOperation as ManageMembershipsOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNSetMembershipsOperation:
                        if (savedEndpointOperation is SetMembershipsOperation)
                        {
                            SetMembershipsOperation endpoint = savedEndpointOperation as SetMembershipsOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNRemoveMembershipsOperation:
                        if (savedEndpointOperation is RemoveMembershipsOperation)
                        {
                            RemoveMembershipsOperation endpoint = savedEndpointOperation as RemoveMembershipsOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNManageChannelMembersOperation:
                        if (savedEndpointOperation is ManageChannelMembersOperation)
                        {
                            ManageChannelMembersOperation endpoint = savedEndpointOperation as ManageChannelMembersOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNSetChannelMembersOperation:
                        if (savedEndpointOperation is SetChannelMembersOperation)
                        {
                            SetChannelMembersOperation endpoint = savedEndpointOperation as SetChannelMembersOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNRemoveChannelMembersOperation:
                        if (savedEndpointOperation is RemoveChannelMembersOperation)
                        {
                            RemoveChannelMembersOperation endpoint = savedEndpointOperation as RemoveChannelMembersOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetMembershipsOperation:
                        if (savedEndpointOperation is GetMembershipsOperation)
                        {
                            GetMembershipsOperation endpoint = savedEndpointOperation as GetMembershipsOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetChannelMembersOperation:
                        if (savedEndpointOperation is GetChannelMembersOperation)
                        {
                            GetChannelMembersOperation endpoint = savedEndpointOperation as GetChannelMembersOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNAddMessageActionOperation:
                        if (savedEndpointOperation is AddMessageActionOperation)
                        {
                            AddMessageActionOperation endpoint = savedEndpointOperation as AddMessageActionOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNRemoveMessageActionOperation:
                        if (savedEndpointOperation is RemoveMessageActionOperation)
                        {
                            RemoveMessageActionOperation endpoint = savedEndpointOperation as RemoveMessageActionOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNPublishFileMessageOperation:
                        if (savedEndpointOperation is PublishFileMessageOperation)
                        {
                            PublishFileMessageOperation endpoint = savedEndpointOperation as PublishFileMessageOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNFileUrlOperation:
                        if (savedEndpointOperation is GetFileUrlOperation)
                        {
                            GetFileUrlOperation endpoint = savedEndpointOperation as GetFileUrlOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNListFilesOperation:
                        if (savedEndpointOperation is ListFilesOperation)
                        {
                            ListFilesOperation endpoint = savedEndpointOperation as ListFilesOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNDeleteFileOperation:
                        if (savedEndpointOperation is DeleteFileOperation)
                        {
                            DeleteFileOperation endpoint = savedEndpointOperation as DeleteFileOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNDownloadFileOperation:
                        if (savedEndpointOperation is DownloadFileOperation)
                        {
                            DownloadFileOperation endpoint = savedEndpointOperation as DownloadFileOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
