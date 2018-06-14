namespace PubnubApi
{
    public class PNException: System.Exception
    {
        public bool DirectException { get; }

        public PNException()
        {
        }

        public PNException(string message):base(message)
        {
        }

        public PNException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public PNException(System.Exception errorException) : base("", errorException)
        {
            DirectException = true;
        }
    }
}
