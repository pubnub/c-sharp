using PubnubApi;
using PubnubApi.Unity;
using UnityEngine;

public class DemoManager : PNManagerBehaviour {
	public string defaultUserId;
	public string defaultChannel;
	
	// trivial singleton - not guaranteed
	public static DemoManager instance {
		get;
		private set;
	}

	protected override void Awake() {
		base.Awake();
		instance = this;
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

		// list files
		await pubnub.ListFiles().Channel(defaultChannel).ExecuteAsync();

		await pubnub.GetFileUrl().Channel(defaultChannel).FileId("lol").ExecuteAsync();
		
		if (!result.Status.Error) {
			Debug.Log("Message sent successfully");
		}
	}

	private void OnPnMessage(Pubnub pn, PNMessageResult<object> msg) {
		Debug.Log(msg.Message);
	}
}