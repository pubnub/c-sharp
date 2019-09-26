using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.CBOR.Tags
{
    internal class Base64Tag : ItemTag
    {
        public static ulong[] TAG_NUM { get; set; } = new ulong[] { 33, 34 };

        public Base64Tag(ulong tagNum)
        {
            this.tagNumber = tagNum;
        }

        public override object processData(object data)
        {
            if (this.tagNumber == 33)
            {
                String s = (data as string);
                s = s.Replace("_", "/");
                s = s.Replace("-", "+");
                s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');

                byte[] decoded = System.Convert.FromBase64String(s);

                String decodedString = System.Text.Encoding.UTF8.GetString(decoded, 0, decoded.Length);

                return new Uri(decodedString);
            }
            else
            {
                byte[] decoded = System.Convert.FromBase64String((data as string));
                return decoded;
            }

        }

        public override bool isDataSupported(object data)
        {
            return (data is string);
        }
    }
}
