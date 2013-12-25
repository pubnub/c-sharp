using UnityEngine;

public class UUnitTestResult
{
	private int runCount = 0;
	private int failedCount = 0;

	public void TestStarted ()
	{
		runCount += 1;
	}

	public void TestFailed ()
	{
		failedCount += 1;
	}

	public string Summary ()
	{
		return runCount + " run, " + failedCount + " failed";
	}
			
}