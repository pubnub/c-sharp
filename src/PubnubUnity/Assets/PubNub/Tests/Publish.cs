using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.EndPoint;
using PubnubApi.Unity;

namespace PubnubApi.Unity.Tests {

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
			pn.Publish().Channel("test").Message("test")
				.Execute(Callback(out var awaiter, out var assigner));
			yield return awaiter;
			var s = assigner().status;

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
}