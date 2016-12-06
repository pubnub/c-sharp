using System;

namespace PubnubApi
{
    public abstract class PNCallback<T>
    {
        public abstract void OnResponse(T result, PNStatus status);
    }
}
