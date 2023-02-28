using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedTextPopup : MonoBehaviour {
	public float speed = 5f;
	public AnimationCurve inAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public AnimationCurve outAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	private AnimationCurve currentAnimationCurve;
	private float progress = 0;
	private int direction = 1;
	private RectTransform rt => (RectTransform)transform;

	private CanvasGroup cg;

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

	private void OnEnable() {
		currentAnimationCurve = direction == 1 ? inAnimationCurve : outAnimationCurve;
	}

	private void Awake() {
		cg = GetComponent<CanvasGroup>();
	}

	void Update() {
		progress = Mathf.Clamp01(Time.deltaTime * direction * speed + progress);
		
		// rt.localScale = new Vector3(1f, Mathf.Max(float.Epsilon,  animationCurve.Evaluate(progress)), 1f);
		rt.localScale = Vector3.one * Mathf.Max(float.Epsilon,  currentAnimationCurve.Evaluate(progress));

		cg.alpha = progress;
		
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