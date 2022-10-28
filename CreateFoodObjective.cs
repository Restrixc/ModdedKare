using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class CreateFoodObjective : DragonMailObjective
{
	[SerializeField]
	private LocalizedString description;

	[SerializeField]
	private List<ScriptableReagent> reagentFilter;

	[SerializeField]
	private int foodNeeded = 4;

	private int foodMade;

	public override void Register()
	{
		BucketWeapon.foodCreated += OnFoodCreatedEvent;
	}

	public override void Unregister()
	{
		BucketWeapon.foodCreated -= OnFoodCreatedEvent;
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		foodMade++;
		TriggerUpdate();
		if (foodMade >= foodNeeded)
		{
			TriggerComplete();
		}
	}

	private void OnFoodCreatedEvent(BucketWeapon bucket, ScriptableReagent reagent)
	{
		if (reagentFilter.Contains(reagent))
		{
			Advance(bucket.transform.position);
		}
	}

	public override string GetTitle()
	{
		return title.GetLocalizedString() + " " + foodMade + "/" + foodNeeded;
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}
}
