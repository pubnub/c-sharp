using System;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace PubnubApi
{
	#region "Logging and error codes -- code split required"

	public class LoggingMethod
	{
		private static int logLevel = 0;
		private static IPubnubLog pubnubLog = null;

		public static IPubnubLog PubnubLog
		{
			get{
				return pubnubLog;
			}
			set {
				pubnubLog = value;
				logLevel = (int)pubnubLog.LogLevel;

			}
		}
		public static Level LogLevel
		{
			get
			{
				return (Level)logLevel;
			}
			set
			{
				logLevel = (int)value;
			}
		}
		public enum Level
		{
			Off,
			Error,
			Info,
			Verbose,
			Warning
		}

		public static bool LevelError
		{
			get
			{
				return (int)LogLevel >= 1;
			}
		}

		public static bool LevelInfo
		{
			get
			{
				return (int)LogLevel >= 2;
			}
		}

		public static bool LevelVerbose
		{
			get
			{
				return (int)LogLevel >= 3;
			}
		}

		public static bool LevelWarning
		{
			get
			{
				return (int)LogLevel >= 4;
			}
		}

		public static void WriteToLog(string logText, bool writeToLog)
		{
			if (writeToLog)
            {
				//System.Diagnostics.Debug.WriteLine(logText);
				try
				{
					if (pubnubLog != null)
					{
						pubnubLog.WriteToLog(logText);
					}
				}
				catch(Exception ex) {
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}
		}
	}
	#endregion
}

