using System;

namespace PubnubApi
{
    public class PubnubProxy: IPubnubProxy
    {
        private string proxyServer = "";
        private int proxyPort;
        private string proxyUserName = "";
        private string proxyPassword = "";

        string IPubnubProxy.Server
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

        int IPubnubProxy.Port
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

        string IPubnubProxy.UserName
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

        string IPubnubProxy.Password
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
