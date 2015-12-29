// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Text;
using System.Collections;

namespace NUnit.Framework
{
    /// <summary>
    /// Static methods used in creating messages
    /// </summary>
    class MsgUtils
    {
        private static readonly string ELLIPSIS = "...";

        /// <summary>
        /// Returns the representation of a type as used in NUnitLite.
        /// This is the same as Type.ToString() except for arrays,
        /// which are displayed with their declared sizes.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetTypeRepresentation(object obj)
        {
            Array array = obj as Array;
            if ( array == null )
                return obj.GetType().ToString();

            StringBuilder sb = new StringBuilder();
            Type elementType = array.GetType();
            int nest = 0;
            while (elementType.IsArray)
            {
                elementType = elementType.GetElementType();
                ++nest;
            }
            sb.Append(elementType.ToString());
            sb.Append('[');
            for (int r = 0; r < array.Rank; r++)
            {
                if (r > 0) sb.Append(',');
                sb.Append(array.GetLength(r));
            }
            sb.Append(']');

            while (--nest > 0)
                sb.Append("[]");

            return sb.ToString();
        }

        /// <summary>
        /// Converts any control characters in a string 
        /// to their escaped representation.
        /// </summary>
        /// <param name="s">The string to be converted</param>
        /// <returns>The converted string</returns>
        public static string ConvertWhitespace(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        /// <summary>
        /// Return the a string representation for a set of indices into an array
        /// </summary>
        /// <param name="indices">Array of indices for which a string is needed</param>
        public static string GetArrayIndicesAsString(int[] indices)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            for (int r = 0; r < indices.Length; r++)
            {
                if (r > 0) sb.Append(',');
                sb.Append(indices[r].ToString());
            }
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Get an array of indices representing the point in a collection or
        /// array corresponding to a single int index into the collection.
        /// </summary>
        /// <param name="collection">The collection to which the indices apply</param>
        /// <param name="index">Index in the collection</param>
        /// <returns>Array of indices</returns>
        public static int[] GetArrayIndicesFromCollectionIndex(ICollection collection, int index)
        {
            Array array = collection as Array;
            int rank = array == null ? 1 : array.Rank;
            int[] result = new int[rank];

            for (int r = array.Rank; --r > 0; )
            {
                int l = array.GetLength(r);
                result[r] = index % l;
                index /= l;
            }

            result[0] = index;
            return result;
        }

        /// <summary>
        /// Clip a string around a particular point, returning the clipped
        /// string with ellipses representing the removed parts
        /// </summary>
        /// <param name="s">The string to be clipped</param>
        /// <param name="maxStringLength">The maximum permitted length of the result string</param>
        /// <param name="mismatch">The point around which clipping is to occur</param>
        /// <returns>The clipped string</returns>
        public static string ClipString(string s, int maxStringLength, int mismatch)
        {
            int clipLength = maxStringLength - ELLIPSIS.Length;

            if (mismatch >= clipLength)
            {
                int clipStart = mismatch - clipLength / 2;
                mismatch = mismatch - clipStart + ELLIPSIS.Length;

                // Clip the expected value at start and at end if needed
                if (s.Length - clipStart > maxStringLength)
                    return ELLIPSIS + s.Substring(
                        clipStart, clipLength - ELLIPSIS.Length) + ELLIPSIS;
                else
                    return ELLIPSIS + s.Substring(clipStart);
            }
            else if (s.Length > maxStringLength)
                return s.Substring(0, clipLength) + ELLIPSIS;
            else return s;
        }

        /// <summary>
        /// Shows the position two strings start to differ.  Comparison 
        /// starts at the start index.
        /// </summary>
        /// <param name="expected">The expected string</param>
        /// <param name="actual">The actual string</param>
        /// <param name="istart">The index in the strings at which comparison should start</param>
	/// <param name="ignoreCase">Boolean indicating whether case should be ignored</param>
        /// <returns>-1 if no mismatch found, or the index where mismatch found</returns>
        static public int FindMismatchPosition(string expected, string actual, int istart, bool ignoreCase)
        {
            int length = Math.Min(expected.Length, actual.Length);

            string s1 = ignoreCase ? expected.ToLower() : expected;
            string s2 = ignoreCase ? actual.ToLower() : actual;

            for (int i = istart; i < length; i++)
            {
                if (s1[i] != s2[i])
                    return i;
            }

            //
            // Strings have same content up to the length of the shorter string.
            // Mismatch occurs because string lengths are different, so show
            // that they start differing where the shortest string ends
            //
            if (expected.Length != actual.Length)
                return length;

            //
            // Same strings : We shouldn't get here
            //
            return -1;
        }
    }
}
