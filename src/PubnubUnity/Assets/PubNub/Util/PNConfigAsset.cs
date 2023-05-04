using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PubnubApi.Unity {
	[CreateAssetMenu(fileName = "PNConfigAsset", menuName = "PubNub/PubNub Config Asset")]
	public class PNConfigAsset : ScriptableObject {
		public string PublishKey;
		public string SubscribeKey;
		public string AuthKey;
		public string CipherKey;
		public bool EnableTelemetry;
		public bool Secure;
		public PNLogVerbosity LogVerbosity;
		
		[SerializeField] private bool externalJsonEnabled = false;
		[SerializeField] private UnityEngine.TextAsset externalJsonFile;

		[System.NonSerialized] public string UserId;

		public static implicit operator PNConfiguration(PNConfigAsset asset) {
			if (string.IsNullOrEmpty(asset.UserId)) {
				throw new NullReferenceException("You need to set the UserId before passing configuration");
			}

			var config = new PNConfiguration(new UserId(new UserId(asset.UserId)));
			config.SubscribeKey = asset.SubscribeKey;
			config.PublishKey = asset.PublishKey;
			config.AuthKey = asset.AuthKey;
			config.CipherKey = asset.CipherKey;
			config.Secure = asset.Secure;
			config.LogVerbosity = asset.LogVerbosity;
			config.EnableTelemetry = asset.EnableTelemetry;
			return config;
		}
	}
}