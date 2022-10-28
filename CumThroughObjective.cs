using System;
using PenetrationTech;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class CumThroughObjective : DragonMailObjective
{
	[SerializeField]
	private LocalizedString description;

	public override void Register()
	{
		base.Register();
		DickInfo.cumThrough += OnCumThrough;
	}

	public override void Unregister()
	{
		base.Unregister();
		DickInfo.cumThrough -= OnCumThrough;
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnCumThrough(Penetrable genes)
	{
		Kobold kobold = genes.GetComponentInParent<Kobold>();
		if (kobold != null)
		{
			Advance(kobold.transform.position);
		}
	}

	public override string GetTitle()
	{
		return title.GetLocalizedString() ?? "";
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}
}
