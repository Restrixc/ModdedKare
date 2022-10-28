using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyFloater : MonoBehaviour
{
	[SerializeField]
	private Transform textTransform;

	[SerializeField]
	private TMP_Text text;

	private Coroutine routine;

	private int nextUpdate;

	private const float fadeDistance = 5f;

	public void SetBounds(Bounds target)
	{
		textTransform.position = target.center;
		textTransform.localScale = Vector3.one * target.size.magnitude;
	}

	public void SetText(string newText)
	{
		text.text = newText;
	}

	private void OnEnable()
	{
		routine = StartCoroutine(UpdateRoutine());
	}

	private void OnDisable()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
		}
	}

	private IEnumerator UpdateRoutine()
	{
		while (base.isActiveAndEnabled)
		{
			for (int i = 0; i < nextUpdate; i++)
			{
				yield return null;
			}
			Camera check = Camera.main;
			if (check == null)
			{
				nextUpdate = 64;
				continue;
			}
			float distance = textTransform.DistanceTo(check.transform);
			textTransform.LookAt(check.transform, Vector3.up);
			text.alpha = Mathf.Clamp01(5f - distance);
			nextUpdate = ((distance < 6f) ? 1 : 64);
		}
	}
}
