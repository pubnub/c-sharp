using System;

namespace PubnubApi
{
    //public class PNCallback<T>
    //{
    //    public Action<T> Result { get; set; }
    //    public Action<PubnubClientError> Error { get; set; }
    //}
    public abstract class PNCallback<T>
    {
        public abstract void OnResponse(T result, PNStatus status);
    }
}
