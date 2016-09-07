using System;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace MockServer
{
	#region "Logging and error codes -- code split required"

	public class LoggingMethod
	{
		private static int logLevel = 0;
		private static IMockServerLog mockServerLog = null;

        /// <summary>
        /// Set the IMockServerLog class
        /// </summary>
		public static IMockServerLog MockServerLog
		{
			get
            {
				return mockServerLog;
			}

			set
            {
				mockServerLog = value;
				logLevel = (int)mockServerLog.LogLevel;
			}
		}

        /// <summary>
        /// Set the Log Level
        /// </summary>
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

        /// <summary>
        /// Level Enum
        /// </summary>
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

        /// <summary>
        /// Write the Log
        /// </summary>
        /// <param name="logText">Log to write</param>
        /// <param name="writeToLog">True or False</param>
        public static void WriteToLog(string logText, bool writeToLog)
		{
			if (writeToLog)
            {
				//System.Diagnostics.Debug.WriteLine(logText);
				try
				{
					if (mockServerLog != null)
					{
						mockServerLog.WriteToLog(logText);
					}
				}
				catch(Exception ex)
                {
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}
		}
	}
	#endregion
}
