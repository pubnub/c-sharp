using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PubNubMessaging.Core
{
    #region EventExt and Args
    static class EventExtensions
    {
        public static void Raise<T> (this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            if (handler != null) {
                handler (sender, args);
            }
        }
    }

    internal class CustomEventArgs<T> : EventArgs
    {
        internal string Message;
        internal RequestState<T> PubnubRequestState;
        internal bool IsError;
        internal bool IsTimeout;
        internal CoroutineClass.CurrentRequestType CurrRequestType;
    }
    #endregion

    #region CoroutineClass
    /*internal class ForceQuitCoroutieArgs<T> : EventArgs
    {
        internal bool isTimeout;
        internal IEnumerator crTimeout;
        internal IEnumerator crRequest;
    }*/

    /*class ProcessRequests<T> {
        public event EventHandler<EventArgs> CoroutineComplete;
        public event EventHandler<EventArgs> ForceStopCoroutine;
        public bool isComplete = false;

        public string url;
        public WWW www;
        public RequestState<T> pubnubRequestState;
        public int timeout;
        public IEnumerator crTimeout;
        public IEnumerator crRequest;

        public ProcessRequests(string url, RequestState<T> pubnubRequestState, int timeout){
            this.url = url;
            this.pubnubRequestState = pubnubRequestState;
            this.timeout = timeout;
        }

        public void FireForceStopCoRoutine(bool isTimeout){
            if (ForceStopCoroutine != null) {
                ForceQuitCoroutieArgs<T> fqca = new ForceQuitCoroutieArgs<T> ();
                fqca.isTimeout = isTimeout;
                fqca.crRequest = crRequest;
                fqca.crTimeout = crTimeout;
                ForceStopCoroutine.Raise (this, fqca);
            }
        }

        public IEnumerator SendRequest<T> ()
        {
            Debug.Log ("URL:" + url.ToString ());
            isComplete = false;
            www = new WWW (url);
            yield return www;

            try {
                if(www != null){
                    FireForceStopCoRoutine (false);

                    if (www.error == null) {
                        isComplete = true;
                        UnityEngine.Debug.Log ("Message: " + www.text);
                        FireEvent (www.text, false, false, pubnubRequestState);
                    } else {
                        isComplete = true;
                        UnityEngine.Debug.Log ("Error: " + www.error);
                        FireEvent (www.error, true, false, pubnubRequestState);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1}", DateTime.Now.ToString (), www.error), LoggingMethod.LevelError);
                    } 
                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutine {1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequest exit", DateTime.Now.ToString ()), LoggingMethod.LevelError);
        }

        public IEnumerator CheckTimeout<T> ()
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);
            yield return new WaitForSeconds(timeout); 
            if (!isComplete) {

                if (www != null) {
                    www.Dispose ();
                }

                FireEvent ("Timed out", true, true, pubnubRequestState);

                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);

                FireForceStopCoRoutine (true);
            }
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit", DateTime.Now.ToString ()), LoggingMethod.LevelError);
        }

        public void FireEvent<T> (string message, bool isError, bool isTimeout, RequestState<T> pubnubRequestState)
        {
            if (CoroutineComplete != null) {
                CustomEventArgs<T> cea = new CustomEventArgs<T> ();
                cea.pubnubRequestState = pubnubRequestState;
                cea.message = message;
                cea.isError = isError;
                cea.isTimeout = isTimeout;
                CoroutineComplete.Raise (this, cea);
            }
        }
    }*/

    class CoroutineParams
    {
        public string url;
        //public RequestState<T> pubnubRequestState;
        public int timeout;
        public int pause;
        public CoroutineClass.CurrentRequestType crt;
        //public CoroutineParams(string url, RequestState<T> pubnubRequestState, int timeout, int pause, CoroutineClass.CurrentRequestType crt){
        public CoroutineParams (string url, int timeout, int pause, CoroutineClass.CurrentRequestType crt)
        {
            this.url = url;
            //this.pubnubRequestState = pubnubRequestState;
            this.timeout = timeout;
            this.pause = pause;
            this.crt = crt;
        }
    }

    //TODO: Refactor after stable code, separate out repeat code.
    //Sending a IEnumerator from a complex object in StartCoroutine doesn't work for Web/WebGL
    //Dispose of www leads to random unhandled exceptions.
    //Generic methods dont work in StartCoroutine when the called with the string param name StartCoroutine("method", param)
    //StopCoroutine only works when the coroutine is started with string overload.
    class CoroutineClass : MonoBehaviour
    {
        private bool isHearbeatComplete = false;
        private bool isPresenceHeartbeatComplete = false;
        private bool isSubscribeComplete = false;
        private bool isNonSubscribeComplete = false;

        public RequestState<string> pubnubRequestStateSub;
        public RequestState<string> pubnubRequestStateNonSub;
        public RequestState<string> pubnubRequestStateHeartbeat;
        public RequestState<string> pubnubRequestStatePresenceHeartbeat;

        public Type typeParameterType;

        /*private bool isHearbeat = false;
        private bool ispresenceHeartbeat = false;
        private bool isSubscribe = false;
        private bool isNonSubscribe = false;*/
        public enum CurrentRequestType
        {
            Heartbeat,
            PresenceHeartbeat,
            Subscribe,
            NonSubscribe
        }

        //int subreqcount = 0;
        //bool isCheckTimeoutSub2Running = false;

        WWW subscribeWww;
        WWW heartbeatWww;
        WWW presenceHeartbeatWww;
        WWW nonSubscribeWww;

        //public bool isComplete = false;
        //public event EventHandler<EventArgs> CoroutineComplete;
        private EventHandler<EventArgs> subCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> SubCoroutineComplete {
            add {
                if (subCoroutineComplete == null || !subCoroutineComplete.GetInvocationList ().Contains (value)) {
                    subCoroutineComplete += value;
                }
            }
            remove {
                subCoroutineComplete -= value;
            }
        }

        private EventHandler<EventArgs> nonSubCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> NonSubCoroutineComplete {
            add {
                if (nonSubCoroutineComplete == null || !nonSubCoroutineComplete.GetInvocationList ().Contains (value)) {
                    nonSubCoroutineComplete += value;
                }
            }
            remove {
                nonSubCoroutineComplete -= value;
            }
        }

        private EventHandler<EventArgs> presenceHeartbeatCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> PresenceHeartbeatCoroutineComplete {
            add {
                if (presenceHeartbeatCoroutineComplete == null || !presenceHeartbeatCoroutineComplete.GetInvocationList ().Contains (value)) {
                    presenceHeartbeatCoroutineComplete += value;
                }
            }
            remove {
                presenceHeartbeatCoroutineComplete -= value;
            }
        }

        private EventHandler<EventArgs> heartbeatCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> HeartbeatCoroutineComplete {
            add {
                if (heartbeatCoroutineComplete == null || !heartbeatCoroutineComplete.GetInvocationList ().Contains (value)) {
                    heartbeatCoroutineComplete += value;
                }
            }
            remove {
                heartbeatCoroutineComplete -= value;
            }
        }


        public void Run<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause)
        {
            /*ProcessRequests<T> pr = new ProcessRequests<T> (url, pubnubRequestState, timeout);
            pr.CoroutineComplete += HandleCoroutineComplete;
            pr.ForceStopCoroutine += HandleForceStopCoroutine<T>;
            pr.crTimeout = pr.CheckTimeout<T> ();
            pr.crRequest = pr.SendRequest<T> ();

            //for heartbeat and presence heartbeat treat reconnect as pause
            if (((pubnubRequestState.Type == ResponseType.Heartbeat) || (pubnubRequestState.Type == ResponseType.PresenceHeartbeat))
                && (pubnubRequestState.Reconnect == true)) {

                StartCoroutine (PausedRequest<T>(pr, pause));
            } else {
                IEnumerator crTimeout = pr.crTimeout;
                IEnumerator crRequest = pr.crRequest;

                StartCoroutine (crTimeout);
                StartCoroutine (crRequest);
            }*/
            //for heartbeat and presence heartbeat treat reconnect as pause
            CurrentRequestType crt;
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RequestType {1} {2}", DateTime.Now.ToString (), typeof(T), pubnubRequestState.GetType ()), LoggingMethod.LevelError);
            typeParameterType = pubnubRequestState.GetType ();//typeof(T);
            if ((pubnubRequestState.Type == ResponseType.Heartbeat) || (pubnubRequestState.Type == ResponseType.PresenceHeartbeat)) {
                crt = CurrentRequestType.PresenceHeartbeat;
                if (pubnubRequestState.Type == ResponseType.Heartbeat) {
                    crt = CurrentRequestType.Heartbeat;
                } 
                CheckComplete (crt);

                if (pubnubRequestState.Reconnect) {
                    StartCoroutine (DelayRequest<T> (url, pubnubRequestState, timeout, pause, crt));
                } else {
                    StartCoroutinesByName<T> (url, pubnubRequestState, timeout, pause, crt);
                }
            } else if ((pubnubRequestState.Type == ResponseType.Subscribe) || (pubnubRequestState.Type == ResponseType.Presence)) {
                crt = CurrentRequestType.Subscribe;
                
                if ((subscribeWww != null) && (!subscribeWww.isDone)) {
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, subscribeWww running trying to abort {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                    if (subscribeWww == null) {
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, subscribeWww aborted {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                    }
                }
                StartCoroutinesByName<T> (url, pubnubRequestState, timeout, pause, crt);
            } else {
                crt = CurrentRequestType.NonSubscribe;
                CheckComplete (crt);
                StartCoroutinesByName<T> (url, pubnubRequestState, timeout, pause, crt);
            } 
        }

        private void StartCoroutinesByName<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            CoroutineParams cp = new CoroutineParams (url, timeout, pause, crt);

            if (crt == CurrentRequestType.Subscribe) {
                this.pubnubRequestStateSub = pubnubRequestState as RequestState<string>;
                StartCoroutine ("CheckTimeoutSub", cp);
                //StartCoroutine ("CheckTimeout", cp);
                StartCoroutine ("SendRequestSub", cp);
                //StartCoroutine ("SendRequest", cp);
            } else if (crt == CurrentRequestType.NonSubscribe) {
                this.pubnubRequestStateNonSub = pubnubRequestState as RequestState<string>;
                StartCoroutine ("CheckTimeoutNonSub", cp);
                //StartCoroutine ("CheckTimeout", cp);
                StartCoroutine ("SendRequestNonSub", cp);
                //StartCoroutine ("SendRequest", cp);
            } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                this.pubnubRequestStatePresenceHeartbeat = pubnubRequestState as RequestState<string>;
                StartCoroutine ("CheckTimeoutPresenceHeartbeat", cp);
                //StartCoroutine ("CheckTimeout", cp);
                StartCoroutine ("SendRequestPresenceHeartbeat", cp);
                //StartCoroutine ("SendRequest", cp);
            } else if (crt == CurrentRequestType.Heartbeat) {
                this.pubnubRequestStateHeartbeat = pubnubRequestState as RequestState<string>;
                StartCoroutine ("CheckTimeoutHeartbeat", cp);
                //StartCoroutine ("CheckTimeout", cp);
                StartCoroutine ("SendRequestHeartbeat", cp);
                //StartCoroutine ("SendRequest", cp);
            }
        }


        /*private void StartCoroutines<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            StartCoroutine (CheckTimeout<T> (pubnubRequestState, timeout, pause, crt));
            StartCoroutine (SendRequest<T> (url, pubnubRequestState, timeout, pause, crt));
        }

        private void StartCoroutinesSub<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            StartCoroutine (CheckTimeoutSub<T> (pubnubRequestState, timeout, pause, crt));
            StartCoroutine (SendRequestSub<T> (url, pubnubRequestState, timeout, pause, crt));
        }*/

        public IEnumerator DelayRequest<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            yield return new WaitForSeconds (pause); 
            StartCoroutinesByName<T> (url, pubnubRequestState, timeout, pause, crt);

            /*IEnumerator crTimeout = pr.crTimeout;
            IEnumerator crRequest = pr.crRequest;

            StartCoroutine (crTimeout);
            StartCoroutine (crRequest);*/
        }

        public IEnumerator SendRequestSub (CoroutineParams cp)
        {
            Debug.Log ("URL Sub:" + cp.url.ToString ());
            WWW www;


            isSubscribeComplete = false;

            subscribeWww = new WWW (cp.url);
            yield return subscribeWww;
            //while ((subscribeWww != null) && (!subscribeWww.isDone)) { yield return null; }
            if ((subscribeWww != null) && (subscribeWww.isDone)) {
                www = subscribeWww;
            } else {
                www = null;
                System.GC.Collect ();
            }
            //subreqcount++;


            try {
                if (www != null) {

                    SetComplete (cp.crt);
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete sub {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
                    string message = "";
                    bool isError = false;

                    if (string.IsNullOrEmpty (www.error)) {
                        //UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Message: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.text), LoggingMethod.LevelError);

                        //FireEvent (www.text, false, false, this.pubnubRequestStateSub, cp.crt);
                        message = www.text;
                        isError = false;
                        //CallFireEvent<constructed> (www.text, false, false, this.pubnubRequestStateSub, cp);
                        //FireEvent (www.text, false, false, repository, cp.crt);
                    } else {
                        //UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Error: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.error), LoggingMethod.LevelError);
                        message = www.error;
                        isError = true;
                        //CallFireEvent<typeParameterType> (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, null, cp.crt);
                    } 
                    FireEvent (message, isError, false, this.pubnubRequestStateSub, cp.crt);
                    /*Type[] typeArgs = { typeParameterType };
                    Type generic = typeof(RequestState<>);
                    Type constructed = generic.MakeGenericType (typeArgs);
                    object repository = Activator.CreateInstance (constructed);
                    repository = this.pubnubRequestStateSub;
                    MethodInfo method = GetType ().GetMethod ("CallFireEvent")
                        .MakeGenericMethod (typeArgs);
                    method.Invoke (this, new[] { message, isError, false, repository, cp.crt });*/

                    //StopCoroutine ("CheckTimeoutSub");
                    //StopCoroutine ("SendRequestSub2");

                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutineSub {1}, Exception: {2}", DateTime.Now.ToString (), cp.crt.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1} {2}", DateTime.Now.ToString (), cp.crt.ToString (), subreqcount.ToString()), LoggingMethod.LevelError);
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);


        }

        public IEnumerator SendRequestNonSub (CoroutineParams cp)
        {
            Debug.Log ("URL NonSub:" + cp.url.ToString ());
            WWW www;


            isNonSubscribeComplete = false;
            nonSubscribeWww = new WWW (cp.url);
            yield return nonSubscribeWww;
            if ((nonSubscribeWww != null) && (nonSubscribeWww.isDone)) {
                www = nonSubscribeWww;
            } else {
                www = null;
                System.GC.Collect ();
            }
             

            try {
                if (www != null) {

                    SetComplete (cp.crt);
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete sub {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
                    string message = "";
                    bool isError = false;

                    if (string.IsNullOrEmpty (www.error)) {
                        //UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Message: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.text), LoggingMethod.LevelError);

                        //FireEvent (www.text, false, false, this.pubnubRequestStateSub, cp.crt);
                        message = www.text;
                        isError = false;
                        //CallFireEvent<constructed> (www.text, false, false, this.pubnubRequestStateSub, cp);
                        //FireEvent (www.text, false, false, repository, cp.crt);
                    } else {
                        //UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Error: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.error), LoggingMethod.LevelError);
                        message = www.error;
                        isError = true;
                        //CallFireEvent<typeParameterType> (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, null, cp.crt);
                    } 
                    FireEvent (message, isError, false, this.pubnubRequestStateNonSub, cp.crt);
                    /*Type[] typeArgs = { typeParameterType };
                    Type generic = typeof(RequestState<>);
                    Type constructed = generic.MakeGenericType (typeArgs);
                    object repository = Activator.CreateInstance (constructed);
                    repository = this.pubnubRequestStateSub;
                    MethodInfo method = GetType ().GetMethod ("CallFireEvent")
                        .MakeGenericMethod (typeArgs);
                    method.Invoke (this, new[] { message, isError, false, repository, cp.crt });*/

                    //StopCoroutine ("CheckTimeoutSub");
                    //StopCoroutine ("SendRequestSub2");

                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutineSub {1}, Exception: {2}", DateTime.Now.ToString (), cp.crt.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1} {2}", DateTime.Now.ToString (), cp.crt.ToString (), subreqcount.ToString()), LoggingMethod.LevelError);
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);


        }

        public IEnumerator SendRequestPresenceHeartbeat (CoroutineParams cp)
        {
            Debug.Log ("URL PresenceHB:" + cp.url.ToString ());
            WWW www;

            isPresenceHeartbeatComplete = false;
            presenceHeartbeatWww = new WWW (cp.url);
            yield return presenceHeartbeatWww;
            //www = heartbeatWww;
            if ((presenceHeartbeatWww != null) && (presenceHeartbeatWww.isDone)) {
                www = presenceHeartbeatWww;
            } else {
                www = null;
                System.GC.Collect ();
            }


            try {
                if (www != null) {

                    SetComplete (cp.crt);
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete sub {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
                    string message = "";
                    bool isError = false;

                    if (string.IsNullOrEmpty (www.error)) {
                        //UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Message: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.text), LoggingMethod.LevelError);

                        //FireEvent (www.text, false, false, this.pubnubRequestStateSub, cp.crt);
                        message = www.text;
                        isError = false;
                        //CallFireEvent<constructed> (www.text, false, false, this.pubnubRequestStateSub, cp);
                        //FireEvent (www.text, false, false, repository, cp.crt);
                    } else {
                        //UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Error: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.error), LoggingMethod.LevelError);
                        message = www.error;
                        isError = true;
                        //CallFireEvent<typeParameterType> (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, null, cp.crt);
                    } 
                    FireEvent (message, isError, false, this.pubnubRequestStatePresenceHeartbeat, cp.crt);
                    /*Type[] typeArgs = { typeParameterType };
                    Type generic = typeof(RequestState<>);
                    Type constructed = generic.MakeGenericType (typeArgs);
                    object repository = Activator.CreateInstance (constructed);
                    repository = this.pubnubRequestStateSub;
                    MethodInfo method = GetType ().GetMethod ("CallFireEvent")
                        .MakeGenericMethod (typeArgs);
                    method.Invoke (this, new[] { message, isError, false, repository, cp.crt });*/

                    //StopCoroutine ("CheckTimeoutSub");
                    //StopCoroutine ("SendRequestSub2");

                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutineSub {1}, Exception: {2}", DateTime.Now.ToString (), cp.crt.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1} {2}", DateTime.Now.ToString (), cp.crt.ToString (), subreqcount.ToString()), LoggingMethod.LevelError);
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);


        }

        public IEnumerator SendRequestHeartbeat (CoroutineParams cp)
        {
            Debug.Log ("URL Heartbeat:" + cp.url.ToString ());
            WWW www;

            isHearbeatComplete = false;
            heartbeatWww = new WWW (cp.url);
            yield return heartbeatWww;
            //www = heartbeatWww;
            if ((heartbeatWww != null) && (heartbeatWww.isDone)) {
                www = heartbeatWww;
            } else {
                www = null;
                System.GC.Collect ();
            }

            try {
                if (www != null) {

                    SetComplete (cp.crt);
                    
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete sub {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
                    string message = "";
                    bool isError = false;

                    if (string.IsNullOrEmpty (www.error)) {
                        //UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Message: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.text), LoggingMethod.LevelError);

                        //FireEvent (www.text, false, false, this.pubnubRequestStateSub, cp.crt);
                        message = www.text;
                        isError = false;
                        //CallFireEvent<constructed> (www.text, false, false, this.pubnubRequestStateSub, cp);
                        //FireEvent (www.text, false, false, repository, cp.crt);
                    } else {
                        //UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Error: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.error), LoggingMethod.LevelError);
                        message = www.error;
                        isError = true;
                        //CallFireEvent<typeParameterType> (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, null, cp.crt);
                    } 
                    FireEvent (message, isError, false, this.pubnubRequestStateHeartbeat, cp.crt);
                    /*Type[] typeArgs = { typeParameterType };
                    Type generic = typeof(RequestState<>);
                    Type constructed = generic.MakeGenericType (typeArgs);
                    object repository = Activator.CreateInstance (constructed);
                    

                    repository = this.pubnubRequestStateSub;
                    MethodInfo method = GetType ().GetMethod ("CallFireEvent")
                        .MakeGenericMethod (typeArgs);
                    method.Invoke (this, new [] { message, isError, false, repository, cp.crt });*/

                    //StopCoroutine ("CheckTimeoutSub");
                    //StopCoroutine ("SendRequestSub2");

                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutineSub {1}, Exception: {2}", DateTime.Now.ToString (), cp.crt.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1} {2}", DateTime.Now.ToString (), cp.crt.ToString (), subreqcount.ToString()), LoggingMethod.LevelError);
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);


        }

        /*public IEnumerator SendRequest (CoroutineParams cp)
        {
            Debug.Log ("URL:" + cp.url.ToString ());
            WWW www;

            if (cp.crt == CurrentRequestType.Heartbeat) {
                isHearbeatComplete = false;
                heartbeatWww = new WWW (cp.url);
                yield return heartbeatWww;
                //www = heartbeatWww;
                if ((heartbeatWww != null) && (heartbeatWww.isDone)) {
                    www = heartbeatWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } else if (cp.crt == CurrentRequestType.PresenceHeartbeat) {
                isPresenceHeartbeatComplete = false;
                presenceHeartbeatWww = new WWW (cp.url);
                yield return presenceHeartbeatWww;
                //www = presenceHeartbeatWww;
                if ((presenceHeartbeatWww != null) && (presenceHeartbeatWww.isDone)) {
                    www = presenceHeartbeatWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } else if (cp.crt == CurrentRequestType.Subscribe) {
                isSubscribeComplete = false;

                subscribeWww = new WWW (cp.url);
                yield return subscribeWww;
                //while ((subscribeWww != null) && (!subscribeWww.isDone)) { yield return null; }
                if ((subscribeWww != null) && (subscribeWww.isDone)) {
                    www = subscribeWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
                //subreqcount++;
            } else {
                isNonSubscribeComplete = false;
                nonSubscribeWww = new WWW (cp.url);
                yield return nonSubscribeWww;
                if ((nonSubscribeWww != null) && (nonSubscribeWww.isDone)) {
                    www = nonSubscribeWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } 

            try {
                if (www != null) {

                    //SetComplete (cp.crt);
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete sub {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
                    string message = "";
                    bool isError = false;

                    if (string.IsNullOrEmpty (www.error)) {
                        //UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Message: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.text), LoggingMethod.LevelError);

                        //FireEvent (www.text, false, false, this.pubnubRequestStateSub, cp.crt);
                        message = www.text;
                        isError = false;
                        //CallFireEvent<constructed> (www.text, false, false, this.pubnubRequestStateSub, cp);
                        //FireEvent (www.text, false, false, repository, cp.crt);
                    } else {
                        //UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Error: {2}", DateTime.Now.ToString (), cp.crt.ToString (), www.error), LoggingMethod.LevelError);
                        message = www.error;
                        isError = true;
                        //CallFireEvent<typeParameterType> (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, this.pubnubRequestStateSub, cp.crt);
                        //FireEvent (www.error, true, false, null, cp.crt);
                    } 
                    Type[] typeArgs = { typeParameterType };
                    Type generic = typeof(RequestState<>);
                    Type constructed = generic.MakeGenericType(typeArgs);
                    object repository = Activator.CreateInstance(constructed);
                    repository = this.pubnubRequestStateSub;
                    MethodInfo method = GetType().GetMethod("CallFireEvent")
                        .MakeGenericMethod(typeArgs);
                    method.Invoke(this, new[] { message, isError, false, repository, cp.crt });

                    //StopCoroutine ("CheckTimeoutSub");
                    //StopCoroutine ("SendRequestSub2");

                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutineSub {1}, Exception: {2}", DateTime.Now.ToString (), cp.crt.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1} {2}", DateTime.Now.ToString (), cp.crt.ToString (), subreqcount.ToString()), LoggingMethod.LevelError);
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);


        }*/

        void CallFireEvent<T> (string message, bool isError, bool isTimeout, RequestState<T> pubnubRequestState, CoroutineParams cp)
        {
            if (cp.crt == CurrentRequestType.Heartbeat) {
                FireEvent (message, isError, isTimeout, this.pubnubRequestStateHeartbeat, cp.crt);
            } else if (cp.crt == CurrentRequestType.PresenceHeartbeat) {
                FireEvent (message, isError, isTimeout, this.pubnubRequestStatePresenceHeartbeat, cp.crt);
            } else if (cp.crt == CurrentRequestType.Subscribe) {
                FireEvent (message, isError, isTimeout, this.pubnubRequestStateSub, cp.crt);
            } else {
                FireEvent (message, isError, isTimeout, this.pubnubRequestStateNonSub, cp.crt);
            }
        }

        /*public IEnumerator SendRequestSub<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            Debug.Log ("URL:" + url.ToString ());
            WWW www;

            if (crt == CurrentRequestType.Heartbeat) {
                isHearbeatComplete = false;
                heartbeatWww = new WWW (url);
                yield return heartbeatWww;
                //www = heartbeatWww;
                if ((heartbeatWww != null) && (heartbeatWww.isDone)) {
                    www = heartbeatWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                isPresenceHeartbeatComplete = false;
                presenceHeartbeatWww = new WWW (url);
                yield return presenceHeartbeatWww;
                //www = presenceHeartbeatWww;
                if ((presenceHeartbeatWww != null) && (presenceHeartbeatWww.isDone)) {
                    www = presenceHeartbeatWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } else if (crt == CurrentRequestType.Subscribe) {
                isSubscribeComplete = false;

                subscribeWww = new WWW (url);
                yield return subscribeWww;
                //while ((subscribeWww != null) && (!subscribeWww.isDone)) { yield return null; }
                if ((subscribeWww != null) && (subscribeWww.isDone)) {
                    www = subscribeWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
                //subreqcount++;
            } else {
                isNonSubscribeComplete = false;
                nonSubscribeWww = new WWW (url);
                yield return nonSubscribeWww;
                if ((nonSubscribeWww != null) && (nonSubscribeWww.isDone)) {
                    www = nonSubscribeWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } 

            try {
                if (www != null) {

                    SetCompleteSub (crt);
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete sub {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                    if (string.IsNullOrEmpty (www.error)) {
                        //UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Message: {2}", DateTime.Now.ToString (), crt.ToString (), www.text), LoggingMethod.LevelError);
                        FireEvent (www.text, false, false, pubnubRequestState, crt);
                    } else {
                        //UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Sub {1} Error: {2}", DateTime.Now.ToString (), crt.ToString (), www.error), LoggingMethod.LevelError);
                        FireEvent (www.error, true, false, pubnubRequestState, crt);
                    } 
                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutineSub {1}, Exception: {2}", DateTime.Now.ToString (), crt.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1} {2}", DateTime.Now.ToString (), crt.ToString (), subreqcount.ToString()), LoggingMethod.LevelError);
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequestSub exit {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
        }

        public IEnumerator SendRequest<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            Debug.Log ("URL:" + url.ToString ());
            WWW www;

            if (crt == CurrentRequestType.Heartbeat) {
                isHearbeatComplete = false;
                heartbeatWww = new WWW (url);
                yield return heartbeatWww;
                //www = heartbeatWww;
                if ((heartbeatWww != null) && (heartbeatWww.isDone)) {
                    www = heartbeatWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                isPresenceHeartbeatComplete = false;
                presenceHeartbeatWww = new WWW (url);
                yield return presenceHeartbeatWww;
                //www = presenceHeartbeatWww;
                if ((presenceHeartbeatWww != null) && (presenceHeartbeatWww.isDone)) {
                    www = presenceHeartbeatWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } else if (crt == CurrentRequestType.Subscribe) {
                isSubscribeComplete = false;

                subscribeWww = new WWW (url);
                yield return subscribeWww;
                //while ((subscribeWww != null) && (!subscribeWww.isDone)) { yield return null; }
                if ((subscribeWww != null) && (subscribeWww.isDone)) {
                    www = subscribeWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } else {
                isNonSubscribeComplete = false;
                nonSubscribeWww = new WWW (url);
                yield return nonSubscribeWww;
                if ((nonSubscribeWww != null) && (nonSubscribeWww.isDone)) {
                    www = nonSubscribeWww;
                } else {
                    www = null;
                    System.GC.Collect ();
                }
            } 

            try {
                if (www != null) {

                    SetComplete (crt);
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                    if (string.IsNullOrEmpty (www.error)) {
                        //UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW {1} Message: {2}", DateTime.Now.ToString (), crt.ToString (), www.text), LoggingMethod.LevelError);
                        FireEvent (www.text, false, false, pubnubRequestState, crt);
                    } else {
                        //UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW {1} Error: {2}", DateTime.Now.ToString (), crt.ToString (), www.error), LoggingMethod.LevelError);
                        FireEvent (www.error, true, false, pubnubRequestState, crt);
                    } 
                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutine {1}, Exception: {2}", DateTime.Now.ToString (), crt.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequest exit {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
        }*/

        void SetComplete (CurrentRequestType crt)
        {
            try {
                if (crt == CurrentRequestType.Heartbeat) {
                    isHearbeatComplete = true;
                    StopCoroutine("CheckTimeoutHeartbeat");  
                } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                    isPresenceHeartbeatComplete = true;
                    StopCoroutine("CheckTimeoutPresenceHeartbeat");
                } else if (crt == CurrentRequestType.Subscribe) {
                    isSubscribeComplete = true;
                    StopCoroutine("CheckTimeoutSub");
                } else {
                    isNonSubscribeComplete = true;
                    StopCoroutine("CheckTimeoutNonSub");
                } 
                //StopCoroutine("CheckTimeout");

                //FireForceStopCoroutine (false, crt);
                //FireForceStopCoroutineSub (false, crt);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SetComplete Exception: ", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }

        }


        /*void SetCompleteSub (CurrentRequestType crt)
        {
            try {
                if (crt == CurrentRequestType.Heartbeat) {
                    isHearbeatComplete = true;
                } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                    isPresenceHeartbeatComplete = true;
                } else if (crt == CurrentRequestType.Subscribe) {
                    isSubscribeComplete = true;
                } else {
                    isNonSubscribeComplete = true;
                } 

                //FireForceStopCoroutine (false, crt);
                //FireForceStopCoroutineSub (false, crt);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SetComplete Exception: ", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }

        }*/

        /*public bool CheckCompleteAndDispose (CurrentRequestType crt)
        {
            try {
                if (crt == CurrentRequestType.Heartbeat) {
                    //if ((!isHearbeatComplete) && (heartbeatWww != null) && (!heartbeatWww.isDone)) {
                    if ((!isHearbeatComplete) && (heartbeatWww != null) && (!heartbeatWww.isDone)) {
                        heartbeatWww.Dispose ();
                        heartbeatWww = null;
                        System.GC.Collect();
                        return false;
                    }
                } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                    if ((!isPresenceHeartbeatComplete) && (presenceHeartbeatWww != null) && (!presenceHeartbeatWww.isDone)) {
                        presenceHeartbeatWww.Dispose ();
                        presenceHeartbeatWww = null;
                        System.GC.Collect();
                        return false;
                    }
                } else if (crt == CurrentRequestType.Subscribe) {
                    if ((!isSubscribeComplete) && (subscribeWww != null) && (!subscribeWww.isDone)) {
                        subscribeWww.Dispose ();
                        subscribeWww = null;
                        System.GC.Collect();
                        return false;
                    }

                } else {
                    if ((!isNonSubscribeComplete) && (nonSubscribeWww != null) && (!nonSubscribeWww.isDone)) {
                        nonSubscribeWww.Dispose ();
                        nonSubscribeWww = null;
                        System.GC.Collect();
                        return false;
                    }
                } 
                
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, GetCompleteAndDispose Exception: ", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }

            return true;
        }*/

        public bool CheckComplete (CurrentRequestType crt)
        {
            try {
                if (crt == CurrentRequestType.Heartbeat) {
                    //if ((!isHearbeatComplete) && (heartbeatWww != null) && (!heartbeatWww.isDone)) {
                    if ((!isHearbeatComplete) && (heartbeatWww != null) && (!heartbeatWww.isDone)) {    
                        StopCoroutine ("SendRequestHeartbeat");
                        return false;
                    }
                } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                    if ((!isPresenceHeartbeatComplete) && (presenceHeartbeatWww != null) && (!presenceHeartbeatWww.isDone)) {
                        StopCoroutine ("SendRequestPresenceHeartbeat");
                        return false;
                    }
                } else if (crt == CurrentRequestType.Subscribe) {
                    if ((!isSubscribeComplete) && (subscribeWww != null) && (!subscribeWww.isDone)) {
                        StopCoroutine ("SendRequestSub");
                        return false;
                    }
                } else {
                    if ((!isNonSubscribeComplete) && (nonSubscribeWww != null) && (!nonSubscribeWww.isDone)) {
                        StopCoroutine ("SendRequestNonSub");
                        return false;
                    }
                } 

            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, GetCompleteAndDispose Exception: ", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }

            return true;
        }

        public void BounceRequest<T> (CurrentRequestType crt, RequestState<T> pubnubRequestState, bool fireEvent)
        {
            try {
                if ((crt == CurrentRequestType.Heartbeat) && (heartbeatWww != null) && (!heartbeatWww.isDone)) {
                    heartbeatWww.Dispose ();
                    heartbeatWww = null;
                    StopCoroutine ("SendRequestHeartbeat");
                    SetComplete (CurrentRequestType.Heartbeat);
                } else if ((crt == CurrentRequestType.PresenceHeartbeat) && (presenceHeartbeatWww != null) && (!presenceHeartbeatWww.isDone)) {
                    presenceHeartbeatWww.Dispose ();
                    presenceHeartbeatWww = null;
                    StopCoroutine ("SendRequestPresenceHeartbeat");
                    SetComplete (CurrentRequestType.PresenceHeartbeat);
                } else if ((crt == CurrentRequestType.Subscribe) && (subscribeWww != null) && (!subscribeWww.isDone)) {
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Dispose subscribeWww: ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                    subscribeWww.Dispose ();
                    
                    subscribeWww = null;
                    /*RequestState<T> requestState = new RequestState<T> ();
                    requestState.Channels = null;
                    requestState.Type = ResponseType.Subscribe;
                    requestState.UserCallback = null;
                    requestState.ErrorCallback = null;
                    requestState.Reconnect = false;

                    FireEvent ("Aborted", true, false, requestState);*/
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After Dispose subscribeWww: ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                    StopCoroutine ("SendRequestSub");

                    SetComplete (CurrentRequestType.Subscribe);
                } else if ((crt == CurrentRequestType.NonSubscribe) && (nonSubscribeWww != null) && (!nonSubscribeWww.isDone)) {
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Dispose nonSubscribeWww: ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                    nonSubscribeWww.Dispose ();
                    
                    nonSubscribeWww = null;
                    StopCoroutine ("SendRequestNonSub");
                    SetComplete (CurrentRequestType.NonSubscribe);
                }
                System.GC.Collect ();
                if ((pubnubRequestState != null) && (fireEvent)) {
                    FireEvent ("Aborted", true, false, pubnubRequestState, crt);
                }
                //FireForceStopCoroutineSub (false, crt);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, BounceRequest Exception: {1}", DateTime.Now.ToString (), ex.ToString ()), LoggingMethod.LevelError);
            }
            //StopCoroutine ("CheckTimeout");
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, BounceRequest {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
        }

        /*public IEnumerator CheckTimeout<T> (RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);
            yield return new WaitForSeconds (timeout); 
            try {
                if (!CheckComplete (crt)) {
                    FireEvent ("Timed out", true, true, pubnubRequestState, crt);

                    //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);

                    FireForceStopCoroutine (true, crt);
                }
                //LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), ex.ToString (), crt.ToString ()), LoggingMethod.LevelError);
            }
        }

        public IEnumerator CheckTimeoutSub<T> (RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);
            yield return new WaitForSeconds (timeout); 
            try {
                //if (!CheckCompleteAndDispose (crt)) {
                FireEvent ("Timed out", true, true, pubnubRequestState, crt);

                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);

                FireForceStopCoroutineSub (true, crt);
                //}
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), ex.ToString (), crt.ToString ()), LoggingMethod.LevelError);
            }
        }*/

        //public IEnumerator CheckTimeoutSub2<T> (CoroutineParams<T> cp)
        /*public IEnumerator CheckTimeout (CoroutineParams cp)
        {

            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);
            //isCheckTimeoutSub2Running = true;
            yield return new WaitForSeconds (cp.timeout); 
            try {

                if (!CheckComplete (cp.crt)) {
                    FireEvent ("Timed out", true, true, this.pubnubRequestStateSub, cp.crt);
                    //FireEvent ("Timed out", true, true, null, cp.crt);

                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);

                    //FireForceStopCoroutineSub (true, cp.crt);
                }
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), ex.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            }
            //isCheckTimeoutSub2Running = false;
        }*/
        public IEnumerator CheckTimeoutSub (CoroutineParams cp)
        {

            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);
            //isCheckTimeoutSub2Running = true;
            yield return new WaitForSeconds (cp.timeout); 
            try {

                if (!CheckComplete (cp.crt)) {
                    FireEvent ("Timed out", true, true, this.pubnubRequestStateSub, cp.crt);
                    //FireEvent ("Timed out", true, true, null, cp.crt);

                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);

                    //FireForceStopCoroutineSub (true, cp.crt);
                }
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), ex.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            }
            //isCheckTimeoutSub2Running = false;
        }

        public IEnumerator CheckTimeoutNonSub (CoroutineParams cp)
        {

            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);
            //isCheckTimeoutSub2Running = true;
            yield return new WaitForSeconds (cp.timeout); 
            try {

                if (!CheckComplete (cp.crt)) {
                    FireEvent ("Timed out", true, true, this.pubnubRequestStateSub, cp.crt);
                    //FireEvent ("Timed out", true, true, null, cp.crt);

                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);

                    //FireForceStopCoroutineSub (true, cp.crt);
                }
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), ex.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            }
            //isCheckTimeoutSub2Running = false;
        }

        public IEnumerator CheckTimeoutPresenceHeartbeat (CoroutineParams cp)
        {

            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);
            //isCheckTimeoutSub2Running = true;
            yield return new WaitForSeconds (cp.timeout); 
            try {

                if (!CheckComplete (cp.crt)) {
                    FireEvent ("Timed out", true, true, this.pubnubRequestStateSub, cp.crt);
                    //FireEvent ("Timed out", true, true, null, cp.crt);

                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);

                    //FireForceStopCoroutineSub (true, cp.crt);
                }
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), ex.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            }
            //isCheckTimeoutSub2Running = false;
        }

        public IEnumerator CheckTimeoutHeartbeat (CoroutineParams cp)
        {

            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);
            //isCheckTimeoutSub2Running = true;
            yield return new WaitForSeconds (cp.timeout); 
            try {

                if (!CheckComplete (cp.crt)) {
                    FireEvent ("Timed out", true, true, this.pubnubRequestStateSub, cp.crt);
                    //FireEvent ("Timed out", true, true, null, cp.crt);

                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), cp.timeout.ToString ()), LoggingMethod.LevelError);

                    //FireForceStopCoroutineSub (true, cp.crt);
                }
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit {1}", DateTime.Now.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), ex.ToString (), cp.crt.ToString ()), LoggingMethod.LevelError);
            }
            //isCheckTimeoutSub2Running = false;
        }

        /*public void FireForceStopCoroutineSub (bool isTimeout, CurrentRequestType crt)
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, FireForceStopCoroutineSub {1} {2}", DateTime.Now.ToString (), isTimeout, crt.ToString ()), LoggingMethod.LevelError);
            try {
                if (isTimeout) {
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, subscribeWww running trying to abort 2 {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                    StopCoroutine ("SendRequestSub");
                    if (subscribeWww == null) {
                        //subreqcount--;
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, subscribeWww aborted 2 {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                    }
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequest: {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                } else {
                    StopCoroutine ("CheckTimeoutSub");
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1} {2}", DateTime.Now.ToString (), crt.ToString (), isCheckTimeoutSub2Running), LoggingMethod.LevelError);
                }
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, FireForceStopCoroutineSub: {1} {2}", DateTime.Now.ToString (), ex.ToString (), crt.ToString ()), LoggingMethod.LevelError);
            }
        }*/

        /*public void FireForceStopCoroutine (bool isTimeout, CurrentRequestType crt)
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, FireForceStopCoroutine {1} {2}", DateTime.Now.ToString (), isTimeout, crt.ToString ()), LoggingMethod.LevelError);
            try {
                if (isTimeout) {
                    //StopCoroutine ("SendRequest");
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequest: {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                } else {
                    //StopCoroutine ("CheckTimeout");
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: {1}", DateTime.Now.ToString (), crt.ToString ()), LoggingMethod.LevelError);
                }
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, FireForceStopCoroutine: {1} {2}", DateTime.Now.ToString (), ex.ToString (), crt.ToString ()), LoggingMethod.LevelError);
            }
        }*/

        //public void FireEvent<T> (string message, bool isError, bool isTimeout, RequestState<T> pubnubRequestState)
        public void FireEvent<T> (string message, bool isError, bool isTimeout, RequestState<T> pubnubRequestState, CurrentRequestType crt)
        {
            CustomEventArgs<T> cea = new CustomEventArgs<T> ();
            cea.PubnubRequestState = pubnubRequestState;
            cea.Message = message;
            cea.IsError = isError;
            cea.IsTimeout = isTimeout;
            if ((crt == CurrentRequestType.Heartbeat) && (heartbeatCoroutineComplete != null)) {
                heartbeatCoroutineComplete.Raise (this, cea);
            } else if ((crt == CurrentRequestType.PresenceHeartbeat) && (presenceHeartbeatCoroutineComplete != null)) {
                presenceHeartbeatCoroutineComplete.Raise (this, cea);
            } else if ((crt == CurrentRequestType.Subscribe) && (subCoroutineComplete != null)) {
                subCoroutineComplete.Raise (this, cea);
            } else if ((crt == CurrentRequestType.NonSubscribe) && (nonSubCoroutineComplete != null)) {
                nonSubCoroutineComplete.Raise (this, cea);
            }
        }

        /*void HandleForceStopCoroutine<T> (object sender, EventArgs e)
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Stopping coroutine", DateTime.Now.ToString ()), LoggingMethod.LevelError);
            ForceQuitCoroutieArgs<T> fqca = e as ForceQuitCoroutieArgs<T>;
            if (fqca.isTimeout) {
                StopCoroutine (fqca.crRequest);
            } else {
                StopCoroutine (fqca.crTimeout);
            }
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, Coroutine Stopped", DateTime.Now.ToString ()), LoggingMethod.LevelError);
        }

        void HandleCoroutineComplete (object sender, EventArgs cea)
        {
            if (CoroutineComplete != null) {
                CoroutineComplete.Raise (this, cea);
            }
        }*/
    }
    #endregion

    #region "PubnubWebResponse and PubnubWebRequest"
    public class PubnubWebResponse
    {
        WWW www;

        public PubnubWebResponse (WWW www)
        {
            this.www = www;
        }

        public string ResponseUri {
            get {
                return www.url;
            }
        }

        public Dictionary<string, string> Headers {
            get {
                return www.responseHeaders;
            }
        }
    }

    public class PubnubWebRequest
    {
        WWW www;

        public PubnubWebRequest (WWW www)
        {
            this.www = www;
        }

        public string RequestUri {
            get {
                return www.url;
            }
        }

        public Dictionary<string, string> Headers {
            get {
                return www.responseHeaders;
            }
        }

    }
    #endregion
}

