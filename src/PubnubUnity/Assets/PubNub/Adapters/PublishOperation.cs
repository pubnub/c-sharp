using System.Collections;
using System.Collections.Generic;
using PubnubApi.EndPoint;
using UnityEngine;

namespace PubnubApi.Unity {
	public static class PublishOperationExtensions {
		// TODO document change - Async -> Execute
		public static async void Execute(this PublishOperation po, System.Action<PNPublishResult, PNStatus> callback) {
			var res = await po.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
	}
}