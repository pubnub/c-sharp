using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using PubNubMessaging.Core;
using System.Collections.Concurrent;
using System;
using System.Reflection;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

public class PubnubExample : MonoBehaviour {
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
			allowUserSettingsChange = false;
			pubnub.Presence<string>(channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
		}
		if (GUI.Button(new Rect(140,370,120,25), "Subscribe"))
		{
			InstantiatePubnub();
			allowUserSettingsChange = false;
			pubnub.Subscribe<string>(channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayErrorMessage);
		}

		if (GUI.Button(new Rect(10,400,120,25), "Detailed History"))
		{
			InstantiatePubnub();
			allowUserSettingsChange = false;
			pubnub.DetailedHistory<string>(channel, 100, DisplayReturnMessage, DisplayErrorMessage);
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
			allowUserSettingsChange = false;
			pubnub.Unsubscribe<string>(channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
		}
		
		if (GUI.Button(new Rect(140,430,120,25), "Here Now"))
		{
			InstantiatePubnub();
			allowUserSettingsChange = false;
			pubnub.HereNow<string>(channel, DisplayReturnMessage, DisplayErrorMessage);
		}
		
		if (GUI.Button(new Rect(10,460,120,25), "Presence-Unsub"))
		{
			InstantiatePubnub();
			allowUserSettingsChange = false;
			pubnub.PresenceUnsubscribe<string>(channel, DisplayReturnMessage, DisplayConnectStatusMessage, DisplayDisconnectStatusMessage, DisplayErrorMessage);
		}
		if (GUI.Button(new Rect(140,460,120,25), "Time"))
		{
			InstantiatePubnub();
			allowUserSettingsChange = false;
			pubnub.Time<string>(DisplayReturnMessage, DisplayErrorMessage);
		}

		if (GUI.Button(new Rect(10,490,120,25), "Disable Network"))
		{
			Task disableNetwork = Task.Factory.StartNew(() => 
			{
				InstantiatePubnub();
				pubnub.EnableSimulateNetworkFailForTestingOnly();
			});
		}
		if (GUI.Button(new Rect(140,490,120,25), "Enable Network"))
		{
			Task enableNetwork = Task.Factory.StartNew(() => 
			{
				InstantiatePubnub();
				pubnub.DisableSimulateNetworkFailForTestingOnly();
			});
		}

		if (GUI.Button(new Rect(10,520,120,25), "Disconnect/Retry"))
		{
			Task disconnectRetry = Task.Factory.StartNew(() => 
			{
				InstantiatePubnub();
				pubnub.TerminateCurrentSubscriberRequest();
			});
		}
		
		if (showPublishPopupWindow)
		{
			scrollPosition = GUI.BeginScrollView(new Rect(300,10,500,200), scrollPosition, new Rect(0,0,250,500),false, false);
			pubnubApiResult = GUI.TextArea(new Rect(0,0,485,200), pubnubApiResult);
		}
		else
		{
			scrollPosition = GUI.BeginScrollView(new Rect(300,10,500,500), scrollPosition, new Rect(0,0,450,1000),false, false);
			pubnubApiResult = GUI.TextArea(new Rect(0,0,485,1000), pubnubApiResult);			
		}
		GUI.EndScrollView();
		
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
            string recordTest;
			if (pubnubApiResult.Length > 10000)
			{
				pubnubApiResult = pubnubApiResult.Substring(pubnubApiResult.Length/2);
			}
            if (_recordQueue.TryPeek(out recordTest))
            {
                string currentRecord;
                while (_recordQueue.TryDequeue(out currentRecord))
                {
					pubnubApiResult += currentRecord + "\n";
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


