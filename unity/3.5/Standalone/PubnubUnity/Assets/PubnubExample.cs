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
using System.Text;

public class PubnubExample : MonoBehaviour {

    enum PubnubState
    {
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
        DisableNetwork
    }

    bool ssl = false;
    bool resumeOnReconnect = true;

    string cipherKey = "";
    string secretKey = "";
    string uuid = Guid.NewGuid().ToString();

    string subscribeTimeoutInSeconds = "310";
    string operationTimeoutInSeconds = "15";
    string networkMaxRetries = "50";
    string networkRetryIntervalInSeconds = "10";
    string heartbeatIntervalInSeconds = "10";
	static public bool showErrorMessageSegments = true;

    string channel = "hello_my_channel";
    string publishedMessage = "";
	string authKey = "";

    static Pubnub pubnub;

    private static ConcurrentQueue<string> recordQueue = new ConcurrentQueue<string>();

    //GameObject PubnubApiResult;
    Vector2 scrollPosition = Vector2.zero;
    string pubnubApiResult = "";
    #if(UNITY_IOS)
    bool requestInProcess = false;
    #endif
    bool showPublishPopupWindow = false;
	bool showGrantPresenceWindow = false;
	bool showGrantSubscribeWindow = false;
	bool showAuthWindow = false;
	bool showActionsPopupWindow = false;
	bool showPamPopupWindow = false;
	bool canRead = false;
	bool canWrite = false;
	string ttl = "1440";

    Rect publishWindowRect = new Rect(60, 365, 300, 150);
	Rect authWindowRect = new Rect(60, 365, 300, 150);

    bool allowUserSettingsChange = true;
	float fLeft = 20;
	float fLeftInit = 20;
	float fTop = 10;
	float fTopInit = 10;
	float fRowHeight = 45;
	float fHeight = 35;
	float fButtonHeight = 40;
	float fButtonWidth = 120;
    
