using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PubnubApi.Unity {
	public class PNManagerBehaviour : MonoBehaviour {
		public PNConfiguration pnConfiguration;

		public Pubnub pubnub {
			get;
			protected set;
		}

		protected virtual void Awake() {
			if (Application.isPlaying) {
				DontDestroyOnLoad(gameObject);
			}
		}

		protected virtual Pubnub Inintialize(string userId) {
			pnConfiguration.UserId = userId;
			pubnub = new Pubnub(pnConfiguration);
			return pubnub;
		}
	}
}