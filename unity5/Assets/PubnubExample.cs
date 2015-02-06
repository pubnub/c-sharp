using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using PubNubMessaging.Core;
//using System.Collections.Concurrent;
using System;
using System.Reflection;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
//using System.Threading;
using System.Text;
using System.Collections.Generic;

public class PubnubExample : MonoBehaviour
{
    enum PubnubState
    {
        None,
        GrantSubscribe,
        AuditSubscribe,
        RevokeSubscribe,
        GrantPresence,
        AuditPresence,
        RevokePresence,
        AuthKey,
        Presence,
        Subscribe,
        Publish,
        DetailedHistory,
        HereNow,
        Time,
        Unsubscribe,
        PresenceUnsubscribe,
        DisconnectRetry,
        EnableNetwork,
        DisableNetwork,
        SetUserStateKeyValue,
        ViewUserState,
        DelUserState,
        GetUserState,
        SetUserStateJson,
        PresenceHeartbeat,
        PresenceInterval,
        WhereNow,
        GlobalHereNow,
        ChangeUUID
    }

    bool ssl = false;
    bool resumeOnReconnect = true;
    string cipherKey = "";
    string secretKey = "demo";
    string publishKey = "demo";
    string subscribeKey = "demo";
    string uuid = Guid.NewGuid ().ToString ();
    string subscribeTimeoutInSeconds = "310";
    string operationTimeoutInSeconds = "45";
    string networkMaxRetries = "50";
    string networkRetryIntervalInSeconds = "10";
    string heartbeatIntervalInSeconds = "10";
    static public bool showErrorMessageSegments = true;
    string channel = "hello_world";
    string publishedMessage = "";
    string input = "";
    static Pubnub pubnub;
    PubnubState state;
    //private static ConcurrentQueue<string> recordQueue = new ConcurrentQueue<string> ();
    private static Queue<string> recordQueue = new Queue<string> ();
    Vector2 scrollPosition = Vector2.zero;
    string pubnubApiResult = "";

    #if(UNITY_IOS)
   	bool requestInProcess = false;
    #endif
    bool showPublishPopupWindow = false;
    bool showGrantWindow = false;
    bool showAuthWindow = false;
    bool showTextWindow = false;
    bool showActionsPopupWindow = false;
    bool showPamPopupWindow = false;
    bool toggle1 = false;
    bool toggle2 = false;
    string valueToSet = "";
    string valueToSetSubs = "1440";
    string valueToSetAuthKey = "";
    string text1 = "";
    string text2 = "";
    string text3 = "";
    bool storeInHistory = true;

    Rect publishWindowRect = new Rect (60, 365, 300, 180);
    Rect authWindowRect = new Rect (60, 365, 300, 200);
    Rect textWindowRect = new Rect (60, 365, 300, 250);
    Rect textWindowRect2 = new Rect (60, 365, 300, 250);
    bool allowUserSettingsChange = true;
    float fLeft = 20;
    float fLeftInit = 20;
    float fTop = 10;
    float fTopInit = 10;
    float fRowHeight = 45;
    float fHeight = 35;
    float fButtonHeight = 40;
    float fButtonWidth = 120;

