namespace PubnubApi
{
    public class PubnubProxy
    {
        string proxyServer;
        int proxyPort;
        string proxyUserName;
        string proxyPassword;

        public string ProxyServer
        {
            get
            {
                return proxyServer;
            }
            set
            {
                proxyServer = value;
            }
        }

        public int ProxyPort
        {
            get
            {
                return proxyPort;
            }
            set
            {
                proxyPort = value;
            }
        }

        public string ProxyUserName
        {
            get
            {
                return proxyUserName;
            }
            set
            {
                proxyUserName = value;
            }
        }

        public string ProxyPassword
        {
            get
            {
                return proxyPassword;
            }
            set
            {
                proxyPassword = value;
            }
        }
    }
}
