using System;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace PubnubApi
{
	#region "Logging and error codes -- code split required"

	#if (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IOS || UNITY_ANDROID)
	internal class LoggingMethod:MonoBehaviour
	#else
	public class LoggingMethod
	#endif
	{
		private static int logLevel = 0;
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
                #if (SILVERLIGHT || WINDOWS_PHONE || MONOTOUCH || __IOS__ || MONODROID || __ANDROID__ || NETFX_CORE)
                System.Diagnostics.Debug.WriteLine(logText);
				#elif (UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_IOS || UNITY_ANDROID)
				print(logText);
				UnityEngine.Debug.Log (logText);
				#else
				try
				{
					Trace.WriteLine(logText);
					Trace.Flush();
				}
				catch { }
				#endif
			}
		}
	}

	#endregion
}

