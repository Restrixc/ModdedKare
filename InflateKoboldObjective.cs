using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class InflateKoboldObjective : DragonMailObjective
{
	[SerializeField]
	private LocalizedString description;

	public override void Register()
	{
		base.Register();
		GenericReagentContainer.containerInflated += OnInflatedEvent;
	}

	public override void Unregister()
	{
		base.Unregister();
		GenericReagentContainer.containerInflated -= OnInflatedEvent;
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnInflatedEvent(GenericReagentContainer container)
	{
		if (container.maxVolume > 20f && container.TryGetComponent<Kobold>(out var kobold))
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
