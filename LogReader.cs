using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace ApacheLogViewer
{
	public class LogReader
	{
		private string _logFileName;
		private string _rawLog = null;

		public List<string> Lines { get; set; }
		public List<LogEntry> Entries { get; set; }

		public bool ShowDate { get; set; }
		public bool ShowCategory { get; set; }
		public bool ShowPID { get; set; }
		public bool ShowIP { get; set; }

		public bool ShowPHPWarnings { get; set; }
		public bool ShowPHPNotices { get; set; }
		public bool ShowPHPStackTraces { get; set; }

		public LogReader (string logFileName)
		{
			_logFileName = logFileName;

			ShowDate  = true;
			ShowCategory = true;
			ShowPID = false;
			ShowIP = false;

			ShowPHPWarnings = true;
			ShowPHPNotices = true;
			ShowPHPStackTraces = true;

			ReLoad();
		}

		public string FullLog
		{
			get { return _rawLog; }
		}

		public void ReLoad()
		{
			_rawLog = null;

			// Open the file into a StreamReader
			using(var sr = File.OpenText(_logFileName))
			{
				_rawLog = sr.ReadToEnd();
			}

			Parse();
		}

		private void Parse()
		{
			var linesArray = FullLog.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			Lines = new List<string>(linesArray);

			Entries = new List<LogEntry>();

			LogEntry fetchingStackTrace = null;

			foreach (var line in Lines)
			{
				if ((line != null) && (line.Trim() != String.Empty))
				{
					var entry = new LogEntry(line);

					if (fetchingStackTrace != null) 
					{
						// next stack trace line ?
						if ((entry.KindEnum == LogEntryEnum.Other) && (entry.Text.StartsWith ("PHP "))) 
						{
							var textWithoutPHP = entry.Text.Substring (4).Trim();
							if (textWithoutPHP.Contains (".")) 
							{
								var stackNumberAndStackText = textWithoutPHP.Split (new char[] {'.'},2);
								var stackNumberAsString = stackNumberAndStackText [0];
								int stackNumber;
								if (int.TryParse (stackNumberAsString, out stackNumber)) 
								{
									fetchingStackTrace.StackFrames.Add (entry.Text);
									continue;
								}
							}
						}
					}

					Entries.Add(entry);

					if (entry.KindEnum == LogEntryEnum.StackTrace) 
					{
						// next lines starting with "PHP " and number are stack trace
						fetchingStackTrace = entry;
					}
				}
			}
		}

		public override string ToString ()
		{
			var res = new StringBuilder();

			foreach (var entry in Entries)
			{
				if ( (entry.KindEnum == LogEntryEnum.Warning) && (!ShowPHPWarnings))
					// skipping warning
					continue;

				if ( (entry.KindEnum == LogEntryEnum.Notice) && (!ShowPHPNotices))
					// skipping notices
					continue;

				if ( (entry.KindEnum == LogEntryEnum.StackTrace) && (!ShowPHPStackTraces))
					// skipping stack traces
					continue;

				res.Append(entry.ToString(ShowDate,ShowCategory,ShowIP,ShowPID));
			}

			return res.ToString();
		}
	}
}