    public void OnGUI()
    {
        GUI.enabled = !allowUserSettingsChange;
        GUIStyle customStyle = new GUIStyle(GUI.skin.button);
        customStyle.fontSize = 10;
        customStyle.hover.textColor = Color.yellow;
        customStyle.fontStyle = FontStyle.Italic;

		fLeft = fLeftInit;
		fTop = fTopInit + 0 * fRowHeight;
        if (GUI.Button(new Rect(fLeft, fTop, 120, 40), "Reset Settings",customStyle))
        {
            allowUserSettingsChange = true;
            ResetPubnubInstance();
            pubnubApiResult = "";
        }

        GUI.enabled = allowUserSettingsChange;

        fLeft = fLeftInit + 150;
        ssl = GUI.Toggle(new Rect(fLeft, fTop, 100, fButtonHeight), ssl," Enable SSL ");

        fLeft = fLeft + 100;
        resumeOnReconnect = GUI.Toggle(new Rect(fLeft, fTop, 200, fButtonHeight), resumeOnReconnect," Resume On Reconnect ");

		GUI.enabled = true;

		fTop = fTopInit + 1 * fRowHeight;
		fLeft = fLeftInit;
		GUI.Label(new Rect(fLeft, fTop, 90, fHeight), "Channel Name");
		fLeft = fLeft + 90;
		channel = GUI.TextField(new Rect(fLeft, fTop, 140, fHeight),channel,100);

		fLeft = fLeft + 140 + 10; 
		if (GUI.Button(new Rect(fLeft, fTop, 90, fButtonHeight), "Actions"))
		{
			showPamPopupWindow = false;
			showActionsPopupWindow = !showActionsPopupWindow;
		}
		if (showActionsPopupWindow) 
		{
			ShowActions (fLeft, fTop, fButtonHeight);
		}

		fLeft = fLeft + 90 + 10;
		if (GUI.Button(new Rect(fLeft, fTop, 90, fButtonHeight), "PAM"))
		{
			showActionsPopupWindow = false;
			showPamPopupWindow = !showPamPopupWindow;
		}
		if (showPamPopupWindow) 
		{
			ShowPamActions (fLeft, fTop, fButtonHeight);
		}

		GUI.enabled = allowUserSettingsChange;

        fTop = fTopInit + 2 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label(new Rect(fLeft, fTop, 70, fHeight), "Cipher Key");

        fLeft = fLeft + 75;
        cipherKey = GUI.TextField(new Rect(fLeft, fTop, 130, fHeight),cipherKey);

        fLeft = fLeft + 145;
        GUI.Label(new Rect(fLeft, fTop, 70, fHeight), "UUID");

        fLeft = fLeft + 45;
        uuid = GUI.TextField(new Rect(fLeft, fTop, 170, fHeight),uuid);

        fTop = fTopInit + 3 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label(new Rect(fLeft, fTop, 70, fHeight), "Secret Key");
        fLeft = fLeft + 75;
        secretKey = GUI.TextField(new Rect(fLeft, fTop, 130, fHeight),secretKey);

        fLeft = fLeft + 145;
        GUI.Label(new Rect(fLeft, fTop, 160, fHeight), "Subscribe Timeout (secs)");

        fLeft = fLeft + 185;
        subscribeTimeoutInSeconds = GUI.TextField(new Rect(fLeft, fTop, 30, fHeight),subscribeTimeoutInSeconds,6);
        subscribeTimeoutInSeconds = Regex.Replace(subscribeTimeoutInSeconds, "[^0-9]", "");

        fTop = fTopInit + 4 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label(new Rect(fLeft, fTop, 160, fHeight), "MAX retries");

        fLeft = fLeft + 175;
        networkMaxRetries = GUI.TextField(new Rect(fLeft, fTop, 30, fHeight),networkMaxRetries,6);
        networkMaxRetries = Regex.Replace(networkMaxRetries, "[^0-9]", "");

        fLeft = fLeft + 45;
        GUI.Label(new Rect(fLeft, fTop, 180, fHeight), "Non Subscribe Timeout (secs)");

        fLeft = fLeft + 185;
        operationTimeoutInSeconds = GUI.TextField(new Rect(fLeft, fTop, 30, fHeight),operationTimeoutInSeconds,6);
        operationTimeoutInSeconds = Regex.Replace(operationTimeoutInSeconds, "[^0-9]", "");

        fTop = fTopInit + 5 * fRowHeight;
        fLeft = fLeftInit;
        GUI.Label(new Rect(fLeft, fTop, 160, fHeight), "Retry Interval (secs)");

        fLeft = fLeft + 175;
        networkRetryIntervalInSeconds = GUI.TextField(new Rect(fLeft, fTop, 30, fHeight),networkRetryIntervalInSeconds,6);
        networkRetryIntervalInSeconds = Regex.Replace(networkRetryIntervalInSeconds, "[^0-9]", "");

        fLeft = fLeft + 45;
        GUI.Label(new Rect(fLeft, fTop, 180, fHeight), "Heartbeat Interval (secs)");
        fLeft = fLeft + 185;
        heartbeatIntervalInSeconds = GUI.TextField(new Rect(fLeft, fTop, 30, fHeight),heartbeatIntervalInSeconds,6);
        heartbeatIntervalInSeconds = Regex.Replace(heartbeatIntervalInSeconds, "[^0-9]", "");

        GUI.enabled = true;

		if (showPublishPopupWindow)
		{
			GUI.backgroundColor = Color.black;
			publishWindowRect = GUI.ModalWindow(0, publishWindowRect, DoPublishWindow, "Message Publish");
			GUI.backgroundColor = new Color(1,1,1,1);
		}
		if (showAuthWindow)
		{
			GUI.backgroundColor = Color.black;
			authWindowRect = GUI.ModalWindow(0, authWindowRect, DoAuthWindow, "Enter Auth Key");
			GUI.backgroundColor = new Color(1,1,1,1);
		}		
		if (showGrantPresenceWindow)
		{
			GUI.backgroundColor = Color.black;
			authWindowRect = GUI.ModalWindow(0, authWindowRect, DoGrantWindow, "Presence Grant");
			GUI.backgroundColor = new Color(1,1,1,1);
		}
		if (showGrantSubscribeWindow)
		{
			GUI.backgroundColor = Color.black;
			authWindowRect = GUI.ModalWindow(0, authWindowRect, DoGrantWindow, "Subscribe Grant");
			GUI.backgroundColor = new Color(1,1,1,1);
		}


        fTop = fTopInit + 6 * fRowHeight;
        fLeft = fLeftInit;
        scrollPosition = GUI.BeginScrollView(new Rect(fLeft, fTop, 430, 420), scrollPosition, new Rect(fLeft, fTop, 430, 420),false, true);
        GUI.enabled = false;
        pubnubApiResult = GUI.TextArea(new Rect(fLeft, fTop, 430, 420), pubnubApiResult);            
        GUI.enabled = true;
        GUI.EndScrollView();
    }

	void ShowPamActions (float fLeft, float fTop, float fButtonHeight)
	{
		Rect windowRect = new Rect (fLeft, fTop + fButtonHeight, 140, 550);
		GUI.backgroundColor = Color.black;
		windowRect = GUI.Window(0, windowRect, DoPamActionWindow, "");
		GUI.backgroundColor = new Color(1,1,1,1);
	}

