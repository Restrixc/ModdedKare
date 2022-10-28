using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

public class ExpandFarmObjective : ObjectiveWithSpaceBeam
{
	[SerializeField]
	private LocalizedString description;

	[SerializeField]
	private MonoBehaviour thinker;

	public override void Register()
	{
		bool found = false;
		SoilTile[] array = Object.FindObjectsOfType<SoilTile>();
		foreach (SoilTile tile in array)
		{
			if (tile.GetDebris())
			{
				spaceBeamTarget = tile.transform;
				found = true;
				break;
			}
		}
		base.Register();
		SoilTile.tileCleared += OnTileClear;
		if (!found)
		{
			thinker.StartCoroutine(WaitThenClear());
		}
	}

	private IEnumerator WaitThenClear()
	{
		yield return null;
		TriggerComplete();
	}

	public override void Unregister()
	{
		base.Unregister();
		SoilTile.tileCleared -= OnTileClear;
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnTileClear(SoilTile tile)
	{
		Advance(tile.transform.position);
	}

	public override string GetTitle()
	{
		return title.GetLocalizedString() + " 0/1";
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}
}
