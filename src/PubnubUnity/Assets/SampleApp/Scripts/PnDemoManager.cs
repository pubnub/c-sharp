using System.Linq;
using PubnubApi;
using PubnubApi.Unity;
using UnityEngine;

public class PnDemoManager : PNManagerBehaviour {
	public string defaultUserId;
	public string defaultChannel;
	
	// trivial singleton - not guaranteed
	public static PnDemoManager instance {
		get;
		private set;
	}

	protected override void Awake() {
		base.Awake();
		instance = this;
		Inintialize(defaultUserId);
		pubnub.Subscribe<string>().Channels(new[] { defaultChannel }).WithPresence().Execute();
		
		listener.onMessage += OnPnMessage;
		listener.onSignal += OnPnSignal;
		listener.onPresence += OnPnPresence;
		listener.onFile += OnPnFile;
	}
	
	private void OnPnPresence(Pubnub pn, PNPresenceEventResult p) {
		string joined = p.Join is null ? "none" : string.Join(';', p.Join);
		string left = p.Leave is null ? "none" : string.Join(';', p.Leave);
		
		Debug.Log($"PRESENCE {p.Uuid}\nJOIN: {joined}\nLEAVE: {left}");
	}

	private void OnPnSignal(Pubnub arg1, PNSignalResult<object> arg2) {
		Debug.Log(arg2.Message);
	}

	private void OnPnFile(Pubnub pn, PNFileEventResult file) {
		Debug.Log($"File received {file.File.Url}");
	}

	private async void Start() {
		// async version
		var result = await pubnub.Publish().Channel(defaultChannel).Message("hello there").ExecuteAsync();
		
		if (!result.Status.Error) {
			Debug.Log("Message sent successfully");
		}

		// callback version
		pubnub.Publish().Channel(defaultChannel).Message("hello there 2").Execute((a, b) => Debug.Log("Published"));
		
		// let's signal
		pubnub.Signal().Channel(defaultChannel).Message("SampleSignal").Execute((a, b) => Debug.Log("Signalled"));
		
		// list files
		Debug.Log((await pubnub.ListFiles().Channel(defaultChannel).ExecuteAsync()).Result.Count);
	}

	private void OnPnMessage(Pubnub pn, PNMessageResult<object> msg) {
		Debug.Log(msg.Message);
	}
}