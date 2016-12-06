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
		private static IPubnubLog pubnubLog = null;

		public static IPubnubLog PubnubLog
		{
			get{
				return pubnubLog;
			}
			set {
				pubnubLog = value;
			}
		}

		public static void WriteToLog(string logText, PNLogVerbosity logVerbosity)
		{
			if (logVerbosity == PNLogVerbosity.BODY)
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

