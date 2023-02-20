using System;
using System.Collections;
using System.Collections.Generic;
using PubnubApi;
using PubnubApi.Unity;
using UnityEngine;

public class DemoManager : PNManagerBehaviour {
	public string defaultUserId;
	public string defaultChannel;


	protected override void Awake() {
		base.Awake();
		Inintialize(defaultUserId);
		pubnub.Subscribe<string>().Channels(new[] { defaultChannel }).Execute();
		
		listener.onMessage += OnPnMessage;
	}

	private async void Start() {
		var result = await pubnub.Publish().Channel(defaultChannel).Message("hello there").ExecuteAsync();
		if (!result.Status.Error) {
			Debug.Log("Message sent successfully");
		}
	}

	private void OnPnMessage(Pubnub pn, PNMessageResult<object> msg) {
		Debug.Log(msg.Message);
	}
}