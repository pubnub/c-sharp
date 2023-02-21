using System.Collections;
using System.Collections.Generic;
using PubnubApi.EndPoint;
using UnityEngine;

namespace PubnubApi.Unity {
	public static class FireOperationExtensions {
		// TODO document change - Async -> Execute
		public static async void Execute(this FireOperation fo, System.Action<PNPublishResult, PNStatus> callback) {
			var res = await fo.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
	}
}