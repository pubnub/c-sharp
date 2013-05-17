using System;
using Gtk;

namespace PubnubMessagingExampleGTK
{
	public partial class AskChannelDialog : Gtk.Dialog
	{
		private MainWindow MainWin
		{
			get;set;
		}
		
		public AskChannelDialog (MainWindow parent, string channels)
		{
			MainWin = parent;
			this.Build ();
			entryChannel.Text = channels;
		}
	    
		protected bool Validated ()
		{
			if(string.IsNullOrWhiteSpace (entryChannel.Text))
			{
				ShowMessage("Please enter a channel name", "Error");
				return false;
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
				this.MainWin.TempChannel = entryChannel.Text;
				this.Respond(Gtk.ResponseType.Ok);
			}
		}
	}
}

