using System.Collections;
using UnityEngine;

public class SpeechBubblePopup : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve bounceCurve;

	[SerializeField]
	private RectTransform targetTransform;

	private void OnEnable()
	{
		base.transform.localScale = Vector3.one * 0.001f;
		StartCoroutine(ShowRoutine());
	}

	private IEnumerator ShowRoutine()
	{
		float startTime = Time.time;
		float duration = 0.4f;
		Vector2 sizeDelta = targetTransform.sizeDelta;
		Vector3 desiredScale = new Vector3(sizeDelta.x, sizeDelta.y, 1f);
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			float sample = bounceCurve.Evaluate(t);
			base.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.001f, desiredScale, sample);
			yield return null;
		}
		base.transform.localScale = desiredScale;
	}
}
