using UnityEngine;
using UnityEngine.Localization;

public class FinalObjective : ObjectiveWithSpaceBeam
{
	[SerializeField]
	private LocalizedString description;

	public override void Register()
	{
		base.Register();
		KoboldDelivery.spawnedKobold += OnSpawnedKobold;
	}

	public override void Unregister()
	{
		base.Unregister();
		KoboldDelivery.spawnedKobold -= OnSpawnedKobold;
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnSpawnedKobold(Kobold a)
	{
		Advance(a.transform.position);
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}

	public override string GetTitle()
	{
		return title.GetLocalizedString() ?? "";
	}
}
