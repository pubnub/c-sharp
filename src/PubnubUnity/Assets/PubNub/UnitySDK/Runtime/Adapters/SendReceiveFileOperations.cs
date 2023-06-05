using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
using PubnubApi.Unity.Internal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

// File operation convenience methods
namespace PubnubApi.Unity.FileOperations {
	public static class SendReceiveFileOperations {
		/// <summary>
		/// Converts a texture into bytes and uploads it to PubNub.
		///
		/// This method overrides <c>Message</c> with input texture's size and format.
		/// </summary>
		/// <example>
		/// <c>var res = await pubnub.SendFile().Texture(texture).Channel("test").FileName("texture.tex").ExecuteAsync()</c>;
		/// </example>
		/// <param name="texture">A Texture compatible with <c>GetRawTextureData</c></param>
		/// <returns></returns>
		public static SendFileOperation Texture(this SendFileOperation o, Texture2D texture) {
			return o.File(texture.GetRawTextureData()).Message(new JsonSafeVector(texture.width, texture.height, (int)texture.graphicsFormat));
		}

		public static SendFileOperation Texture(this SendFileOperation o, RenderTexture texture) {
			var active = RenderTexture.active;

			RenderTexture.active = texture;
			Texture2D tex = new Texture2D(texture.width, texture.height);
			tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			var bytes = tex.GetRawTextureData();

			RenderTexture.active = active;
			Object.Destroy(tex);

			return o.File(bytes).Message(new JsonSafeVector(texture.width, texture.height, (int)texture.graphicsFormat));
		}

		public static async Task<Texture> DownloadTexture(this Pubnub pubnub, PNFileEventResult fileEventResult, TextureCreationFlags textureCreationFlags = TextureCreationFlags.None) {
			var size = new JsonSafeVector(fileEventResult.Message as Dictionary<string, object>);
			var f = await pubnub.DownloadFile().FileId(fileEventResult.File.Id).FileName(fileEventResult.File.Name).Channel(fileEventResult.Channel).ExecuteAsync();

			Texture2D tex;
			if (size.z >= 0) {
				tex = new Texture2D((int)size.x, (int)size.y, (GraphicsFormat)size.z, 0);
				tex.LoadRawTextureData(f.Result.FileBytes);
				tex.Apply();
			} else {
				tex = new Texture2D(2, 2);
				tex.LoadImage(f.Result.FileBytes);
			}
			return tex;
		}
	}
}