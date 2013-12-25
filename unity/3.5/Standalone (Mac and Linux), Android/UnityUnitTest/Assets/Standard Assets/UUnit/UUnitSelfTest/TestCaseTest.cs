/*
using UnityEngine;

public class TestCaseTest //: UUnitTestCase
{

	[UUnitTest]
	public void TestRunning ()
	{
		TestCaseDummy local = new TestCaseDummy ("TestMethod");
		UUnitAssert.False (local.wasRun, " not wasRun");
		local.Run ();
		UUnitAssert.True (local.wasRun, "wasRun");
	}

	[UUnitTest]
	void TestSetUp ()
	{
		TestCaseDummy local = new TestCaseDummy ("TestMethod");
		local.Run ();
		UUnitAssert.True (local.wasSetUp, "wasSetUp");
		UUnitAssert.Equals (local.log, "setUp ", "setup");
	}

	[UUnitTest]				
	public void TestResult ()
	{
		TestCaseDummy local = new TestCaseDummy ("TestMethod");
		UUnitTestResult result = local.Run ();
		UUnitAssert.Equals ("1 run, 0 failed", result.Summary (), "testResult");		
	}

	[UUnitTest]				
	public void TestFailure ()
	{
		TestCaseDummy local = new TestCaseDummy ("TestFail");
		UUnitTestResult result = local.Run ();
		UUnitAssert.Equals ("1 run, 1 failed", result.Summary (), "Failure");
	}

	[UUnitTest]		
	public void TestTestSuiteAdd ()
	{
		UUnitTestSuite suite = new UUnitTestSuite ();
		suite.Add (new TestCaseDummy ("TestMethod"));
		suite.Add (new TestCaseDummy ("TestFail"));
		UUnitTestResult result = suite.Run ();
		UUnitAssert.Equals ("2 run, 1 failed", result.Summary (), "Suite");
	}

	[UUnitTest]		
	public void TestTestSuiteAddAll ()
	{	
		UUnitTestSuite suite = new UUnitTestSuite ();
		suite.AddAll (typeof(TestCaseDummy));
		UUnitTestResult result = suite.Run ();
		UUnitAssert.Equals ("2 run, 1 failed", result.Summary (), "Suite");
	}
			
}
*/