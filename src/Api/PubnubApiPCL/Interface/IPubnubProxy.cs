using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public interface IPubnubProxy
    {
        string Server
        {
            get;
            set;
        }

        int Port
        {
            get;
            set;
        }

        string UserName
        {
            get;
            set;
        }

        string Password
        {
            get;
            set;
        }
    }
}
