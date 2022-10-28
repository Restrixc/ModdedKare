using System.Collections;
using TMPro;
using UnityEngine;

public class SliderAmountDisplay : MonoBehaviour
{
	public CanvasGroup group;

	private WaitForEndOfFrame wait = new WaitForEndOfFrame();

	public TextMeshProUGUI targetText;

	public IEnumerator FadeOut()
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + 2f)
		{
			yield return null;
		}
		while (group.alpha != 0f)
		{
			group.alpha = Mathf.MoveTowards(group.alpha, 0f, Time.unscaledDeltaTime);
			yield return wait;
		}
	}

	public void UpdateText(float single)
	{
		if (single.ToString().Length > 4)
		{
			targetText.text = single.ToString("0.00");
		}
		else
		{
			targetText.text = single.ToString();
		}
		group.alpha = 1f;
		StopAllCoroutines();
		StartCoroutine("FadeOut");
	}
}
