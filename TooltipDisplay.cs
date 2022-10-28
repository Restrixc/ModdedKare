using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public static GameObject tooltip;

	public GameObject tooltipPrefab;

	public Object thingToDisplay;

	public Coroutine runningTask;

	public IEnumerator DisplayForSomeTime(float duration)
	{
		if (tooltip == null)
		{
			tooltip = Object.Instantiate(tooltipPrefab, null);
			tooltip.SetActive(value: false);
		}
		for (int i = 0; i < tooltip.transform.childCount; i++)
		{
			Object.Destroy(tooltip.transform.GetChild(i).gameObject);
		}
		Canvas c = base.transform.GetComponentInParent<Canvas>();
		tooltip.transform.SetParent(c.transform, worldPositionStays: false);
		tooltip.transform.position = base.transform.position;
		tooltip.transform.localScale = Vector3.one;
		tooltip.SetActive(value: true);
		if (thingToDisplay is ITooltipDisplayable displayable)
		{
			displayable.OnTooltipDisplay(tooltip.GetComponent<RectTransform>());
		}
		yield return new WaitForSeconds(duration);
		tooltip.SetActive(value: false);
		tooltip.transform.SetParent(null);
	}

	public void OnDisable()
	{
		if (runningTask != null)
		{
			StopCoroutine(runningTask);
		}
		tooltip.SetActive(value: false);
		tooltip.transform.SetParent(null);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (runningTask != null)
		{
			StopCoroutine(runningTask);
		}
		runningTask = StartCoroutine(DisplayForSomeTime(20f));
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnDisable();
	}
}
