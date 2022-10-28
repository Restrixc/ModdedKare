using System.Collections;
using TMPro;
using UnityEngine;

public class CommandTextDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text outputText;

	[SerializeField]
	private CanvasGroup group;

	private bool forceVisible;

	private WaitForSeconds waitForSeconds;

	private void Awake()
	{
		waitForSeconds = new WaitForSeconds(5f);
	}

	private void OnEnable()
	{
		CheatsProcessor.AddOutputChangedListener(OnTextOutputChanged);
	}

	private void OnDisable()
	{
		CheatsProcessor.RemoveOutputChangedListener(OnTextOutputChanged);
	}

	private void OnTextOutputChanged(string message)
	{
		group.interactable = true;
		group.alpha = 1f;
		outputText.text = message;
		StopAllCoroutines();
		StartCoroutine(WaitThenRemove());
	}

	public void ForceVisible(bool visible)
	{
		if (visible)
		{
			StopAllCoroutines();
			group.interactable = true;
			group.alpha = 1f;
		}
		else if (base.isActiveAndEnabled)
		{
			StopAllCoroutines();
			StartCoroutine(WaitThenRemove());
		}
		else
		{
			group.interactable = false;
			group.alpha = 0f;
		}
		forceVisible = visible;
	}

	private IEnumerator WaitThenRemove()
	{
		group.interactable = false;
		yield return waitForSeconds;
		yield return new WaitUntil(() => !forceVisible);
		float startTime = Time.time;
		float duration = 1f;
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			group.alpha = 1f - t;
			yield return null;
		}
		group.interactable = false;
		group.alpha = 0f;
	}
}
