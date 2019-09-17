using System;

namespace PubnubApi.CBOR
{
    public abstract class ItemTag
    {
        public ulong tagNumber;
        public abstract object processData(object data);
        public abstract bool isDataSupported(object data);
    }
}
