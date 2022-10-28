using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatTextDisplay : MonoBehaviour
{
	private TextMeshProUGUI text;

	[SerializeField]
	private string startingText;

	[SerializeField]
	private MoneyHolder holder;

	private float oldMoney;

	private Coroutine routine;

	private void Start()
	{
		text = GetComponent<TextMeshProUGUI>();
		text.text = startingText + Mathf.Round(holder.GetMoney());
		oldMoney = holder.GetMoney();
		MoneyHolder moneyHolder = holder;
		moneyHolder.moneyChanged = (MoneyHolder.MoneyChangedAction)Delegate.Combine(moneyHolder.moneyChanged, new MoneyHolder.MoneyChangedAction(OnMoneyChanged));
	}

	private void OnMoneyChanged(float newMoney)
	{
		if (routine != null)
		{
			StopCoroutine(routine);
		}
		routine = StartCoroutine(MoneyUpdateRoutine(oldMoney, newMoney));
	}

	private IEnumerator MoneyUpdateRoutine(float from, float to)
	{
		float startTime = Time.time;
		float duration = 1f;
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			oldMoney = Mathf.Lerp(from, to, t);
			text.text = startingText + Mathf.Round(oldMoney);
			yield return null;
		}
	}
}
