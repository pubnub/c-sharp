using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.Unity;
using UnityEditor.SceneManagement;

namespace Tests {
	public class PNTestBase {
		public static Pubnub pn;
		public static SubscribeCallbackListener listener = new();

		public static string lastMessage = null;
		
		[OneTimeSetUp]
		public void OneTimeSetUp() {
			PNConfiguration pnConfiguration = new PNConfiguration("unit-test") {
				PublishKey = "pub-c-0f7e09a8-d82d-42e9-972c-f1f5040673df",
				SubscribeKey = "sub-c-004c90c7-f844-4aeb-8f5c-8e2738d1a91e"
			};
			pn = new Pubnub(pnConfiguration);

			pn.AddListener(listener);
			
			pn.Subscribe<string>().Channels(new[] { "test" }).WithPresence().Execute();
			pn.Subscribe<string>().ChannelGroups(new[] { "testgroup" }).WithPresence().Execute();
			
			listener.onMessage += OnMessage;
		}

		private void OnMessage(Pubnub arg1, PNMessageResult<object> arg2) {
			lastMessage = arg2.Message as string;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown() {
			listener.onMessage -= OnMessage;
			pn.UnsubscribeAll<string>();
		}
	}
}