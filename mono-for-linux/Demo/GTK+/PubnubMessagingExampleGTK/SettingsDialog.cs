using System;
using Gtk;

namespace PubnubMessagingExampleGTK
{
	public partial class SettingsDialog : Gtk.Dialog
	{
		private MainWindow MainWin
		{
			get;set;
		}
		
		public bool Ssl
		{
			get;set;
		}
		
		public int port;
		
		public SettingsDialog (MainWindow parent)
		{
			MainWin = parent;
			this.Build ();
			Ssl = false;
			sslToggle.Active = false;
			sslToggle.Label = "Off";
			SetInitialValues();
		}

		public void SetInitialValues ()
		{
			if(!string.IsNullOrWhiteSpace(this.MainWin.Channel))
			{
				entryChannel.Text = this.MainWin.Channel;
			}
			
			Ssl = this.MainWin.Ssl;
			sslToggle.Active = Ssl;
			
			if(Ssl)
			{
				sslToggle.Label = "On";
			}
			else
			{
				sslToggle.Label = "Off";
			}
			
			if(!string.IsNullOrWhiteSpace(this.MainWin.Cipher))
			{
				entryCipher.Text = this.MainWin.Cipher;
			}

			if(!string.IsNullOrWhiteSpace(this.MainWin.CustomUuid))
			{
				entryUuid.Text = this.MainWin.CustomUuid;
			}

			if(!string.IsNullOrWhiteSpace(this.MainWin.Server))
			{
				entryServer.Text = this.MainWin.Server;
							
				if(!string.IsNullOrWhiteSpace(this.MainWin.Username))
				{
					entryUsername.Text = this.MainWin.Username;
				}
		
				if(!string.IsNullOrWhiteSpace(this.MainWin.Password))
				{
					entryPassword.Text = this.MainWin.Password;
				}
				
				if(this.MainWin.Port >0) 
				{
					port = this.MainWin.Port;
					entryPort.Text = port.ToString();
				}
			}
		}
		
		protected void ProxyExpanderActivated (object sender, System.EventArgs e)
		{
			if(this.mainFixedPanel.HeightRequest >= 300)
			{
				this.mainFixedPanel.HeightRequest = 400;
			}
			else if(this.mainFixedPanel.HeightRequest >= 400)
			{
				this.mainFixedPanel.HeightRequest = 300;
			}
		}

		protected bool Validated ()
		{
			if(string.IsNullOrWhiteSpace (entryChannel.Text))
			{
				ShowMessage("Please enter a channel name", "Error");
				return false;
			}			
			
			if(!string.IsNullOrWhiteSpace(entryServer.Text))
			{
				if((Int32.TryParse(entryPort.Text, out port)) && ((port >= 1) && (port <= 65535))) 
				{
					this.MainWin.Port = port;
				}
				else
				{
					ShowMessage("Proxy port must be a valid integer between 1 to 65535", "Error");
					return false;
				}
				
				if(string.IsNullOrWhiteSpace(entryUsername.Text))
				{
					ShowMessage("Proxy username is empty", "Error");
					return false;
				}
				
				if(string.IsNullOrWhiteSpace(entryPassword.Text))
				{
					ShowMessage("Proxy password is empty", "Error");
					return false;
				}
			}
			
			return true;
		}
		
		void ShowMessage(string message, string title)
		{
			MessageDialog md = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, title); 
			md.Modal = true;
			md.Text = message;
			md.Run ();
			md.Destroy();
		}

		protected void OkClicked (object sender, System.EventArgs e)
		{
			if(Validated())
			{
				this.MainWin.Channel = entryChannel.Text;
				this.MainWin.Ssl = Ssl;
				this.MainWin.Cipher = entryCipher.Text;
				this.MainWin.CustomUuid = entryUuid.Text;
				this.MainWin.Server = entryServer.Text;
				this.MainWin.Username = entryUsername.Text;
				this.MainWin.Password = entryPassword.Text;

				this.Respond(Gtk.ResponseType.Ok);
			}			
		}

		protected void SslToggled (object sender, System.EventArgs e)
		{
			if (((ToggleButton) sender).Active)
			{
				Ssl = true;
				((ToggleButton) sender).Label = "On";
			}
			else	
			{
				Ssl = false;
				((ToggleButton) sender).Label = "Off";
			}
		}
	}
}

