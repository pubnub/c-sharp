using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public static class ApiKeys {
	public static string PubKey => System.Environment.GetEnvironmentVariable("PN_PUB_KEY") ?? "";
	public static string SubKey => System.Environment.GetEnvironmentVariable("PN_SUB_KEY") ?? "";
}

public class Subscribe {
	[Test]
	public void SubscribeSimplePasses() {
		// Use the Assert class to test conditions
	}

	[UnityTest]
	public IEnumerator SubscribeWithEnumeratorPasses() {
		// Use the Assert class to test conditions.
		// Use yield to skip a frame.
		yield return null;
	}
}