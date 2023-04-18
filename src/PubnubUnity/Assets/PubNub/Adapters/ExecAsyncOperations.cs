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
		
		// PublishFileMessage
		// TODO document change - Async -> Execute
		public static async void Execute(this PublishFileMessageOperation o, System.Action<PNPublishFileMessageResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// FetchHistory
		// TODO document change - Async -> Execute
		public static async void Execute(this FetchHistoryOperation o, System.Action<PNFetchHistoryResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// HereNow
		// TODO document change - Async -> Execute
		public static async void Execute(this HereNowOperation o, System.Action<PNHereNowResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// WhereNow
		// TODO document change - Async -> Execute
		public static async void Execute(this WhereNowOperation o, System.Action<PNWhereNowResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		
		// GetChannelMembers
		// TODO document change - Async -> Execute
		public static async void Execute(this GetChannelMembersOperation o, System.Action<PNChannelMembersResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// SetChannelMembership
		// TODO document change - Async -> Execute
		public static async void Execute(this SetChannelMembersOperation o, System.Action<PNChannelMembersResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// GetMessageAction
		// TODO document change - Async -> Execute
		public static async void Execute(this GetMessageActionsOperation o, System.Action<PNGetMessageActionsResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// AddMessageAction
		// TODO document change - Async -> Execute
		public static async void Execute(this AddMessageActionOperation o, System.Action<PNAddMessageActionResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// RemoveMessageAction
		// TODO document change - Async -> Execute
		public static async void Execute(this RemoveMessageActionOperation o, System.Action<PNRemoveMessageActionResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// SetMembership
		// TODO document change - Async -> Execute
		public static async void Execute(this SetMembershipsOperation o, System.Action<PNMembershipsResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// GetMembership
		// TODO document change - Async -> Execute
		public static async void Execute(this GetMembershipsOperation o, System.Action<PNMembershipsResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}

		// RemoveMembership
		// TODO document change - Async -> Execute
		public static async void Execute(this RemoveMembershipsOperation o, System.Action<PNMembershipsResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
	}
	
}