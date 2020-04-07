using System;
using System.Diagnostics;
using System.IO;
using ApacheLogViewer;
using Gtk;

public partial class MainWindow: Gtk.Window
{	
	private string _logPath = "/var/log/apache2/error.log";							    
	private LogReader _logReader;
	private bool _toggleEnabled = false;
	private bool _refreshing = false;
	public FileSystemWatcher Watcher { get; set; }

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
		btnShowStackTrace.Active = _logReader.ShowPHPStackTraces;

		textview.SizeAllocated += delegate {  GoToEnd(); };

		_toggleEnabled = true;
		OnRefreshAction1Activated(this,null);
		
			Watcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(_logPath));
			Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
			Watcher.Changed += new FileSystemEventHandler(OnConfigDirChange);
			Watcher.Deleted += new FileSystemEventHandler (OnConfigDirChange);
			Watcher.Created += new FileSystemEventHandler (OnConfigDirChange);
			Watcher.Renamed += new RenamedEventHandler (OnConfigDirChange);
			Watcher.EnableRaisingEvents = true;
	}

	public string LogPath
	{
		get 
		{
			return _logPath;
		}
		set 
		{
			_logPath = value;
		}
	}
	
	protected void OnConfigDirChange(object sender,FileSystemEventArgs args)
	{
		try
		{	
			if (_refreshing) 
				return;
			
			Gtk.Application.Invoke(delegate {
        		OnRefreshAction1Activated(this,null);
    		});    
			
			
		} catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());			
		}
	}

	private void GoToEnd()
	{
		textview.ScrollToIter(textview.Buffer.EndIter,0,false,0,0);
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnRefreshAction1Activated (object sender, EventArgs e)
	{
		_refreshing = true;
	
		_logReader.ReLoad();

		textview.Buffer.Text = _logReader.ToString();

		GoToEnd();
		
		_refreshing = false;
	}	

	protected void OnTextviewKeyPressEvent (object o, KeyPressEventArgs args)
	{	
		if (args.Event.Key == Gdk.Key.F5)
		{
			OnRefreshAction1Activated(this,null);
		}
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
		_logReader.ShowPHPStackTraces = btnShowStackTrace.Active;

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
		var output = ExecuteAndReturnOutput("sh","\" -c '/etc/init.d/apache2 stop;rm -f /var/log/apache2/error.log;/etc/init.d/apache2 start';\"");
		
		if (output != null)
		{
			InfoDialog(output,MessageType.Info);
		} 

		OnRefreshAction1Activated(this,null);
	}

}
