using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        internal string message;
        internal RequestState<T> pubnubRequestState;
        internal bool isError;
        internal bool isTimeout;
        internal CoroutineClass.CurrentRequestType crt;
    }
    #endregion


    #region CoroutineClass
    internal class ForceQuitCoroutieArgs<T> : EventArgs
    {
        internal bool isTimeout;
        internal IEnumerator crTimeout;
        internal IEnumerator crRequest;
    }

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

    class CoroutineClass : MonoBehaviour
    {
        private bool isHearbeatComplete = false;
        private bool isPresenceHeartbeatComplete = false;
        private bool isSubscribeComplete = false;
        private bool isNonSubscribeComplete = false;

        /*private bool isHearbeat = false;
        private bool ispresenceHeartbeat = false;
        private bool isSubscribe = false;
        private bool isNonSubscribe = false;*/
        public enum CurrentRequestType{
            Heartbeat,
            PresenceHeartbeat,
            Subscribe,
            NonSubscribe
        }

        WWW subscribeWww;
        WWW heartbeatWww;
        WWW presenceHeartbeatWww;
        WWW nonSubscribeWww;

        //public bool isComplete = false;
        //public event EventHandler<EventArgs> CoroutineComplete;
        private EventHandler<EventArgs> subCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> SubCoroutineComplete
        {
            add
            {
                if (subCoroutineComplete == null || !subCoroutineComplete.GetInvocationList().Contains(value))
                {
                    subCoroutineComplete += value;
                }
            }
            remove
            {
                subCoroutineComplete -= value;
            }
        }

        private EventHandler<EventArgs> nonSubCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> NonSubCoroutineComplete
        {
            add
            {
                if (nonSubCoroutineComplete == null || !nonSubCoroutineComplete.GetInvocationList().Contains(value))
                {
                    nonSubCoroutineComplete += value;
                }
            }
            remove
            {
                nonSubCoroutineComplete -= value;
            }
        }

        private EventHandler<EventArgs> presenceHeartbeatCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> PresenceHeartbeatCoroutineComplete
        {
            add
            {
                if (presenceHeartbeatCoroutineComplete == null || !presenceHeartbeatCoroutineComplete.GetInvocationList().Contains(value))
                {
                    presenceHeartbeatCoroutineComplete += value;
                }
            }
            remove
            {
                presenceHeartbeatCoroutineComplete -= value;
            }
        }

        private EventHandler<EventArgs> heartbeatCoroutineComplete;
        //Register single event handler
        public event EventHandler<EventArgs> HeartbeatCoroutineComplete
        {
            add
            {
                if (heartbeatCoroutineComplete == null || !heartbeatCoroutineComplete.GetInvocationList().Contains(value))
                {
                    heartbeatCoroutineComplete += value;
                }
            }
            remove
            {
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
            if ((pubnubRequestState.Type == ResponseType.Heartbeat) || (pubnubRequestState.Type == ResponseType.PresenceHeartbeat)) {
                crt = CurrentRequestType.PresenceHeartbeat;
                if (pubnubRequestState.Type == ResponseType.Heartbeat) {
                    crt = CurrentRequestType.Heartbeat;
                } 
                GetCompleteAndDispose (crt);
                if (pubnubRequestState.Reconnect) {
                    StartCoroutine (DelayRequest<T> (url, pubnubRequestState, timeout, pause, crt));
                } else {
                    StartCoroutines<T> (url, pubnubRequestState, timeout, pause, crt);
                }
            }else if((pubnubRequestState.Type == ResponseType.Subscribe) || (pubnubRequestState.Type == ResponseType.Presence)){
                crt = CurrentRequestType.Subscribe;
                GetCompleteAndDispose (crt);
                StartCoroutines<T> (url, pubnubRequestState, timeout, pause, crt);
            } else {
                crt = CurrentRequestType.NonSubscribe;
                GetCompleteAndDispose (crt);
                StartCoroutines<T> (url, pubnubRequestState, timeout, pause, crt);
            } 
        }

        private void StartCoroutines<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt){
            StartCoroutine (CheckTimeout<T>(pubnubRequestState, timeout, pause, crt));
            StartCoroutine (SendRequest<T>(url, pubnubRequestState, timeout, pause, crt));
        }

        //public IEnumerator PausedRequest<T> (ProcessRequests<T> pr, int pause){
        public IEnumerator DelayRequest<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt){
            yield return new WaitForSeconds(pause); 
            StartCoroutines<T> (url, pubnubRequestState, timeout, pause, crt);
            //StartCoroutine (CheckTimeout<T>(pubnubRequestState, timeout, pause));
            //StartCoroutine (SendRequest<T>(url, pubnubRequestState, timeout, pause));

            /*IEnumerator crTimeout = pr.crTimeout;
            IEnumerator crRequest = pr.crRequest;

            StartCoroutine (crTimeout);
            StartCoroutine (crRequest);*/
        }

        public IEnumerator SendRequest<T> (string url, RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            Debug.Log ("URL:" + url.ToString ());
            WWW www;

            if (crt == CurrentRequestType.Heartbeat) {
                isHearbeatComplete = false;
                heartbeatWww = new WWW (url);
                yield return heartbeatWww;
                www = heartbeatWww;
            } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                isPresenceHeartbeatComplete = false;
                presenceHeartbeatWww = new WWW (url);
                yield return presenceHeartbeatWww;
                www = presenceHeartbeatWww;
            } else if (crt == CurrentRequestType.Subscribe) {
                isSubscribeComplete = false;

                subscribeWww = new WWW (url);
                yield return subscribeWww;
                //while ((subscribeWww != null) && (!subscribeWww.isDone)) { yield return null; }
                if ((subscribeWww != null) && (subscribeWww.isDone)) {
                    www = subscribeWww;
                } else {
                    www = null;
                }
            } else {
                isNonSubscribeComplete = false;
                nonSubscribeWww = new WWW (url);
                yield return nonSubscribeWww;
                www = nonSubscribeWww;
            } 

            try {
                if(www != null){

                    SetComplete(crt);
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, After set complete {1}", DateTime.Now.ToString (), crt.ToString()), LoggingMethod.LevelError);
                    if (string.IsNullOrEmpty(www.error)) {
                        UnityEngine.Debug.Log ("Message: " + www.text);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW {1} Message: {2}", DateTime.Now.ToString (), crt.ToString(), www.text), LoggingMethod.LevelError);
                        FireEvent (www.text, false, false, pubnubRequestState, crt);
                    } else {
                        UnityEngine.Debug.Log ("Error: " + www.error);
                        LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW {1} Error: {2}", DateTime.Now.ToString (), crt.ToString(), www.error), LoggingMethod.LevelError);
                        FireEvent (www.error, true, false, pubnubRequestState, crt);
                    } 
                } 
            } catch (Exception ex) {
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, RunCoroutine {1}, Exception: {2}", DateTime.Now.ToString (), crt.ToString(), ex.ToString ()), LoggingMethod.LevelError);
            }
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequest exit {1}", DateTime.Now.ToString (), crt.ToString()), LoggingMethod.LevelError);
        }

        void SetComplete (CurrentRequestType crt)
        {
            try
            {
                if (crt == CurrentRequestType.Heartbeat) {
                    isHearbeatComplete = true;
                } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                    isPresenceHeartbeatComplete = true;
                } else if (crt == CurrentRequestType.Subscribe) {
                    isSubscribeComplete = true;
                } else {
                    isNonSubscribeComplete = true;
                } 

                FireForceStopCoroutine (false);
            }catch (Exception ex){
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SetComplete Exception: ", DateTime.Now.ToString (), ex.ToString()), LoggingMethod.LevelError);
            }

        }

        public bool GetCompleteAndDispose (CurrentRequestType crt)
        {
            try{
                if (crt == CurrentRequestType.Heartbeat) {
                    if ((!isHearbeatComplete) && (heartbeatWww != null)) {
                        heartbeatWww.Dispose ();
                        heartbeatWww = null;
                        return false;
                    }
                } else if (crt == CurrentRequestType.PresenceHeartbeat) {
                    if ((!isPresenceHeartbeatComplete) && (presenceHeartbeatWww != null)) {
                        presenceHeartbeatWww.Dispose ();
                        presenceHeartbeatWww = null;
                        return false;
                    }
                } else if (crt == CurrentRequestType.Subscribe) {
                    if ((!isSubscribeComplete) && (subscribeWww != null)) {
                        subscribeWww.Dispose ();
                        subscribeWww = null;
                        return false;
                    }
                } else {
                    if ((!isNonSubscribeComplete) && (nonSubscribeWww != null)) {
                        nonSubscribeWww.Dispose ();
                        nonSubscribeWww = null;
                        return false;
                    }
                } 
            }catch (Exception ex){
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, GetCompleteAndDispose Exception: ", DateTime.Now.ToString (), ex.ToString()), LoggingMethod.LevelError);
            }

            return true;
        }

        //public void BounceSubscribe<T> ()
        public void BounceRequest (CurrentRequestType crt)
        {
            try{
                if ((crt == CurrentRequestType.Heartbeat) && (heartbeatWww != null)) {
                    heartbeatWww.Dispose ();
                    heartbeatWww = null;
                    SetComplete (CurrentRequestType.Heartbeat);
                } else if ((crt == CurrentRequestType.PresenceHeartbeat) && (presenceHeartbeatWww != null)){
                    presenceHeartbeatWww.Dispose ();
                    presenceHeartbeatWww = null;
                    SetComplete (CurrentRequestType.PresenceHeartbeat);
                } else if ((crt == CurrentRequestType.Subscribe) && (subscribeWww != null)){
                    subscribeWww.Dispose ();
                    subscribeWww = null;
                    /*RequestState<T> requestState = new RequestState<T> ();
                    requestState.Channels = null;
                    requestState.Type = ResponseType.Subscribe;
                    requestState.UserCallback = null;
                    requestState.ErrorCallback = null;
                    requestState.Reconnect = false;

                    FireEvent ("Aborted", true, false, requestState);*/
                    SetComplete (CurrentRequestType.Subscribe);
                } else if ((crt == CurrentRequestType.NonSubscribe) && (nonSubscribeWww != null)){
                    nonSubscribeWww.Dispose ();
                    nonSubscribeWww = null;
                    SetComplete (CurrentRequestType.NonSubscribe);
                }

                FireForceStopCoroutine (false);
            }catch (Exception ex){
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, BounceRequest Exception: ", DateTime.Now.ToString (), ex.ToString()), LoggingMethod.LevelError);
            }
            //StopCoroutine ("CheckTimeout");
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, BounceRequest", DateTime.Now.ToString ()), LoggingMethod.LevelError);
        }

        public IEnumerator CheckTimeout<T> (RequestState<T> pubnubRequestState, int timeout, int pause, CurrentRequestType crt)
        {
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, yielding: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);
            yield return new WaitForSeconds(timeout); 
            if (!GetCompleteAndDispose(crt)) {
                FireEvent ("Timed out", true, true, pubnubRequestState, crt);

                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, WWW Error: {1} sec timeout", DateTime.Now.ToString (), timeout.ToString ()), LoggingMethod.LevelError);

                FireForceStopCoroutine (true);
            }
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout exit ", DateTime.Now.ToString (), crt.ToString()), LoggingMethod.LevelError);
        }

        public void FireForceStopCoroutine(bool isTimeout){
            LoggingMethod.WriteToLog (string.Format ("DateTime {0}, FireForceStopCoroutine {1}", DateTime.Now.ToString (), isTimeout), LoggingMethod.LevelError);
            try{
                if (isTimeout) {
                    //StopCoroutine ("SendRequest");
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, SendRequest: ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                } else {
                    //StopCoroutine ("CheckTimeout");
                    LoggingMethod.WriteToLog (string.Format ("DateTime {0}, CheckTimeout: ", DateTime.Now.ToString ()), LoggingMethod.LevelError);
                }
            } catch (Exception ex){
                LoggingMethod.WriteToLog (string.Format ("DateTime {0}, FireForceStopCoroutine: {1}", DateTime.Now.ToString (), ex.ToString()), LoggingMethod.LevelError);
            }
        }

        //public void FireEvent<T> (string message, bool isError, bool isTimeout, RequestState<T> pubnubRequestState)
        public void FireEvent<T> (string message, bool isError, bool isTimeout, RequestState<T> pubnubRequestState, CurrentRequestType crt)
        {
            CustomEventArgs<T> cea = new CustomEventArgs<T> ();
            cea.pubnubRequestState = pubnubRequestState;
            cea.message = message;
            cea.isError = isError;
            cea.isTimeout = isTimeout;

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

