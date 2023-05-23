using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using PubnubApi;
using PubnubApi.Unity;
using UnityEditor.SceneManagement;


public class PNTestBase {
	protected static Pubnub pn;
	protected static SubscribeCallbackListener listener = new();

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
	}


	[OneTimeTearDown]
	public void OneTimeTearDown() {
		pn.UnsubscribeAll<string>();
	}
}