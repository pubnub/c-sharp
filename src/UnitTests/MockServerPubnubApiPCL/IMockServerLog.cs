namespace MockServer
{
    public interface IMockServerLog
    {
        LoggingMethod.Level LogLevel
        {
            get;
            set;
        }

        void WriteToLog(string logText);
    }
}
