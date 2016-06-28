
namespace PubnubApi
{
    public class PubnubErrorFilter
    {

        private static int errorLevel = 0;
        public static Level ErrorLevel
        {
            get
            {
                return (Level)errorLevel;
            }
            set
            {
                errorLevel = (int)value;
            }
        }

        public enum Level
        {
            Critical = 1,
            Warning = 2,
            Info = 3
        }

        public static bool Critical
        {
            get
            {
                return (int)errorLevel >= 1;
            }
        }

        public static bool Warn
        {
            get
            {
                return (int)errorLevel >= 2;
            }
        }

        public static bool Info
        {
            get
            {
                return (int)errorLevel >= 3;
            }
        }
    }
}
