using System;
using UnityEngine;
using Newtonsoft.Json;

namespace PubnubApi.Unity {
	[Serializable]
	public class VectorSerializable {
		public float x;
		public float y;
		public float z;
		public float w;

		public VectorSerializable(float x, float y, float z = 0f, float w = 0f) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public VectorSerializable(Vector3 sourceVector) {
			x = sourceVector.x;
			y = sourceVector.y;
			z = sourceVector.z;
		}

		public VectorSerializable(Vector2 sourceVector) {
			x = sourceVector.x;
			y = sourceVector.y;
		}

		public VectorSerializable(Quaternion sourceQuaternion) {
			x = sourceQuaternion.x;
			y = sourceQuaternion.y;
			z = sourceQuaternion.z;
			w = sourceQuaternion.w;
		}

		public static implicit operator Vector3(VectorSerializable v) {
			return new Vector3(v.x, v.y, v.z);
		}
		
		public static implicit operator Vector2(VectorSerializable v) {
			return new Vector2(v.x, v.y);
		}
		
		public static implicit operator Quaternion(VectorSerializable v) {
			return new Quaternion(v.x, v.y, v.z, v.w);
		}

		public static implicit operator VectorSerializable(Vector3 v) {
			return new VectorSerializable(v);
		}
		
		public static implicit operator VectorSerializable(Vector2 v) {
			return new VectorSerializable(v);
		}
		
		public static implicit operator VectorSerializable(Quaternion v) {
			return new VectorSerializable(v);
		}
		

		// TODO investigate if it's worth adding other json libraries
		[JsonIgnore]
		public Vector2 vector2 => this;
		[JsonIgnore]
		public Vector3 vector3 => this;
		[JsonIgnore]
		public Quaternion quaternion => this;
	}
   
	public static class PNVectorExtensions {
		public static VectorSerializable GetSerializable(this Vector3 v) {
			return v;
		}

		public static VectorSerializable GetSerializable(this Vector2 v) {
			return v;
		}
		
		public static VectorSerializable GetSerializable(this Quaternion v) {
			return v;
		}
	}
}