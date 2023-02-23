using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedTextPopup : MonoBehaviour {
	public float speed = 5f;
	
	private float progress = 0;
	private int direction = 1;
	private RectTransform rt => (RectTransform)transform;
	private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	private Image image;
	private float initialAlhpa;

	public event Action<int> onFinished;
	
	public void PlayForward() {
		direction = 1;
		enabled = true;
	}

	public void PlayBackwards(bool destroy = false) {
		direction = -1;
		enabled = true;

		if (destroy) {
			onFinished += (n) => Destroy(gameObject);
		}
	}

	private void Awake() {
		image = GetComponent<Image>();
		initialAlhpa = image.color.a;
	}

	void Update() {
		progress = Mathf.Clamp01(Time.deltaTime * direction * speed + progress);
		
		// rt.localScale = new Vector3(1f, Mathf.Max(float.Epsilon,  animationCurve.Evaluate(progress)), 1f);
		rt.localScale = Vector3.one * Mathf.Max(float.Epsilon,  animationCurve.Evaluate(progress));

		var c = image.color;
		c.a = Mathf.Lerp(0, initialAlhpa, progress);
		image.color = c;
		
		if (progress * 2 - 1 == direction) {
			enabled = false;
		}
	}

	private void OnDisable() {
		direction *= -1;

		if (progress % 1f != 0) {
			enabled = true;
			return;
		}
		
		onFinished?.Invoke((int)progress);
	}
}