using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public static class ApiKeys {
	public static string PubKey => System.Environment.GetEnvironmentVariable("PN_PUB_KEY") ?? "pub-c-57776916-073a-4f19-b7e6-70dcce8eaab3";
	public static string SubKey => System.Environment.GetEnvironmentVariable("PN_SUB_KEY") ?? "sub-c-02cfab22-55db-4ba8-9768-8df98c0b9d60";
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