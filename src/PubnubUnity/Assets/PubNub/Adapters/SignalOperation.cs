using System.Collections;
using System.Collections.Generic;
using PubnubApi.EndPoint;
using UnityEngine;

namespace PubnubApi.Unity {
	public static class SignalOperationExtensions {
		// TODO document change - Async -> Execute
		public static async void Execute(this SignalOperation so, System.Action<PNPublishResult, PNStatus> callback) {
			var res = await so.ExecuteAsync();
			callback?.Invoke(res.Result, res.Status);
		}
	}
}