    public void OnGUI ()
    {
        GUI.enabled = !allowUserSettingsChange;

        GUIStyle customStyle = new GUIStyle (GUI.skin.button);

        customStyle.fontSize = 10;
        customStyle.hover.textColor = Color.yellow;
        customStyle.fontStyle = FontStyle.Italic;

        fLeft = fLeftInit;
        fTop = fTopInit + 0 * fRowHeight;
        if (GUI.Button (new Rect (fLeft, fTop, 120, 40), "Reset Settings", customStyle)) {
            allowUserSettingsChange = true;
            ResetPubnubInstance ();
            pubnubApiResult = "";
        }

        GUI.enabled = allowUserSettingsChange;

        fLeft = fLeftInit + 150;
        ssl = GUI.Toggle (new Rect (fLeft, fTop, 100, fButtonHeight), ssl, " Enable SSL ");

        fLeft = fLeft + 100;
        resumeOnReconnect = GUI.Toggle (new Rect (fLeft, fTop, 200, fButtonHeight), resumeOnReconnect, " Resume On Reconnect ");

        GUI.enabled = true;

        fTop = fTopInit + 1 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 90, fHeight), "Channel Name");
        fLeft = fLeft + 90;
        channel = GUI.TextField (new Rect (fLeft, fTop, 140, fHeight), channel, 100);

        fLeft = fLeft + 140 + 10; 
        if (GUI.Button (new Rect (fLeft, fTop, 90, fButtonHeight), "Actions")) {
            showPamPopupWindow = false;
            showActionsPopupWindow = !showActionsPopupWindow;
        }
        if (showActionsPopupWindow) {
            ShowActions (fLeft, fTop, fButtonHeight);
        }

        fLeft = fLeft + 90 + 10;
        if (GUI.Button (new Rect (fLeft, fTop, 90, fButtonHeight), "PAM & More")) {
            showActionsPopupWindow = false;
            showPamPopupWindow = !showPamPopupWindow;
        }
        if (showPamPopupWindow) {
            ShowPamActions (fLeft + 90, fTop, fButtonHeight);
        }

        GUI.enabled = allowUserSettingsChange;

        fTop = fTopInit + 2 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Cipher Key");

        fLeft = fLeft + 75;
        cipherKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), cipherKey);

        fLeft = fLeft + 145;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "UUID");

        fLeft = fLeft + 45;
        uuid = GUI.TextField (new Rect (fLeft, fTop, 170, fHeight), uuid);

        fTop = fTopInit + 3 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Subscribe Key");

        fLeft = fLeft + 75;
        subscribeKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), subscribeKey);

        fLeft = fLeft + 145;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Publish Key");

        fLeft = fLeft + 85;
        publishKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), publishKey);


        fTop = fTopInit + 4 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 70, fHeight), "Secret Key");
        fLeft = fLeft + 75;
        secretKey = GUI.TextField (new Rect (fLeft, fTop, 130, fHeight), secretKey);

        fLeft = fLeft + 145;
        GUI.Label (new Rect (fLeft, fTop, 160, fHeight), "Subscribe Timeout (secs)");

        fLeft = fLeft + 185;
        subscribeTimeoutInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), subscribeTimeoutInSeconds, 6);
        subscribeTimeoutInSeconds = Regex.Replace (subscribeTimeoutInSeconds, "[^0-9]", "");

        fTop = fTopInit + 5 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 160, fHeight), "MAX retries");

        fLeft = fLeft + 175;
        networkMaxRetries = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), networkMaxRetries, 6);
        networkMaxRetries = Regex.Replace (networkMaxRetries, "[^0-9]", "");

        fLeft = fLeft + 45;
        GUI.Label (new Rect (fLeft, fTop, 180, fHeight), "Non Subscribe Timeout (secs)");

        fLeft = fLeft + 185;
        operationTimeoutInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), operationTimeoutInSeconds, 6);
        operationTimeoutInSeconds = Regex.Replace (operationTimeoutInSeconds, "[^0-9]", "");

        fTop = fTopInit + 6 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label (new Rect (fLeft, fTop, 160, fHeight), "Retry Interval (secs)");

        fLeft = fLeft + 175;
        networkRetryIntervalInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), networkRetryIntervalInSeconds, 6);
        networkRetryIntervalInSeconds = Regex.Replace (networkRetryIntervalInSeconds, "[^0-9]", "");

        fLeft = fLeft + 45;
        GUI.Label (new Rect (fLeft, fTop, 180, fHeight), "Heartbeat Interval (secs)");
        fLeft = fLeft + 185;
        heartbeatIntervalInSeconds = GUI.TextField (new Rect (fLeft, fTop, 30, fHeight), heartbeatIntervalInSeconds, 6);
        heartbeatIntervalInSeconds = Regex.Replace (heartbeatIntervalInSeconds, "[^0-9]", "");

        GUI.enabled = true;

        if (showPublishPopupWindow) {
            GUI.backgroundColor = Color.black;
            publishWindowRect = GUI.ModalWindow (0, publishWindowRect, DoPublishWindow, "Message Publish");
            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }
        if (showAuthWindow) {
            GUI.backgroundColor = Color.black;
            if (state == PubnubState.AuthKey) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoAuthWindow, "Enter Auth Key");
            } else if (state == PubnubState.ChangeUUID) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoChangeUuidWindow, "Change UUID");
            } else if (state == PubnubState.WhereNow) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoWhereNowWindow, "Where Now");
            } else if (state == PubnubState.PresenceHeartbeat) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoPresenceHeartbeatWindow, "Presence Heartbeat");
            } else if (state == PubnubState.AuditPresence) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoAuditPresenceWindow, "Audit Presence");
            } else if (state == PubnubState.AuditSubscribe) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoAuditSubscribeWindow, "Audit Subscribe");
            } else if (state == PubnubState.RevokePresence) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoRevokePresenceWindow, "Revoke Presence");
            } else if (state == PubnubState.RevokeSubscribe) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoRevokeSubscribeWindow, "Revoke Subscribe");
            } else if (state == PubnubState.PresenceInterval) {
                authWindowRect = GUI.ModalWindow (0, authWindowRect, DoPresenceIntervalWindow, "Presence Interval");
            }
            //state = PubnubState.None;
            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }        
        if (showGrantWindow) {
            GUI.backgroundColor = Color.black;
            string title = "";
            if (state == PubnubState.GrantPresence) {
                title = "Presence Grant";
            } else if (state == PubnubState.GrantSubscribe) {
                title = "Subscribe Grant";
            } else if (state == PubnubState.HereNow) {
                title = "Here now";
            } else if (state == PubnubState.GlobalHereNow) {
                title = "Global here now";
            }
            authWindowRect = GUI.ModalWindow (0, authWindowRect, DoGrantWindow, title);
            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }
        if (showTextWindow) {
            GUI.backgroundColor = Color.black;
            string title = "";
            if (state == PubnubState.GetUserState) {
                title = "Get User State";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.DelUserState) {
                title = "Delete User Statue";
                textWindowRect2 = GUI.ModalWindow (0, textWindowRect2, DoTextWindow, title);
            } else if (state == PubnubState.SetUserStateJson) {
                title = "Set User State using Json";
                textWindowRect = GUI.ModalWindow (0, textWindowRect, DoTextWindow, title);
            } else if (state == PubnubState.SetUserStateKeyValue) {
                title = "Set User State using Key-Value pair";
                textWindowRect = GUI.ModalWindow (0, textWindowRect, DoTextWindow, title);
            }

            GUI.backgroundColor = new Color (1, 1, 1, 1);
        }

        fTop = fTopInit + 7 * fRowHeight;
        fLeft = fLeftInit;
        scrollPosition = GUI.BeginScrollView (new Rect (fLeft, fTop, 430, 420), scrollPosition, new Rect (fLeft, fTop, 430, 420), false, true);
        GUI.enabled = false;
        pubnubApiResult = GUI.TextArea (new Rect (fLeft, fTop, 430, 420), pubnubApiResult);            
        GUI.enabled = true;
        GUI.EndScrollView ();
    }

    void ShowPamActions (float fLeft, float fTop, float fButtonHeight)
    {
        Rect windowRect = new Rect (fLeft - 160, fTop + fButtonHeight, 160, 650);
        GUI.backgroundColor = Color.black;
        windowRect = GUI.Window (0, windowRect, DoPamActionWindow, "");
        GUI.backgroundColor = new Color (1, 1, 1, 1);
    }

    void ShowActions (float fLeft, float fTop, float fButtonHeight)
    {
        Rect windowRect = new Rect (fLeft, fTop + fButtonHeight, 140, 650);
        GUI.backgroundColor = Color.black;
        windowRect = GUI.Window (0, windowRect, DoActionWindow, "");
        GUI.backgroundColor = new Color (1, 1, 1, 1);
    }

    void DoTextWindow (int windowID)
    {
        string title = "";
        string buttonTitle = "";
        string label1 = "";
        string label2 = "";
        string label3 = "";

        if (state == PubnubState.GetUserState) {
            title = "Get User State";
            label1 = "Channel";
            label2 = "UUID";
            buttonTitle = "Get";
        } else if (state == PubnubState.DelUserState) {
            title = "Delete User Statue";
            label1 = "Channel";
            label2 = "Key";
            buttonTitle = "Delete";
        } else if (state == PubnubState.SetUserStateJson) {
            title = "Set User State";
            label1 = "Channel";
            label2 = "UUID";
            label3 = "Enter Json";
            buttonTitle = "Set";
        } else if (state == PubnubState.SetUserStateKeyValue) {
            title = "Set User State";
            label1 = "Channel";
            label2 = "Key";
            label3 = "Value";
            buttonTitle = "Set";
        }

        fLeft = fLeftInit;
        fTop = 20;
        GUI.Label (new Rect (fLeft, fTop, 100, fHeight), label1);
        fLeft = fLeftInit + 100;

        text1 = GUI.TextArea (new Rect (fLeft, fTop, 90, fButtonHeight), text1, 20);

        fLeft = fLeftInit;
        fTop = fTop + fButtonHeight;
        GUI.Label (new Rect (fLeft, fTop, 100, fHeight), label2);
        fLeft = fLeftInit + 100;

        text2 = GUI.TextArea (new Rect (fLeft, fTop, 90, fButtonHeight), text2, 20);


        if ((state == PubnubState.SetUserStateJson) || (state == PubnubState.SetUserStateKeyValue)) {
            fLeft = fLeftInit;
            fTop = fTop + 2 * fHeight - 20;
            GUI.Label (new Rect (fLeft, fTop, 100, fHeight + 30), label3);
            fLeft = fLeftInit + 100;

            text3 = GUI.TextArea (new Rect (fLeft, fTop, 90, fButtonHeight), text3, 20);
            fLeft = fLeftInit;
            fTop = fTop + 3 * fHeight - 40;

        } else {
            fLeft = fLeftInit;
            fTop = fTop + 2 * fButtonHeight;
        }
        if (GUI.Button (new Rect (fLeft, fTop, 100, fButtonHeight), buttonTitle)) {
            string currentChannel = text1;

            if (state == PubnubState.GetUserState) {
                AddToPubnubResultContainer ("Running get user state");
                pubnub.GetUserState<string> (text1, text2, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.DelUserState) {
                AddToPubnubResultContainer ("Running delete user state");
                string stateKey = text2;
                /*ThreadPool.QueueUserWorkItem (new WaitCallback (
                    delegate {*/
                        pubnub.SetUserState<string> (currentChannel, new KeyValuePair<string, object> (stateKey, null), DisplayReturnMessage, DisplayErrorMessage);
                    /*}
                ));*/
            } else if (state == PubnubState.SetUserStateJson) {
                AddToPubnubResultContainer ("Running Set User State Json");
                string currentUuid = text2;
                string jsonUserState = "";

                if (string.IsNullOrEmpty (text3)) {
                    //jsonUserState = pubnub.GetLocalUserState (text1);
                } else {
                    jsonUserState = text3;
                }
                /*ThreadPool.QueueUserWorkItem (new WaitCallback (
                    delegate {*/
                        pubnub.SetUserState<string> (currentChannel, currentUuid, jsonUserState, DisplayReturnMessage, DisplayErrorMessage);
                    /*}
                ));*/
            } else if (state == PubnubState.SetUserStateKeyValue) {
                AddToPubnubResultContainer ("Running Set User State Key Value");
                int valueInt;
                double valueDouble;
                string currentState = "";
                string stateKey = text2;
                if (Int32.TryParse (text3, out valueInt)) {
                    /*ThreadPool.QueueUserWorkItem (new WaitCallback (
                        delegate {*/
                            pubnub.SetUserState<string> (currentChannel, "", new KeyValuePair<string, object> (stateKey, valueInt), DisplayReturnMessage, DisplayErrorMessage);
                        //}));
                } else if (Double.TryParse (text3, out valueDouble)) {
                    /*ThreadPool.QueueUserWorkItem (new WaitCallback (
                        delegate {*/
                            pubnub.SetUserState<string> (currentChannel, "", new KeyValuePair<string, object> (stateKey, valueDouble), DisplayReturnMessage, DisplayErrorMessage);
                        //}));
                } else {
                    string val = text3;
                    /*ThreadPool.QueueUserWorkItem (new WaitCallback (
                        delegate {*/
                            pubnub.SetUserState<string> (currentChannel, "", new KeyValuePair<string, object> (stateKey, val), DisplayReturnMessage, DisplayErrorMessage);
                        //}));
                }
            }

            text1 = "";
            text2 = "";
            if ((state == PubnubState.SetUserStateJson) || (state == PubnubState.SetUserStateKeyValue)) {
                text3 = "";
            }
            showTextWindow = false;
            showPamPopupWindow = false;
        }

        fLeft = fLeftInit + 100;
        if (GUI.Button (new Rect (fLeft, fTop, 100, fButtonHeight), "Cancel")) {
            text1 = "";
            text2 = "";
            if ((state == PubnubState.SetUserStateJson) || (state == PubnubState.SetUserStateKeyValue)) {
                text3 = "";
            }
            showTextWindow = false;
            showPamPopupWindow = false;
        }
        GUI.DragWindow (new Rect (0, 0, 800, 400));
    }

    void DoAuditPresenceWindow (int windowID)
    {
        ShowWindow (PubnubState.AuditPresence);
    }

    void DoAuditSubscribeWindow (int windowID)
    {
        ShowWindow (PubnubState.AuditSubscribe);
    }

    void DoRevokePresenceWindow (int windowID)
    {
        ShowWindow (PubnubState.RevokePresence);
    }

    void DoRevokeSubscribeWindow (int windowID)
    {
        ShowWindow (PubnubState.RevokeSubscribe);
    }

    void DoAuthWindow (int windowID)
    {
        ShowWindow (PubnubState.AuthKey);
    }

    void DoChangeUuidWindow (int windowID)
    {
        ShowWindow (PubnubState.ChangeUUID);
    }

    void DoWhereNowWindow (int windowID)
    {
        ShowWindow (PubnubState.WhereNow);
    }

    void DoPresenceHeartbeatWindow (int windowID)
    {
        ShowWindow (PubnubState.PresenceHeartbeat);
    }

    void DoPresenceIntervalWindow (int windowID)
    {
        ShowWindow (PubnubState.PresenceInterval);
    }

    void ShowGuiButton (string buttonTitle, PubnubState state)
    {
        if (GUI.Button (new Rect (30, 80, 100, fButtonHeight), buttonTitle)) {
            if (state == PubnubState.AuthKey) {
                pubnub.AuthenticationKey = input;
            } else if (state == PubnubState.ChangeUUID) {
                pubnub.ChangeUUID (input);
            } else if (state == PubnubState.WhereNow) {
                pubnub.WhereNow<string> (input, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.PresenceHeartbeat) {
                int iInterval;

                Int32.TryParse (input, out iInterval);
                if (iInterval < 0) {
                    AddToPubnubResultContainer ("ALERT: Please enter an integer value.");
                } else {
                    pubnub.PresenceHeartbeat = int.Parse (input);
                    AddToPubnubResultContainer (string.Format ("Presence Heartbeat is set to {0}", pubnub.PresenceHeartbeat));
                }
            } else if (state == PubnubState.PresenceInterval) {
                int iInterval;

                Int32.TryParse (input, out iInterval);
                if (iInterval == 0) {
                    AddToPubnubResultContainer ("ALERT: Please enter an integer value.");
                } else {
                    pubnub.PresenceHeartbeatInterval = int.Parse (input);
                    AddToPubnubResultContainer (string.Format ("Presence Heartbeat Interval is set to {0}", pubnub.PresenceHeartbeatInterval));
                }
            } else if (state == PubnubState.AuditPresence) {
                AddToPubnubResultContainer ("Running Audit Presence");
                pubnub.AuditPresenceAccess<string> (channel, input, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.AuditSubscribe) {
                AddToPubnubResultContainer ("Running Audit Subscribe");
                pubnub.AuditAccess<string> (channel, input, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.RevokePresence) {
                AddToPubnubResultContainer ("Running Revoke Presence");
                pubnub.GrantPresenceAccess<string> (channel, input, false, false, DisplayReturnMessage, DisplayErrorMessage);
            } else if (state == PubnubState.RevokeSubscribe) {
                AddToPubnubResultContainer ("Running Revoke Subscribe");
                pubnub.GrantAccess<string> (channel, input, false, false, DisplayReturnMessage, DisplayErrorMessage);
            }

            input = "";
            showAuthWindow = false;
            showPamPopupWindow = false;
        }
    }

    void ShowWindow (PubnubState state)
    {
        string title = "";
        string buttonTitle = "";
        if (state == PubnubState.AuthKey) {
            title = "Enter Auth Key";
            buttonTitle = "Set";
        } else if (state == PubnubState.ChangeUUID) {
            title = "Enter UUID";
            buttonTitle = "Change";
        } else if (state == PubnubState.WhereNow) {
            title = "Enter UUID";
            buttonTitle = "Run";
        } else if (state == PubnubState.PresenceHeartbeat) {
            title = "Enter Presence Heartbeat";
            buttonTitle = "Set";
        } else if (state == PubnubState.PresenceInterval) {
            title = "Enter Presence Interval";
            buttonTitle = "Set";
        } else if (state == PubnubState.AuditPresence) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Run Audit";
        } else if (state == PubnubState.AuditSubscribe) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Run Audit";
        } else if (state == PubnubState.RevokePresence) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Revoke";
        } else if (state == PubnubState.RevokeSubscribe) {
            title = "Enter Auth Key (Optional)";
            buttonTitle = "Revoke";
        }
        GUI.Label (new Rect (10, 30, 100, fHeight), title);
        input = GUI.TextArea (new Rect (110, 30, 150, fHeight), input, 200);
        ShowGuiButton (buttonTitle, state);

        if (GUI.Button (new Rect (150, 80, 100, fButtonHeight), "Cancel")) {
            showAuthWindow = false;
            showPamPopupWindow = false;
        }
        GUI.DragWindow (new Rect (0, 0, 800, 400));
    }

    void DoGrantWindow (int windowID)
    {
        string title = "";
        string buttonTitle = "";
        string toggleTitle1 = "";
        string toggleTitle2 = "";
        string labelTitle = "";
        string labelTitle2 = "";
        int fill = 0;
        if ((state == PubnubState.GrantPresence) || (state == PubnubState.GrantSubscribe)) {
            title = "Enter Auth Key";
            toggleTitle1 = " Can Read ";
            toggleTitle2 = " Can Write ";
            labelTitle = "TTL";
            buttonTitle = "Grant";
            labelTitle2 = "Auth Key (optional)";
        } else if (state == PubnubState.HereNow) {
            title = "Here Now";
            toggleTitle1 = " show uuid ";
            toggle1 = true;
            toggleTitle2 = " include state ";
            labelTitle = "Channel";
            buttonTitle = "Run";
        } else if (state == PubnubState.GlobalHereNow) {
            title = "Global Here Now";
            toggleTitle1 = " show uuid ";
            toggle1 = true;
            toggleTitle2 = " include state ";
            buttonTitle = "Run";
        }

        fLeft = fLeftInit;
        fTop = 20;
        toggle1 = GUI.Toggle (new Rect (fLeft, fTop, 95, fButtonHeight), toggle1, toggleTitle1);

        fLeft = fLeftInit + 100;
        toggle2 = GUI.Toggle (new Rect (fLeft, fTop, 95, fButtonHeight), toggle2, toggleTitle2);

        if ((state == PubnubState.GrantPresence) || (state == PubnubState.GrantSubscribe)) {
            GUI.Label (new Rect (30, 45, 100, fHeight), labelTitle);

            valueToSetSubs = GUI.TextArea (new Rect (110, 45, 100, fHeight), valueToSetSubs, 20);
            GUI.Label (new Rect (30, 90, 100, fHeight), labelTitle2);

            valueToSetAuthKey = GUI.TextArea (new Rect (110, 90, 100, fHeight), valueToSetAuthKey, 20);
            fill = 45;
        } else if (state == PubnubState.HereNow) {
            GUI.Label (new Rect (30, 45, 100, fHeight), labelTitle);

            valueToSet = GUI.TextArea (new Rect (110, 45, 100, fHeight), valueToSet, 20);
        } else if (state == PubnubState.GlobalHereNow) {
            //no text needed
        }

        if (GUI.Button (new Rect (30, 90 + fill, 100, fButtonHeight), buttonTitle)) {
            if (state == PubnubState.GrantSubscribe) {
                int iTtl;
                Int32.TryParse (valueToSetSubs, out iTtl);
                if (iTtl < 0)
                    iTtl = 1440;
                try {
                    pubnub.GrantAccess<string> (channel, valueToSetAuthKey, toggle1, toggle2, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                } catch (Exception ex) {
                    DisplayErrorMessage (ex.ToString ());
                }
            } else if (state == PubnubState.GrantPresence) {
                int iTtl;
                Int32.TryParse (valueToSetSubs, out iTtl);
                if (iTtl < 0)
                    iTtl = 1440;
                try {
                    pubnub.GrantPresenceAccess<string> (channel, valueToSetAuthKey, toggle1, toggle2, iTtl, DisplayReturnMessage, DisplayErrorMessage);
                } catch (Exception ex) {
                    DisplayErrorMessage (ex.ToString ());
                }
            } else if (state == PubnubState.HereNow) {
                allowUserSettingsChange = false;
                if (string.IsNullOrEmpty (valueToSet)) {
                    DisplayErrorMessage ("Please enter channel name.");
                } else {
                    pubnub.HereNow<string> (valueToSet, toggle1, toggle2, DisplayReturnMessage, DisplayErrorMessage);
                }
            } else if (state == PubnubState.GlobalHereNow) {
                pubnub.GlobalHereNow<string> (toggle1, toggle2, DisplayReturnMessage, DisplayErrorMessage);
            }

            valueToSet = "";
            valueToSetSubs = "";
            valueToSetAuthKey = "";
            showGrantWindow = false;
            toggle1 = false;
            toggle2 = false;
        }

        if (GUI.Button (new Rect (150, 90 + fill, 100, fButtonHeight), "Cancel")) {
            showGrantWindow = false;
            toggle1 = false;
            toggle2 = false;
        }
        GUI.DragWindow (new Rect (0, 0, 800, 400));
    }

    void DoPamActionWindow (int windowID)
    {
        fLeft = fLeftInit - 10;
        fTop = fTopInit + 10;
        float fButtonWidth = 140;

        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Grant Subscribe")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GrantSubscribe);
            state = PubnubState.GrantSubscribe;
            showPamPopupWindow = false;
            showGrantWindow = true;
        }
        fTop = fTopInit + 1 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Audit Subscribe")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.AuditSubscribe);
            showPamPopupWindow = false;
            showAuthWindow = true;
            state = PubnubState.AuditSubscribe;
        }

        fTop = fTopInit + 2 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke Subscribe")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.RevokeSubscribe);
            showPamPopupWindow = false;
            showAuthWindow = true;
            state = PubnubState.RevokeSubscribe;
        }

        fTop = fTopInit + 3 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Grant Presence")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GrantPresence);
            state = PubnubState.GrantPresence;
            showPamPopupWindow = false;
            showGrantWindow = true;
        }

        fTop = fTopInit + 4 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Audit Presence")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.AuditPresence);
            showPamPopupWindow = false;
            showAuthWindow = true;
            state = PubnubState.AuditPresence;
        }

        fTop = fTopInit + 5 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke Presence")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.RevokePresence);
            showPamPopupWindow = false;
            showAuthWindow = true;
            state = PubnubState.RevokePresence;
        }

        fTop = fTopInit + 6 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Auth Key")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.AuthKey);
            showAuthWindow = true;
            showPamPopupWindow = false;
            state = PubnubState.AuthKey;
        }
        fTop = fTopInit + 7 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Add/Edit User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.SetUserStateKeyValue);
            showActionsPopupWindow = false;
            state = PubnubState.SetUserStateKeyValue;
            showTextWindow = true;
        }
        /*fTop = fTopInit + 8 * fRowHeight + 10;
				if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "View Local State")) {
						InstantiatePubnub ();
						AsyncOrNonAsyncCall (PubnubState.ViewLocalUserState);
						showActionsPopupWindow = false;
				}*/
        fTop = fTopInit + 8 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Del User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.DelUserState);
            showActionsPopupWindow = false;
            state = PubnubState.DelUserState;
            showTextWindow = true;
        }
        fTop = fTopInit + 9 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Set User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.SetUserStateJson);
            showActionsPopupWindow = false;
            state = PubnubState.SetUserStateJson;
            showTextWindow = true;
        }
        fTop = fTopInit + 10 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Get User State")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GetUserState);
            showActionsPopupWindow = false;
            state = PubnubState.GetUserState;
            showTextWindow = true;
        }
        fTop = fTopInit + 11 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence Heartbeat")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.PresenceHeartbeat);
            state = PubnubState.PresenceHeartbeat;
            showAuthWindow = true;
            showActionsPopupWindow = false;
        }
        fTop = fTopInit + 12 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence Interval")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.PresenceInterval);
            state = PubnubState.PresenceInterval;
            showAuthWindow = true;
            showActionsPopupWindow = false;
        }
    }

    void DoActionWindow (int windowID)
    {
        fLeft = fLeftInit - 10;
        fTop = fTopInit + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Disconnect/Retry")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.DisconnectRetry);
            showActionsPopupWindow = false;
        }
        fTop = fTopInit + 1 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.Presence);
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 2 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Subscribe")) {
            InstantiatePubnub ();
            DoAction (PubnubState.Subscribe);

            //pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);

            showActionsPopupWindow = false;
            //UnityEngine.Object.Destroy (gobj);
        }

        fTop = fTopInit + 3 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Detailed History")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.DetailedHistory);
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 4 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Publish")) {
            InstantiatePubnub ();
            allowUserSettingsChange = false;
            showActionsPopupWindow = false;
            showPublishPopupWindow = true;
        }

        fTop = fTopInit + 5 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Unsubscribe")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.Unsubscribe);
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 6 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Presence-Unsub")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.PresenceUnsubscribe);
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 7 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Here Now")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.HereNow);
            state = PubnubState.HereNow;
            showGrantWindow = true;
            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 8 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Time")) {
            InstantiatePubnub ();
            //AsyncOrNonAsyncCall (PubnubState.Time);
            //GameObject gobj = new GameObject ();
            //pubnub = gobj.AddComponent<Pubnub> ();

            //pubnub.Time<string> (DisplayReturnMessage, DisplayErrorMessage);
            DoAction (PubnubState.Time);

            showActionsPopupWindow = false;
        }

        fTop = fTopInit + 9 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Where Now")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.WhereNow);
            showActionsPopupWindow = false;
            state = PubnubState.WhereNow;
            showAuthWindow = true;
        }
        fTop = fTopInit + 10 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Global Here Now")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.GlobalHereNow);
            showActionsPopupWindow = false;
            state = PubnubState.GlobalHereNow;
            showGrantWindow = true;
        }

        fTop = fTopInit + 11 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Change UUID")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.ChangeUUID);
            showActionsPopupWindow = false;
            state = PubnubState.ChangeUUID;
            showAuthWindow = true;
        }
        #if(UNITY_IOS || UNITY_ANDROID)
				GUI.enabled = false;
        #endif

        fTop = fTopInit + 12 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Disable Network")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.DisableNetwork);
            showActionsPopupWindow = false;
        }
        fTop = fTopInit + 13 * fRowHeight + 10;
        if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Enable Network")) {
            InstantiatePubnub ();
            AsyncOrNonAsyncCall (PubnubState.EnableNetwork);
            showActionsPopupWindow = false;
        }

        #if(UNITY_IOS || UNITY_ANDROID)
				GUI.enabled = true;
        #endif

    }

    /// <summary>
    /// Determines whether to send an asynchronous or synchronous call on the button click
    /// Async calls on button click when used in iOS results in random crashes thus sync calls 
    /// are preferred in case iOS
    /// </summary>
    /// <param name="pubnubState">Pubnub state.</param>
    void AsyncOrNonAsyncCall (PubnubState pubnubState)
    {
        DoAction(pubnubState);
    }

    public IEnumerator RunCoroutine (){
        AddToPubnubResultContainer ("Running Subscribe");
        allowUserSettingsChange = false;
        pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
        yield return null;
    }

    void Awake ()
    {
        Application.RegisterLogCallback (new Application.LogCallback (CaptureLogs));
    }

    void CaptureLogs (string condition, string stacktrace, LogType logType)
    {
        StringBuilder sb = new StringBuilder ();
        sb.AppendLine ("Type");
        sb.AppendLine (logType.ToString ());
        sb.AppendLine ("Condition");
        sb.AppendLine (condition);
        sb.AppendLine ("stacktrace");
        sb.AppendLine (stacktrace);
        //UnityEngine.Debug.Log("Type: ", );
    }

    private void DoAction (object pubnubState)
    {
        try {
            if ((PubnubState)pubnubState == PubnubState.Presence) {
                AddToPubnubResultContainer ("Running Presence");
                allowUserSettingsChange = false;
                pubnub.Presence<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.Subscribe) {
                AddToPubnubResultContainer ("Running Subscribe");
                allowUserSettingsChange = false;
                pubnub.Subscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.DetailedHistory) {
                AddToPubnubResultContainer ("Running Detailed History");
                allowUserSettingsChange = false;
                pubnub.DetailedHistory<string> (channel, 100, DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.HereNow) {
                AddToPubnubResultContainer ("Running Here Now");
            } else if ((PubnubState)pubnubState == PubnubState.Time) {
                AddToPubnubResultContainer ("Running Time");
                allowUserSettingsChange = false;
                pubnub.Time<string> (DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.Unsubscribe) {
                AddToPubnubResultContainer ("Running Unsubscribe");
                allowUserSettingsChange = false;
                pubnub.Unsubscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.PresenceUnsubscribe) {
                AddToPubnubResultContainer ("Running Presence Unsubscribe");
                allowUserSettingsChange = false;
                pubnub.PresenceUnsubscribe<string> (channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
            /*} else if ((PubnubState)pubnubState == PubnubState.EnableNetwork) {
                AddToPubnubResultContainer ("Running Enable Network");
                pubnub.DisableSimulateNetworkFailForTestingOnly ();
            } else if ((PubnubState)pubnubState == PubnubState.DisableNetwork) {
                AddToPubnubResultContainer ("Running Disable Network");
                pubnub.EnableSimulateNetworkFailForTestingOnly ();*/
            } else if ((PubnubState)pubnubState == PubnubState.GrantSubscribe) {
                AddToPubnubResultContainer ("Running Grant Subscribe");
            } else if ((PubnubState)pubnubState == PubnubState.AuditSubscribe) {
                //AddToPubnubResultContainer ("Running Audit Subscribe");
                //pubnub.AuditAccess<string> (channel, DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.RevokeSubscribe) {
                //AddToPubnubResultContainer ("Running Revoke Subscribe");
                //pubnub.GrantAccess<string> (channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.GrantPresence) {
                AddToPubnubResultContainer ("Running Grant Presence");
            } else if ((PubnubState)pubnubState == PubnubState.AuditPresence) {
                //AddToPubnubResultContainer ("Running Audit Presence");
                //pubnub.AuditPresenceAccess<string> (channel, DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.RevokePresence) {
                //AddToPubnubResultContainer ("Running Revoke Presence");
                //pubnub.GrantPresenceAccess<string> (channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.AuthKey) {
                AddToPubnubResultContainer ("Running Auth Key");
            } else if ((PubnubState)pubnubState == PubnubState.ChangeUUID) {
                AddToPubnubResultContainer ("Changing UUID");
            } else if ((PubnubState)pubnubState == PubnubState.WhereNow) {
                AddToPubnubResultContainer ("Running Where Now");
            } else if ((PubnubState)pubnubState == PubnubState.GlobalHereNow) {
                AddToPubnubResultContainer ("Running Global Here Now");
            } else if ((PubnubState)pubnubState == PubnubState.PresenceHeartbeat) {
                AddToPubnubResultContainer ("Running Presence Heartbeat");
            } else if ((PubnubState)pubnubState == PubnubState.PresenceInterval) {
                AddToPubnubResultContainer ("Running Presence Interval");
            } else if ((PubnubState)pubnubState == PubnubState.ViewUserState) {
                string[] channels = channel.Split (',');
                foreach (string channelToCall in channels) {
                    //string currentUserStateView = pubnub.GetLocalUserState (channelToCall);
                    /*if (!string.IsNullOrEmpty (currentUserStateView)) {
												AddToPubnubResultContainer (string.Format("User state for channel {0}:{1}", channelToCall, currentUserStateView));
										} else {
												AddToPubnubResultContainer (string.Format("No User State Exists for channel {0}", channelToCall));
										}*/
                }
            } else if ((PubnubState)pubnubState == PubnubState.DisconnectRetry) {
                AddToPubnubResultContainer ("Running Disconnect Retry");
                pubnub.TerminateCurrentSubscriberRequest ();
                #if(UNITY_IOS)
                requestInProcess = false;
                #endif
            } else if ((PubnubState)pubnubState == PubnubState.SetUserStateJson) {
                AddToPubnubResultContainer ("Setting User State");
            } else if ((PubnubState)pubnubState == PubnubState.GetUserState) {
                AddToPubnubResultContainer ("Getting User State");
            }
        } catch (Exception ex) {
            UnityEngine.Debug.Log ("DoAction exception:" + ex.ToString ());
        }
    }

    void InstantiatePubnub ()
    {
        if (pubnub == null) {
            //GameObject gobj = new GameObject ();
            //pubnub = gobj.AddComponent<Pubnub> ();

            pubnub = new Pubnub (publishKey, subscribeKey, secretKey, cipherKey, ssl);
            pubnub.SessionUUID = uuid;
            pubnub.SubscribeTimeout = int.Parse (subscribeTimeoutInSeconds);
            pubnub.NonSubscribeTimeout = int.Parse (operationTimeoutInSeconds);
            pubnub.NetworkCheckMaxRetries = int.Parse (networkMaxRetries);
            pubnub.NetworkCheckRetryInterval = int.Parse (networkRetryIntervalInSeconds);
            pubnub.LocalClientHeartbeatInterval = int.Parse (heartbeatIntervalInSeconds);
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
        }
    }

    void DoPublishWindow (int windowID)
    {
        GUI.Label (new Rect (10, 25, 100, 25), "Enter Message");

        publishedMessage = GUI.TextArea (new Rect (110, 25, 150, 60), publishedMessage, 2000);
		storeInHistory = GUI.Toggle (new Rect (10, 100, 150, 25), storeInHistory, "Store in History");

        //publishedMessage = "Text with 😜 emoji 🎉.";
        string stringMessage = publishedMessage;
        if (GUI.Button (new Rect (30, 130, 100, 30), "Publish")) {
            //stringMessage = "Text with 😜 emoji 🎉.";
            pubnub.Publish<string> (channel, stringMessage, storeInHistory, DisplayReturnMessage, DisplayErrorMessage);
            publishedMessage = "";
            showPublishPopupWindow = false;
        }

        if (GUI.Button (new Rect (150, 130, 100, 30), "Cancel")) {
            showPublishPopupWindow = false;
        }
        GUI.DragWindow (new Rect (0, 0, 800, 400));
    }

    void Start ()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
    }

    private static bool ValidateServerCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0) {
            if (chain != null && chain.ChainStatus != null) {
                X509Certificate2 cert2 = new X509Certificate2 (certificate);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                //chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                //chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(1000);
                //chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                //chain.ChainPolicy.VerificationTime = DateTime.Now;
                chain.Build (cert2);

                foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus) {
                    if ((certificate.Subject == certificate.Issuer) &&
                        (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot)) {
                        // Self-signed certificates with an untrusted root are valid. 
                        continue;
                    } else {
                        if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError) {
                            // If there are any other errors in the certificate chain, the certificate is invalid,
                            // so the method returns false.
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Do not allow this client to communicate with unauthenticated servers. 
        return false;
    }

    void Update ()
    {
        //if (pubnub == null)
            //return;
        //this.transform.Rotate(new Vector3(0,1,0));
        try {
            //UnityEngine.Debug.Log(DateTime.Now.ToLongTimeString() + " Update called " + pubnubApiResult.Length.ToString());            
            //string recordTest;
            System.Text.StringBuilder sbResult = new System.Text.StringBuilder ();

            int existingLen = pubnubApiResult.Length;
            int newRecordLen = 0;
            sbResult.Append (pubnubApiResult);

            //if (recordQueue.TryPeek (out recordTest)) {
            //recordTest = recordQueue.Peek ();
			if(recordQueue.Count > 0){
            //if (!recordTest.Equals(null)) {
                string currentRecord = "";
                //while (recordQueue.TryDequeue (out currentRecord)) {
				lock(recordQueue){
					do
				    {
						currentRecord = recordQueue.Dequeue();
                        Debug.Log( "currentRecord: " + currentRecord );
						sbResult.AppendLine (currentRecord);
					} while (recordQueue.Count != 0);
				}

                pubnubApiResult = sbResult.ToString ();

                newRecordLen = pubnubApiResult.Length - existingLen;
                int windowLength = 600;

                if (pubnubApiResult.Length > windowLength) {
                    bool trimmed = false;
                    if (pubnubApiResult.Length > windowLength) {
                        trimmed = true;
                        int lengthToTrim = (((pubnubApiResult.Length - windowLength) < pubnubApiResult.Length - newRecordLen) ? pubnubApiResult.Length - windowLength : pubnubApiResult.Length - newRecordLen);
                        pubnubApiResult = pubnubApiResult.Substring (lengthToTrim);
                    }
                    if (trimmed) {
                        string prefix = "Output trimmed...\n";

                        pubnubApiResult = prefix + pubnubApiResult;
                    }
                }
            } 
        } catch (Exception ex) {
            Debug.Log ("Update exception:" + ex.ToString ());
        }
    }

    void OnApplicationQuit ()
    {
        ResetPubnubInstance ();
    }

    void ResetPubnubInstance ()
    {
        if (pubnub != null) {
            pubnub.EndPendingRequests ();
            //System.Threading.Thread.Sleep (1000);
            pubnub = null;
        }
    }

    void DisplayReturnMessage (string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format ("REGULAR CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("REGULAR CALLBACK: {0}", result));
    }

    void DisplayConnectStatusMessage (string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format ("CONNECT CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("CONNECT CALLBACK: {0}", result));
    }

    void DisplayDisconnectStatusMessage (string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format ("DISCONNECT CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("DISCONNECT CALLBACK: {0}", result));
    }

    void DisplayErrorMessage (string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format ("ERROR CALLBACK LOG: {0}", result));
        AddToPubnubResultContainer (string.Format ("ERROR CALLBACK: {0}", result));
    }

    /// <summary>
    /// Callback method for error messages
    /// </summary>
    /// <param name="result"></param>
    void DisplayErrorMessage (PubnubClientError result)
    {
        UnityEngine.Debug.Log (result.Description);

        switch (result.StatusCode) {
        case 103:
            //Warning: Verify origin host name and internet connectivity
            break;
        case 104:
            //Critical: Verify your cipher key
            break;
        case 106:
            //Warning: Check network/internet connection
            break;
        case 108:
            //Warning: Check network/internet connection
            break;
        case 109:
            //Warning: No network/internet connection. Please check network/internet connection
            break;
        case 110:
            //Informational: Network/internet connection is back. Active subscriber/presence channels will be restored.
            break;
        case 111:
            //Informational: Duplicate channel subscription is not allowed. Internally Pubnub API removes the duplicates before processing.
            break;
        case 112:
            //Informational: Channel Already Subscribed/Presence Subscribed. Duplicate channel subscription not allowed
            break;
        case 113:
            //Informational: Channel Already Presence-Subscribed. Duplicate channel presence-subscription not allowed
            break;
        case 114:
            //Warning: Please verify your cipher key
            break;
        case 115:
            //Warning: Protocol Error. Please contact PubNub with error details.
            break;
        case 116:
            //Warning: ServerProtocolViolation. Please contact PubNub with error details.
            break;
        case 117:
            //Informational: Input contains invalid channel name
            break;
        case 118:
            //Informational: Channel not subscribed yet
            break;
        case 119:
            //Informational: Channel not subscribed for presence yet
            break;
        case 120:
            //Informational: Incomplete unsubscribe. Try again for unsubscribe.
            break;
        case 121:
            //Informational: Incomplete presence-unsubscribe. Try again for presence-unsubscribe.
            break;
        case 122:
            //Informational: Network/Internet connection not available. C# client retrying again to verify connection. No action is needed from your side.
            break;
        case 123:
            //Informational: During non-availability of network/internet, max retries for connection were attempted. So unsubscribed the channel.
            break;
        case 124:
            //Informational: During non-availability of network/internet, max retries for connection were attempted. So presence-unsubscribed the channel.
            break;
        case 125:
            //Informational: Publish operation timeout occured.
            break;
        case 126:
            //Informational: HereNow operation timeout occured
            break;
        case 127:
            //Informational: Detailed History operation timeout occured
            break;
        case 128:
            //Informational: Time operation timeout occured
            break;
        case 4000:
            //Warning: Message too large. Your message was not sent. Try to send this again smaller sized
            break;
        case 4001:
            //Warning: Bad Request. Please check the entered inputs or web request URL
            break;
        case 4002:
            //Warning: Invalid Key. Please verify the publish key
            break;
        case 4010:
            //Critical: Please provide correct subscribe key. This corresponds to a 401 on the server due to a bad sub key
            break;
        case 4020:
            // PAM is not enabled. Please contact PubNub support
            break;
        case 4030:
            //Warning: Not authorized. Check the permimissions on the channel. Also verify authentication key, to check access.
            break;
        case 4031:
            //Warning: Incorrect public key or secret key.
            break;
        case 4140:
            //Warning: Length of the URL is too long. Reduce the length by reducing subscription/presence channels or grant/revoke/audit channels/auth key list
            break;
        case 5000:
            //Critical: Internal Server Error. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
            break;
        case 5020:
            //Critical: Bad Gateway. Unexpected error occured at PubNub Server. Please try again. If same problem persists, please contact PubNub support
            break;
        case 5030:
            //"Service Unavailable. Please try again. If the issue continues, please contact PubNub support"
            break;
        case 5040:
            //Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
            break;
        case 0:
            //Undocumented error. Please contact PubNub support with full error object details for further investigation
            break;
        default:
            break;
        }

        if (showErrorMessageSegments) {
            DisplayErrorMessageSegments (result);
        }
    }

    void DisplayErrorMessageSegments (PubnubClientError pubnubError)
    {
        UnityEngine.Debug.Log (string.Format ("<STATUS CODE>: {0}", pubnubError.StatusCode)); // Unique ID of Error

        UnityEngine.Debug.Log (string.Format ("<MESSAGE>: {0}", pubnubError.Message)); // Message received from server/clent or from .NET exception
        AddToPubnubResultContainer (string.Format ("Error: {0}", pubnubError.Message));
        UnityEngine.Debug.Log (string.Format ("<SEVERITY>: {0}", pubnubError.Severity)); // Info can be ignored, Warning and Error should be handled

        if (pubnubError.DetailedDotNetException != null) {
            //Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
            UnityEngine.Debug.Log (string.Format ("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString ())); // Full Details of .NET exception
        }
        UnityEngine.Debug.Log (string.Format ("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource)); // Did this originate from Server or Client-side logic
        if (pubnubError.PubnubWebRequest != null) {
            //Captured Web Request details
            UnityEngine.Debug.Log (string.Format ("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString ())); 
            UnityEngine.Debug.Log (string.Format ("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString ())); 
        }
        if (pubnubError.PubnubWebResponse != null) {
            //Captured Web Response details
            UnityEngine.Debug.Log (string.Format ("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString ()));
        }
        UnityEngine.Debug.Log (string.Format ("<DESCRIPTION>: {0}", pubnubError.Description)); // Useful for logging and troubleshooting and support
        AddToPubnubResultContainer (string.Format ("DESCRIPTION: {0}", pubnubError.Description));
        UnityEngine.Debug.Log (string.Format ("<CHANNEL>: {0}", pubnubError.Channel)); //Channel name(s) at the time of error
        UnityEngine.Debug.Log (string.Format ("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT)); //GMT time of error

    }

    void AddToPubnubResultContainer (string result)
    {
        UnityEngine.Debug.Log ("result:" + result);
        //recordQueue.Enqueue (result);
		lock (recordQueue) {
			UnityEngine.Debug.Log (string.Format ("Enqueuing {0}", result)); 
			recordQueue.Enqueue (result);
		}
    }
}


