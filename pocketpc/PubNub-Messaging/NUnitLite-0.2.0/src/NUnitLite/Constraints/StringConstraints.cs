// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
#if !NETCF
using System.Text.RegularExpressions;
#endif

namespace NUnit.Framework.Constraints
{
    public abstract class StringConstraint : Constraint
    {
        protected string expected;

        protected abstract void WriteFailureMessageTo(MessageWriter writer);
        protected abstract bool IsMatch(string expected, string actual );

        /// <summary>
        /// Initializes a new instance of the <see cref="T:StringConstraint"/> class.
        /// </summary>
        /// <param name="expected">The expected.</param>
        public StringConstraint(string expected)
        {
            this.expected = expected;
        }

        public override bool Matches(object actual)
        {
            this.actual = actual;

            if ( !(actual is string) )
                return false;

            if (caseInsensitive)
                return IsMatch(expected.ToLower(), ((string)actual).ToLower());
            else
                return IsMatch(expected, (string)actual );
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            WriteFailureMessageTo(writer);
            writer.DisplayStringDifferences((string)expected, (string)actual, -1, caseInsensitive);
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            //WritePredicateTo(writer);
            writer.WriteExpectedValue(expected);
            //if (ignoreCase)
            //    writer.Write(" ignoring case");
        }
    }

    /// <summary>
    /// SubstringConstraint can test whether a string contains
    /// the expected substring.
    /// </summary>
    public class SubstringConstraint : StringConstraint
    {
        // Substring constraint failure messages
        public static readonly string msg_DoesNotContain =
            "String did not contain expected string.";
        public static readonly string msg_DoesNotContain_IC =
            "String did not contain expected string, ignoring case.";

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SubstringConstraint"/> class.
        /// </summary>
        /// <param name="expected">The expected.</param>
        public SubstringConstraint(string expected) : base(expected) { }

        protected override bool IsMatch(string expected, string actual)
        {
            return actual.IndexOf(expected) >= 0;
        }

        protected override void WriteFailureMessageTo(MessageWriter writer)
        {
            if (caseInsensitive)
                writer.WriteMessageLine(msg_DoesNotContain_IC);
            else
                writer.WriteMessageLine(msg_DoesNotContain);
        }
    }

    public class StartsWithConstraint : StringConstraint
    {
        // StartsWithConstraint failure messages
        public static readonly string msg_DoesNotStartWith =
            "String did not start with expected string.";
        public static readonly string msg_DoesNotStartWith_IC =
            "String did not start with expected string, ignoring case.";

        public StartsWithConstraint(string expected) : base(expected) { }

        protected override bool IsMatch(string expected, string actual)
        {
            return actual.StartsWith( expected );
        }

        protected override void WriteFailureMessageTo(MessageWriter writer)
        {
            if (caseInsensitive)
                writer.WriteMessageLine(msg_DoesNotStartWith_IC);
            else
                writer.WriteMessageLine(msg_DoesNotStartWith);
        }
    }

    public class EndsWithConstraint : StringConstraint
    {
        // EndsWithConstraint failure messges
        public static readonly string msg_DoesNotEndWith =
            "String did not end with expected string.";
        public static readonly string msg_DoesNotEndWith_IC =
            "String did not end with expected string, ignoring case.";

        public EndsWithConstraint(string expected) : base(expected) { }

        protected override bool IsMatch(string expected, string actual)
        {
            return actual.EndsWith(expected);
        }

        protected override void WriteFailureMessageTo(MessageWriter writer)
        {
            if (caseInsensitive)
                writer.WriteMessageLine(msg_DoesNotEndWith_IC);
            else
                writer.WriteMessageLine(msg_DoesNotEndWith);
        }
    }

#if !NETCF
    /// <summary>
    /// RegexConstraint can test whether a string matches
    /// the pattern provided.
    /// </summary>
    public class RegexConstraint : StringConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:RegexConstraint"/> class.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        public RegexConstraint(string expected) : base( expected ) { }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        protected override bool IsMatch(string expected, string actual)
        {
            return Regex.IsMatch(
                    (string)actual,
                    this.expected,
                    this.caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        protected override void WriteFailureMessageTo(MessageWriter writer)
        {
            writer.WriteMessageLine( "String does not match the pattern provided" );
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.WritePredicate("String matching");
            writer.WriteExpectedValue(this.expected);
            if (this.caseInsensitive)
                writer.WriteModifier("ignoring case");
        }
    }
#endif
}
