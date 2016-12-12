using MockServer;
using System.Diagnostics;

namespace PubNubMessaging.Tests
{
    public class UnitTestLog : IMockServerLog
    {
        private LoggingMethod.Level _logLevel = LoggingMethod.Level.Info;
        private string logFilePath = "";

        public UnitTestLog()
        {
            // Get folder path may vary based on environment
            string folder = System.IO.Directory.GetCurrentDirectory();
            //For console
            //string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // For iOS
            System.Diagnostics.Debug.WriteLine(folder);
            logFilePath = System.IO.Path.Combine(folder, "pubnubUnitTestLog.log");
            Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
        }

        /// <summary>
        /// Set the Log Level
        /// </summary>
        public LoggingMethod.Level LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
            }
        }

        /// <summary>
        /// Write a log
        /// </summary>
        /// <param name="log">Log string</param>
        public void WriteToLog(string log)
        {
            Trace.WriteLine(log);
            Trace.Flush();
        }
    }
}
