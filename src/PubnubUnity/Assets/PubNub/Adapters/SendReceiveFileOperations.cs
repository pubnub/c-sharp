using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PubnubApi.EndPoint;
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
		public static SendFileOperation Texture(this SendFileOperation o, Texture texture) {
			if ((texture as Texture2D) is null) {
				Debug.LogError("Texture cannot be converted");
				return o;
			}
			
			return o.File(((Texture2D)texture).GetRawTextureData()).Message(new VectorSerializable(texture.width, texture.height, (int)texture.graphicsFormat));
		}

		public static async Task<Texture> DownloadTexture(this Pubnub pubnub, PNFileEventResult fileEventResult,  string channel, TextureCreationFlags textureCreationFlags = TextureCreationFlags.None) {
			var size = new VectorSerializable(fileEventResult.Message as Dictionary<string, object>);
			var f = await pubnub.DownloadFile().FileId(fileEventResult.File.Id).FileName(fileEventResult.File.Name).Channel(channel).ExecuteAsync();
		
			var tex = new Texture2D((int)size.x, (int)size.y, (GraphicsFormat)size.z, 0);
			tex.LoadRawTextureData(f.Result.FileBytes);
			tex.Apply();
			return tex;
		}
	}
}