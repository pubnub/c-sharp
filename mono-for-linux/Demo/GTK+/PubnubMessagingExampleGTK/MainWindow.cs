using System;
using Gtk;
using PubNubMessaging.Core;
using PubnubMessagingExampleGTK;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace PubnubMessagingExampleGTK
{
	public partial class MainWindow : Gtk.Window
	{	
		public enum Actions{
			Subscribe,
			Publish,
			Presence,
			DetailedHistory,
			HereNow,
			Unsubscribe,
			PresenceUnsubscribe
		}
		
		public string PublishMessage
		{
			get;set;	
		}
		
		public string Channel
		{
			get;set;
		}
		
		public string TempChannel
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
		bool cleanDetailedHistoryDisplay = true;
		bool cleanHereNowDisplay = true;
		
		public MainWindow () : 
				base(Gtk.WindowType.Toplevel)
		{
			Build ();
			ShowSettings();
		}
        
		protected override bool OnDeleteEvent (Gdk.Event ev)
        {
			pubnub.EndPendingRequests();
            Gtk.Application.Quit ();
            return true;
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
				sb.Append(logs.Buffer.Text);
				logs.Buffer.Text = sb.ToString();
			});		
		}

		protected void DisplayConnectStatusMessage(string result)
		{
			WriteToOutput("Connect Presence: " + result);
		}
		
		protected void DisplayReturnMessageUnsubscribe(string result)
		{
			WriteToOutput("Connect Unsubscribe: " + result);
			string[] fields = JsonConvert.DeserializeObject<string[]>(result);
                
            if ((fields [0] != null) && (fields [2] != null) && !fields[2].ToString().Contains("-pnpres"))
            {
				Gtk.Application.Invoke(delegate {
					string[] connectedChannels = tvChannels.Buffer.Text.Split('\n');
					tvChannels.Buffer.Text = "";
					StringBuilder sb = new StringBuilder();
					foreach(string ch in connectedChannels) {
						if(ch.Equals(fields[2].ToString()) || string.IsNullOrWhiteSpace(ch)){
							continue;
						} else {
							sb.AppendLine(ch);
						}
					}
					tvChannels.Buffer.Text = sb.ToString();
				});	
			} 
		}
		
		protected void DisplayConnectStatusMessageSubscribe(string result)
		{
			WriteToOutput("Connect Subscribe: " + result);
			string[] fields = JsonConvert.DeserializeObject<string[]>(result);
                
            if ((fields [0] != null) && (fields [2] != null) && !fields[2].ToString().Contains("-pnpres"))
            {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine(fields[2]);

				Gtk.Application.Invoke(delegate {
					tvChannels.Buffer.Text += sb.ToString();
				});	
			} 
		}
		
		protected void DisplayPresenceReturnMessage(string result)
		{
			WriteToOutput(result);
			DoHereNow();
		}
		
		protected void DisplayHereNowReturnMessage(string result)
		{
			WriteToOutput(result);
			IList<object> fields = JsonConvert.DeserializeObject<IList<object>>(result);
            StringBuilder sb = new StringBuilder();    
            if ((fields [0] != null) && (!fields[0].ToString().Equals("0")) && (fields [1] != null))
            {
				JContainer dictionary = fields[0] as JContainer;
				if(dictionary!=null && dictionary["uuids"] != null){
					IList<string> uuids = JsonConvert.DeserializeObject<IList<string>>(dictionary["uuids"].ToString());
					if(uuids!=null){
						foreach (string uuid in uuids)
						{	
							sb.AppendLine(uuid);
						}
					}
				}
				sb.Append(" [Ch: ");
				sb.Append(fields[1]);
				sb.AppendLine("]");
				sb.AppendLine("");
				
				Gtk.Application.Invoke(delegate {
					if(cleanHereNowDisplay)
					{
						tvConnectedUsers.Buffer.Text = sb.ToString();
						cleanHereNowDisplay = false;
					}
					else
					{
						tvConnectedUsers.Buffer.Text += sb.ToString();
					}					
				});	
			}
		}
		
		protected void DisplaySubscribeReturnMessage (string result)
		{
			WriteToOutput("Subscribe:" + result);
			
			object[] fields = JsonConvert.DeserializeObject<object[]>(result);
                
            if ((fields [0] != null) && (fields [2] != null))
            {
				StringBuilder sb = new StringBuilder();
				sb.Append(fields[0]);
				sb.Append(" [Ch: ");
				sb.Append(fields[2]);
				sb.AppendLine("]");

				Gtk.Application.Invoke(delegate {
					sb.Append(tvMessages.Buffer.Text);
					tvMessages.Buffer.Text = sb.ToString();
					//SetTimeToken(fields[1]);
				});	
			}
		}

		protected void SetTimeToken (string strTime)
		{
			long timetoken;
			if(long.TryParse(strTime, out timetoken))
			{
				entryServerTime.Text = DateTime.FromFileTime(timetoken).ToString("HH:mm");
			}
		}
		
		public void AskChannelNameAndDoAction(Actions actions){
			AskChannelDialog askChannelNameDialog = new AskChannelDialog(this, this.Channel);
			askChannelNameDialog.Modal = true;
			askChannelNameDialog.Name = "Enter Message";
			
			askChannelNameDialog.Response += delegate(object o, ResponseArgs args) {
				if(args.ResponseId == Gtk.ResponseType.Ok)
				{
					if(actions == Actions.Subscribe) {
						pubnub.Subscribe<string>(this.TempChannel, DisplaySubscribeReturnMessage, DisplayConnectStatusMessageSubscribe);					
					} else if(actions == Actions.Publish) {
						ShowPublishMessageDialog();
					} else if(actions == Actions.Presence) {
						pubnub.Presence<string>(this.TempChannel, DisplayPresenceReturnMessage, DisplayConnectStatusMessage);
					} else if(actions == Actions.DetailedHistory) {
						DoDetailedHistory();
					} else if(actions == Actions.HereNow) {
						DoHereNow();
					} else if(actions == Actions.Unsubscribe) {
						pubnub.Unsubscribe<string>(this.TempChannel, DisplayReturnMessage, DisplayReturnMessage, DisplayReturnMessageUnsubscribe);
					} else if(actions == Actions.PresenceUnsubscribe) {
				        pubnub.PresenceUnsubscribe<string>(this.TempChannel, DisplayReturnMessage, DisplayReturnMessage, DisplayReturnMessage);
					}
				}
			};
			askChannelNameDialog.Run();
			askChannelNameDialog.Destroy();
		}
	
		protected void SubscribeClicked (object sender, System.EventArgs e)
		{
			WriteToOutput("Running Subscribe");
			AskChannelNameAndDoAction(Actions.Subscribe);
		}
	
		protected void PublishClicked (object sender, System.EventArgs e)
		{
		    WriteToOutput("Running Publish");
			AskChannelNameAndDoAction(Actions.Publish);
		}
		
		void ShowPublishMessageDialog(){
			PublishMessageDialog publishMessage = new PublishMessageDialog(this);
			publishMessage.Modal = true;
			publishMessage.Name = "Enter Message";
			
			publishMessage.Response += delegate(object o, ResponseArgs args) {
				if(args.ResponseId == Gtk.ResponseType.Ok)
				{
					Console.WriteLine(this.PublishMessage);
					string []channels = this.TempChannel.Split(',');
					foreach (string ch in channels)
					{
						pubnub.Publish<string>(ch, this.PublishMessage, DisplayReturnMessage);
					}
				}
			};
			publishMessage.Run();
			publishMessage.Destroy();
		}
	
		protected void PresenceClicked (object sender, System.EventArgs e)
		{
			AskChannelNameAndDoAction(Actions.Presence);
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
			btnUnsubscribe.Sensitive = false;
			btnPresenceUnsubscribe.Sensitive = false;
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
			btnUnsubscribe.Sensitive = true;
			btnPresenceUnsubscribe.Sensitive = true;
		}
	
		protected void ShowSettings ()
		{
			DisableActions();
			if(pubnub != null){
				pubnub.EndPendingRequests();
			}
			SettingsDialog settings = new SettingsDialog(this);
			settings.Modal = true;
			
			settings.Name = "Settings";
			bool errorFree = true;
			settings.Response += delegate(object o, ResponseArgs args) {
				if(args.ResponseId == Gtk.ResponseType.Ok)
				{
					//string channel = this.Channel; 
					pubnub = new Pubnub("demo", "demo", "", this.Cipher , this.Ssl);
					if(!string.IsNullOrEmpty(this.CustomUuid))
					{
						pubnub.SessionUUID = this.CustomUuid;
					}
					StringBuilder sbHead = new StringBuilder();
					//sbHead.Append("Ch:");
					foreach(string ch in this.Channel.Split(','))
					{
						sbHead.Append(ch.Trim());
						sbHead.Append("\n");
					}
					
					entrySsl.Text =  (this.Ssl)?"On": "Off";
					entryUuid.Text = pubnub.SessionUUID;
					entryCipher.Text = this.Cipher;
					//entryServerTime.Text = this.time

					//this.tvChannels.Buffer.Text = sbHead.ToString();
					
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
						SubscribeToChannels();
					}
					else
					{
						ShowSettings();
					}
				} else {
					if(pubnub != null){
						EnableActions();
					}
				}
			};
			settings.Run ();
			settings.Destroy();
		}
		
		private void SubscribeToChannels()
		{	
			tvChannels.Buffer.Text = "";
			//pubnub.Presence<string>(this.Channel, DisplayPresenceReturnMessage, DisplayConnectStatusMessage);
			pubnub.Subscribe<string>(this.Channel, DisplaySubscribeReturnMessage, DisplayConnectStatusMessageSubscribe);
		}
	
		protected void SettingsClicked (object sender, System.EventArgs e)
		{
			ShowSettings();
		}
	
		protected void UnsubscribePresenceClicked (object sender, System.EventArgs e)
		{
			AskChannelNameAndDoAction(Actions.PresenceUnsubscribe);
		}
	
		protected void UnsubscribeClicked (object sender, System.EventArgs e)
		{
			AskChannelNameAndDoAction(Actions.Unsubscribe);
		}

		protected void DoHereNow ()
		{
			string[] channels = this.TempChannel.Split(',');
			cleanHereNowDisplay = true;
			foreach (string ch in channels)
			{
				pubnub.HereNow<string>(ch, DisplayHereNowReturnMessage);
			}
		}
	
		protected void HereNowClicked (object sender, System.EventArgs e)
		{
			AskChannelNameAndDoAction(Actions.HereNow);
		}
		
		protected void DoDetailedHistory ()
		{
			string[] channels = this.TempChannel.Split(',');
			cleanDetailedHistoryDisplay = true;
			foreach (string ch in channels)
			{
				pubnub.DetailedHistory<string>(ch, 100, DisplayDetailedHistoryReturnMessage);
			}
		}

		protected void DetailedHistoryClicked (object sender, System.EventArgs e)
		{
			AskChannelNameAndDoAction(Actions.DetailedHistory);
		}
		
		protected void DisplayDetailedHistoryReturnMessage (string result)
		{
			WriteToOutput(result);
			IList<object> fields = JsonConvert.DeserializeObject<IList<object>>(result);
                
            if ((fields [0] != null) && (!fields[0].ToString().Equals("0")) && (fields [2] != null))
            {
				IList<object> enumerable = JsonConvert.DeserializeObject<IList<object>>(fields[0].ToString());
				StringBuilder sb = new StringBuilder();
				sb.Append(" [Ch: ");
				sb.Append(fields[3]);
				sb.AppendLine("]");
				if ((enumerable != null) && (enumerable.Count > 0))
				{
					foreach (object element in enumerable)
					{
						string response = element.ToString();
						sb.AppendLine(response);
					}
				}
				sb.AppendLine("");
				
				Gtk.Application.Invoke(delegate {
					if(cleanDetailedHistoryDisplay)
					{
						tvDetailedHistory.Buffer.Text = sb.ToString();
						cleanDetailedHistoryDisplay = false;
					}
					else
					{
						tvDetailedHistory.Buffer.Text += sb.ToString();
					}
						
				});	
			}
		}
	
		protected void CancelClicked (object sender, System.EventArgs e)
		{
			//pubnub.TerminateCurrentSubscriberRequest();
			pubnub.EndPendingRequests();
		}
	
		protected void TimeClicked (object sender, System.EventArgs e)
		{
			pubnub.Time<string>(DisplayReturnMessage);
		}

		protected void ShowConnectedUsers (object sender, System.EventArgs e)
		{
			HereNowClicked(sender, e);
		}
	}
}

