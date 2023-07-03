using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnimatedVerticalLayout : MonoBehaviour {

	public float offset = 80f;
	
	
	void Update() {
		int i = 0;
		var children = transform.Cast<RectTransform>().Reverse();
		foreach (RectTransform t  in children) {
			var pos = t.anchoredPosition;
			pos.y = Mathf.Lerp( pos.y ,offset * i, Time.deltaTime * Mathf.Max(16 - i * 4, 8f));
			t.anchoredPosition = pos;
			i++;
		}
	}
}