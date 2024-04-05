using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.Security.Crypto.Common
{
    internal static class ByteArrayExtensions
    {
        public static string ToDisplayFormat(this byte[] bytes)
        {
            if (bytes == null)
            {
                return string.Empty;
            }

            StringBuilder outBuilder = new StringBuilder("{ ");
            for (int index = 0; index < bytes.Length; index++)
            {
                outBuilder.Append(bytes[index]);
                if (index < bytes.Length - 1)
                {
                    outBuilder.Append(", ");
                }
            }
            outBuilder.Append(" }");
            return outBuilder.ToString();
        }
    }
}
