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
        if (Loggers == null || Loggers.Count == 0) return;
        
        if (logLevel >= MinLogLevel)
        {
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
    }
    
    public void Debug(string logMessage) => Log(PubnubLogLevel.Debug, logMessage);
    public void Trace(string logMessage) => Log(PubnubLogLevel.Trace, logMessage);
    public void Error(string logMessage) => Log(PubnubLogLevel.Error, logMessage);
    public void Info(string logMessage) => Log(PubnubLogLevel.Info, logMessage);
    public void Warn(string logMessage) => Log(PubnubLogLevel.Warn, logMessage);
}