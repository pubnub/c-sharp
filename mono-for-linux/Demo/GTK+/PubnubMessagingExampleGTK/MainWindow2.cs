using System;
using Gtk;
using PubNubMessaging.Core;
using PubnubMessagingExampleGTK;
using System.Text;

public partial class MainWindow2: Gtk.Window
{	
	public string PublishMessage
	{
		get;set;	
	}
	
	public string Channel
	{
		get;set;
	}
	
	public bool Ssl
	{
		get;set;
	}
	
	public string Cipher
	{
		get;set;
	}
	
	public string CustomUuid
	{
		get;set;
	}
	
	public string Server
	{
		get;set;
	}
	
	public int Port
	{
		get;set;
	}
	
	public string Username
	{
		get;set;
	}
	
	public string Password
	{
		get;set;
	}
	
	public Pubnub pubnub;
	
	public MainWindow2 (): base (Gtk.WindowType.Toplevel)
	{
		//Build ();
		DisableActions();
		ShowSettings();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void DisplayReturnMessage (string result)
	{
		WriteToOutput(result);
	}
	
	void WriteToOutput(string output)
	{
		Gtk.Application.Invoke(delegate {
			StringBuilder sb = new StringBuilder();
			sb.Append(DateTime.Now.ToLongTimeString());
			sb.Append(" : ");
			sb.AppendLine(output);
			//sb.Append(textViewOutput.Buffer.Text);
			//textViewOutput.Buffer.Text = sb.ToString();
		});		
	}
	
	protected void DisplayConnectStatusMessage(string result)
	{
		WriteToOutput(result);
	}

	protected void SubscribeClicked (object sender, System.EventArgs e)
	{
		WriteToOutput("Running Subscribe");
		pubnub.Subscribe<string>(this.Channel, DisplayReturnMessage, DisplayConnectStatusMessage);
	}

	protected void PublishClicked (object sender, System.EventArgs e)
	{
		/*PublishMessageDialog publishMessage = new PublishMessageDialog(this);
		publishMessage.Modal = true;
		publishMessage.Name = "Enter Message";
		
		publishMessage.Response += delegate(object o, ResponseArgs args) {
			if(args.ResponseId == Gtk.ResponseType.Ok)
			{
				Console.WriteLine(this.PublishMessage);
				string []channels = this.Channel.Split(',');
				foreach (string ch in channels)
				{
					pubnub.Publish<string>(ch, this.PublishMessage, DisplayReturnMessage);
				}
			}
		};
		publishMessage.Run();
		publishMessage.Destroy();*/
	}

	protected void PresenceClicked (object sender, System.EventArgs e)
	{
		pubnub.Presence<string>(this.Channel, DisplayReturnMessage, DisplayConnectStatusMessage);
	}

	public void DisableActions ()
	{
		btnCancel.Sensitive = false;
		btnDetailedHistory.Sensitive = false;
		btnHereNow.Sensitive = false;
		btnPresence.Sensitive = false;
		btnPublish.Sensitive = false;
		btnSubscribe.Sensitive = false;
		btnTime.Sensitive = false;
		btnUnsub.Sensitive = false;
		btnUnsubPres.Sensitive = false;
	}

	protected void EnableActions ()
	{
		btnCancel.Sensitive = true;
		btnDetailedHistory.Sensitive = true;
		btnHereNow.Sensitive = true;
		btnPresence.Sensitive = true;
		btnPublish.Sensitive = true;
		btnSubscribe.Sensitive = true;
		btnTime.Sensitive = true;
		btnUnsub.Sensitive = true;
		btnUnsubPres.Sensitive = true;
	}

	protected void ShowSettings ()
	{
		/*SettingsDialog settings = new SettingsDialog(this);
		settings.Modal = true;
		settings.Name = "Settings";
		bool errorFree = true;
		settings.Response += delegate(object o, ResponseArgs args) {
			if(args.ResponseId == Gtk.ResponseType.Ok)
			{
				//string channel = this.Channel; 
				pubnub = new Pubnub("demo", "demo", "", this.Cipher , this.Ssl);
				pubnub.SessionUUID = this.CustomUuid;
				
				StringBuilder sbHead = new StringBuilder();
				sbHead.Append("Ch:");
				sbHead.Append(this.Channel);
				sbHead.Append("; ");
				sbHead.Append((this.Ssl)?"Ssl": "");
				
				this.lblHead.Text = sbHead.ToString();
				
				PubnubProxy proxy = new PubnubProxy();
				if(!string.IsNullOrWhiteSpace(this.Server))
				{
	                proxy.ProxyServer = this.Server;
	                proxy.ProxyPort = this.Port;
	                proxy.ProxyUserName = this.Username;
	                proxy.ProxyPassword = this.Password;
					
	                try
	                {
	                    pubnub.Proxy = proxy;
	                }
	                catch (MissingFieldException mse)
	                {
						Console.WriteLine(mse.Message);
						errorFree = false;
	                }
				}
				if(errorFree)
				{
					EnableActions();
				}
				else
				{
					ShowSettings();
				}
			}
		};
		settings.Run ();
		settings.Destroy();*/
	}

	protected void SettingsClicked (object sender, System.EventArgs e)
	{
		ShowSettings();
	}

	protected void UnsubscribePresenceClicked (object sender, System.EventArgs e)
	{
        pubnub.PresenceUnsubscribe<string>(this.Channel, DisplayReturnMessage, DisplayReturnMessage, DisplayReturnMessage);
	}

	protected void UnsubscribeClicked (object sender, System.EventArgs e)
	{
		pubnub.Unsubscribe<string>(this.Channel, DisplayReturnMessage, DisplayReturnMessage, DisplayReturnMessage);
	}

	protected void HereNowClicked (object sender, System.EventArgs e)
	{
		string[] channels = this.Channel.Split(',');
		foreach (string ch in channels)
		{
			pubnub.HereNow<string>(ch, DisplayReturnMessage);
		}
	}

	protected void DetailedHistoryClicked (object sender, System.EventArgs e)
	{
		string[] channels = this.Channel.Split(',');
		foreach (string ch in channels)
		{
			pubnub.DetailedHistory<string>(ch, 100, DisplayReturnMessage);
		}
	}

	protected void CancelClicked (object sender, System.EventArgs e)
	{
		pubnub.TerminateCurrentSubscriberRequest();
	}

	protected void TimeClicked (object sender, System.EventArgs e)
	{
		pubnub.Time<string>(DisplayReturnMessage);
	}
}
