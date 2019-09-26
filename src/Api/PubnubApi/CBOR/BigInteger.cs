using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NET35 || NETSTANDARD10
namespace System.Numerics
{
    //Reference: https://github.com/mono/mono/blob/master/mcs/class/Mono.Security/Mono.Math/BigInteger.cs
    public class BigInteger
    {
        uint length = 1;

        uint[] data;
        const uint DEFAULT_LEN = 20;

        public enum Sign : int
        {
            Negative = -1,
            Zero = 0,
            Positive = 1
        };

        const string WouldReturnNegVal = "Operation would return a negative value";

        public BigInteger()
        {
            data = new uint[DEFAULT_LEN];
            this.length = DEFAULT_LEN;
        }

        public BigInteger(Sign sign, uint len)
        {
            this.data = new uint[len];
            this.length = len;
        }

        public BigInteger(BigInteger bi)
        {
            this.data = (uint[])bi.data.Clone();
            this.length = bi.length;
        }

        public BigInteger(BigInteger bi, uint len)
        {

            this.data = new uint[len];

            for (uint i = 0; i < bi.length; i++)
                this.data[i] = bi.data[i];

            this.length = bi.length;
        }

        public BigInteger(byte[] inData)
        {
            if (inData.Length == 0)
                inData = new byte[1];
            length = (uint)inData.Length >> 2;
            int leftOver = inData.Length & 0x3;

            // length not multiples of 4
            if (leftOver != 0) length++;

            data = new uint[length];

            for (int i = inData.Length - 1, j = 0; i >= 3; i -= 4, j++)
            {
                data[j] = (uint)(
                    (inData[i - 3] << (3 * 8)) |
                    (inData[i - 2] << (2 * 8)) |
                    (inData[i - 1] << (1 * 8)) |
                    (inData[i])
                    );
            }

            switch (leftOver)
            {
                case 1: data[length - 1] = (uint)inData[0]; break;
                case 2: data[length - 1] = (uint)((inData[0] << 8) | inData[1]); break;
                case 3: data[length - 1] = (uint)((inData[0] << 16) | (inData[1] << 8) | inData[2]); break;
            }

            this.Normalize();
        }

        public BigInteger(uint[] inData)
        {
            if (inData.Length == 0)
                inData = new uint[1];
            length = (uint)inData.Length;

            data = new uint[length];

            for (int i = (int)length - 1, j = 0; i >= 0; i--, j++)
                data[j] = inData[i];

            this.Normalize();
        }

        public BigInteger(uint ui)
        {
            data = new uint[] { ui };
        }

        public BigInteger(ulong ul)
        {
            data = new uint[2] { (uint)ul, (uint)(ul >> 32) };
            length = 2;

            this.Normalize();
        }

        public static implicit operator BigInteger(uint value)
        {
            return (new BigInteger(value));
        }

        public static implicit operator BigInteger(int value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException("value");
            return (new BigInteger((uint)value));
        }

        public static implicit operator BigInteger(ulong value)
        {
            return (new BigInteger(value));
        }

        public static BigInteger Subtract(BigInteger bi1, BigInteger bi2)
        {
            return (bi1 - bi2);
        }

        public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
        {
            if (bi2 == 0)
                return new BigInteger(bi1);

            if (bi1 == 0)
                throw new ArithmeticException(WouldReturnNegVal);

            switch (Kernel.Compare(bi1, bi2))
            {

                case Sign.Zero:
                    return 0;

                case Sign.Positive:
                    return Kernel.Subtract(bi1, bi2);

                case Sign.Negative:
                    throw new ArithmeticException(WouldReturnNegVal);
                default:
                    throw new Exception();
            }
        }

        public static bool operator ==(BigInteger bi1, uint ui)
        {
            if (bi1.length != 1) bi1.Normalize();
            return bi1.length == 1 && bi1.data[0] == ui;
        }

        public static bool operator ==(BigInteger bi1, BigInteger bi2)
        {
            // we need to compare with null
            if ((bi1 as object) == (bi2 as object))
                return true;
            if (null == bi1 || null == bi2)
                return false;
            return Kernel.Compare(bi1, bi2) == 0;
        }

