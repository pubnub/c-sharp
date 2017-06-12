using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PubnubApi
{
    internal class PNPlatform
    {
        public static void Print(PNConfiguration config, IPubnubLog log)
        {
#if NETSTANDARD10
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NETSTANDARD10", DateTime.Now.ToString()), config.LogVerbosity);
#elif NETSTANDARD11
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NETSTANDARD11", DateTime.Now.ToString()), config.LogVerbosity);
#elif NETSTANDARD12
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NETSTANDARD12", DateTime.Now.ToString()), config.LogVerbosity);
#elif NETSTANDARD13
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NETSTANDARD13", DateTime.Now.ToString()), config.LogVerbosity);
#elif NETSTANDARD14
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NETSTANDARD14", DateTime.Now.ToString()), config.LogVerbosity);
#elif UAP
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = UAP", DateTime.Now.ToString()), config.LogVerbosity);
#elif NETFX_CORE
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NETFX_CORE", DateTime.Now.ToString()), config.LogVerbosity);
#elif WINDOWS_UWP
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = WINDOWS_UWP", DateTime.Now.ToString()), config.LogVerbosity);
#elif PORTABLE111
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = PORTABLE111", DateTime.Now.ToString()), config.LogVerbosity);
#elif NET35
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NET35", DateTime.Now.ToString()), config.LogVerbosity);
#elif NET40
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NET40", DateTime.Now.ToString()), config.LogVerbosity);
#elif NET45
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NET45", DateTime.Now.ToString()), config.LogVerbosity);
#elif NET461
            LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = NET461", DateTime.Now.ToString()), config.LogVerbosity);
            #else
                        LoggingMethod.WriteToLog(log, string.Format("DateTime {0} PLATFORM = UNKNOWN", DateTime.Now.ToString()), config.LogVerbosity);
            #endif
        }
    }
}
