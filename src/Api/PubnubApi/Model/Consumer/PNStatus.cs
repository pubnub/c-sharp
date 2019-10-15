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

        internal PNStatus(object endpointOperation)
        {
            this.savedEndpointOperation = endpointOperation;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public PNStatusCategory Category { get; set; }

        public PNErrorData ErrorData { get; set; }
        public bool Error { get; set; }

        public int StatusCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PNOperationType Operation { get; set; }

        public bool TlsEnabled { get; set; }

        public string Uuid { get; set; }
        public string AuthKey { get; set; }
        public string Origin { get; set; }
        public object ClientRequest { get; set; }

        // send back channel, channel groups that were affected by this operation
        public List<string> AffectedChannels { get; set; } = new List<string>();
        public List<string> AffectedChannelGroups { get; set; } = new List<string>();

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
                    case PNOperationType.PNCreateUserOperation:
                        if (savedEndpointOperation is CreateUserOperation)
                        {
                            CreateUserOperation endpoint = savedEndpointOperation as CreateUserOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNCreateSpaceOperation:
                        if (savedEndpointOperation is CreateSpaceOperation)
                        {
                            CreateSpaceOperation endpoint = savedEndpointOperation as CreateSpaceOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNUpdateUserOperation:
                        if (savedEndpointOperation is UpdateUserOperation)
                        {
                            UpdateUserOperation endpoint = savedEndpointOperation as UpdateUserOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNUpdateSpaceOperation:
                        if (savedEndpointOperation is UpdateSpaceOperation)
                        {
                            UpdateSpaceOperation endpoint = savedEndpointOperation as UpdateSpaceOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNDeleteUserOperation:
                        if (savedEndpointOperation is DeleteUserOperation)
                        {
                            DeleteUserOperation endpoint = savedEndpointOperation as DeleteUserOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNDeleteSpaceOperation:
                        if (savedEndpointOperation is DeleteSpaceOperation)
                        {
                            DeleteSpaceOperation endpoint = savedEndpointOperation as DeleteSpaceOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetUserOperation:
                        if (savedEndpointOperation is GetUserOperation)
                        {
                            GetUserOperation endpoint = savedEndpointOperation as GetUserOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetSpaceOperation:
                        if (savedEndpointOperation is GetSpaceOperation)
                        {
                            GetSpaceOperation endpoint = savedEndpointOperation as GetSpaceOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetUsersOperation:
                        if (savedEndpointOperation is GetUsersOperation)
                        {
                            GetUsersOperation endpoint = savedEndpointOperation as GetUsersOperation;
                            if (endpoint != null)
                            {
                                endpoint.Retry();
                            }
                        }
                        break;
                    case PNOperationType.PNGetSpacesOperation:
                        if (savedEndpointOperation is GetSpacesOperation)
                        {
                            GetSpacesOperation endpoint = savedEndpointOperation as GetSpacesOperation;
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
                    case PNOperationType.PNManageMembersOperation:
                        if (savedEndpointOperation is ManageMembersOperation)
                        {
                            ManageMembersOperation endpoint = savedEndpointOperation as ManageMembersOperation;
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
                    case PNOperationType.PNGetMembersOperation:
                        if (savedEndpointOperation is GetMembersOperation)
                        {
                            GetMembersOperation endpoint = savedEndpointOperation as GetMembersOperation;
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
                    default:
                        break;
                }
            }
        }

    }
}
