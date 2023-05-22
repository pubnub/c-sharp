using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.EndPoint;
using PubnubApi.Unity;

namespace Tests {
	public class Publish : PNTestBase {
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
	}
}