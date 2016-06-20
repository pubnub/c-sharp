using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    internal enum ResponseType
    {
        Publish,
        History,
        Time,
        Subscribe,
        Presence,
        Here_Now,
        DetailedHistory,
        Leave,
        Unsubscribe,
        PresenceUnsubscribe,
        GrantAccess,
        AuditAccess,
        RevokeAccess,
        PresenceHeartbeat,
        SetUserState,
        GetUserState,
        Where_Now,
        GlobalHere_Now,
        PushRegister,
        PushRemove,
        PushGet,
        PushUnregister,
        ChannelGroupAdd,
        ChannelGroupRemove,
        ChannelGroupGet,
        ChannelGroupGrantAccess,
        ChannelGroupAuditAccess,
        ChannelGroupRevokeAccess
    }
}
