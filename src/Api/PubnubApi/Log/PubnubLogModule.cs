using System;
using System.Collections.Generic;
using System.Globalization;

namespace PubnubApi;

public class PubnubLogModule
{
    public PubnubLogLevel MinLogLevel { get; set; }
    public List<IPubnubLogger> Loggers { get; set; }
    
    public PubnubLogModule(PubnubLogLevel logLevel, List<IPubnubLogger> loggers = null)
    {
        MinLogLevel = logLevel;
        Loggers = loggers ?? [];
    }

    public void AddLogger(IPubnubLogger logger)
    {
        Loggers.Add(logger);
    }

    public void RemoveLogger(IPubnubLogger logger)
    {
        Loggers.Remove(logger);
    }

    private void Log(PubnubLogLevel logLevel, string message)
    {
        if (!IsEnabled(logLevel)) return;
        try
        {
            foreach (var logger in Loggers)
            {
                switch (logLevel)
                {
                    case PubnubLogLevel.Info:
                        logger.Info(message);
                        break;
                    case PubnubLogLevel.Debug:
                        logger.Debug(message);
                        break;
                    case PubnubLogLevel.Error:
                        logger.Error(message);
                        break;
                    case PubnubLogLevel.Trace:
                        logger.Trace(message);
                        break;
                    case PubnubLogLevel.Warn:
                        logger.Warn(message);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} Error while logging {e.Message} \n {e.StackTrace}" );
        }
    }
    
    public bool IsEnabled(PubnubLogLevel logLevel) =>
        Loggers != null && Loggers.Count > 0 && logLevel >= MinLogLevel;

    // Lazy overload: the factory is only invoked when the level is enabled, so callers
    // can avoid building expensive messages (e.g. serializing large payloads) when logging
    // is filtered out. The factory runs inside a try so that a throwing message build
    // (e.g. a user object with a throwing ToString) can never crash the calling operation.
    private void Log(PubnubLogLevel logLevel, Func<string> messageFactory)
    {
        if (!IsEnabled(logLevel)) return;
        string message;
        try
        {
            message = messageFactory();
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} Error while building log message {e.Message} \n {e.StackTrace}");
            return;
        }
        Log(logLevel, message);
    }

    public void Debug(string logMessage) => Log(PubnubLogLevel.Debug, logMessage);
    public void Trace(string logMessage) => Log(PubnubLogLevel.Trace, logMessage);
    public void Error(string logMessage) => Log(PubnubLogLevel.Error, logMessage);
    public void Info(string logMessage) => Log(PubnubLogLevel.Info, logMessage);
    public void Warn(string logMessage) => Log(PubnubLogLevel.Warn, logMessage);

    public void Debug(Func<string> messageFactory) => Log(PubnubLogLevel.Debug, messageFactory);
    public void Trace(Func<string> messageFactory) => Log(PubnubLogLevel.Trace, messageFactory);
    public void Error(Func<string> messageFactory) => Log(PubnubLogLevel.Error, messageFactory);
    public void Info(Func<string> messageFactory) => Log(PubnubLogLevel.Info, messageFactory);
    public void Warn(Func<string> messageFactory) => Log(PubnubLogLevel.Warn, messageFactory);
}