	void ShowActions (float fLeft, float fTop, float fButtonHeight)
	{
		Rect windowRect = new Rect (fLeft, fTop + fButtonHeight, 140, 550);
		GUI.backgroundColor = Color.black;
		windowRect = GUI.Window(0, windowRect, DoActionWindow, "");
		GUI.backgroundColor = new Color(1,1,1,1);
	}

	void DoAuthWindow(int windowID) {
		GUI.Label(new Rect(10,30,100,fHeight), "Enter Auth Key");
		authKey = GUI.TextArea(new Rect(110,30,150,fHeight),authKey,200);
		if (GUI.Button(new Rect(30, 80, 100, fButtonHeight), "Set Auth Key"))
		{
			pubnub.AuthenticationKey = authKey;
			authKey = "";
			showAuthWindow = false;
		}

		if (GUI.Button(new Rect(150, 80, 100, fButtonHeight), "Cancel"))
		{
			showAuthWindow = false;
		}
		GUI.DragWindow(new Rect(0,0,800,400));
	}   

	void DoGrantWindow(int windowID) {
		fLeft = fLeftInit;
		fTop = 20;
		canRead = GUI.Toggle(new Rect(fLeft, fTop, 90, fButtonHeight), canRead," Can Read ");

		fLeft = fLeftInit + 100;
		canWrite = GUI.Toggle(new Rect(fLeft, fTop, 90, fButtonHeight), canWrite," Can Write ");

		GUI.Label(new Rect(30, 45, 100, fHeight), "TTL");

		ttl = GUI.TextArea(new Rect(110, 45, 100, fHeight), ttl, 20);

		if (GUI.Button(new Rect(30, 90, 100, fButtonHeight), "Grant"))
		{
			int iTtl;
			Int32.TryParse(ttl, out iTtl);
			if (iTtl == 0) iTtl = 1440;
			try{
				if (showGrantSubscribeWindow) {
					pubnub.GrantAccess<string>(channel, canRead, canWrite, iTtl, DisplayReturnMessage, DisplayErrorMessage);
				}else if(showGrantPresenceWindow){
					pubnub.GrantPresenceAccess<string>(channel, canRead, canWrite, iTtl, DisplayReturnMessage, DisplayErrorMessage);
				}
			}catch (Exception ex){
				DisplayErrorMessage (ex.ToString());
			}
			ttl = "";
			showGrantPresenceWindow = false;
			showGrantSubscribeWindow = false;
			canRead = false;
			canWrite = false;
		}

		if (GUI.Button(new Rect(150, 90, 100, fButtonHeight), "Cancel"))
		{
			showGrantPresenceWindow = false;
			showGrantSubscribeWindow = false;
			canRead = false;
			canWrite = false;
		}
		GUI.DragWindow(new Rect(0,0,800,400));
	}    

