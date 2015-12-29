// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnitLite.Tests
{
    #region UniqueItemsConstraint
    [TestFixture]
    public class UniqueItemsTests : ConstraintTestBase
    {
        public UniqueItemsTests(string name) : base(name) { }

        protected override void SetUp()
        {
            Matcher = new UniqueItemsConstraint();
            GoodValues = new object[] { new int[] { 1, 3, 17, -2, 34 }, new object[0] };
            BadValues = new object[] { new int[] { 1, 3, 17, 3, 34 } };
            Description = "all items unique";
        }
    }
    #endregion

    #region AllItemsConstraint
    [TestFixture]
    public class AllItemsTests : IExpectException
    {
        private string expectedMessage;

        [Test]
        public void AllItemsAreNotNull()
        {
            object[] c = new object[] { 1, "hello", 3, Environment.OSVersion };
            Assert.That(c, new AllItemsConstraint(Is.Not.Null));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void AllItemsAreNotNullFails()
        {
            object[] c = new object[] { 1, "hello", null, 3 };
            expectedMessage = TextMessageWriter.Pfx_Expected + "all items not null" + Env.NewLine +
                TextMessageWriter.Pfx_Actual + "< 1, \"hello\", null, 3 >" + Env.NewLine;
            Assert.That(c, new AllItemsConstraint(Is.Not.Null));
        }

        [Test]
        public void AllItemsAreInRange()
        {
            int[] c = new int[] { 12, 27, 19, 32, 45, 99, 26 };
            Assert.That(c, new AllItemsConstraint(Is.GreaterThan(10) & Is.LessThan(100)));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void AllItemsAreInRangeFailureMessage()
        {
            int[] c = new int[] { 12, 27, 19, 32, 107, 99, 26 };
            expectedMessage = 
                TextMessageWriter.Pfx_Expected + "all items greater than 10 and less than 100" + Env.NewLine +
                TextMessageWriter.Pfx_Actual   + "< 12, 27, 19, 32, 107, 99, 26 >" + Env.NewLine;
            Assert.That(c, new AllItemsConstraint(Is.GreaterThan(10) & Is.LessThan(100)));
        }

        [Test]
        public void AllItemsAreInstancesOfType()
        {
            object[] c = new object[] { 'a', 'b', 'c' };
            Assert.That(c, new AllItemsConstraint(Is.InstanceOfType(typeof(char))));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void AllItemsAreInstancesOfTypeFailureMessage()
        {
            object[] c = new object[] { 'a', "b", 'c' };
            expectedMessage = 
                TextMessageWriter.Pfx_Expected + "all items instance of <System.Char>" + Env.NewLine +
                TextMessageWriter.Pfx_Actual   + "< 'a', \"b\", 'c' >" + Env.NewLine;
            Assert.That(c, new AllItemsConstraint(Is.InstanceOfType(typeof(char))));
        }

        public void HandleException(Exception ex)
        {
            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
        }
    }
    #endregion

    #region CollectionContainsConstraint
    [TestFixture]
    public class CollectionContainsTests
    {
        [Test]
        public void CanTestContentsOfArray()
        {
            object item = "xyz";
            object[] c = new object[] { 123, item, "abc" };
            Assert.That(c, new CollectionContainsConstraint(item));
        }

        [Test]
        public void CanTestContentsOfArrayList()
        {
            object item = "xyz";
            ArrayList list = new ArrayList( new object[] { 123, item, "abc" } );
            Assert.That(list, new CollectionContainsConstraint(item));
        }

#if !NETCF_1_0
        [Test]
        public void CanTestContentsOfSortedList()
        {
            object item = "xyz";
            SortedList list = new SortedList();
            list.Add("a", 123);
            list.Add("b", item);
            list.Add("c", "abc");
            Assert.That(list.Values, new CollectionContainsConstraint(item));
            Assert.That(list.Keys, new CollectionContainsConstraint("b"));
        }
#endif
    }
    #endregion

    #region CollectionEquivalentConstraint
    [TestFixture]
    public class CollectionEquivalentTests : ConstraintTestBase
    {
        public CollectionEquivalentTests(string name) : base(name) { }

        protected override void SetUp()
        {
            Matcher = new CollectionEquivalentConstraint(new int[] { 1, 2, 3, 4, 5 } );
            GoodValues = new object[] { new int[] { 1, 3, 5, 4, 2 } };
            BadValues = new object[] {
                new int[] { 1, 2, 3, 7, 5 }, 
                new int[] { 1, 2, 2, 2, 5 }, 
                new int[] { 1, 2, 2, 3 , 4, 5 } };
            Description = "equivalent to < 1, 2, 3, 4, 5 >";
        }
    }
    #endregion

    #region CollectionSubsetConstraint
    [TestFixture]
    public class CollectionSubsetTests : ConstraintTestBase
    {
        public CollectionSubsetTests(string name) : base(name) { }

        protected override void SetUp()
        {
            Matcher = new CollectionSubsetConstraint(new int[] { 1, 2, 3, 4, 5 });
            GoodValues = new object[] { new int[] { 1, 3, 5 }, new int[] { 1, 2, 3, 4, 5 } };
            BadValues = new object[] { new int[] { 1, 3, 7 }, new int[] { 1, 2, 2, 2, 5 } };
            Description = "subset of < 1, 2, 3, 4, 5 >";
        }
    }
    #endregion
}
