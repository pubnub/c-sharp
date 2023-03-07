using System;
using System.Collections;
using System.Collections.Generic;
using PubnubApi;
using PubnubApi.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PnImageSendDemo : MonoBehaviour {

	public RawImage sourceImageComponent;
	public RawImage targetImageComponent;

	private Action<Texture> onTextureChange;
	private Texture previousTexture;

	private PnDemoManager pnManager => PnDemoManager.instance;

	private void Start() {
		pnManager.listener.onFile += PnOnFile;
		previousTexture = sourceImageComponent.texture;
	}

	private async void PnOnFile(Pubnub pn, PNFileEventResult file) {
		var f = await pnManager.pubnub.DownloadFile().Channel(pnManager.defaultChannel).FileId(file.File.Id).ExecuteAsync();
		var tex = new Texture2D(1, 1);
		tex.LoadRawTextureData(f.Result.FileBytes);
		tex.Apply();
		targetImageComponent.texture = tex;
	}

	async void Update() {
		// RawImage doesn't have an event for that o_O
		if (previousTexture != sourceImageComponent.texture) {
			onTextureChange?.Invoke(sourceImageComponent.texture);
			previousTexture = sourceImageComponent.texture;

			Debug.Log($"Sending image, len {(previousTexture as Texture2D).GetRawTextureData().Length}");
			
			var res = await pnManager.pubnub.SendFile().FileName("demoTexture").File((previousTexture as Texture2D).GetRawTextureData()).Channel(pnManager.defaultChannel).Message("tex").ExecuteAsync();
			Debug.Log(res);
		}
	}

	private void OnDisable() {
		pnManager.listener.onFile -= PnOnFile;
	}
}