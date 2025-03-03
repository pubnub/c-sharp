namespace PubnubApi;

public interface IPubnubLogger
{
    void Trace(string logMessage);
    void Debug(string logMessage);
    void Info(string logMessage);
    void Warn(string logMessage);
    void Error(string logMessage);
}

public enum PubnubLogLevel {Trace, Debug, Info, Warn, Error, None };