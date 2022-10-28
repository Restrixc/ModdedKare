using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;


public class StatusEffect : ScriptableObject, ITooltipDisplayable
{
	[Serializable]
	public class Statistic
	{
		public Stat statType;

		public float multiplier = 1f;

		public float addAmount = 0f;
	}

	public GameObject statDisplayPrefab;

	[HideInInspector]
	public int guid;

	private static Dictionary<int, StatusEffect> availableStatuses = new Dictionary<int, StatusEffect>();

	public Sprite sprite;

	public LocalizedString localizedName;

	public LocalizedString localizedDescription;

	public List<Statistic> statistics;

	public bool stacks = false;

	public float duration = float.PositiveInfinity;

	[RuntimeInitializeOnLoadMethod]
	public void OnInitialize()
	{
		if (!availableStatuses.ContainsKey(guid))
		{
			availableStatuses.Add(guid, this);
		}
	}

	public void OnEnable()
	{
		OnInitialize();
	}

	public int GetID()
	{
		return guid;
	}

	public static StatusEffect GetFromID(int id)
	{
		if (availableStatuses.ContainsKey(id))
		{
			return availableStatuses[id];
		}
		return null;
	}

	public void OnTooltipDisplay(RectTransform panel)
	{
		GameObject someText = new GameObject("StatusName", typeof(TextMeshProUGUI));
		someText.transform.SetParent(panel, worldPositionStays: false);
		someText.GetComponent<TextMeshProUGUI>().text = localizedName.GetLocalizedString();
		foreach (Statistic s in statistics)
		{
			GameObject display = UnityEngine.Object.Instantiate(statDisplayPrefab, panel);
			string displayString = "";
			if (s.addAmount > 0f)
			{
				displayString = displayString + " +" + s.addAmount;
			}
			else if (s.addAmount < 0f)
			{
				displayString += s.addAmount;
			}
			if (s.multiplier != 1f)
			{
				displayString = displayString + " x" + s.multiplier;
			}
			display.GetComponentInChildren<TextMeshProUGUI>().text = displayString;
			display.transform.Find("Image").GetComponentInChildren<Image>().sprite = s.statType.sprite;
		}
	}

	public void OnValidate()
	{
		OnInitialize();
		while (GetFromID(guid) != this)
		{
			guid++;
			OnInitialize();
		}
	}
}
