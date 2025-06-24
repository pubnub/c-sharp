// snippet.using
using PubnubApi;

// snippet.end
using System;
using System.Globalization;

public class LoggingSample
{
    // snippet.custom_logger
    // A custom logger that logs information on console.
    // Use can implement logger that can log information using log4Net or file etc.
    public class PubnubConsoleLogger : IPubnubLogger
    {
        public void Trace(string traceLog) =>
            Console.WriteLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] [TRACE] {traceLog}");

        public void Debug(string debugLog) =>
            Console.WriteLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] [DEBUG] {debugLog}");

        public void Info(string infoLog) =>
            Console.WriteLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] [INFO] {infoLog}");

        public void Warn(string warningLog) => 
            Console.WriteLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] [WARN] {warningLog}");

        public void Error(string errorLog) => 
            Console.WriteLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] [ERROR] {errorLog}");
    }
    // snippet.end

    public static void EnableLogging()
    {
        // snippet.enable_logging
        var pubnubConfiguration = new PNConfiguration(new UserId("uniqueUserId"))
        {
            SubscribeKey = "[yourSubscribeKey]",
            PublishKey = "[yourPublishKey]",
            LogLevel = PubnubLogLevel.Debug,
        };
        var pubnub = new Pubnub(pubnubConfiguration);

        var customLogger = new PubnubConsoleLogger();
        pubnub.SetLogger(customLogger);

        // To remove the custom logger. Use RemoveLogger().
        pubnub.RemoveLogger(customLogger);
        // snippet.end
    }
} 