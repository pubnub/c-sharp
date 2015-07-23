// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// EqualConstraint is able to compare an actual value with the
    /// expected value provided in its constructor.
    /// </summary>
    public class EqualConstraint : Constraint
    {
        private static IDictionary constraintHelpers = new Hashtable();

        private object expected;

        private ArrayList failurePoints;

        private static readonly string StringsDiffer_1 =
            "String lengths are both {0}. Strings differ at index {1}.";
        private static readonly string StringsDiffer_2 =
            "Expected string length {0} but was {1}. Strings differ at index {2}.";
        private static readonly string StreamsDiffer_1 =
            "Stream lengths are both {0}. Streams differ at offset {1}.";
        private static readonly string StreamsDiffer_2 =
            "Expected Stream length {0} but was {1}.";// Streams differ at offset {2}.";
        private static readonly string CollectionType_1 =
            "Expected and actual are both <{0}>";
        private static readonly string CollectionType_2 =
            "Expected is <{0}>, actual is <{1}>";
        private static readonly string ValuesDiffer_1 =
            "Values differ at index {0}";
        private static readonly string ValuesDiffer_2 =
            "Values differ at expected index {0}, actual index {1}";

        private static readonly int BUFFER_SIZE = 4096;

        #region Static Method to Add Equality Helpers - Experimental
        public static void SetConstraintForType(Type argumentType, Type constraintType)
        {
            constraintHelpers[argumentType] = constraintType;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EqualConstraint"/> class.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        public EqualConstraint(object expected)
        {
            this.expected = expected;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;
            this.failurePoints = new ArrayList();

            return ObjectsEqual( expected, actual );
        }

        /// <summary>
        /// Write a failure message. Overridden to provide custom 
        /// failure messages for EqualConstraint.
        /// </summary>
        /// <param name="writer">The MessageWriter to write to</param>
        public override void WriteMessageTo(MessageWriter writer)
        {
            DisplayDifferences(writer, expected, actual, 0);
        }


        /// <summary>
        /// Write description of this constraint
        /// </summary>
        /// <param name="writer">The MessageWriter to write to</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WriteExpectedValue( expected );
        }

        private void DisplayDifferences(MessageWriter writer, object expected, object actual, int depth)
        {
            if (expected is string && actual is string)
                DisplayStringDifferences(writer, (string)expected, (string)actual);
            else if (expected is ICollection && actual is ICollection)
                DisplayCollectionDifferences(writer, (ICollection)expected, (ICollection)actual, depth);
			else if (expected is Stream && actual is Stream)
				DisplayStreamDifferences(writer, (Stream)expected, (Stream)actual, depth);
			else if ( tolerance != null )
				writer.DisplayDifferences( expected, actual, tolerance );
            else
                writer.DisplayDifferences(expected, actual);
        }
        #endregion

        #region ObjectsEqual
        private bool ObjectsEqual(object expected, object actual)
        {
            if (expected == null && actual == null)
                return true;

            if (expected == null || actual == null)
                return false;

            Type expectedType = expected.GetType();
            Type actualType = actual.GetType();

            if (expectedType.IsArray && actualType.IsArray && !compareAsCollection)
                return ArraysEqual((Array)expected, (Array)actual);

            if (expected is ICollection && actual is ICollection)
                return CollectionsEqual((ICollection)expected, (ICollection)actual);

            if (expected is Stream && actual is Stream)
                return StreamsEqual((Stream)expected, (Stream)actual);

            if (compareWith != null)
                return compareWith.Compare(expected, actual) == 0;

            if (Numerics.IsNumericType(expected) && Numerics.IsNumericType(actual))
            {
                return Numerics.AreEqual(expected, actual, tolerance);
            }

            if (expected is string && actual is string)
            {
                return string.Compare((string)expected, (string)actual, caseInsensitive) == 0;
            }

            foreach (Type type in constraintHelpers.Keys)
            {
                if (type.IsInstanceOfType(expected) && type.IsInstanceOfType(actual))
                {
                    Type constraintType = (Type)constraintHelpers[type];
                    Constraint constraint = (Constraint)NUnitLite.Reflect.Construct(constraintType, expected);
                    return constraint.Matches(actual);
                }
            }

            return expected.Equals(actual);
        }

        /// <summary>
        /// Helper method to compare two arrays
        /// </summary>
        protected virtual bool ArraysEqual(Array expected, Array actual)
        {
            int rank = expected.Rank;

            if (rank != actual.Rank)
                return false;

            for (int r = 1; r < rank; r++)
                if (expected.GetLength(r) != actual.GetLength(r))
                    return false;

            return CollectionsEqual((ICollection)expected, (ICollection)actual);
        }

        private bool CollectionsEqual(ICollection expected, ICollection actual)
        {
            IEnumerator expectedEnum = expected.GetEnumerator();
            IEnumerator actualEnum = actual.GetEnumerator();

            int count;
            for (count = 0; expectedEnum.MoveNext() && actualEnum.MoveNext(); count++)
            {
                if (!ObjectsEqual(expectedEnum.Current, actualEnum.Current))
                    break;
            }

            if (count == expected.Count && count == actual.Count)
                return true;

            failurePoints.Insert(0, count);
            return false;
        }

        private bool StreamsEqual(Stream expected, Stream actual)
        {
            if (expected.Length != actual.Length) return false;

            byte[] bufferExpected = new byte[BUFFER_SIZE];
            byte[] bufferActual = new byte[BUFFER_SIZE];

            BinaryReader binaryReaderExpected = new BinaryReader(expected);
            BinaryReader binaryReaderActual = new BinaryReader(actual);

            binaryReaderExpected.BaseStream.Seek(0, SeekOrigin.Begin);
            binaryReaderActual.BaseStream.Seek(0, SeekOrigin.Begin);

            for (long readByte = 0; readByte < expected.Length; readByte += BUFFER_SIZE)
            {
                binaryReaderExpected.Read(bufferExpected, 0, BUFFER_SIZE);
                binaryReaderActual.Read(bufferActual, 0, BUFFER_SIZE);

                for (int count = 0; count < BUFFER_SIZE; ++count)
                {
                    if (bufferExpected[count] != bufferActual[count])
                    {
                        failurePoints.Insert(0, readByte + count);
                        //FailureMessage.WriteLine("\tIndex : {0}", readByte + count);
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region DisplayStringDifferences
        private void DisplayStringDifferences(MessageWriter writer, string expected, string actual)
        {
            int mismatch = MsgUtils.FindMismatchPosition(expected, actual, 0, this.caseInsensitive);

            if (expected.Length == actual.Length)
                writer.WriteMessageLine(StringsDiffer_1, expected.Length, mismatch);
            else
                writer.WriteMessageLine(StringsDiffer_2, expected.Length, actual.Length, mismatch);

            writer.DisplayStringDifferences(expected, actual, mismatch, caseInsensitive);
        }
        #endregion

        #region DisplayStreamDifferences
        private void DisplayStreamDifferences(MessageWriter writer, Stream expected, Stream actual, int depth)
        {
            if (expected.Length == actual.Length)
            {
                long offset = (long)failurePoints[depth];
                writer.WriteMessageLine(StreamsDiffer_1, expected.Length, offset);
            }
            else
                writer.WriteMessageLine(StreamsDiffer_2, expected.Length, actual.Length);
        }
        #endregion

        #region DisplayCollectionDifferences
        /// <summary>
        /// Display the failure information for two collections that did not match.
        /// </summary>
		/// <param name="writer">The MessageWriter on which to display</param>
        /// <param name="expected">The expected collection.</param>
        /// <param name="actual">The actual collection</param>
        /// <param name="depth">The depth of this failure in a set of nested collections</param>
	private void DisplayCollectionDifferences(MessageWriter writer, ICollection expected, ICollection actual, int depth)
        {
            int failurePoint = failurePoints.Count > depth ? (int)failurePoints[depth] : -1;

            DisplayCollectionTypesAndSizes(writer, expected, actual, depth);

            if (failurePoint >= 0)
            {
                DisplayFailurePoint(writer, expected, actual, failurePoint, depth);
                if (failurePoint < expected.Count && failurePoint < actual.Count)
                    DisplayDifferences(
                        writer,
                        GetValueFromCollection(expected, failurePoint),
                        GetValueFromCollection(actual, failurePoint),
                        ++depth);
                else if (expected.Count < actual.Count)
                    DisplayExtraElements(writer, actual, failurePoint, 3);
                else
                    DisplayMissingElements(writer, expected, failurePoint, 3);
            }
        }

        /// <summary>
        /// Displays a single line showing the types and sizes of the expected
        /// and actual collections or arrays. If both are identical, the value is 
        /// only shown once.
        /// </summary>
        /// <param name="writer">The MessageWriter on which to display</param>
        /// <param name="expected">The expected collection or array</param>
        /// <param name="actual">The actual collection or array</param>
        /// <param name="indent">The indentation level for the message line</param>
        private void DisplayCollectionTypesAndSizes(MessageWriter writer, ICollection expected, ICollection actual, int indent)
        {
            string expectedType = MsgUtils.GetTypeRepresentation(expected);
            string actualType = MsgUtils.GetTypeRepresentation(actual);

            if (expectedType == actualType)
                writer.WriteMessageLine(indent, CollectionType_1, expectedType);
            else
                writer.WriteMessageLine(indent, CollectionType_2, expectedType, actualType);
        }

        /// <summary>
        /// Displays a single line showing the point in the expected and actual
        /// arrays at which the comparison failed. If the arrays have different
        /// structures or dimensions, both values are shown.
        /// </summary>
        /// <param name="writer">The MessageWriter on which to display</param>
        /// <param name="expected">The expected array</param>
        /// <param name="actual">The actual array</param>
        /// <param name="failurePoint">Index of the failure point in the underlying collections</param>
        /// <param name="indent">The indentation level for the message line</param>
        private void DisplayFailurePoint(MessageWriter writer, ICollection expected, ICollection actual, int failurePoint, int indent)
        {
            Array expectedArray = expected as Array;
            Array actualArray = actual as Array;

            int expectedRank = expectedArray != null ? expectedArray.Rank : 1;
            int actualRank = actualArray != null ? actualArray.Rank : 1;

            bool useOneIndex = expectedRank == actualRank;

            if (expectedArray != null && actualArray != null)
                for (int r = 1; r < expectedRank && useOneIndex; r++)
                    if (expectedArray.GetLength(r) != actualArray.GetLength(r))
                        useOneIndex = false;

            int[] expectedIndices = MsgUtils.GetArrayIndicesFromCollectionIndex(expected, failurePoint);
            if (useOneIndex)
            {
                writer.WriteMessageLine(indent, ValuesDiffer_1, MsgUtils.GetArrayIndicesAsString(expectedIndices));
            }
            else
            {
                int[] actualIndices = MsgUtils.GetArrayIndicesFromCollectionIndex(actual, failurePoint);
                writer.WriteMessageLine(indent, ValuesDiffer_2,
                    MsgUtils.GetArrayIndicesAsString(expectedIndices), MsgUtils.GetArrayIndicesAsString(actualIndices));
            }
        }

        private void DisplayMissingElements(MessageWriter writer, ICollection expected, int failurePoint, int max)
        {
            // TODO: We should not really assume that we are using a TextMessageWriter
            DisplayElements(writer, TextMessageWriter.Pfx_Missing, expected, failurePoint, max);
        }

        private void DisplayExtraElements(MessageWriter writer, ICollection actual, int failurePoint, int max)
        {
            // TODO: We should not really assume that we are using a TextMessageWriter
            DisplayElements(writer, TextMessageWriter.Pfx_Extra, actual, failurePoint, max);
        }

        /// <summary>
        /// Displays elements from a collection on a line
        /// </summary>
        /// <param name="label">Text to prefix the line with</param>
        /// <param name="list">The list of items to display</param>
        /// <param name="index">The index in the collection of the first element to display</param>
        /// <param name="max">The maximum number of elements to display</param>
        private void DisplayElements(MessageWriter writer, string prefix, ICollection collection, int index, int max)
        {
            writer.Write(prefix + "< ");

            if (collection == null)
                writer.Write("null");
            else if (collection.Count == 0)
                writer.Write("empty");
            else
            {
                for (int i = 0; i < max && index < collection.Count; i++)
                {
                    if (i > 0) writer.Write(", ");

                    writer.WriteValue(GetValueFromCollection(collection, index++));
                }

                if (index < collection.Count)
                    writer.Write("...");
            }

            writer.Write(" >");
        }

        private static object GetValueFromCollection(ICollection collection, int index)
        {
            Array array = collection as Array;

            if (array != null && array.Rank > 1)
                return array.GetValue(MsgUtils.GetArrayIndicesFromCollectionIndex(array, index));

            if (collection is IList)
                return ((IList)collection)[index];

            foreach (object obj in collection)
                if (--index < 0)
                    return obj;

            return null;
        }
        #endregion
    }
}
