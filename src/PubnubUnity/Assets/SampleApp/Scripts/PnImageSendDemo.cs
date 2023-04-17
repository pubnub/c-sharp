using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PubnubApi;
using PubnubApi.Unity;
using PubnubApi.Unity.FileOperations;
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
		targetImageComponent.texture = await pn.DownloadTexture(file, TextureCreationFlags.None);
		Debug.Log($"TEXTURE FORMAT: {GraphicsFormatUtility.GetFormatString(targetImageComponent.texture.graphicsFormat)}");
	}

	async void Update() {
		// RawImage doesn't have an event for that o_O
		if (previousTexture != sourceImageComponent.texture) {
			onTextureChange?.Invoke(sourceImageComponent.texture);
			previousTexture = sourceImageComponent.texture;

			var res = await pnManager.pubnub.SendFile().Texture((RenderTexture)previousTexture).Channel("test")
				.FileName("degzdura.tex").ExecuteAsync();
			Debug.Log(res.Status.Error);
		}

		if (Input.GetKeyDown(KeyCode.A)) {
			var res = await pnManager.pubnub.SendFile().Texture(ScreenCapture.CaptureScreenshotAsTexture()).Channel("test")
				.FileName("degzdura.tex").ExecuteAsync();
		}
	}

	private void OnDisable() {
		pnManager.listener.onFile -= PnOnFile;
	}
}