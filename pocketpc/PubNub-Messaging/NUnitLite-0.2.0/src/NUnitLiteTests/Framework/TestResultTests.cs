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
    public class TestResultTests : TestCase
    {
        private static readonly string MESSAGE = "my message";
#if !NETCF_1_0
        private static readonly string STACKTRACE = "stack trace";
#endif

        private TestResult result;

        public TestResultTests(string name) : base(name) { }

        protected override void SetUp()
        {
            result = new TestResult(null);
        }

        void VerifyResultState(ResultState expectedState, bool executed, bool success, bool failure, bool error, string message )
        {
            Assert.That( result.ResultState , Is.EqualTo( expectedState ) );
            Assert.That( result.Executed, Is.EqualTo( executed ) );
            Assert.That( result.IsSuccess, Is.EqualTo( success ) );
            Assert.That( result.IsFailure, Is.EqualTo( failure ) );
            Assert.That( result.IsError, Is.EqualTo( error ) );
            if ( error )
                Assert.That(result.Message, Is.EqualTo("System.Exception : " + message));
            else
                Assert.That(result.Message, Is.EqualTo(message));
        }

        public void testDefaultStateIsNotRun()
        {
            VerifyResultState(ResultState.NotRun, false, false, false, false, null);
        }

        public void testCanMarkAsSuccess()
        {
            result.Success();
            VerifyResultState(ResultState.Success, true, true, false, false, null);
        }

        public void testCanMarkAsFailure()
        {
#if NETCF_1_0
            result.Failure(MESSAGE);
            VerifyResultState(ResultState.Failure, true, false, true, false, MESSAGE);
#else
            result.Failure(MESSAGE, STACKTRACE);
            VerifyResultState(ResultState.Failure, true, false, true, false, MESSAGE);
            Assert.That( result.StackTrace, Is.EqualTo( STACKTRACE ) );
#endif
        }

        public void testCanMarkAsError()
        {
            Exception caught;
            try
            {
                throw new Exception(MESSAGE);
            }
            catch(Exception ex)
            {
                caught = ex;          
            }

            result.Error(caught);
            VerifyResultState(ResultState.Error, true, false, false, true, MESSAGE);
#if !NETCF_1_0
            Assert.That( result.StackTrace, Is.EqualTo( caught.StackTrace ) );
#endif
        }

        public void testCanMarkAsNotRun()
        {
            result.NotRun(MESSAGE);
            VerifyResultState(ResultState.NotRun, false, false, false, false, MESSAGE);
        }
    }
}
