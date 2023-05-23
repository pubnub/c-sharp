using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.EndPoint;
using PubnubApi.Unity;

public class Publish : PNTestBase {
	string lastMessage = null;

	private void OnMessage(Pubnub arg1, PNMessageResult<object> arg2) {
		lastMessage = arg2.Message as string;
	}

	[SetUp]
	public void SetUp() {
		listener.onMessage += OnMessage;
	}

	[UnityTest]
	public IEnumerator PublishTestMessage() {
		PNStatus s = null;

		pn.Publish().Channel("test").Message("test").Execute((result, status) => s = status);

		yield return new WaitUntil(() => s != null);

		Assert.IsFalse(s.Error);
	}

	[UnityTest]
	public IEnumerator ReceiveMessage() {
		yield return new WaitUntil(() => lastMessage != null);

		Assert.AreEqual(lastMessage, "test");
		lastMessage = null;
	}

	[TearDown]
	public void TearDown() {
		listener.onMessage -= OnMessage;
	}
}