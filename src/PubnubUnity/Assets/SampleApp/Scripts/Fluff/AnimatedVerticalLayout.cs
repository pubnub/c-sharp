using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedVerticalLayout : MonoBehaviour {

	public float offset = 80f;
	
	
	void Update() {
		int i = 0;
		foreach (RectTransform t  in transform) {
			var pos = t.anchoredPosition;
			pos.y = Mathf.Lerp( pos.y ,offset * i, Time.deltaTime * 24);
			t.anchoredPosition = pos;
			i++;
		}
	}
}