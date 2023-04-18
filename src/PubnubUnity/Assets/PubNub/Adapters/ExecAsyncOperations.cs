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
		
		// SetChannelMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this SetChannelMetadataOperation o, System.Action<PNSetChannelMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// GetChannelMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this GetChannelMetadataOperation o, System.Action<PNGetChannelMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// RemoveChannelMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this RemoveChannelMetadataOperation o, System.Action<PNRemoveChannelMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// RemoveChannelMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this AuditPushChannelOperation o, System.Action<PNPushListProvisionsResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// GetAllChannelMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this GetAllChannelMetadataOperation o, System.Action<PNGetAllChannelMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// RemoveChannelMembers
		// TODO document change - Async -> Execute
		public static async void Execute(this RemoveChannelMembersOperation o, System.Action<PNChannelMembersResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// RemoveUUIDMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this RemoveUuidMetadataOperation o, System.Action<PNRemoveUuidMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// SetUUIDMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this SetUuidMetadataOperation o, System.Action<PNSetUuidMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// GetUUIDMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this GetUuidMetadataOperation o, System.Action<PNGetUuidMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// GetAllUUIDMetadata
		// TODO document change - Async -> Execute
		public static async void Execute(this GetAllUuidMetadataOperation o, System.Action<PNGetAllUuidMetadataResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// RemoveAllPushNotificationsFromDeviceWithPushToken
		// TODO document change - Async -> Execute
		public static async void Execute(this RemoveAllPushChannelsOperation o, System.Action<PNPushRemoveAllChannelsResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// RemovePushNotificationsFromChannels
		// TODO document change - Async -> Execute
		public static async void Execute(this RemovePushChannelOperation o, System.Action<PNPushRemoveChannelResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
		
		// AddPushNotificationsOnChannels
		// TODO document change - Async -> Execute
		public static async void Execute(this AddPushChannelOperation o, System.Action<PNPushAddChannelResult, PNStatus> callback) {
			var res = await o.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
	}
	
}