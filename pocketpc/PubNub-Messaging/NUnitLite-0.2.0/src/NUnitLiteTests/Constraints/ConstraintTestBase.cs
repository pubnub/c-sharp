// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnitLite.Tests
{
    public abstract class ConstraintTestBase : TestCase
    {
        protected Constraint Matcher;
        protected object[] GoodValues;
        protected object[] BadValues;
        protected string Description;

        public ConstraintTestBase(string name) : base(name) { }

        public void testSucceedsOnGoodValues()
        {
            foreach (object value in GoodValues)
                Assert.That(value, Matcher, "Test should succeed with {0}", value);
        }

        public void testFailsOnBadValues()
        {
            foreach (object value in BadValues)
            {
                Assert.That(Matcher.Matches(value), Is.False, "Test should fail with value {0}", value);
            }
        }

        public void testProvidesProperDescription()
        {
            TextMessageWriter writer = new TextMessageWriter();
            Matcher.WriteDescriptionTo(writer);
            Assert.That(writer.ToString(), Is.EqualTo(Description), null);
        }

        public void testProvidesProperFailureMessage()
        {
            TextMessageWriter writer = new TextMessageWriter();
            Matcher.Matches(BadValues[0]);
            Matcher.WriteMessageTo(writer);
            Assert.That(writer.ToString(), Is.StringContaining(Description));
            Assert.That(writer.ToString(), Is.Not.StringContaining("<UNSET>"));
        }
    }
}
