// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using NUnit.Framework;
using NUnitLite.Runner;

namespace NUnitLite.Tests
{
    [TestFixture]
    public class TestSuiteCreationTests : TestCase
    {
        public TestSuiteCreationTests(string name ) : base(name) { }

        public void testCanCreateSuiteAutomaticallyFromClass()
        {
            TestSuite suite = new TestSuite(typeof(SimpleTestCase));
            Assert.That(suite.TestCaseCount, Is.EqualTo(6));
            foreach (ITest test in suite.Tests)
                Assert.That(test, Is.InstanceOfType(typeof(SimpleTestCase)) | Is.InstanceOfType(typeof(InvalidTestCase)), "Not a TestCase");
            RecordingTestListener listener = new RecordingTestListener();
            TestResult result = suite.Run(listener);
            Assert.That(result.ResultState, Is.EqualTo(ResultState.Failure));
            Assert.That(result.Results.Count, Is.EqualTo(6));
            Assert.That(listener.Events, Is.EqualTo(
                "<SimpleTestCase:<test1::Success><test2::Success><Test3::Success><TEST4::Success><test6::Failure><test7::Failure>:Failure>"));
        }

        public void testTestCaseSuitesRecognizeMethodsWithTestAttribute()
        {
            TestSuite suite = new TestSuite(typeof(TestCaseClassWithTestAttributes));
            Assert.That(suite.TestCaseCount, Is.EqualTo(3));
            RecordingTestListener listener = new RecordingTestListener();
            suite.Run(listener);
            Assert.That(listener.Events, Is.EqualTo(
                "<TestCaseClassWithTestAttributes:<FirstTest::Success><SecondTest::Success><TestThree::Success>:Success>"));
        }

        public void testInvalidConstructorGivesErrorMessage()
        {
            TestSuite suite = new TestSuite(typeof(ClassWithNoValidConstructor));
            Assert.That( suite.RunState, Is.EqualTo( RunState.NotRunnable ) );
            Assert.That(suite.IgnoreReason, Contains.Substring("no default constructor"));
            Assert.That(suite.TestCaseCount, Is.EqualTo(0) );
            TestResult result = suite.Run();
            Assert.That(result.ResultState, Is.EqualTo(ResultState.Error ));
            Assert.That(result.Message, Is.EqualTo( suite.IgnoreReason ));
        }

        public void testNonTestCaseSuitesRecognizeMethodsWithTestAttribute()
        {
            TestSuite suite = new TestSuite(typeof(NonTestCaseClassWithTestAttributes));
            Assert.That(suite.TestCaseCount, Is.EqualTo(3));
            RecordingTestListener listener = new RecordingTestListener();
            suite.Run(listener);
            Assert.That(listener.Events, Is.EqualTo(
                "<NonTestCaseClassWithTestAttributes:<FirstTest::Success><SecondTest::Success><TestThree::Success>:Success>"));
        }

        #region Nested Classes for Testing
        public class TestCaseClassWithTestAttributes : TestCase
        {
            public TestCaseClassWithTestAttributes(string name) : base(name) { }

            [Test]
            public void FirstTest() { }

            [Test]
            public void SecondTest() { }

            [Test]
            public void TestThree() { }
        }

        public class NonTestCaseClassWithTestAttributes
        {
            [Test]
            public void FirstTest() { }

            [Test]
            public void SecondTest() { }

            [Test]
            public void TestThree() { }
        }

        public class ClassWithNoValidConstructor
        {
            private ClassWithNoValidConstructor() { }
        }
        #endregion
    }
}
