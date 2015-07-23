// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;

namespace NUnitLite.Tests
{
    [TestFixture]
    public class AssertionExceptionTests : TestCase, IExpectException
    {
        public AssertionExceptionTests(string name) : base(name) { }

        [ExpectedException(typeof(AssertionException))]
        public void testCanThrowAndCatchAssertionException()
        {
            throw new AssertionException("My message");
        }

        [ExpectedException(typeof(AssertionException))]
        public void testThatCallingFailThrowsAssertionException()
        {
            Assert.Fail("My message");
        }

        public void HandleException( Exception ex )
        {
            Assert.That( ex.Message, Is.EqualTo( "My message" ) );
        }
    }
}
