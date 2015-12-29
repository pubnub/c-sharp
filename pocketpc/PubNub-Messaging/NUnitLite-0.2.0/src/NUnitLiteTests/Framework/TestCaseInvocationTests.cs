// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Collections;
using NUnit.Framework;

namespace NUnitLite.Tests
{
    [TestFixture]
    public class TestCaseInvocationTests : TestCase
    {
        DummyTestCase test;
        TestResult result;

        public TestCaseInvocationTests(string name) : base(name) { }

        protected override void SetUp()
        {
            test = new DummyTestCase( "TheTest" );
        }

        public void testSetUpCalled()
        {
            RunTestAndVerifyResult(ResultState.Success);
            Assert.That(test.calledSetUp);
        }

        public void testSetupFailureIsReported()
        {
            test.simulateSetUpFailure = true;
            RunTestAndVerifyResult(ResultState.Failure);
            Assert.That( result.Message, Is.EqualTo("Simulated SetUp Failure") );
            VerifyStackTraceContainsMethod("SetUp");
        }

        public void testSetupErrorIsReported()
        {
            test.simulateSetUpError = true;
            RunTestAndVerifyResult(ResultState.Error);
            Assert.That( result.Message, Is.EqualTo( "System.Exception : Simulated SetUp Error" ) );
            VerifyStackTraceContainsMethod("SetUp");
        }

        public void testTearDownCalled()
        {
            RunTestAndVerifyResult(ResultState.Success);
            Assert.That(test.calledTearDown);
        }

        public void testTearDownCalledAfterTestFailure()
        {
            test.simulateTestFailure = true;
            test.Run();
            Assert.That(test.calledTearDown);
        }

        public void testTearDownCalledAfterTestError()
        {
            test.simulateTestError = true;
            test.Run();
            Assert.That(test.calledTearDown);
        }

        public void testThatTestAndTearDownAreNotCalledAfterSetUpFailure()
        {
            test.simulateSetUpFailure = true;
            test.Run();
            Assert.That( test.calledTheTest, Is.False, "Test" );
            Assert.That(test.calledTearDown, Is.False, "TearDown" );
        }

        public void testThatTestAndTearDownAreNotCalledAfterSetUpError()
        {
            test.simulateSetUpError = true;
            test.Run();
            Assert.That(test.calledTheTest, Is.False, "Test");
            Assert.That(test.calledTearDown, Is.False, "TearDown");
        }

        public void testTearDownFailureIsReported()
        {
            test.simulateTearDownFailure = true;
            RunTestAndVerifyResult(ResultState.Failure);
            Assert.That( result.Message, Is.EqualTo( "Simulated TearDown Failure" ) );
            VerifyStackTraceContainsMethod("TearDown");
        }

        //public void testTearDownFailureDoesNotOverWriteTestFailureInfo()
        //{
        //    test.simulateTestFailure = true;
        //    test.simulateTearDownFailure = true;
        //    RunTestAndVerifyResult(ResultState.Failure);
        //    NUnit.Framework.StringAssert.Contains("Simulated Test Failure", result.Message);
        //    NUnit.Framework.StringAssert.Contains("Simulated TearDown Failure", result.Message);
        //    VerifyStackTraceContainsMethod("TheTest");
        //    VerifyStackTraceContainsMethod("TearDown");
        //}

        public void testTearDownErrorIsReported()
        {
            test.simulateTearDownError = true;
            RunTestAndVerifyResult(ResultState.Error);
            Assert.That( result.Message, Is.EqualTo( "System.Exception : Simulated TearDown Error" ) );
            VerifyStackTraceContainsMethod("TearDown");
        }

        public void TestCalled()
        {
            RunTestAndVerifyResult(ResultState.Success);
            Assert.That(test.calledTheTest);
        }

        public void TestErrorIsReported()
        {
            test.simulateTestError = true;
            RunTestAndVerifyResult(ResultState.Error);
            Assert.That( result.Message, Is.EqualTo( "System.Exception : Simulated Error" ) );
            VerifyStackTraceContainsMethod("TheTest");
        }

        public void TestFailureIsReported()
        {
            test.simulateTestFailure = true;
            RunTestAndVerifyResult(ResultState.Failure);
            Assert.That( result.Message, Is.EqualTo( "Simulated Failure" ) );
            VerifyStackTraceContainsMethod("TheTest");
        }

        public void TestListenerIsCalled()
        {
            RecordingTestListener listener = new RecordingTestListener();
            test.Run(listener);
            Assert.That( listener.Events, Is.EqualTo( "<TheTest::Success>" ) );
        }

        public void TestListenerReceivesFailureMessage()
        {
            RecordingTestListener listener = new RecordingTestListener();
            test.simulateTestFailure = true;
            test.Run(listener);
            Assert.That( listener.Events, Is.EqualTo( "<TheTest::Failure>" ) );
        }

        #region Helper Methods
        private void RunTestAndVerifyResult(ResultState expected)
        {
            result = test.Run();
            VerifyResult(expected);
        }

        private void VerifyResult(ResultState expected)
        {
            Assert.That( result.ResultState, Is.EqualTo( expected ) );
        }

        private void VerifyStackTraceContainsMethod(string methodName)
        {
#if !NETCF_1_0
            Assert.That( result.StackTrace, Is.Not.Null, "StackTrace is null" );
            string fullName = string.Format("{0}.{1}", typeof(DummyTestCase).FullName, methodName);
            Assert.That( result.StackTrace, Contains.Substring( fullName ) );
#endif
        }
        #endregion
    }
}
