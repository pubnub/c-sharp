
using System.Net;

namespace PubnubApi
{
    public interface IPubnubUnitTest
    {
        bool EnableStubTest
        {
            get;
            set;
        }

        string TestClassName
        {
            get;
            set;
        }

        string TestCaseName
        {
            get;
            set;
        }

        string GetStubResponse(HttpWebRequest request);
    }
}
