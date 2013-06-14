using System;
using Gtk;

namespace PubnubMessagingExampleGTK
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			//MainWindow win = new MainWindow ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
