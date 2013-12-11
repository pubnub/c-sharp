using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using PubNubMessaging.Core;
using System.Collections.Concurrent;
using System;
using System.Reflection;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

public class PubnubExample : MonoBehaviour {

    enum PubnubState
    {
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
		GrantAccess,
		AuditAccess,
		RevokeAccess,
		PresenceGrantAccess,
		PresenceAuditAccess,
		PresenceRevokeAccess
    }

    bool ssl = false;
    bool resumeOnReconnect = true;

    string origin = "pubsub.pubnub.com";
	string publishKey = "demo";
	string subscribeKey = "demo";
	string cipherKey = "";
    string secretKey = "";
	string authenticationKey = "";
    string uuid = Guid.NewGuid().ToString();

    string subscribeTimeoutInSeconds = "310";
    string operationTimeoutInSeconds = "15";
    string networkMaxRetries = "50";
    string networkRetryIntervalInSeconds = "10";
    string heartbeatIntervalInSeconds = "10";

    string channel = "hello_my_channel";
    string publishedMessage = "";

    static Pubnub pubnub;

    private static ConcurrentQueue<string> recordQueue = new ConcurrentQueue<string>();

    //GameObject PubnubApiResult;
    Vector2 scrollPosition = Vector2.zero;
    string pubnubApiResult = "";

	bool requestInProcess = false;

    bool showPublishPopupWindow = false;

    Rect publishWindowRect = new Rect(270, 250, 300, 150);

    bool allowUserSettingsChange = true;

