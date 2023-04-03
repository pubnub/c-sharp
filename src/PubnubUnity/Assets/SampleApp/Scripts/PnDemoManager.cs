using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PubnubApi;
using PubnubApi.Unity;
using UnityEngine;

public class PnDemoManager : PNManagerBehaviour {
	public string defaultUserId;
	public string defaultChannel;
	private string[] defaultChannels => new[] { defaultChannel };
	
	// trivial singleton - not guaranteed
	public static PnDemoManager instance {
		get;
		private set;
	}

	void Awake() {
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
		long sentMessageTimetoken = 0;
		long sentActionTimetoken = 0;

		if (!result.Status.Error) {
			Debug.Log("Message sent successfully");
		}

		// callback version
		pubnub.Publish().Channel(defaultChannel).Message("hello there 2").Execute((a, b) => {
			Debug.Log("Published");
			sentMessageTimetoken = a.Timetoken;
		});

		// let's signal
		pubnub.Signal().Channel(defaultChannel).Message("SampleSignal").Execute((a, b) => Debug.Log("Signalled"));
		
		

		// fetch history
		pubnub.FetchHistory().Channels(defaultChannels).Execute((a, b) => Debug.Log($"History messages: {string.Join(',', a.Messages)}"));

		// herenow
		pubnub.HereNow().Channels(defaultChannels).Execute((a, b) => Debug.Log($"Herenow: {a.TotalOccupancy}"));
		
		// wherenow
		pubnub.WhereNow().Execute((a, b) => Debug.Log($"Wherenow: {a.Channels.Count}"));
		
		// channel members
		pubnub.SetChannelMembers().Channel(defaultChannel).Uuids(new List<PNChannelMember>() {new() {Uuid = defaultUserId}}).Execute((a, b) => Debug.Log($"Setting membership status: {!b.Error}"));
		await Task.Delay(1000);
		pubnub.GetChannelMembers().Channel(defaultChannel).Execute((a, b) => Debug.Log($"ChannelMembers: {string.Join(',', a.ChannelMembers.Select(m => m.UuidMetadata.Uuid))}"));
		
		
		// message actions:
		pubnub.AddMessageAction().Channel(defaultChannel).Action(new() {Type = "reaction", Value = "sadface"}).MessageTimetoken(sentMessageTimetoken).Execute((a, b) => {
			Debug.Log($"Message action added {!b.Error}");
			sentActionTimetoken = a.ActionTimetoken;
		});
		await Task.Delay(500);
		pubnub.GetMessageActions().Channel(defaultChannel).Execute((a, b) => Debug.Log(a.MessageActions[0].Action.Value));
		pubnub.RemoveMessageAction().Channel(defaultChannel).MessageTimetoken(sentMessageTimetoken).ActionTimetoken(sentActionTimetoken).Execute((a, b) => Debug.Log($"Removed message action {(b.Error ? b.ErrorData.Information : "true")}"));

		// list files
		Debug.Log((await pubnub.ListFiles().Channel(defaultChannel).ExecuteAsync()).Result.Count);
	}

	private void OnPnMessage(Pubnub pn, PNMessageResult<object> msg) {
		Debug.Log(msg.Message);
	}

	protected override void OnDestroy() {
		listener.onMessage -= OnPnMessage;
		listener.onSignal -= OnPnSignal;
		listener.onPresence -= OnPnPresence;
		listener.onFile -= OnPnFile;
		base.OnDestroy();
	}
}