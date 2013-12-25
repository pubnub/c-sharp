using UnityEngine;
using System;
using System.Reflection;

public class UUnitTestCase
{
	private string testMethodName;
	
	public UUnitTestCase ()
	{
	}
	
	public UUnitTestCase (String testMethodName)
	{
		this.testMethodName = testMethodName;
	}
	
	public void SetTest (string testMethodName)
	{
		this.testMethodName = testMethodName;
	}
	
	public UUnitTestResult Run ()
	{
		return Run (null);
	}
	
	public UUnitTestResult Run (UUnitTestResult testResult)
	{
		if (testResult == null) {
			testResult = new UUnitTestResult ();
		}
		
		SetUp ();
		
		testResult.TestStarted ();
		try {
			Type type = this.GetType ();
			MethodInfo method = type.GetMethod (testMethodName);
			method.Invoke (this, null);
		} catch (TargetInvocationException e) {
			testResult.TestFailed ();
			Debug.Log (e.InnerException);
		} finally {
			TearDown ();
		}
		
		return testResult;
	}
		
	protected virtual void SetUp ()
	{
	}
	
	protected virtual void TearDown ()
	{
	}
}