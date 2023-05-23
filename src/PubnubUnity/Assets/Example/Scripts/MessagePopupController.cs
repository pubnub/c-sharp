using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PubnubApi;
using PubnubApi.Unity;
using TMPro;

public class MessagePopupController : MonoBehaviour {

	private RectTransform textContainer;
	private GameObject textGo;
	
	void Start() {
		textContainer = transform.GetChild(0) as RectTransform;
		textGo = textContainer!.gameObject;
		textGo.SetActive(false);
		
		PnDemoManager.instance.listener.onMessage += OnPnMessage;
		PnDemoManager.instance.listener.onPresence += OnPnPresence;
	}

	private async void OnPnMessage(Pubnub pn, PNMessageResult<object> msg) {
		var spawned = Instantiate(textContainer, transform);
		
		var pos = (spawned.transform as RectTransform).anchoredPosition;
		pos.y = -50;
		(spawned.transform as RectTransform).anchoredPosition = pos;
		
		spawned.GetComponentInChildren<TextMeshProUGUI>().text = msg.Message as string;
		spawned.gameObject.SetActive(true);
		spawned.GetComponent<AnimatedTextPopup>().PlayForward();
		await Task.Delay(2000);
		spawned.GetComponent<AnimatedTextPopup>().PlayBackwards(true);
	}
	
	// copy-pasted, refactor
	private async void OnPnPresence(Pubnub pn, PNPresenceEventResult msg) {
		var spawned = Instantiate(textContainer, transform);
		
		var pos = (spawned.transform as RectTransform).anchoredPosition;
		pos.y = -50;
		(spawned.transform as RectTransform).anchoredPosition = pos;
		
		spawned.GetComponentInChildren<TextMeshProUGUI>().text = msg?.Uuid as string;
		spawned.gameObject.SetActive(true);
		spawned.GetComponent<AnimatedTextPopup>().PlayForward();
		await Task.Delay(2000);
		spawned.GetComponent<AnimatedTextPopup>().PlayBackwards(true);
	}

	private async void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			// AAAAAAAAAAAAAAAAAARRRGHHHhhh
			var res = await PnDemoManager.instance.pubnub.Publish().Channel(PnDemoManager.instance.defaultChannel).Message(Vector3.zero.GetJsonSafe()).ExecuteAsync();
			Debug.Log($"Publish to {PnDemoManager.instance.defaultChannel}: {res.Status.ErrorData?.Information}");
		}
	}

	private void OnDestroy() {
		Debug.Log("Message popup controller cleanup");
		PnDemoManager.instance.listener.onMessage -= OnPnMessage;
		PnDemoManager.instance.listener.onPresence -= OnPnPresence;
	}
}