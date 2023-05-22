using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.EndPoint;
using PubnubApi.Unity;


namespace Tests {
	public class Subscribe : PNTestBase {
		[SetUp]
		public void SetUp() {
			listener.onStatus += OnStatus;
		}

		private void OnStatus(Pubnub arg1, PNStatus arg2) {
			Assert.IsFalse(arg2.Error, $"Error in status: {arg2.ErrorData?.Information}");
		}

		[TearDown]
		public void TearDown() {
			listener.onStatus -= OnStatus;
		}
	}
}