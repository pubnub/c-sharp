using System;
using UnityEngine;
using Newtonsoft.Json;

namespace PubnubApi.Unity {
	[Serializable]
	public class VectorSerializable {
		public float x;
		public float y;
		public float z;

		public VectorSerializable(float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public VectorSerializable(Vector3 sourceVector) {
			x = sourceVector.x;
			y = sourceVector.y;
			z = sourceVector.z;
		}

		public VectorSerializable(Vector2 sourceVector) {
			x = sourceVector.x;
			y = sourceVector.y;
			z = 0;
		}

		public static implicit operator Vector3(VectorSerializable v) {
			return new Vector3(v.x, v.y, v.z);
		}
		
		public static implicit operator Vector2(VectorSerializable v) {
			return new Vector2(v.x, v.y);
		}

		public static implicit operator VectorSerializable(Vector3 v) {
			return new VectorSerializable(v);
		}
		
		public static implicit operator VectorSerializable(Vector2 v) {
			return new VectorSerializable(v);
		}

		// TODO investigate if it's worth adding other json libraries
		[JsonIgnore]
		public Vector2 Vector2 => this;
		[JsonIgnore]
		public Vector3 Vector3 => this;
	}
   
	public static class VectorExtensions {
		public static VectorSerializable Serializable(this Vector3 v) {
			return v;
		}

		public static VectorSerializable Serializable(this Vector2 v) {
			return v;
		}
	}
}