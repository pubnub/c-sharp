
using UnityEngine;

public class TestCaseRunner : MonoBehaviour
{

	public void Start ()
	{
		UUnitTestSuite suite = new UUnitTestSuite ();
//		suite.AddAll (typeof(TestCaseTest));
		suite.AddAll (typeof(UUnitTestCase));
		UUnitTestResult result = suite.Run ();
		Debug.Log (result.Summary ());
	}
			
}


