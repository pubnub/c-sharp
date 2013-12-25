using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class UUnitTestRunner
{
	private static void FindAndAddAllTestCases (UUnitTestSuite suite)
	{
		IEnumerable<Type> testCasesTypes = AppDomain.CurrentDomain.GetAssemblies ()
									    .Select (x => x.GetTypes ())
									    .SelectMany (x => x)
										.Where (c => !c.IsAbstract)
									    .Where (c => c.IsSubclassOf (typeof(UUnitTestCase)));

		foreach (Type testCaseType in testCasesTypes) {
			suite.AddAll (testCaseType);
		}
	}
	
	private static void ClearDebugLog ()
	{
		Assembly assembly = Assembly.GetAssembly (typeof(SceneView));
		Type type = assembly.GetType ("UnityEditorInternal.LogEntries");
		MethodInfo method = type.GetMethod ("Clear");
		method.Invoke (new object (), null);
	}
	
	[MenuItem("UUnit/Run All Tests %#t")]
	private static void RunAllTests ()
	{
		ClearDebugLog ();
		
		UUnitTestSuite suite = new UUnitTestSuite ();
		FindAndAddAllTestCases (suite);
		UUnitTestResult result = suite.Run ();
		
		Debug.Log (result.Summary ());
	}

}
