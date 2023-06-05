using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace PubnubApi.Unity {
	[Serializable]
	public class JsonSafeVector {
		public float x;
		public float y;
		public float z;
		public float w;

		public JsonSafeVector(float x, float y, float z = 0f, float w = 0f) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public JsonSafeVector(Vector3 sourceVector) {
			x = sourceVector.x;
			y = sourceVector.y;
			z = sourceVector.z;
		}

		public JsonSafeVector(Vector2 sourceVector) {
			x = sourceVector.x;
			y = sourceVector.y;
		}

		public JsonSafeVector(Quaternion sourceQuaternion) {
			x = sourceQuaternion.x;
			y = sourceQuaternion.y;
			z = sourceQuaternion.z;
			w = sourceQuaternion.w;
		}

		public JsonSafeVector(Dictionary<string, object> jsonResponse) {
			jsonResponse.TryGetValue("x", out var ox);
			jsonResponse.TryGetValue("y", out var oy);
			jsonResponse.TryGetValue("z", out var oz);
			jsonResponse.TryGetValue("w", out var ow);

			x = ox is not null ? (float)(double)ox : 0;
			y = oy is not null ? (float)(double)oy : 0;
			z = oz is not null ? (float)(double)oz : 0;
			w = ow is not null ? (float)(double)ow : 0;
		}

		public static implicit operator Vector3(JsonSafeVector v) {
			return new Vector3(v.x, v.y, v.z);
		}
		
		public static implicit operator Vector2(JsonSafeVector v) {
			return new Vector2(v.x, v.y);
		}
		
		public static implicit operator Quaternion(JsonSafeVector v) {
			return new Quaternion(v.x, v.y, v.z, v.w);
		}

		public static implicit operator JsonSafeVector(Vector3 v) {
			return new JsonSafeVector(v);
		}
		
		public static implicit operator JsonSafeVector(Vector2 v) {
			return new JsonSafeVector(v);
		}
		
		public static implicit operator JsonSafeVector(Quaternion v) {
			return new JsonSafeVector(v);
		}
		

		// TODO investigate if it's worth adding other json libraries
		[JsonIgnore]
		public Vector2 vector2 => this;
		[JsonIgnore]
		public Vector3 vector3 => this;
		[JsonIgnore]
		public Quaternion quaternion => this;
	}

	[Serializable]
	public class JsonSafeTransform {
		public JsonSafeVector localPosition;
		public JsonSafeVector localScale;
		public JsonSafeVector localRotation;

		public JsonSafeTransform(Transform t) {
			localPosition = t.localRotation;
			localScale = t.localScale;
			localRotation = t.localRotation;
		}

		public JsonSafeTransform(string json) {
			JsonConvert.PopulateObject(json, this);
		}

		public Transform Assign(Transform t) {
			t.localRotation = localRotation;
			t.localPosition = localPosition;
			t.localScale = localScale;
			return t;
		}

		public static implicit operator JsonSafeTransform(Transform t) {
			return new JsonSafeTransform(t);
		}
	}

	[Serializable]
	public class JsonSafeRectTransform : JsonSafeTransform {
		public JsonSafeVector sizeDelta;
		public JsonSafeVector anchorMin;
		public JsonSafeVector anchorMax;
		public JsonSafeVector pivot;

		public JsonSafeRectTransform(RectTransform t) : base(t) {
			sizeDelta = t.sizeDelta;
			anchorMin = t.anchorMin;
			anchorMax = t.anchorMax;
		}

		public JsonSafeRectTransform(string json) : base(json) { }

		public RectTransform Assign(RectTransform t) {
			t.sizeDelta = sizeDelta;
			t.anchorMax = anchorMax;
			t.anchorMin = anchorMin;
			t.pivot = pivot;
			return (RectTransform)base.Assign(t);
		}
		
		public static implicit operator JsonSafeRectTransform(RectTransform t) {
			return new JsonSafeRectTransform(t);
		}
	}
	
   
	public static class PNJsonSafeExtensions {
		public static JsonSafeVector GetJsonSafe(this Vector3 v) {
			return v;
		}

		public static JsonSafeVector GetJsonSafe(this Vector2 v) {
			return v;
		}
		
		public static JsonSafeVector GetJsonSafe(this Quaternion v) {
			return v;
		}

		public static JsonSafeTransform GetJsonSafe(this Transform t) {
			return t;
		}

		public static JsonSafeRectTransform GetJsonSafe(this RectTransform t) {
			return t;
		}
	}
}