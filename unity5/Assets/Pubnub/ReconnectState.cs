using System;

namespace PubNubMessaging.Core
{
    public class ReconnectState<T>
    {
        public string[] Channels;
        public ResponseType Type;
        public Action<T> Callback;
        public Action<PubnubClientError> ErrorCallback;
        public Action<T> ConnectCallback;
        public object Timetoken;
        public bool Reconnect;

        public ReconnectState ()
        {
            Channels = null;
            Callback = null;
            ConnectCallback = null;
            Timetoken = null;
            Reconnect = false;
        }
    }
    #region "States and ResposeTypes"
    public enum ResponseType
    {
        Publish,
        History,
        Time,
        Subscribe,
        Presence,
        Here_Now,
        Heartbeat,
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
        GlobalHere_Now
    }

    internal class InternetState<T>
    {
        public Action<bool> Callback;
        public Action<PubnubClientError> ErrorCallback;
        public string[] Channels;

        public InternetState ()
        {
            Callback = null;
            ErrorCallback = null;
            Channels = null;
        }
    }

    public class RequestState<T>
    {
        public Action<T> UserCallback;
        public Action<PubnubClientError> ErrorCallback;
        public Action<T> ConnectCallback;
        public PubnubWebRequest Request;
        public PubnubWebResponse Response;
        public ResponseType Type;
        public string[] Channels;
        public bool Timeout;
        public bool Reconnect;
        public long Timetoken;

        public RequestState ()
        {
            UserCallback = null;
            ConnectCallback = null;
            Request = null;
            Response = null;
            Channels = null;
        }
    }
    #endregion
}