    public void OnGUI()
    {
        GUI.enabled = !allowUserSettingsChange;
        GUIStyle customStyle = new GUIStyle(GUI.skin.button);
        customStyle.fontSize = 10;
        //customStyle.normal.background
        customStyle.hover.textColor = Color.yellow;
        customStyle.fontStyle = FontStyle.Italic;
        customStyle.fixedHeight = 20;
        if (GUI.Button(new Rect(200,10,90,25), "Reset Settings",customStyle))
        {
            allowUserSettingsChange = true;
            ResetPubnubInstance();
            pubnubApiResult = "";
        }

        GUI.enabled = allowUserSettingsChange;

        ssl = GUI.Toggle(new Rect(10,10,200,25), ssl," Enable SSL ");

        resumeOnReconnect = GUI.Toggle(new Rect(10,25,200,25), resumeOnReconnect," Resume On Reconnect ");

        GUI.Label(new Rect(10,50,70,25), "Origin");
        origin = GUI.TextField(new Rect(80,50,200,25), origin);

        GUI.Label(new Rect(10,80,70,25), "Pub Key");
        publishKey = GUI.TextField(new Rect(80,80,150,25), publishKey);

        GUI.Label(new Rect(10,110,70,25), "Sub Key");
        subscribeKey = GUI.TextField(new Rect(80,110,150,25), subscribeKey);
		
		GUI.Label(new Rect(10,140,70,25), "Cipher Key");
        cipherKey = GUI.TextField(new Rect(80,140,150,25), cipherKey);

        GUI.Label(new Rect(10,170,70,25), "Secret Key");
        secretKey = GUI.TextField(new Rect(80,170,150,25), secretKey);

        GUI.Label(new Rect(10,200,70,25), "Auth Key");
        authenticationKey = GUI.TextField(new Rect(80,200,150,25), authenticationKey);

        GUI.Label(new Rect(10,230,70,25), "UUID");
        uuid = GUI.TextField(new Rect(80,230,150,25),uuid);
		
        GUI.Label(new Rect(10,260,200,25), "Subscriber Timeout (in sec)");
        subscribeTimeoutInSeconds = GUI.TextField(new Rect(200,260,50,25),subscribeTimeoutInSeconds,6);
        subscribeTimeoutInSeconds = Regex.Replace(subscribeTimeoutInSeconds, "[^0-9]", "");

        GUI.Label(new Rect(10,290,200,25), "Non Subscribe Timeout (in sec)");
        operationTimeoutInSeconds = GUI.TextField(new Rect(200,290,50,25),operationTimeoutInSeconds,6);
        operationTimeoutInSeconds = Regex.Replace(operationTimeoutInSeconds, "[^0-9]", "");

        GUI.Label(new Rect(10,320,200,25), "Number of MAX retries");
        networkMaxRetries = GUI.TextField(new Rect(200,320,50,25),networkMaxRetries,6);
        networkMaxRetries = Regex.Replace(networkMaxRetries, "[^0-9]", "");

        GUI.Label(new Rect(10,350,200,25), "Retry Interval (in sec)");
        networkRetryIntervalInSeconds = GUI.TextField(new Rect(200,350,50,25),networkRetryIntervalInSeconds,6);
        networkRetryIntervalInSeconds = Regex.Replace(networkRetryIntervalInSeconds, "[^0-9]", "");

        GUI.Label(new Rect(10,380,200,25), "Heartbeat Interval (in sec)");
        heartbeatIntervalInSeconds = GUI.TextField(new Rect(200,380,50,25),heartbeatIntervalInSeconds,6);
        heartbeatIntervalInSeconds = Regex.Replace(heartbeatIntervalInSeconds, "[^0-9]", "");

        GUI.enabled = true;

        GUI.Label(new Rect(10,410,100,25), "Channel Name");
        channel = GUI.TextField(new Rect(100,410,150,25),channel,100);

        if (GUI.Button(new Rect(730,10,120,25), "Presence"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.Presence);
        }
        if (GUI.Button(new Rect(860,10,120,25), "Subscribe"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.Subscribe);
        }

        if (GUI.Button(new Rect(730,40,120,25), "Detailed History"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.DetailedHistory);
        }
        if (GUI.Button(new Rect(860,40,120,25), "Publish"))
        {
            InstantiatePubnub();
            allowUserSettingsChange = false;
            showPublishPopupWindow = true;
        }
        if (showPublishPopupWindow)
        {
            GUI.backgroundColor = Color.black;
            publishWindowRect = GUI.ModalWindow(0, publishWindowRect, DoPublishWindow, "Message Publish");
            GUI.backgroundColor = new Color(1,1,1,1);
        }
		
        if (GUI.Button(new Rect(730,70,120,25), "Unsubscribe"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.Unsubscribe);
        }

        if (GUI.Button(new Rect(860,70,120,25), "Here Now"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.HereNow);
        }

        if (GUI.Button(new Rect(730,100,120,25), "Presence-Unsub"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.PresenceUnsubscribe);
        }
        if (GUI.Button(new Rect(860,100,120,25), "Time"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.Time);
        }

        if (GUI.Button(new Rect(730,130,120,25), "Disable Network"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.DisableNetwork);
        }
        if (GUI.Button(new Rect(860,130,120,25), "Enable Network"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.EnableNetwork);
        }

        if (GUI.Button(new Rect(730,160,120,25), "Disconnect/Retry"))
        {
            InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.DisconnectRetry);
        }
        if (GUI.Button(new Rect(860,160,120,25), "Grant Access"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.GrantAccess);
        }

        if (GUI.Button(new Rect(730,190,120,25), "Audit Access"))
        {
            InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.AuditAccess);
        }
        if (GUI.Button(new Rect(860,190,120,25), "Revoke Access"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.RevokeAccess);
        }

        if (GUI.Button(new Rect(730,220,120,25), "Presence Grant"))
        {
            InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.PresenceGrantAccess);
        }
        if (GUI.Button(new Rect(860,220,120,25), "Presence Audit"))
        {
            InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.PresenceAuditAccess);
        }

        if (GUI.Button(new Rect(730,250,120,25), "Presence Revoke"))
        {
            InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.PresenceRevokeAccess);
        }
		
//		if (GUI.Button(new Rect(10,440,120,25), "Presence"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.Presence);
//        }
//        if (GUI.Button(new Rect(140,440,120,25), "Subscribe"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.Subscribe);
//        }

