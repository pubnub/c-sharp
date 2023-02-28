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
		
		DemoManager.instance.listener.onMessage += OnPnMessage;
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

	private async void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			// AAAAAAAAAAAAAAAAAARRRGHHHhhh
			var res = await DemoManager.instance.pubnub.Publish().Channel("test").Message(Vector3.zero.GetSerializable()).ExecuteAsync();
			Debug.Log(res.Status.ErrorData?.Information);
		}
	}
}