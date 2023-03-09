using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PubnubApi.EndPoint;

namespace PubnubApi.Unity {
	public static class ExecAsyncOperations {
	
		// Fire
		// TODO document change - Async -> Execute
		public static async void Execute(this FireOperation fo, System.Action<PNPublishResult, PNStatus> callback) {
			var res = await fo.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// Publish
		// TODO document change - Async -> Execute
		public static async void Execute(this PublishOperation po, System.Action<PNPublishResult, PNStatus> callback) {
			var res = await po.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// Signal
		// TODO document change - Async -> Execute
		public static async void Execute(this SignalOperation so, System.Action<PNPublishResult, PNStatus> callback) {
			var res = await so.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// ListFiles
		// TODO document change - Async -> Execute
		public static async void Execute(this ListFilesOperation o, System.Action<PNListFilesResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// GetFileUrl
		// TODO document change - Async -> Execute
		public static async void Execute(this GetFileUrlOperation o, System.Action<PNFileUrlResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		
		// SendFile
		// TODO document change - Async -> Execute
		public static async void Execute(this SendFileOperation o, System.Action<PNFileUploadResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// DownloadFile
		// TODO document change - Async -> Execute
		public static async void Execute(this DownloadFileOperation o, System.Action<PNDownloadFileResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
	}
	
}