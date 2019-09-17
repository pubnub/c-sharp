using System;

namespace PubnubApi.CBOR.Tags
{
    class UriTag : ItemTag
    {
        public static ulong[] TAG_NUM = new ulong[] { 32 };
        public UriTag(ulong tagNum)
        {
            this.tagNumber = tagNum;
        }
        public override object processData(object data)
        {
            return new Uri((data as string));
        }

        public override bool isDataSupported(object data)
        {
            return (data is string);
        }
    }
}