//        if (GUI.Button(new Rect(10,470,120,25), "Detailed History"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.DetailedHistory);
//        }
//
//        if (GUI.Button(new Rect(140,470,120,25), "Publish"))
//        {
//            InstantiatePubnub();
//            allowUserSettingsChange = false;
//            showPublishPopupWindow = true;
//        }
//        if (showPublishPopupWindow)
//        {
//            GUI.backgroundColor = Color.black;
//            publishWindowRect = GUI.ModalWindow(0, publishWindowRect, DoPublishWindow, "Message Publish");
//            GUI.backgroundColor = new Color(1,1,1,1);
//        }

//        if (GUI.Button(new Rect(10,500,120,25), "Unsubscribe"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.Unsubscribe);
//        }
//
//        if (GUI.Button(new Rect(140,500,120,25), "Here Now"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.HereNow);
//        }
//
//        if (GUI.Button(new Rect(10,530,120,25), "Presence-Unsub"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.PresenceUnsubscribe);
//        }
//        if (GUI.Button(new Rect(140,530,120,25), "Time"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.Time);
//        }
//
//        if (GUI.Button(new Rect(10,560,120,25), "Disable Network"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.DisableNetwork);
//        }
//        if (GUI.Button(new Rect(140,560,120,25), "Enable Network"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.EnableNetwork);
//        }
//
//        if (GUI.Button(new Rect(10,590,120,25), "Disconnect/Retry"))
//        {
//            InstantiatePubnub();
//			AsyncOrNonAsyncCall (PubnubState.DisconnectRetry);
//        }
//        if (GUI.Button(new Rect(140,590,120,25), "Grant Access"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.GrantAccess);
//        }

