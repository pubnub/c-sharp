using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PubnubApi;
using PubnubApi.Unity;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
		var size = new VectorSerializable(file.Message as Dictionary<string, object>);
		var f = await pnManager.pubnub.DownloadFile().FileId(file.File.Id).FileName(file.File.Name).Channel("test").ExecuteAsync();
		
		var tex = new Texture2D((int)size.x, (int)size.y, GraphicsFormat.RGBA_DXT1_UNorm, 0);
		tex.LoadRawTextureData(f.Result.FileBytes);
		tex.Apply();
		targetImageComponent.texture = tex;
	}

	async void Update() {
		// RawImage doesn't have an event for that o_O
		if (previousTexture != sourceImageComponent.texture) {
			onTextureChange?.Invoke(sourceImageComponent.texture);
			previousTexture = sourceImageComponent.texture;

			var bytes = (previousTexture as Texture2D).GetRawTextureData();

			var res = await pnManager.pubnub.SendFile().File(bytes).Channel("test").Message(new VectorSerializable(previousTexture.width, previousTexture.height)).FileName("dekzdura.tex").ExecuteAsync();
			Debug.Log(res.Status.Error);
		}
	}

	private void OnDisable() {
		pnManager.listener.onFile -= PnOnFile;
	}
}