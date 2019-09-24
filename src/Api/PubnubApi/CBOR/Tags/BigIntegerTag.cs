using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PubnubApi.CBOR.Tags
{
    public class BigIntegerTag : ItemTag
    {
        public static ulong[] TAG_NUM { get; set; } = new ulong[] { 2, 3 };
        public BigIntegerTag(ulong tagId)
        {
            this.tagNumber = tagId;
        }

        public override object processData(object data)
        {
            Array.Reverse((Array)data);
            BigInteger bi = new BigInteger((byte[])data);

            if (this.tagNumber == 2)
            {
                return bi;
            }
            else
            {
#if NET35 || NETSTANDARD10
                return BigInteger.Subtract(-1, bi);
#else
                return BigInteger.Subtract(BigInteger.MinusOne, bi);
#endif
            }
        }

        public override bool isDataSupported(object data)
        {
            bool ret = false;
            try
            {
                if (((byte[])data).Length >= 0)
                {
                    ret = true;
                }
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }
}