//        if (GUI.Button(new Rect(10,620,120,25), "Audit Access"))
//        {
//            InstantiatePubnub();
//			AsyncOrNonAsyncCall (PubnubState.AuditAccess);
//        }
//        if (GUI.Button(new Rect(140,620,120,25), "Revoke Access"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.RevokeAccess);
//        }
//
//        if (GUI.Button(new Rect(300,630,120,25), "Presence Grant"))
//        {
//            InstantiatePubnub();
//			AsyncOrNonAsyncCall (PubnubState.PresenceGrantAccess);
//        }
//        if (GUI.Button(new Rect(430,630,120,25), "Presence Audit"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.PresenceAuditAccess);
//        }
//
//        if (GUI.Button(new Rect(300,660,120,25), "Presence Revoke"))
//        {
//            InstantiatePubnub();
//			AsyncOrNonAsyncCall (PubnubState.PresenceRevokeAccess);
//        }
//        if (GUI.Button(new Rect(730,10,120,25), "?????"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.Time);
//        }
//        if (GUI.Button(new Rect(860,10,120,25), "?????"))
//        {
//            InstantiatePubnub();
//            AsyncOrNonAsyncCall (PubnubState.Time);
//        }
		
        if (showPublishPopupWindow)
        {
            scrollPosition = GUI.BeginScrollView(new Rect(300,10,415,200), scrollPosition, new Rect(0,0,250,500),false, true);
            pubnubApiResult = GUI.TextArea(new Rect(0,0,400,500), pubnubApiResult);
        }
        else
        {
            scrollPosition = GUI.BeginScrollView(new Rect(300,10,415,600), scrollPosition, new Rect(0,0,375,1200),false, true);
            pubnubApiResult = GUI.TextArea(new Rect(0,0,400,1200), pubnubApiResult);            
        }
        GUI.EndScrollView();

    }

    /// <summary>
    /// Determines whether to send an asynchronous or synchronous call on the button click
    /// Async calls on button click when used in iOS results in random crashes thus sync calls 
    /// are preferred in case iOS
    /// </summary>
    /// <param name="pubnubState">Pubnub state.</param>
    void AsyncOrNonAsyncCall (PubnubState pubnubState)
    {
#if(UNITY_IOS)
		if(pubnubState == PubnubState.DisconnectRetry)
		{
			if(!requestInProcess)
			{
				requestInProcess = true;
				ThreadPool.QueueUserWorkItem(new WaitCallback(DoAction), pubnubState);
			}
		}
		else
		{
        	DoAction(pubnubState);
		}
#else
        ThreadPool.QueueUserWorkItem(new WaitCallback(DoAction), pubnubState);
#endif
    }

    private void DoAction (object pubnubState)
    {
		//TODO: Need to refactor. Write switch..case instead of if..else
        try
        {
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
                pubnub.DetailedHistory<string>(channel, 100, DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.HereNow) {
                AddToPubnubResultContainer ("Running Here Now");
                allowUserSettingsChange = false;
                pubnub.HereNow<string>(channel, DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.Time) {
                AddToPubnubResultContainer ("Running Time");
                allowUserSettingsChange = false;
                pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.Unsubscribe) {
                AddToPubnubResultContainer ("Running Unsubscribe");
                allowUserSettingsChange = false;
                pubnub.Unsubscribe<string>(channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.PresenceUnsubscribe) {
                AddToPubnubResultContainer ("Running Presence Subscribe");
                allowUserSettingsChange = false;
                pubnub.PresenceUnsubscribe<string>(channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
            } else if ((PubnubState)pubnubState == PubnubState.EnableNetwork) {
                AddToPubnubResultContainer ("Running Enable Network");
                pubnub.DisableSimulateNetworkFailForTestingOnly();
            } else if ((PubnubState)pubnubState == PubnubState.DisableNetwork)     {
                AddToPubnubResultContainer ("Running Disable Network");
                pubnub.EnableSimulateNetworkFailForTestingOnly();
            } else if ((PubnubState)pubnubState == PubnubState.DisconnectRetry) {
                AddToPubnubResultContainer ("Running Disconnect Retry");
                pubnub.TerminateCurrentSubscriberRequest();
				requestInProcess = false;
            } else if ((PubnubState)pubnubState == PubnubState.GrantAccess) {
                AddToPubnubResultContainer ("Running Grant Access");
				allowUserSettingsChange = false;
                pubnub.GrantAccess<string>(channel, true, true, DisplayReturnMessage, DisplayErrorMessage);
				requestInProcess = false;
            } else if ((PubnubState)pubnubState == PubnubState.AuditAccess) {
                AddToPubnubResultContainer ("Running Audit Access");
				//allowUserSettingsChange = false;
                pubnub.AuditAccess<string>(channel, DisplayReturnMessage, DisplayErrorMessage);
				requestInProcess = false;
            } else if ((PubnubState)pubnubState == PubnubState.RevokeAccess) {
                AddToPubnubResultContainer ("Running Revoke Access");
				allowUserSettingsChange = false;
                pubnub.GrantAccess<string>(channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
				requestInProcess = false;
            } else if ((PubnubState)pubnubState == PubnubState.PresenceGrantAccess) {
                AddToPubnubResultContainer ("Running Grant Presence Access");
				allowUserSettingsChange = false;
                pubnub.GrantPresenceAccess<string>(channel, true, true, DisplayReturnMessage, DisplayErrorMessage);
				requestInProcess = false;
            } else if ((PubnubState)pubnubState == PubnubState.PresenceAuditAccess) {
                AddToPubnubResultContainer ("Running Audit Presence Access");
				//allowUserSettingsChange = false;
                pubnub.AuditPresenceAccess<string>(channel, DisplayReturnMessage, DisplayErrorMessage);
				requestInProcess = false;
            } else if ((PubnubState)pubnubState == PubnubState.PresenceRevokeAccess) {
                AddToPubnubResultContainer ("Running Revoke Access");
				allowUserSettingsChange = false;
                pubnub.GrantPresenceAccess<string>(channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
				requestInProcess = false;
			}
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log (ex.ToString());
        }
    }

    void InstantiatePubnub()
    {
        if (pubnub == null)
        {
            pubnub = new Pubnub(publishKey,subscribeKey,secretKey,cipherKey,ssl);
			pubnub.Origin = origin;
			pubnub.AuthenticationKey = authenticationKey;
            pubnub.SessionUUID = uuid;
            pubnub.SubscribeTimeout = int.Parse(subscribeTimeoutInSeconds);
            pubnub.NonSubscribeTimeout = int.Parse(operationTimeoutInSeconds);
            pubnub.NetworkCheckMaxRetries = int.Parse(networkMaxRetries);
            pubnub.NetworkCheckRetryInterval = int.Parse(networkRetryIntervalInSeconds);
            pubnub.HeartbeatInterval = int.Parse(heartbeatIntervalInSeconds);
            pubnub.EnableResumeOnReconnect = resumeOnReconnect;
        }
    }


    void DoPublishWindow(int windowID) {

        GUI.Label(new Rect(10,25,100,25), "Enter Message");
        publishedMessage = GUI.TextArea(new Rect(110,25,150,60),publishedMessage,2000);

        if (GUI.Button(new Rect(30, 100, 100, 20), "Publish"))
        {
            pubnub.Publish<string>(channel, publishedMessage, DisplayReturnMessage, DisplayErrorMessage);
            publishedMessage = "";
            showPublishPopupWindow = false;
        }

        if (GUI.Button(new Rect(150, 100, 100, 20), "Cancel"))
        {
            showPublishPopupWindow = false;
        }
        GUI.DragWindow(new Rect(0,0,800,400));
    }    

    void Start()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
    }

    private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
        {
            if (chain != null && chain.ChainStatus != null)
            {
                X509Certificate2 cert2 = new X509Certificate2(certificate);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                //chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                //chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(1000);
                //chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                //chain.ChainPolicy.VerificationTime = DateTime.Now;
                chain.Build(cert2);

                foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                {
                    if ((certificate.Subject == certificate.Issuer) &&
                        (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot)) 
                    {
                        // Self-signed certificates with an untrusted root are valid. 
                        continue;
                    }
                    else
                    {
                        if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                        {
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


    void Update()
    {
        if (pubnub == null) return;
        UnityEngine.Debug.Log(DateTime.Now.ToLongTimeString() + " Update called " + pubnubApiResult.Length.ToString());            
        string recordTest;
        System.Text.StringBuilder sbResult = new System.Text.StringBuilder();

        int existingLen = pubnubApiResult.Length;
        int newRecordLen = 0;
        sbResult.Append(pubnubApiResult);

        if (recordQueue.TryPeek(out recordTest))
        {
            string currentRecord = "";
            while (recordQueue.TryDequeue(out currentRecord))
            {
                sbResult.AppendLine(currentRecord);
            }

            pubnubApiResult = sbResult.ToString();

            newRecordLen = pubnubApiResult.Length - existingLen;
            int windowLength = 2000;

            if (pubnubApiResult.Length > windowLength)
            {
                bool trimmed = false;
                if (pubnubApiResult.Length > windowLength){
                    trimmed = true;
                    int lengthToTrim = (((pubnubApiResult.Length - windowLength) < pubnubApiResult.Length -newRecordLen)? pubnubApiResult.Length - windowLength : pubnubApiResult.Length - newRecordLen);
                    pubnubApiResult = pubnubApiResult.Substring(lengthToTrim);
                }
                if(trimmed)
                {
                    string prefix = "Output trimmed...\n";

                    pubnubApiResult = prefix + pubnubApiResult;
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        ResetPubnubInstance();
    }

    void ResetPubnubInstance()
    {
        if (pubnub != null)
        {
            pubnub.EndPendingRequests();
            System.Threading.Thread.Sleep(1000);
            pubnub = null;
        }
    }

    void DisplayReturnMessage(string result)
    {
        print(result);
        AddToPubnubResultContainer(string.Format("REGULAR CALLBACK: {0}",result));
    }

    void DisplayConnectStatusMessage(string result)
    {
        print(result);
        AddToPubnubResultContainer(string.Format("CONNECT CALLBACK: {0}",result));
    }

    void DisplayDisconnectStatusMessage(string result)
    {
        print(result);
        AddToPubnubResultContainer(string.Format("DISCONNECT CALLBACK: {0}",result));
    }

    void DisplayErrorMessage(PubnubClientError result)
    {
        print(result.Description);
        AddToPubnubResultContainer(string.Format("ERROR CALLBACK: {0}",result.Description));
    }

    void AddToPubnubResultContainer(string result)
    {
        recordQueue.Enqueue(result);
    }

}


