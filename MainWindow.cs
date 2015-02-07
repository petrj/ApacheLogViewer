using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Gtk;
using ApacheLogViewer;


public partial class MainWindow: Gtk.Window
{	
	private string _logPath = "/var/log/apache2/error.log";							    
	private LogReader _logReader;
	private bool _toggleEnabled = false;


	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		_logReader = new LogReader(_logPath);

		btnShowDate.Active = _logReader.ShowDate;
		btnShowCategory.Active = _logReader.ShowCategory;
		btnShowIP.Active = _logReader.ShowIP;
		btnShowPID.Active = _logReader.ShowPID;
		btnShowWarnings.Active = _logReader.ShowPHPWarnings;
		btnShowNotices.Active = _logReader.ShowPHPNotices;
		btnShowStackStrace.Active = _logReader.ShowPHPStackTraces;

		textview.SizeAllocated += delegate {  GoToEnd(); };

		_toggleEnabled = true;
		OnRefreshAction1Activated(this,null);
	}

	private void GoToEnd()
	{
		//textview.Buffer.MoveMark = textview.Buffer.EndIter;
		//textview.Buffer.PlaceCursor(textview.Buffer.EndIter);
		//textview.ScrollToIter(textview.Buffer.EndIter,0,false,0d,0d);
		//textview.ScrollToMark(textview.Buffer.MoveMark,0,false,0,0);
		//textview.SetScrollAdjustments(new Adjustment(
		//textview.ScrollToIter(textview.Buffer.EndIter, 0, false, 0, 0);
		//var adj = GtkScrolledWindow.Vadjustment;
		//adj.Value = adj.Upper;
		textview.ScrollToIter(textview.Buffer.EndIter,0,false,0,0);

		//while (Gtk.Application.EventsPending())
			//Gtk.Application.RunIteration();

		// running:
		//var adj = GtkScrolledWindow.Vadjustment;
		//adj.Value = adj.Upper;
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnRefreshAction1Activated (object sender, EventArgs e)
	{
		_logReader.ReLoad();

		textview.Buffer.Text = _logReader.ToString();

		//Reload();
		GoToEnd();
	}	

	protected void OnTextviewKeyPressEvent (object o, KeyPressEventArgs args)
	{	
		if (args.Event.Key == Gdk.Key.F5)
		{
			OnRefreshAction1Activated(this,null);
		}
	}

	protected void OnBtnShowDateChanged (object o, ChangedArgs args)
	{

	}		
	
	protected void OnBtnShowDateToggled (object sender, EventArgs e)
	{		
		if (_logReader == null || !_toggleEnabled)
			return;

		_logReader.ShowDate = btnShowDate.Active;
		_logReader.ShowCategory = btnShowCategory.Active;
		_logReader.ShowPID = btnShowPID.Active;
		_logReader.ShowIP = btnShowIP.Active;
		_logReader.ShowPHPWarnings = btnShowWarnings.Active;
		_logReader.ShowPHPNotices = btnShowNotices.Active;
		_logReader.ShowPHPStackTraces = btnShowStackStrace.Active;

		OnRefreshAction1Activated(this,null);
	}	

	public static string ExecuteAndReturnOutput(string command,string arguments = null)
	{
		var info = new System.Diagnostics.ProcessStartInfo(command,arguments);
		
		info.UseShellExecute = false; 
		info.ErrorDialog = true; 
		info.CreateNoWindow = true; 
		info.RedirectStandardOutput = true; 

	 	Process p = System.Diagnostics.Process.Start(info); 
		p.WaitForExit();
		System.IO.StreamReader sReader = p.StandardOutput; 

		string res = null;
		while (!sReader.EndOfStream)
		{
			res += sReader.ReadLine() + Environment.NewLine;
		}    			
		sReader.Close(); 
		
		return res;

	}

	public static void InfoDialog(string message,MessageType msgType = MessageType.Info )
	{
		MessageDialog md = new MessageDialog (null, 
                                  DialogFlags.DestroyWithParent,
                              	  msgType, 
                                  ButtonsType.Close, message);
			md.Run();
			md.Destroy();
	}

	protected void OnBtnClearLogActivated (object sender, EventArgs e)
	{
		var output = ExecuteAndReturnOutput("/scripts/clear_apache_log.sh");

			if (output.Contains("failed"))
			{
				InfoDialog("Error while clearing log:" + Environment.NewLine+ Environment.NewLine+ output,MessageType.Error);
			} 

		OnRefreshAction1Activated(this,null);
	}





}
