/*
 
using UnityEngine;

public class TestCaseDummy : UUnitTestCase
{
	public bool wasRun;
	public bool wasSetUp;
	public string log;
	
	public TestCaseDummy ()
	{
	}
	
	public TestCaseDummy (string testMethodName) : base(testMethodName)
	{
	}
	
	[UUnitTest]
	public void TestMethod ()
	{
		wasRun = true;
	}
	
	protected override void SetUp ()
	{
		wasRun = false;
		wasSetUp = true;
		log = "setUp ";
	}
	
	[UUnitTest]
	public void TestFail ()
	{
		UUnitAssert.True (false, "Expected Fail Result");
	}
}

*/