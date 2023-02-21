using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PubnubApi.Unity {
	[HelpURL("https://www.pubnub.com/docs/sdks/unity")]
	public class PNManagerBehaviour : MonoBehaviour {
		public PNConfigAsset pnConfiguration;

		public Pubnub pubnub {
			get;
			protected set;
		}
		
		public SubscribeCallbackListener listener { get; }
			= new SubscribeCallbackListener();

		protected virtual void Awake() {
			if (Application.isPlaying) {
				DontDestroyOnLoad(gameObject);
			}
		}

		/// <summary>
		/// Initializes a PubNub instance, and the associated event listener.
		/// </summary>
		/// <param name="userId">You can use one User ID to represent a user on all their devices, or use one User ID per client. If you allow a user to connect from multiple devices simultaneously, use the same User ID for each device, as PubNub features such as Presence, which determine's a user's online status, rely on User IDs.<br/><a href="https://www.pubnub.com/docs/general/setup/application-setup#user-ids">See documentation</a></param>
		/// <returns></returns>
		public Pubnub Inintialize(string userId) {
			if (pnConfiguration is null) {
				Debug.LogError("PNConfigAsset is missing", this);
				return null;
			}
			
			if (pubnub is not null) {
				Debug.LogError("PubNub has already been initialized");
				return pubnub;
			}
			pnConfiguration.UserId = userId;
			pubnub = new Pubnub(pnConfiguration);
			pubnub.AddListener(listener);
			return pubnub;
		}

		protected virtual void OnDestroy() {
			pubnub.UnsubscribeAll<string>();
		}
	}
}