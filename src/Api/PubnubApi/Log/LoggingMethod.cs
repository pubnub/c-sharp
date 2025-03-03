﻿using System;

namespace PubnubApi
{
	#region "Logging and error codes -- code split required"

	public class LoggingMethod
	{
		public static void WriteToLog(IPubnubLog pubnubLog, string logText, PNLogVerbosity logVerbosity)
		{
			if (pubnubLog != null && logVerbosity == PNLogVerbosity.BODY)
            {
				try
				{
                    pubnubLog.WriteToLog(logText);
                }
                catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}
            else
            {
                System.Diagnostics.Debug.WriteLine(logText);
            }
		}
	}
	#endregion
}

