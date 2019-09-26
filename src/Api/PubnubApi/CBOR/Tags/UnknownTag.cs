using System;

namespace PubnubApi.CBOR.Tags
{
    public class UnknownTag : ItemTag
    {

        public UnknownTag(ulong tagId)
        {
            this.tagNumber = tagId;
        }

        public override object processData(object data)
        {
            return data;
        }

        public override bool isDataSupported(object data)
        {
            return true;
        }
    }
}
