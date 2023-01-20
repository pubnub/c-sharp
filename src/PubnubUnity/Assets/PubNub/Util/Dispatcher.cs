using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace PubnubApi.Unity.Internal {
	public sealed class Dispatcher : MonoBehaviour {
		static Dispatcher instance;
		
		static object lockObject = new();
		static volatile Queue<System.Action> dispatchQueue = new();

		void FixedUpdate() {
			HandleDispatch();
		}

		static void HandleDispatch() {
			lock (lockObject) {
				var c = dispatchQueue.Count;
				for (int i = 0; i < c; i++) {
					try {
						dispatchQueue.Dequeue()();
					} catch (Exception e) {
						Debug.LogError($"Dispatched callback error:\n{e.Message} ::\n{e.StackTrace}");
					}
				}
			}
		}

		public static void Dispatch(Action action) {
			if (action is null) {
				Debug.Log("[Dispatcher] NULL");
				return;
			}

			lock (lockObject) {
				dispatchQueue.Enqueue(action);
			}
		}

		public static async void DispatchTask<T>(Task<T> task, System.Action<T> callback) {
			if (callback is null) {
				return;
			}

			T res;
			if (task.IsCompleted) {
				res = task.Result;
			} else {
				res = await task;
			}

			Dispatch(() => callback(res));
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Initialize() {
			if (!instance) {
				instance = new GameObject("[PubNub Dispatcher]").AddComponent<Dispatcher>();
			}

			instance.gameObject.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
			instance.transform.hideFlags = HideFlags.HideInInspector;
			if (Application.isPlaying) {
				DontDestroyOnLoad(instance.gameObject);
			}

			if (lockObject is null) {
				lockObject = new object();
			}
		}
	}

	public static class ActionExtensions {
		public static void Dispatch<T1, T2>(this Action<T1, T2> callback, T1 arg1, T2 arg2) =>
			Dispatcher.Dispatch(() => callback?.Invoke(arg1, arg2));
	}
}