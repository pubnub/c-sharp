using System;


namespace PubnubApi
{
    public sealed class HttpUtility
    {
        public static HttpValueCollection ParseQueryString(string query)
        {
            string queryResult = "";
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            if ((query.Length > 0) && (query[0] == '?'))
            {
                queryResult = query.Substring(1);
            }

            return new HttpValueCollection(queryResult, true);
        }
    }
}
