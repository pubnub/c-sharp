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
		public static void WriteToLog(IPubnubLog pubnubLog, string logText, PNLogVerbosity logVerbosity)
		{
			if (pubnubLog != null && logVerbosity == PNLogVerbosity.BODY)
            {
				//System.Diagnostics.Debug.WriteLine(logText);
				try
				{
                    pubnubLog.WriteToLog(logText);
                }
                catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}
		}
	}
	#endregion
}

