using System;
using System.Text;
using System.Collections.Generic;

namespace ApacheLogViewer
{
	public enum LogEntryEnum
	{
		Other = 0,
		Notice = 1,
		StackTrace = 2,
		Warning = 3,
		ParseError = 4
	}

	public class LogEntry
	{	
		public string Date;
		public string Category;
		public int PID;
		public string IP;

		public string Kind;
		public LogEntryEnum KindEnum = LogEntryEnum.Other;
		public string Text;

		public List<string> StackFrames { get; set; }

		public LogEntry ()
		{
			Clear ();
		}

		public static LogEntryEnum ParseLogEntry(string value)
		{
			var result = LogEntryEnum.Other;

			if (string.IsNullOrEmpty (value))
				return result;

			value = value.Trim ();

			switch (value) 
			{
			case "PHP Notice":
				result = LogEntryEnum.Notice;
				break;
			case "PHP Warning":
				result = LogEntryEnum.Notice;
				break;
			case "PHP Stack trace":
				result = LogEntryEnum.StackTrace;
				break;
			case "PHP Parse error":
				result = LogEntryEnum.ParseError;
				break;
			}

			return result;
		}

		public void Clear()
		{
			Date = "";
			Category = "";
			PID = 0;
			IP = "";
			Kind = "";
			KindEnum = LogEntryEnum.Other;
			Text = "";
			StackFrames = new List<string> ();
		}

		public LogEntry (string line)
		{
			ParseFromString(line);
		}

		public string ToString (bool ShowDate,bool ShowCategory,bool ShowPID,bool ShowIP)
		{
			var res = new StringBuilder();

			if (ShowDate)
			{ 
				res.Append("[");
				res.Append(Date);
				res.Append("] ");
			}
			if (ShowCategory)
			{ 
				res.Append("[");
				res.Append(Category);
				res.Append("] ");
			}
			if (ShowPID)
			{ 
				res.Append("[");
				res.Append(PID.ToString());
				res.Append("] ");
			}
			if (ShowIP)
			{ 
				res.Append("[");
				res.Append(IP);
				res.Append("] ");
			}

			var logLineWOText = res.ToString ();

			res.Append(Text);

			res.Append(Environment.NewLine);

			if (KindEnum == LogEntryEnum.StackTrace) 
			{
				foreach (var line in StackFrames) 
				{
					res.Append(logLineWOText + line);
					res.Append(Environment.NewLine);
				}
			}

			return res.ToString ();
		}

		public void ParseFromString(string line)
		{
			Clear ();

			if (String.IsNullOrEmpty(line))
			return;

			var part = line.Trim();
			var done = false;

			var bracketElementPosition = 0;

			while (!done)
			{
				var brStartPos = part.IndexOf('[');

				if (brStartPos == 0)
				{						
					// [ at first position
					var brEndPos = part.IndexOf(']');
					if (brEndPos > 0)
					{
						// ] exists
						var element = part.Substring(1,brEndPos-1);


						// position 0 .. always date
						// position 1 .. always category
						// pid:PID. 
						// client IP

						if (bracketElementPosition == 0)
						{
							// date like "Sat Jun 07 21:47:25.214357 2014"
							Date = element;
						} else
						if (bracketElementPosition == 1)
						{
							Category = element;
						} else
						{
							if (element.StartsWith("pid "))
							{
								var elementPID = element.Substring(4);
								PID = Convert.ToInt32(elementPID);
							} else
							if (element.StartsWith("client "))
							{
								IP = element;
							} else
							{
								// unknown element
								// ] does not exist 
								Text = part.Trim();
								done = true;
							}
						}						

						bracketElementPosition++;
						part = part.Substring(brEndPos+1).Trim();
					} else
					{
						// ] does not exist 
						Text = part.Trim();
						done = true;
					}
				} else
				{
					// element without [ on beginning 

					var colonPos  = part.IndexOf(":");
					if (colonPos > 0)
					{
						Kind = part.Substring(0,colonPos);
						KindEnum = LogEntry.ParseLogEntry (Kind);
						//Text = part.Substring(colonPos+1).Trim();
						Text = part.Trim();
						done = true;
					} else
					{
						// no Kind found!

						Text = part.Trim();
						done = true;
					}
				}
			}
		}
	}
}

