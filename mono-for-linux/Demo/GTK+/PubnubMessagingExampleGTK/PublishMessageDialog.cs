using System;

namespace PubnubMessagingExampleGTK
{
	public partial class PublishMessageDialog : Gtk.Dialog
	{
		public event MessageEnteredEventHandler MessageEntered;
		public delegate void MessageEnteredEventHandler(string message);
		
		private MainWindow MainWin
		{
			get;set;
		}
		
		public PublishMessageDialog (MainWindow parent)
		{
			this.Build ();
			MainWin = parent;
		}

		protected void MessageFocusOut (object o, Gtk.FocusOutEventArgs args)
		{
			MainWin.PublishMessage = textViewMessage.Buffer.Text;
		}
	}
}