	void DoPamActionWindow(int windowID)
	{
		fLeft = fLeftInit - 10;
		fTop = fTopInit + 10;

		if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Grant Subscribe"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.GrantSubscribe);
			showPamPopupWindow = false;
			showGrantSubscribeWindow = true;
		}
		fTop = fTopInit + 1 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Audit Subscribe"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.AuditSubscribe);
			showPamPopupWindow = false;
		}

		fTop = fTopInit + 2 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke Subscribe"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.RevokeSubscribe);
			showPamPopupWindow = false;
		}

		fTop = fTopInit + 3 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Grant Presence"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.GrantPresence);
			showPamPopupWindow = false;
			showGrantPresenceWindow = true;
		}

		fTop = fTopInit + 4 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Audit Presence"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.AuditPresence);
			showPamPopupWindow = false;
		}

		fTop = fTopInit + 5 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Revoke Presence"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.RevokePresence);
			showPamPopupWindow = false;
		}

		fTop = fTopInit + 6 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Auth Key"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.AuthKey);
			showAuthWindow = true;
			showPamPopupWindow = false;
		}
	}

	void DoActionWindow(int windowID)
	{
		fLeft = fLeftInit - 10;
		fTop = fTopInit + 10;
		if (GUI.Button (new Rect (fLeft, fTop, fButtonWidth, fButtonHeight), "Disconnect/Retry"))
        {
			InstantiatePubnub();
            AsyncOrNonAsyncCall (PubnubState.DisconnectRetry);
			showActionsPopupWindow = false;
        }
		fTop = fTopInit + 1 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Presence"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.Presence);
			showActionsPopupWindow = false;
		}

		fTop = fTopInit + 2 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Subscribe"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.Subscribe);
			showActionsPopupWindow = false;
		}

		fTop = fTopInit + 3 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Detailed History"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.DetailedHistory);
			showActionsPopupWindow = false;
		}

		fTop = fTopInit + 4 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Publish"))
		{
			InstantiatePubnub();
			allowUserSettingsChange = false;
			showActionsPopupWindow = false;
			showPublishPopupWindow = true;
		}

		fTop = fTopInit + 5 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Unsubscribe"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.Unsubscribe);
			showActionsPopupWindow = false;
		}

		fTop = fTopInit + 6 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Presence-Unsub"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.PresenceUnsubscribe);
			showActionsPopupWindow = false;
		}

		fTop = fTopInit + 7 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Here Now"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.HereNow);
			showActionsPopupWindow = false;
		}

		fTop = fTopInit + 8 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Time"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.Time);
			showActionsPopupWindow = false;
		}

		#if(UNITY_IOS || UNITY_ANDROID)
		GUI.enabled = false;
		#endif
		fTop = fTopInit + 9 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Disable Network"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall (PubnubState.DisableNetwork);
			showActionsPopupWindow = false;
		}
		fTop = fTopInit + 10 * fRowHeight +10;
		if (GUI.Button(new Rect(fLeft, fTop, fButtonWidth, fButtonHeight), "Enable Network"))
		{
			InstantiatePubnub();
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
    
    void Awake(){
        Application.RegisterLogCallback(new Application.LogCallback(CaptureLogs));
    }
    
    void CaptureLogs(string condition, string stacktrace, LogType logType)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Type");
        sb.AppendLine(logType.ToString());
        sb.AppendLine("Condition");
        sb.AppendLine(condition);
        sb.AppendLine("stacktrace");
        sb.AppendLine(stacktrace);
        //UnityEngine.Debug.Log("Type: ", );
    }
    
    private void DoAction (object pubnubState)
    {
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
            } else if ((PubnubState)pubnubState == PubnubState.DisableNetwork) {
                AddToPubnubResultContainer ("Running Disable Network");
                pubnub.EnableSimulateNetworkFailForTestingOnly();
			} else if ((PubnubState)pubnubState == PubnubState.GrantSubscribe) {
				AddToPubnubResultContainer ("Running Grant Subscribe");
			} else if ((PubnubState)pubnubState == PubnubState.AuditSubscribe) {
				AddToPubnubResultContainer ("Running Audit Subscribe");
				pubnub.AuditAccess<string>(channel,DisplayReturnMessage, DisplayErrorMessage);
			} else if ((PubnubState)pubnubState == PubnubState.RevokeSubscribe) {
				AddToPubnubResultContainer ("Running Revoke Subscribe");
				pubnub.GrantAccess<string>(channel, false,false, DisplayReturnMessage, DisplayErrorMessage);
			} else if ((PubnubState)pubnubState == PubnubState.GrantPresence) {
				AddToPubnubResultContainer ("Running Grant Presence");
			} else if ((PubnubState)pubnubState == PubnubState.AuditPresence) {
				AddToPubnubResultContainer ("Running Audit Presence");
				pubnub.AuditPresenceAccess<string>(channel, DisplayReturnMessage, DisplayErrorMessage);
			} else if ((PubnubState)pubnubState == PubnubState.RevokePresence) {
				AddToPubnubResultContainer ("Running Revoke Presence");
				pubnub.GrantPresenceAccess<string>(channel, false, false, DisplayReturnMessage, DisplayErrorMessage);
			} else if ((PubnubState)pubnubState == PubnubState.AuthKey) {
				AddToPubnubResultContainer ("Running Auth Key");
            } else if ((PubnubState)pubnubState == PubnubState.DisconnectRetry) {
                AddToPubnubResultContainer ("Running Disconnect Retry");
                pubnub.TerminateCurrentSubscriberRequest();
                #if(UNITY_IOS)
                requestInProcess = false;
                #endif
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log ("DoAction exception:"+ ex.ToString());
        }
    }

    void InstantiatePubnub()
    {
        if (pubnub == null)
        {
			secretKey = "sec-c-MzQ4NzcxYzgtYjcyZS00YzJmLWE5YmItZDMwNWJkNjU1MGY5";
			pubnub = new Pubnub("pub-c-7543998b-9c77-4161-9d6f-b5b8bd3b1138","sub-c-974a9d42-0ebe-11e3-a990-02ee2ddab7fe",secretKey,cipherKey,ssl);

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
        string stringMessage = publishedMessage;
        if (GUI.Button(new Rect(30, 100, 100, 30), "Publish"))
        {
            pubnub.Publish<string>(channel, stringMessage, DisplayReturnMessage, DisplayErrorMessage);
            publishedMessage = "";
            showPublishPopupWindow = false;
        }

        if (GUI.Button(new Rect(150, 100, 100, 30), "Cancel"))
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

        try{
            //UnityEngine.Debug.Log(DateTime.Now.ToLongTimeString() + " Update called " + pubnubApiResult.Length.ToString());            
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
                int windowLength = 600;

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
        catch (Exception ex){
            Debug.Log ("Update exception:" + ex.ToString());
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
        //print(result);
        UnityEngine.Debug.Log (string.Format("REGULAR CALLBACK LOG: {0}",result));
        AddToPubnubResultContainer(string.Format("REGULAR CALLBACK: {0}",result));
    }

    void DisplayConnectStatusMessage(string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format("CONNECT CALLBACK LOG: {0}",result));
        AddToPubnubResultContainer(string.Format("CONNECT CALLBACK: {0}",result));
    }

    void DisplayDisconnectStatusMessage(string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format("DISCONNECT CALLBACK LOG: {0}",result));
        AddToPubnubResultContainer(string.Format("DISCONNECT CALLBACK: {0}",result));
    }

    void DisplayErrorMessage(string result)
    {
        //print(result);
        UnityEngine.Debug.Log (string.Format("ERROR CALLBACK LOG: {0}",result));
        AddToPubnubResultContainer(string.Format("ERROR CALLBACK: {0}",result));
    }

	/// <summary>
	/// Callback method for error messages
	/// </summary>
	/// <param name="result"></param>
	void DisplayErrorMessage(PubnubClientError result)
	{
		UnityEngine.Debug.Log(result.Description);

		switch (result.StatusCode)
		{
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
			case 5040:
			//Critical: Gateway Timeout. No response from server due to PubNub server timeout. Please try again. If same problem persists, please contact PubNub support
			break;
			case 0:
			//Undocumented error. Please contact PubNub support with full error object details for further investigation
			break;
			default:
			break;
		}

		if (showErrorMessageSegments)
		{
			DisplayErrorMessageSegments(result);
		}
	}


	void DisplayErrorMessageSegments(PubnubClientError pubnubError)
	{
		UnityEngine.Debug.Log (string.Format("<STATUS CODE>: {0}", pubnubError.StatusCode)); // Unique ID of Error

		UnityEngine.Debug.Log (string.Format("<MESSAGE>: {0}", pubnubError.Message)); // Message received from server/clent or from .NET exception
		AddToPubnubResultContainer(string.Format("Error: {0}", pubnubError.Message));
		UnityEngine.Debug.Log (string.Format("<SEVERITY>: {0}", pubnubError.Severity)); // Info can be ignored, Warning and Error should be handled

		if (pubnubError.DetailedDotNetException != null)
		{
						//Console.WriteLine(pubnubError.IsDotNetException); // Boolean flag to check .NET exception
			UnityEngine.Debug.Log (string.Format("<DETAILED DOT.NET EXCEPTION>: {0}", pubnubError.DetailedDotNetException.ToString())); // Full Details of .NET exception
		}
		UnityEngine.Debug.Log (string.Format("<MESSAGE SOURCE>: {0}", pubnubError.MessageSource)); // Did this originate from Server or Client-side logic
		if (pubnubError.PubnubWebRequest != null)
		{
			//Captured Web Request details
			UnityEngine.Debug.Log (string.Format("<HTTP WEB REQUEST>: {0}", pubnubError.PubnubWebRequest.RequestUri.ToString())); 
			UnityEngine.Debug.Log (string.Format("<HTTP WEB REQUEST - HEADERS>: {0}", pubnubError.PubnubWebRequest.Headers.ToString())); 
		}
		if (pubnubError.PubnubWebResponse != null)
		{
			//Captured Web Response details
			UnityEngine.Debug.Log (string.Format("<HTTP WEB RESPONSE - HEADERS>: {0}", pubnubError.PubnubWebResponse.Headers.ToString()));
		}
		UnityEngine.Debug.Log (string.Format("<DESCRIPTION>: {0}", pubnubError.Description)); // Useful for logging and troubleshooting and support
		AddToPubnubResultContainer(string.Format("DESCRIPTION: {0}", pubnubError.Description));
		UnityEngine.Debug.Log (string.Format("<CHANNEL>: {0}", pubnubError.Channel)); //Channel name(s) at the time of error
		UnityEngine.Debug.Log (string.Format("<DATETIME>: {0}", pubnubError.ErrorDateTimeGMT)); //GMT time of error

	}

    void AddToPubnubResultContainer(string result)
    {
        recordQueue.Enqueue(result);
    }

}


