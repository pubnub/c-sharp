// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;

namespace NUnitLite.Tests
{
    public class DummyTestCase : TestCase
    {
        public bool calledSetUp = false;
        public bool calledTearDown = false;
        public bool calledTheTest = false;

        public bool simulateTestFailure = false;
        public bool simulateTestError = false;
        public bool simulateSetUpFailure = false;
        public bool simulateSetUpError = false;
        public bool simulateTearDownFailure = false;
        public bool simulateTearDownError = false;

        public DummyTestCase( string name ) : base( name ) { }

        protected override void SetUp()
        {
            calledSetUp = true;
            if (simulateSetUpFailure)
                Assert.Fail("Simulated SetUp Failure");
            else if (simulateSetUpError)
                throw new Exception("Simulated SetUp Error");
        }

        protected override void TearDown()
        {
            calledTearDown = true;
            if (simulateTearDownFailure)
                Assert.Fail("Simulated TearDown Failure");
            else if (simulateTearDownError)
                throw new Exception("Simulated TearDown Error");
        }

        public void TheTest()
        {
            calledTheTest = true;
            if (simulateTestFailure)
                Assert.Fail("Simulated Failure");
            else if (simulateTestError)
                throw new Exception("Simulated Error");
        }
    }
}
