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
	
	string channel = "hello_my_channel";
	string publishedMessage = "";
	
	static Pubnub pubnub;
	
	private static ConcurrentQueue<string> _recordQueue = new ConcurrentQueue<string>();
	
	//GameObject PubnubApiResult;
	Vector2 scrollPosition = Vector2.zero;
	string pubnubApiResult = "";
	
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
		
		resumeOnReconnect = GUI.Toggle(new Rect(10,30,200,25), resumeOnReconnect," Resume On Reconnect ");

		GUI.Label(new Rect(10,70,70,25), "Cipher Key");
		cipherKey = GUI.TextField(new Rect(80,70,150,25),cipherKey);

		GUI.Label(new Rect(10,100,70,25), "Secret Key");
		secretKey = GUI.TextField(new Rect(80,100,150,25),secretKey);

		GUI.Label(new Rect(10,130,70,25), "UUID");
		uuid = GUI.TextField(new Rect(80,130,200,25),uuid);

		GUI.Label(new Rect(10,170,200,25), "Subscriber Timeout (in sec)");
		subscribeTimeoutInSeconds = GUI.TextField(new Rect(200,170,50,25),subscribeTimeoutInSeconds,6);
		subscribeTimeoutInSeconds = Regex.Replace(subscribeTimeoutInSeconds, "[^0-9]", "");

		GUI.Label(new Rect(10,200,200,25), "Non Subscribe Timeout (in sec)");
		operationTimeoutInSeconds = GUI.TextField(new Rect(200,200,50,25),operationTimeoutInSeconds,6);
		operationTimeoutInSeconds = Regex.Replace(operationTimeoutInSeconds, "[^0-9]", "");
		
		GUI.Label(new Rect(10,230,200,25), "Number of MAX retries");
		networkMaxRetries = GUI.TextField(new Rect(200,230,50,25),networkMaxRetries,6);
		networkMaxRetries = Regex.Replace(networkMaxRetries, "[^0-9]", "");

		GUI.Label(new Rect(10,260,200,25), "Retry Interval (in sec)");
		networkRetryIntervalInSeconds = GUI.TextField(new Rect(200,260,50,25),networkRetryIntervalInSeconds,6);
		networkRetryIntervalInSeconds = Regex.Replace(networkRetryIntervalInSeconds, "[^0-9]", "");

		GUI.Label(new Rect(10,290,200,25), "Heartbeat Interval (in sec)");
		heartbeatIntervalInSeconds = GUI.TextField(new Rect(200,290,50,25),heartbeatIntervalInSeconds,6);
		heartbeatIntervalInSeconds = Regex.Replace(heartbeatIntervalInSeconds, "[^0-9]", "");

		GUI.enabled = true;
	
		GUI.Label(new Rect(10,330,100,25), "Channel Name");
		channel = GUI.TextField(new Rect(100,330,150,25),channel,100);
		
		if (GUI.Button(new Rect(10,370,120,25), "Presence"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.Presence);
		}
		if (GUI.Button(new Rect(140,370,120,25), "Subscribe"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.Subscribe);
		}

		if (GUI.Button(new Rect(10,400,120,25), "Detailed History"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.DetailedHistory);
		}
		
		if (GUI.Button(new Rect(140,400,120,25), "Publish"))
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
		
		if (GUI.Button(new Rect(10,430,120,25), "Unsubscribe"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.Unsubscribe);
		}
		
		if (GUI.Button(new Rect(140,430,120,25), "Here Now"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.HereNow);
		}
		
		if (GUI.Button(new Rect(10,460,120,25), "Presence-Unsub"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.PresenceUnsubscribe);
		}
		if (GUI.Button(new Rect(140,460,120,25), "Time"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.Time);
		}

		if (GUI.Button(new Rect(10,490,120,25), "Disable Network"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.DisableNetwork);
		}
		if (GUI.Button(new Rect(140,490,120,25), "Enable Network"))
		{
			InstantiatePubnub();
			AsyncOrNonAsyncCall(PubnubState.EnableNetwork);
		}

		if (GUI.Button(new Rect(10,520,120,25), "Disconnect/Retry"))
		{
			InstantiatePubnub();
			ThreadPool.QueueUserWorkItem(new WaitCallback(DoAction), PubnubState.DisconnectRetry);
		}
		
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
		DoAction(pubnubState);
#else
		ThreadPool.QueueUserWorkItem(new WaitCallback(DoAction), pubnubState);
#endif
	}

	private void DoAction(object pubnubState)
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
			} else if ((PubnubState)pubnubState == PubnubState.DisconnectRetry) {
				AddToPubnubResultContainer ("Running Disconnect Retry");
				pubnub.TerminateCurrentSubscriberRequest();
			}
		}
		catch(Exception ex)
		{
			UnityEngine.Debug.Log (ex.ToString());
		}
	}
	
	void InstantiatePubnub()
	{
		if (pubnub == null)
		{
			pubnub = new Pubnub("demo","demo",secretKey,cipherKey,ssl);
			
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
			UnityEngine.Debug.Log(DateTime.Now.ToLongTimeString() + " Update called " + pubnubApiResult.Length.ToString());			
			string recordTest;
			System.Text.StringBuilder sbResult = new System.Text.StringBuilder();
	
			int existingLen = pubnubApiResult.Length;
			int newRecordLen = 0;
			sbResult.Append(pubnubApiResult);

            if (_recordQueue.TryPeek(out recordTest))
            {
                string currentRecord = "";
                while (_recordQueue.TryDequeue(out currentRecord))
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
	
    void DisplayErrorMessage(string result)
    {
        print(result);
		AddToPubnubResultContainer(string.Format("ERROR CALLBACK: {0}",result));
    }

	void AddToPubnubResultContainer(string result)
    {
        _recordQueue.Enqueue(result);
    }

}


