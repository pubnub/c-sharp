using System;
using System.Collections;
using System.Collections.Generic;
using PubnubApi;
using PubnubApi.Unity;
using UnityEngine;

public class DemoManager : PNManagerBehaviour {
	public string defaultUserId;
	public string defaultChannel;
	
	public static DemoManager instance { get; }

	protected override void Awake() {
		base.Awake();
		Inintialize(defaultUserId);
		pubnub.Subscribe<string>().Channels(new[] { defaultChannel }).Execute();
		
		listener.onMessage += OnPnMessage;
		listener.onSignal += OnPnSignal;
	}

	private void OnPnSignal(Pubnub arg1, PNSignalResult<object> arg2) {
		Debug.Log(arg2.Message);
	}

	private async void Start() {
		// async version
		var result = await pubnub.Publish().Channel(defaultChannel).Message("hello there").ExecuteAsync();

		// callback version
		pubnub.Publish().Channel(defaultChannel).Message("hello there 2").Execute((a, b) => Debug.Log("Published"));
		
		// let's signal
		pubnub.Signal().Channel(defaultChannel).Message("SampleSignal").Execute((a, b) => Debug.Log("Signalled"));
		
		if (!result.Status.Error) {
			Debug.Log("Message sent successfully");
		}
	}

	private void OnPnMessage(Pubnub pn, PNMessageResult<object> msg) {
		Debug.Log(msg.Message);
	}
}