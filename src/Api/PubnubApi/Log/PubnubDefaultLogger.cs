using System;
using System.Globalization;

namespace PubnubApi;

public class PubnubDefaultLogger : IPubnubLogger
{
    public delegate void LogDelegate(string message);
    
    private LogDelegate logFunction;

    private IPubnubLog legacyLogger;
    private string Id { get; set; }

    public PubnubDefaultLogger(string id, LogDelegate logFunction, IPubnubLog legacyLogger = null)
    {
        Id = id;
        this.logFunction = logFunction;
        this.legacyLogger = legacyLogger;
    }
    public void Trace(string logMessage)
    {
        logFunction($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} PubNub-{Id} Trace {logMessage}");
    }

    public void Debug(string logMessage)
    {
        logFunction($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} PubNub-{Id} Debug {logMessage}");
        // Note: This code is temporary to keep backward compatibility till the legacy logger is removed.
        try
        {
            legacyLogger?.WriteToLog(logMessage);
        }
        catch (Exception e)
        {
            Error($"Legacy logger exception {e.Message} \n {e.StackTrace}");
        }
    }

    public void Info(string logMessage)
    {
        logFunction($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} PubNub-{Id} Info {logMessage}");
    }

    public void Warn(string logMessage)
    {
        logFunction($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} PubNub-{Id} Warn {logMessage}");
    }

    public void Error(string logMessage)
    {
        logFunction($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} PubNub-{Id} Error {logMessage}");
    }
}