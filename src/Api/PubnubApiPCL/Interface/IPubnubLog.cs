
namespace PubnubApi
{
    public interface IPubnubLog
    {
        LoggingMethod.Level LogLevel
        {
            get;
            set;
        }

        void WriteToLog(string logText);
    }
}
