using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System;

public class UUnitTestSuite
{

	private List<UUnitTestCase> tests = new List<UUnitTestCase> ();
	
	public void Add (UUnitTestCase testCase)
	{
		tests.Add (testCase);
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
		
		foreach (UUnitTestCase test in tests) {
			testResult = test.Run (testResult);
		}
		return testResult;
	}

	public void AddAll (Type testCaseType)
	{
		foreach (MethodInfo method in testCaseType.GetMethods()) {
			foreach (Attribute attribute in method.GetCustomAttributes(false)) {
				if (attribute != null) {
					ConstructorInfo constructor = testCaseType.GetConstructors () [0];
					UUnitTestCase newTestCase = (UUnitTestCase)constructor.Invoke (null);
					newTestCase.SetTest (method.Name);
					Add (newTestCase);
				}
			}
		}
	}
										
}