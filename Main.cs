using System;
using System.IO;
using Gtk;

namespace ApacheLogViewer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			if (args != null &&
				args.Length == 1 &&
				!string.IsNullOrEmpty (args [0]) && File.Exists (args [0])) 
			{
				win.LogPath = args[0];			
			}
			win.Show ();
			Application.Run ();
		}
	}
}
