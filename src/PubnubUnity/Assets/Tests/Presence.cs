using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.EndPoint;
using PubnubApi.Unity;

public class Presence : PNTestBase {
	string lastMessage = null;

	private void OnPresence(Pubnub arg1, PNPresenceEventResult arg2) {
		lastMessage = arg2.Event as string;
	}

	[SetUp]
	public void SetUp() {
		listener.onPresence += OnPresence;
	}
	
	[UnityTest]
	public IEnumerator HereNow() {
		yield return pn.HereNow().Channels(new[] { "test" }).ExecuteAsync().YieldTask(out var assigner);
		var s = assigner().result.Status;
		Assert.IsFalse(s.Error);
	}
	

	[TearDown]
	public void TearDown() {
		listener.onPresence -= OnPresence;
	}
}