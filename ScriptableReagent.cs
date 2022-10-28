using UnityEngine;
using UnityEngine.Localization;


public class ScriptableReagent : ScriptableObject
{
	[SerializeField]
	private LocalizedString localizedName;

	[SerializeField]
	private Color color;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color emission;

	[SerializeField]
	private float value;

	[SerializeField]
	private float metabolizationHalfLife;

	[SerializeField]
	private bool cleaningAgent;

	[SerializeField]
	private float calories = 0f;

	[SerializeField]
	private GameObject display;

	[SerializeReferenceButton]
	[SerializeReference]
	[SerializeField]
	private ReagentConsumptionEvent consumptionEvent;

	public LocalizedString GetLocalizedName()
	{
		return localizedName;
	}

	public Color GetColor()
	{
		return color;
	}

	public Color GetColorEmission()
	{
		return emission;
	}

	public float GetValue()
	{
		return value;
	}

	public float GetMetabolizationHalfLife()
	{
		return metabolizationHalfLife;
	}

	public bool IsCleaningAgent()
	{
		return cleaningAgent;
	}

	public float GetCalories()
	{
		return calories;
	}

	public GameObject GetDisplayPrefab()
	{
		return display;
	}

	public ReagentConsumptionEvent GetConsumptionEvent()
	{
		return consumptionEvent;
	}

	public Reagent GetReagent(float volume)
	{
		return new Reagent
		{
			id = ReagentDatabase.GetID(this),
			volume = volume
		};
	}

	private void OnValidate()
	{
		consumptionEvent.OnValidate();
	}
}
