using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class KoboldSpaceProgram : DragonMailObjective
{
	[SerializeField]
	private LocalizedString description;

	[SerializeField]
	private float height = 500f;

	[SerializeField]
	private MonoBehaviour logicOwner;

	private float currentMaxHeight = 0f;

	private HashSet<Kobold> kobolds;

	private Coroutine routine;

	public override void Register()
	{
		base.Register();
		kobolds = new HashSet<Kobold>(Object.FindObjectsOfType<Kobold>());
		Kobold.spawned += OnKoboldSpawn;
		routine = logicOwner.StartCoroutine(Think());
	}

	public override void Unregister()
	{
		base.Unregister();
		Kobold.spawned -= OnKoboldSpawn;
		if (routine != null)
		{
			logicOwner.StopCoroutine(routine);
		}
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	private void OnKoboldSpawn(Kobold kobold)
	{
		kobolds.Add(kobold);
	}

	private IEnumerator Think()
	{
		while (logicOwner.isActiveAndEnabled)
		{
			float maxHeight = 0f;
			Kobold maxHeightKobold = null;
			foreach (Kobold kobold in kobolds)
			{
				if (kobold.transform.position.y > maxHeight)
				{
					maxHeightKobold = kobold;
					maxHeight = kobold.hip.transform.position.y;
				}
			}
			if (maxHeightKobold != null && maxHeight > height)
			{
				Advance(maxHeightKobold.transform.position);
				break;
			}
			currentMaxHeight = maxHeight;
			TriggerUpdate();
			yield return null;
		}
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}

	public override string GetTitle()
	{
		return $"{title.GetLocalizedString()} {currentMaxHeight:N0}/{height:N0}";
	}
}