        public static bool operator !=(BigInteger bi1, uint ui)
        {
            if (bi1.length != 1) bi1.Normalize();
            return !(bi1.length == 1 && bi1.data[0] == ui);
        }

        public static bool operator !=(BigInteger bi1, BigInteger bi2)
        {
            // we need to compare with null
            if ((bi1 as object) == (bi2 as object))
                return false;
            if (null == bi1 || null == bi2)
                return true;
            return Kernel.Compare(bi1, bi2) != 0;
        }

        private void Normalize()
        {
            // Normalize length
            while (length > 0 && data[length - 1] == 0) length--;

            // Check for zero
            if (length == 0)
                length++;
        }

        public string ToString(uint radix)
        {
            return ToString(radix, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        public string ToString(uint radix, string characterSet)
        {
            if (characterSet.Length < radix)
                throw new ArgumentException("charSet length less than radix", "characterSet");
            if (radix == 1)
                throw new ArgumentException("There is no such thing as radix one notation", "radix");

            if (this == 0) return "0";
            if (this == 1) return "1";

            string result = "";

            BigInteger a = new BigInteger(this);

            while (a != 0)
            {
                uint rem = Kernel.SingleByteDivideInPlace(a, radix);
                result = characterSet[(int)rem] + result;
            }

            return result;
        }

        private sealed class Kernel
        {
            public static BigInteger Subtract(BigInteger big, BigInteger small)
            {
                BigInteger result = new BigInteger(Sign.Positive, big.length);

                uint[] r = result.data, b = big.data, s = small.data;
                uint i = 0, c = 0;

                do
                {

                    uint x = s[i];
                    if (((x += c) < c) | ((r[i] = b[i] - x) > ~x))
                        c = 1;
                    else
                        c = 0;

                } while (++i < small.length);

                if (i == big.length) goto fixup;

                if (c == 1)
                {
                    do
                        r[i] = b[i] - 1;
                    while (b[i++] == 0 && i < big.length);

                    if (i == big.length) goto fixup;
                }

                do
                    r[i] = b[i];
                while (++i < big.length);

                fixup:

                result.Normalize();
                return result;
            }

            public static Sign Compare(BigInteger bi1, BigInteger bi2)
            {
                //
                // Step 1. Compare the lengths
                //
                uint l1 = bi1.length, l2 = bi2.length;

                while (l1 > 0 && bi1.data[l1 - 1] == 0) l1--;
                while (l2 > 0 && bi2.data[l2 - 1] == 0) l2--;

                if (l1 == 0 && l2 == 0) return Sign.Zero;

                // bi1 len < bi2 len
                if (l1 < l2) return Sign.Negative;
                // bi1 len > bi2 len
                else if (l1 > l2) return Sign.Positive;

                //
                // Step 2. Compare the bits
                //

                uint pos = l1 - 1;

                while (pos != 0 && bi1.data[pos] == bi2.data[pos]) pos--;

                if (bi1.data[pos] < bi2.data[pos])
                    return Sign.Negative;
                else if (bi1.data[pos] > bi2.data[pos])
                    return Sign.Positive;
                else
                    return Sign.Zero;
            }

            public static uint SingleByteDivideInPlace(BigInteger n, uint d)
            {
                ulong r = 0;
                uint i = n.length;

                while (i-- > 0)
                {
                    r <<= 32;
                    r |= n.data[i];
                    n.data[i] = (uint)(r / d);
                    r %= d;
                }
                n.Normalize();

                return (uint)r;
            }
        }

        #region Object Impl

        public override int GetHashCode()
        {
            uint val = 0;

            for (uint i = 0; i < this.length; i++)
                val ^= this.data[i];

            return (int)val;
        }

        public override string ToString()
        {
            return ToString(10);
        }

        public override bool Equals(object o)
        {
            if (o == null)
                return false;
            if (o is int)
                return (int)o >= 0 && this == (uint)o;

            BigInteger bi = o as BigInteger;
            if (bi == null)
                return false;

            return Kernel.Compare(this, bi) == 0;
        }

        #endregion
    }
}
#endif